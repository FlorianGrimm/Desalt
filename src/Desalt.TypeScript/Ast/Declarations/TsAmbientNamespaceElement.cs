// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientNamespaceElement.cs" company="Justin Rockwood">
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
    /// Represents an element in an ambient namespace declaration.
    /// </summary>
    internal class TsAmbientNamespaceElement<T> : AstNode<TsVisitor>, ITsAmbientNamespaceElement
        where T : class, ITsDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientNamespaceElement(T declaration, bool hasExportKeyword = false)
        {
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            HasExportKeyword = hasExportKeyword;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool HasExportKeyword { get; }
        public T Declaration { get; }

        ITsDeclaration ITsAmbientNamespaceElement.Declaration => Declaration;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitAmbientNamespaceElement(this);

        public override string CodeDisplay => HasExportKeyword ? "export " : "" + Declaration.CodeDisplay;

        public override void Emit(Emitter emitter)
        {
            if (HasExportKeyword)
            {
                emitter.Write("export ");
            }

            Declaration.Emit(emitter);
        }
    }
}
