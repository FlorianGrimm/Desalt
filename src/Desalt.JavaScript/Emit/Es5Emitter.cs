// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using System;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.Core.Utility;
    using Desalt.JavaScript.CodeModels;

    /// <summary>
    /// Takes an <see cref="IEs5CodeModel"/> and converts it to text.
    /// </summary>
    public class Es5Emitter : Es5Visitor<bool>, IEmitter<IEs5CodeModel>
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
    }
}
