// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Emit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Extensions;
    using Desalt.Core.Utility;

    /// <summary>
    /// General-purpose emitter for serializing abstract syntax nodes into code.
    /// </summary>
    public class Emitter : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly StreamWriter _streamWriter;
        private readonly IndentedTextWriter _writer;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public Emitter(Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            Encoding = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            Options = options ?? EmitOptions.Default;

            _streamWriter = new StreamWriter(outputStream, Encoding, bufferSize: 1024, leaveOpen: true)
            {
                AutoFlush = true
            };
            _writer = new IndentedTextWriter(_streamWriter, Options.IndentationPrefix)
            {
                NewLine = Options.Newline
            };
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
            _writer.Write(text);
        }

        public void WriteLine()
        {
            _writer.WriteLine();
        }

        public void WriteLine(string text)
        {
            _writer.WriteLine(text);
        }

        public void WriteBlankLine()
        {
            _writer.WriteBlankLine();
        }

        /// <summary>
        /// Writes a block of elements, using the options to format the code.
        /// </summary>
        /// <typeparam name="TElement">The type of <see cref="IAstNode"/> to emit.</typeparam>
        /// <param name="blockElements">The elements to visit.</param>
        /// <param name="elementAction">The action to perform on each element.</param>
        public void WriteBlock<TElement>(IEnumerable<TElement> blockElements, Action<TElement> elementAction)
            where TElement : IAstNode
        {
            if (blockElements == null) { throw new ArgumentNullException(nameof(blockElements)); }
            if (elementAction == null) { throw new ArgumentNullException(nameof(elementAction)); }

            TElement[] array = blockElements.ToSafeArray();

            // check empty blocks
            if (array.Length == 0 && !Options.SimpleBlockOnNewLine)
            {
                _writer.Write("{ }");
                return;
            }

            bool isSimpleBlock = array.Length < 2;

            WriteBlock(isSimpleBlock: isSimpleBlock, writeBodyAction: () =>
            {
                for (int i = 0; i < array.Length; i++)
                {
                    elementAction(array[i]);

                    // don't add a blank line after the last statement
                    if (!isSimpleBlock && i < array.Length - 1)
                    {
                        _writer.WriteLine();
                    }
                }
            });
        }

        /// <summary>
        /// Wraps the <paramref name="writeBodyAction"/> inside of an indented block.
        /// </summary>
        /// <param name="writeBodyAction">The action to wrap inside of an indented block.</param>
        /// <param name="isSimpleBlock">
        /// Indicates whether the block should be treated as a simple block, which has special formatting.
        /// </param>
        public void WriteBlock(Action writeBodyAction, bool isSimpleBlock = false)
        {
            if (writeBodyAction == null)
            {
                throw new ArgumentNullException(nameof(writeBodyAction));
            }

            _writer.Write("{");

            bool indentBlock = isSimpleBlock && Options.SimpleBlockOnNewLine || !isSimpleBlock;
            if (indentBlock)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }
            else
            {
                _writer.Write(" ");
            }

            writeBodyAction();

            if (indentBlock)
            {
                _writer.IndentLevel--;
            }

            if (isSimpleBlock && Options.SimpleBlockOnNewLine || !isSimpleBlock)
            {
                _writer.WriteLine();
            }
            else
            {
                _writer.Write(" ");
            }

            _writer.Write("}");
        }

        /// <summary>
        /// Writes a list of elements using a comma between elements. The comma is not written on the
        /// last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        /// <param name="elementAction">The action to perform on each element.</param>
        public void WriteCommaList<TElem>(IEnumerable<TElem> elements, Action<TElem> elementAction)
        {
            WriteList(elements, ", ", elementAction);
        }

        /// <summary>
        /// Writes a list of elements using the specified delimiter between elements. The delimiter
        /// is not written on the last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        /// <param name="delimiter">The delimiter to use between elements.</param>
        /// <param name="elementAction">The action to perform on each element.</param>
        /// <param name="newlineBetweenElements">
        /// Indicates whether to insert a new line between elements, after the delimiter. This is to
        /// allow the indentation to be correct.
        /// </param>
        public void WriteList<TElem>(
            IEnumerable<TElem> elements,
            string delimiter,
            Action<TElem> elementAction,
            bool newlineBetweenElements = false)
        {
            if (elements == null) { throw new ArgumentNullException(nameof(elements)); }
            if (delimiter == null) { throw new ArgumentNullException(nameof(delimiter)); }
            if (elementAction == null) { throw new ArgumentNullException(nameof(elementAction)); }
            if (delimiter.Length == 0)
            {
                throw new ArgumentException($"{nameof(delimiter)} can't be empty", nameof(delimiter));
            }

            TElem[] array = elements.ToSafeArray();
            for (int i = 0; i < array.Length; i++)
            {
                TElem element = array[i];
                if (element != null)
                {
                    elementAction(element);
                }

                if (i < array.Length - 1)
                {
                    if (newlineBetweenElements)
                    {
                        _writer.WriteLine(delimiter);
                    }
                    else
                    {
                        _writer.Write(delimiter);
                    }
                }
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
    }
}
