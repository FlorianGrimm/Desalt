// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Emit
{
    using System;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.TypeScript.Ast;

    /// <summary>
    /// Takes an <see cref="ITsAstNode"/> and converts it to text.
    /// </summary>
    public partial class TsEmitter : TsVisitor, IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly Emitter _emitter;
        private readonly EmitOptions _options;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsEmitter(Stream outputStream, Encoding encoding = null, EmitOptions options = null)
        {
            _emitter = new Emitter(outputStream, encoding, options);
            _options = options;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Encoding Encoding => _emitter.Encoding;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Dispose()
        {
            _emitter.Dispose();
        }

        public override void VisitIdentifier(ITsIdentifier model)
        {
            _emitter.Write(model.Text);
        }
    }
}
