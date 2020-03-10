// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Emit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Desalt.CompilerUtilities;
    using Desalt.TypeScriptAst.Ast;

    /// <summary>
    /// General-purpose emitter for serializing abstract syntax nodes into code.
    /// </summary>
    public class Emitter : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly Encoding DefaultEncoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);

        private readonly StreamWriter _streamWriter;
        private readonly IndentedTextWriter _writer;

        private bool _lastWriteWasWhitespace = true;
        private bool _nextWriteRequiresSpace;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public Emitter(Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            Encoding = encoding ?? DefaultEncoding;
            Options = options ?? EmitOptions.Default;

            _streamWriter =
                new StreamWriter(outputStream, Encoding, bufferSize: 1024, leaveOpen: true) { AutoFlush = true };
            _writer = new IndentedTextWriter(_streamWriter, Options.IndentationPrefix) { NewLine = Options.Newline };
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Encoding Encoding { get; }

        public EmitOptions Options { get; }

        public int IndentLevel
        {
            get => _writer.IndentLevel;
            set => _writer.IndentLevel = value;
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Write(string text)
        {
            WriteLeadingSpaceIfNeeded(text);
            _writer.Write(text);
            SetLastWriteWasWhitespace(text);
        }

        public void WriteLine()
        {
            _writer.WriteLine();
            _lastWriteWasWhitespace = true;
            _nextWriteRequiresSpace = false;
        }

        public void WriteLine(string text)
        {
            WriteLeadingSpaceIfNeeded(text);
            _writer.WriteLine(text);
            _lastWriteWasWhitespace = true;
        }

        /// <summary>
        /// Writes the specified string to a line without tabs.
        /// </summary>
        public void WriteLineWithoutIndentation()
        {
            _writer.WriteLineWithoutIndentation();
            _lastWriteWasWhitespace = true;
            _nextWriteRequiresSpace = false;
        }

        /// <summary>
        /// Writes the keyword, ensuring that it is surrounded by spaces.
        /// </summary>
        /// <param name="keyword"></param>
        public void WriteKeyword(string keyword)
        {
            _nextWriteRequiresSpace = true;
            WriteLeadingSpaceIfNeeded(keyword);

            _writer.Write(keyword);

            SetLastWriteWasWhitespace(keyword);
            _nextWriteRequiresSpace = true;
        }

        /// <summary>
        /// Writes a list of items wrapped in a {} block.
        /// </summary>
        /// <param name="items">The items to write.</param>
        /// <param name="skipNewlines">
        /// Indicates whether to skip writing newlines between items. Useful for writing blocks of
        /// statements that already have newlines.
        /// </param>
        public void WriteBlock(IReadOnlyList<ITsAstNode> items, bool skipNewlines = false)
        {
            WriteList(
                items,
                indent: true,
                prefix: "{",
                suffix: "}",
                itemDelimiter: skipNewlines ? string.Empty : Options.Newline,
                newLineAfterPrefix: true,
                delimiterAfterLastItem: true,
                newLineBeforeFirstItem: !skipNewlines,
                newLineAfterLastItem: !skipNewlines,
                emptyContents: "{ }");
        }

        /// <summary>
        /// Writes a list of items separated by a comma.
        /// </summary>
        /// <param name="items">The items to write.</param>
        public void WriteCommaList(IReadOnlyList<ITsAstNode> items)
        {
            WriteList(items, indent: false, itemDelimiter: ", ");
        }

        /// <summary>
        /// Writes a list of items wrapped in a {} block where each item is separated by a comma and
        /// new line.
        /// </summary>
        /// <param name="items">The items to write.</param>
        public void WriteCommaNewlineSeparatedBlock(IReadOnlyList<ITsAstNode> items)
        {
            WriteList(
                items,
                indent: true,
                prefix: "{",
                suffix: "}",
                itemDelimiter: "," + Options.Newline,
                newLineAfterPrefix: true,
                newLineAfterLastItem: true,
                emptyContents: "{}");
        }

        /// <summary>
        /// Writes a comma-separated list wrapped in a () block.
        /// </summary>
        /// <param name="items">The items to write.</param>
        public void WriteParameterList(IReadOnlyList<ITsAstNode> items)
        {
            WriteList(items, indent: false, prefix: "(", suffix: ")", itemDelimiter: ", ");
        }

        /// <summary>
        /// Writes a list of items, surrounded by the specified prefix and suffix and delimited by
        /// the specified delimiter.
        /// </summary>
        /// <param name="items">The items to write.</param>
        /// <param name="indent">Indicates whether to indent the items.</param>
        /// <param name="prefix">The prefix to write before writing the items.</param>
        /// <param name="suffix">The suffix to write after writing the items.</param>
        /// <param name="itemDelimiter">The delimiter to write between items.</param>
        /// <param name="newLineAfterPrefix">
        /// Indicates whether a newline should be written after the prefix (useful for blocks).
        /// </param>
        /// <param name="delimiterAfterLastItem">
        /// Indicates whether the last item should also have a delimiter at the end (useful for
        /// blocks to put a newline before the last brace.
        /// </param>
        /// <param name="newLineBeforeFirstItem">
        /// Indicates whether a newline should be included before the first item.
        /// </param>
        /// <param name="newLineAfterLastItem">
        /// Indicates whether the last item should include a newline.
        /// </param>
        /// <param name="emptyContents">
        /// The contents to write if the list is empty. If not supplied, it will just be <c><paramref
        /// name="prefix"/><paramref name="suffix"/></c>
        /// </param>
        public void WriteList(
            IReadOnlyList<ITsAstNode> items,
            bool indent,
            string prefix = null,
            string suffix = null,
            string itemDelimiter = null,
            bool newLineAfterPrefix = false,
            bool delimiterAfterLastItem = false,
            bool newLineBeforeFirstItem = false,
            bool newLineAfterLastItem = false,
            string emptyContents = null)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            prefix = prefix ?? string.Empty;
            suffix = suffix ?? string.Empty;
            itemDelimiter = itemDelimiter ?? string.Empty;

            // Special case - if the list is empty
            if (items.Count == 0)
            {
                Write(emptyContents ?? $"{prefix}{suffix}");
                return;
            }

            bool newlineAfterItems = false;
            if (itemDelimiter.EndsWith("\r\n", StringComparison.Ordinal))
            {
                newlineAfterItems = true;
                itemDelimiter = itemDelimiter.Substring(0, itemDelimiter.Length - 2);
            }
            else if (itemDelimiter.EndsWith("\r", StringComparison.Ordinal) ||
                itemDelimiter.EndsWith("\n", StringComparison.Ordinal))
            {
                newlineAfterItems = true;
                itemDelimiter = itemDelimiter.Substring(0, itemDelimiter.Length - 1);
            }

            // write a new line before the first item if necessary
            if (newLineAfterPrefix || newlineAfterItems && newLineBeforeFirstItem)
            {
                WriteLine(prefix);
            }
            else
            {
                Write(prefix);
            }

            if (indent)
            {
                IndentLevel++;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ITsAstNode item = items[i];
                item?.Emit(this);

                // write the delimiter
                if (i < items.Count - 1 || delimiterAfterLastItem && itemDelimiter.Length > 0)
                {
                    Write(itemDelimiter);
                }

                // write a new line after the last item if necessary
                if (i < items.Count - 1 && newlineAfterItems || i == items.Count - 1 && newLineAfterLastItem)
                {
                    WriteLine();
                }
            }

            if (indent)
            {
                IndentLevel--;
            }

            Write(suffix);
        }

        /// <summary>
        /// Writes a statement on a new line unless the statement is a block, in which case the block
        /// will start on the same line.
        /// </summary>
        /// <param name="statement">The statement to emit.</param>
        /// <param name="isBlockStatement">Indicates if the statement is a block statement.</param>
        /// <param name="prefixForIndentedStatement">
        /// If supplied, the prefix is written before the statement when <paramref
        /// name="isBlockStatement"/> is false.
        /// </param>
        /// <param name="prefixForBlock">
        /// If supplied, the prefix is written before the statement when <paramref
        /// name="isBlockStatement"/> is true.
        /// </param>
        /// <param name="newlineAfterBlock">
        /// Indicates whether to add a newline after the block if it's a block statement.
        /// </param>
        public void WriteStatementIndentedOrInBlock(
            ITsAstNode statement,
            bool isBlockStatement,
            string prefixForIndentedStatement = null,
            string prefixForBlock = null,
            bool newlineAfterBlock = false)
        {
            if (isBlockStatement)
            {
                Write(prefixForBlock ?? string.Empty);
                statement.Emit(this);
                if (newlineAfterBlock)
                {
                    WriteLine();
                }
            }
            else
            {
                Write(prefixForIndentedStatement ?? string.Empty);
                WriteLine();
                IndentLevel++;
                statement.Emit(this);
                IndentLevel--;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.Dispose();
                _streamWriter.Dispose();
            }
        }

        private void WriteLeadingSpaceIfNeeded(string text)
        {
            // don't do anything if we don't need to write a space
            if (!_nextWriteRequiresSpace)
            {
                return;
            }

            // if we already wrote a whitespace, don't do anything
            if (_lastWriteWasWhitespace)
            {
                return;
            }

            // if the first character that we're going to write is whitespace, then we don't need to write another space
            if (!string.IsNullOrEmpty(text) && char.IsWhiteSpace(text, 0))
            {
                return;
            }

            _writer.Write(" ");
            _nextWriteRequiresSpace = false;
            _lastWriteWasWhitespace = true;
        }

        private void SetLastWriteWasWhitespace(string text)
        {
            // don't set the flag if we didn't write anything
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _lastWriteWasWhitespace = char.IsWhiteSpace(text, text.Length - 1);
        }
    }
}
