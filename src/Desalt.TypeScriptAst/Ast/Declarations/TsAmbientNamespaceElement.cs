// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientNamespaceElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an element in an ambient namespace declaration.
    /// </summary>
    internal class TsAmbientNamespaceElement : TsAstNode, ITsAmbientNamespaceElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientNamespaceElement(ITsAmbientDeclarationElement declaration, bool hasExportKeyword = false)
        {
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            HasExportKeyword = hasExportKeyword;
        }

        public TsAmbientNamespaceElement(ITsInterfaceDeclaration interfaceDeclaration, bool hasExportKeyword = false)
        {
            InterfaceDeclaration = interfaceDeclaration ?? throw new ArgumentNullException(nameof(interfaceDeclaration));
            HasExportKeyword = hasExportKeyword;
        }

        public TsAmbientNamespaceElement(
            ITsImportAliasDeclaration importAliasDeclaration,
            bool hasExportKeyword = false)
        {
            ImportAliasDeclaration = importAliasDeclaration ??
                throw new ArgumentNullException(nameof(importAliasDeclaration));
            HasExportKeyword = hasExportKeyword;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool HasExportKeyword { get; }
        public ITsAmbientDeclarationElement? Declaration { get; }
        public ITsInterfaceDeclaration? InterfaceDeclaration { get; }
        public ITsImportAliasDeclaration? ImportAliasDeclaration { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitAmbientNamespaceElement(this);
        }

        public override string CodeDisplay =>
            HasExportKeyword ? "export " : "" +
            $"{Declaration?.CodeDisplay}{InterfaceDeclaration?.CodeDisplay}{ImportAliasDeclaration?.CodeDisplay}";

        protected override void EmitContent(Emitter emitter)
        {
            if (HasExportKeyword)
            {
                emitter.Write("export ");
            }

            Declaration?.Emit(emitter);
            InterfaceDeclaration?.Emit(emitter);
            ImportAliasDeclaration?.Emit(emitter);
        }
    }
}
