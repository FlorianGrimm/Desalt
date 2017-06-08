// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.Core.Utility;
    using Desalt.JavaScript.CodeModels;
    using Desalt.JavaScript.CodeModels.Statements;

    /// <summary>
    /// Takes an <see cref="IEs5CodeModel"/> and converts it to text.
    /// </summary>
    public partial class Es5Emitter : Es5Visitor, IEmitter<IEs5CodeModel>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly Encoding DefaultEncoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private IndentedTextWriter _writer;
        private EmitOptions _options;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Emit(IEs5CodeModel model, Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (outputStream == null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            _options = options ?? EmitOptions.Default;
            encoding = encoding ?? DefaultEncoding;

            using (var streamWriter = new StreamWriter(outputStream, encoding, bufferSize: 1024, leaveOpen: true))
            using (_writer = new IndentedTextWriter(streamWriter, _options.IndentationPrefix))
            {
                streamWriter.AutoFlush = true;
                _writer.NewLine = _options.Newline;

                Visit(model);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _writer?.Dispose();
            _writer = null;
        }

        private void WriteBlock(Es5BlockStatement blockStatement) => WriteBlock(blockStatement.Statements);

        private void WriteBlock(IEnumerable<IEs5SourceElement> sourceElements)
        {
            IEs5SourceElement[] array = sourceElements.ToSafeArray();

            // check empty blocks
            if (array.Length == 0 && !_options.SimpleBlockOnNewLine)
            {
                _writer.Write(_options.SpaceWithinEmptyFunctionBody ? "{ }" : "{}");
                return;
            }

            // check simple blocks and put them on one line
            if (array.Length == 1 && !_options.SimpleBlockOnNewLine)
            {
                _writer.Write(_options.SpaceWithinSimpleBlockBraces ? "{ " : "{");
                Visit(array[0]);
                _writer.Write(_options.SpaceWithinSimpleBlockBraces ? " }" : "}");
                return;
            }

            // write out a normal block with new lines and indentation
            WriteBlock(() =>
            {
                for (int i = 0; i < array.Length; i++)
                {
                    Visit(array[i]);

                    // don't add a blank line after the last statement
                    if (_options.NewlineBetweenStatements && i < array.Length - 1)
                    {
                        _writer.WriteLine();
                    }
                }
            });
        }

        private void WriteBlock(Action writeBodyAction, bool isSimpleBlock = false)
        {
            _writer.Write("{");

            bool indentBlock = (isSimpleBlock && _options.SimpleBlockOnNewLine) ||
                (!isSimpleBlock && _options.NewlineAfterOpeningBrace);

            if (indentBlock)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }
            else if (_options.SpaceWithinSimpleBlockBraces)
            {
                _writer.Write(" ");
            }

            writeBodyAction();

            if (indentBlock)
            {
                _writer.IndentLevel--;
            }

            // ReSharper disable ArrangeBraces_ifelse
            if ((isSimpleBlock && _options.SimpleBlockOnNewLine) ||
                (!isSimpleBlock && _options.NewlineBeforeClosingBrace))
            {
                _writer.WriteLine();
            }
            else if (isSimpleBlock && _options.SpaceWithinSimpleBlockBraces)
            {
                _writer.Write(" ");
            }
            // ReSharper restore ArrangeBraces_ifelse

            _writer.Write("}");
        }

        /// <summary>
        /// Writes a list of elements using a comma between elements. The comma is not written on the
        /// last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        private void WriteCommaList(IEnumerable<IEs5CodeModel> elements)
        {
            WriteList(elements, _options.SpaceAfterComma ? ", " : ",");
        }

        /// <summary>
        /// Writes a list of elements using a comma between elements. The comma is not written on the
        /// last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        /// <param name="writeElementAction">The action taken for each element.</param>
        private void WriteCommaList<T>(IEnumerable<T> elements, Action<T> writeElementAction)
        {
            string delimiter = _options.SpaceAfterComma ? ", " : ",";
            T[] array = elements.ToSafeArray();
            for (int i = 0; i < array.Length; i++)
            {
                writeElementAction(array[i]);
                if (i < array.Length - 1)
                {
                    _writer.Write(delimiter);
                }
            }
        }

        /// <summary>
        /// Writes a list of elements using the specified delimiter between elements. The delimiter
        /// is not written on the last element.
        /// </summary>
        /// <param name="elements">The list of elements to visit.</param>
        /// <param name="delimiter">The delimiter to use between elements.</param>
        private void WriteList(IEnumerable<IEs5CodeModel> elements, string delimiter)
        {
            IEs5CodeModel[] array = elements as IEs5CodeModel[] ?? elements.ToSafeArray();
            for (int i = 0; i < array.Length; i++)
            {
                IEs5CodeModel element = array[i];

                if (element != null)
                {
                    Visit(element);
                }

                if (i < array.Length - 1)
                {
                    _writer.Write(delimiter);
                }
            }
        }

        /// <summary>
        /// Writes a function declaration of the form 'keyword name(params) { body }'.
        /// </summary>
        /// <param name="functionKeyword">
        /// Typically one of the following: 'function', 'get', or 'set'.
        /// </param>
        /// <param name="functionName">The name of the function. Can be null.</param>
        /// <param name="parameters">The function parameters. Can be null or empty.</param>
        /// <param name="functionBody">The function body. Can be null or empty.</param>
        private void WriteFunction(
            string functionKeyword,
            string functionName,
            IEnumerable<Es5Identifier> parameters,
            IEnumerable<IEs5SourceElement> functionBody)
        {
            bool isUnnamed = string.IsNullOrEmpty(functionName);

            // write the keyword, which is typically one of the following: 'function', 'get', or 'set
            _writer.Write(functionKeyword);

            // if there's a function name, write it
            if (isUnnamed)
            {
                _writer.Write(_options.SpaceBeforeAnonymousFunctionDeclarationParentheses ? " (" : "(");
            }
            else
            {
                _writer.Write(" " + functionName);
                _writer.Write(_options.SpaceBeforeNamedFunctionDeclarationParentheses ? " (" : "(");
            }

            // write the parameters
            WriteCommaList(parameters);

            // write the closing parenthesis
            bool spaceAfter = isUnnamed
                ? _options.SpaceAfterAnonymousFunctionDeclarationParentheses
                : _options.SpaceAfterNamedFunctionDeclarationParentheses;

            _writer.Write(spaceAfter ? ") " : ")");

            // write the function body
            WriteBlock(functionBody);
        }

        /// <summary>
        /// Writes a statement of the form 'var x, y = 0'.
        /// </summary>
        /// <param name="declarations">The declaratios to write.</param>
        private void WriteVariableDeclarations(IEnumerable<Es5VariableDeclaration> declarations)
        {
            _writer.Write("var ");
            WriteCommaList(declarations, (Es5VariableDeclaration d) =>
            {
                Visit(d.Identifier);

                if (d.Initializer == null)
                {
                    return;
                }

                _writer.Write(_options.SurroundOperatorsWithSpaces ? " = " : "=");
                Visit(d.Initializer);
            });
        }
    }
}
