// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExportedDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an exported declaration.
    /// </summary>
    internal class TsExportedDeclaration : AstNode<TsVisitor>, ITsExportedDeclaration
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

        public override void Accept(TsVisitor visitor) => visitor.VisitExportedDeclaration(this);

        public override string CodeDisplay => $"export {ExportedDeclaration}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("export ");
            ExportedDeclaration.Emit(emitter);
        }
    }
}
