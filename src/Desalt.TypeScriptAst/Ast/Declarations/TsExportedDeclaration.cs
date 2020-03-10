// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExportedDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an exported declaration.
    /// </summary>
    internal class TsExportedDeclaration : TsAstNode, ITsExportedDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsExportedDeclaration(ITsDeclaration exportedDeclaration)
        {
            ExportedDeclaration = exportedDeclaration ?? throw new ArgumentNullException(nameof(exportedDeclaration));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsDeclaration ExportedDeclaration { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitExportedDeclaration(this);
        }

        public override string CodeDisplay => $"export {ExportedDeclaration}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("export ");
            ExportedDeclaration.Emit(emitter);
        }
    }
}
