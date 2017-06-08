// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IEmitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Emit
{
    using System.IO;
    using System.Text;
    using Desalt.Core.CodeModels;

    /// <summary>
    /// Service contract for an emitter, which takes a code model and translates it into text.
    /// </summary>
    public interface IEmitter<in T> where T : ICodeModel
    {
        /// <summary>
        /// Emits the model as source code into the selected stream.
        /// </summary>
        /// <param name="model">The code model to emit.</param>
        /// <param name="outputStream">The destination stream for the emitted model.</param>
        /// <param name="encoding">The encoding to use for the emit.</param>
        /// <param name="options">Options controlling the way code is formatted.</param>
        void Emit(T model, Stream outputStream, Encoding encoding = null, EmitOptions options = null);
    }
}
