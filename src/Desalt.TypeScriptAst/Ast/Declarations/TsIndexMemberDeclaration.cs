// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIndexMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an index member declaration in a class.
    /// </summary>
    internal class TsIndexMemberDeclaration : TsAstNode, ITsIndexMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsIndexMemberDeclaration(ITsIndexSignature indexSignature)
        {
            IndexSignature = indexSignature ?? throw new ArgumentNullException(nameof(indexSignature));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIndexSignature IndexSignature { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitIndexMemberDeclaration(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            IndexSignature.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
