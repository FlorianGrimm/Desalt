// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExportImplementationElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an exported element in a module file.
    /// </summary>
    internal class TsExportImplementationElement : AstNode, ITsExportImplementationElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsExportImplementationElement(ITsImplementationElement exportedElement)
        {
            ExportedElement = exportedElement ?? throw new ArgumentNullException(nameof(exportedElement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsImplementationElement ExportedElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitExportImplementationElement(this);

        public override string CodeDisplay => $"export {ExportedElement}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("export ");
            ExportedElement.Emit(emitter);
        }
    }
}
