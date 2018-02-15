// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Visits a C# syntax tree, translating from a C# AST into a TypeScript AST.
    /// </summary>
    internal sealed class TranslationVisitor : CSharpSyntaxVisitor<IEnumerable<IAstNode>>
    {
        private readonly List<DiagnosticMessage> _messages = new List<DiagnosticMessage>();
        private readonly SemanticModel _semanticModel;

        public TranslationVisitor(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        }

        public IEnumerable<DiagnosticMessage> Messages => _messages.AsEnumerable();

        public override IEnumerable<IAstNode> DefaultVisit(SyntaxNode node)
        {
            _messages.Add(DiagnosticMessage.Error($"Node of type '{node.GetType().Name}' not supported: {node}"));
            return Enumerable.Empty<IAstNode>();
        }

        /// <summary>
        /// Called when the visitor visits a CompilationUnitSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var elements = new List<ITsImplementationModuleElement>();
            foreach (MemberDeclarationSyntax member in node.Members)
            {
                var element = (ITsImplementationModuleElement)Visit(member).Single();
                elements.Add(element);
            }

            ITsImplementationModule implementationScript = TsAstFactory.ImplementationModule(elements.ToArray());
            return implementationScript.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a NamespaceDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var elements = new List<ITsImplementationModuleElement>();

            foreach (MemberDeclarationSyntax member in node.Members)
            {
                var element = (ITsImplementationModuleElement)Visit(member).Single();
                elements.Add(element);
            }

            return elements;
        }

        /// <summary>
        /// Called when the visitor visits a InterfaceDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = TsAstFactory.Identifier(node.Identifier.Text);

            ITsObjectType body = TsAstFactory.ObjectType();
            ITsTypeParameters typeParameters = TsAstFactory.TypeParameters();
            IEnumerable<ITsTypeReference> extendsClause = Enumerable.Empty<ITsTypeReference>();

            ITsInterfaceDeclaration interfaceDeclaration = TsAstFactory.InterfaceDeclaration(
                interfaceName,
                body,
                typeParameters,
                extendsClause);

            INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                ITsExportImplementationElement exportedInterfaceDeclaration =
                    TsAstFactory.ExportImplementationElement(interfaceDeclaration);
                return exportedInterfaceDeclaration.ToSingleEnumerable();
            }

            return interfaceDeclaration.ToSingleEnumerable();
        }
    }
}
