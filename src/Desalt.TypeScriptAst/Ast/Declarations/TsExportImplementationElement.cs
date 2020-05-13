// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExportImplementationElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an exported element in a module file.
    /// </summary>
    internal class TsExportImplementationElement : TsAstNode, ITsExportImplementationElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsExportImplementationElement(ITsImplementationElement exportedElement)
            : base(exportedElement.LeadingTrivia)
        {
            ExportedElement = exportedElement.WithLeadingTrivia();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsImplementationElement ExportedElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitExportImplementationElement(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("export ");
            ExportedElement.Emit(emitter);
        }
    }
}
