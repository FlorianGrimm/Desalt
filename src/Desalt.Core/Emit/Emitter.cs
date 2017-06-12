// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Emit
{
    using System;
    using System.IO;
    using System.Text;
    using Desalt.Core.Utility;

    /// <summary>
    /// General-purpose emitter for serializing code models into code.
    /// </summary>
    public class Emitter : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly Encoding DefaultEncoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

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

            Encoding = encoding ?? DefaultEncoding;
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

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Write(string text)
        {
            _writer.Write(text);
        }

        public void WriteBlock(Action writeBodyAction, bool isSimpleBlock = false)
        {
            _writer.Write("{");

            bool indentBlock = (isSimpleBlock && Options.SimpleBlockOnNewLine) ||
                (!isSimpleBlock && Options.NewlineAfterOpeningBrace);

            if (indentBlock)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }
            else if (Options.SpaceWithinSimpleBlockBraces)
            {
                _writer.Write(" ");
            }

            writeBodyAction();

            if (indentBlock)
            {
                _writer.IndentLevel--;
            }

            // ReSharper disable ArrangeBraces_ifelse
            if ((isSimpleBlock && Options.SimpleBlockOnNewLine) ||
                (!isSimpleBlock && Options.NewlineBeforeClosingBrace))
            {
                _writer.WriteLine();
            }
            else if (isSimpleBlock && Options.SpaceWithinSimpleBlockBraces)
            {
                _writer.Write(" ");
            }
            // ReSharper restore ArrangeBraces_ifelse

            _writer.Write("}");
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
