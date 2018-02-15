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
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Visits a C# syntax tree, translating from a C# AST into a TypeScript AST.
    /// </summary>
    internal sealed class TranslationVisitor : CSharpSyntaxVisitor<IEnumerable<IAstNode>>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly List<DiagnosticMessage> _messages = new List<DiagnosticMessage>();
        private readonly SemanticModel _semanticModel;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TranslationVisitor(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEnumerable<DiagnosticMessage> Messages => _messages.AsEnumerable();

        //// ===========================================================================================================
        //// Visit Methods
        //// ===========================================================================================================

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
                var element = (ITsImplementationModuleElement)Visit(member).SingleOrDefault();
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            ITsImplementationModule implementationScript = Factory.ImplementationModule(elements.ToArray());
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
                var element = (ITsImplementationModuleElement)Visit(member).SingleOrDefault();
                if (element != null)
                {
                    elements.Add(element);
                }
            }

            return elements;
        }

        /// <summary>
        /// Called when the visitor visits a InterfaceDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            ITsIdentifier interfaceName = Factory.Identifier(node.Identifier.Text);

            // get the interface body
            var typeMembers = new List<ITsTypeMember>();
            foreach (MemberDeclarationSyntax member in node.Members)
            {
                var typeMember = (ITsTypeMember)Visit(member).SingleOrDefault();
                if (typeMember != null)
                {
                    typeMembers.Add(typeMember);
                }
            }

            ITsObjectType body = Factory.ObjectType(typeMembers.ToArray());
            ITsTypeParameters typeParameters = Factory.TypeParameters();
            IEnumerable<ITsTypeReference> extendsClause = Enumerable.Empty<ITsTypeReference>();

            ITsInterfaceDeclaration interfaceDeclaration = Factory.InterfaceDeclaration(
                interfaceName,
                body,
                typeParameters,
                extendsClause);

            INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                ITsExportImplementationElement exportedInterfaceDeclaration =
                    Factory.ExportImplementationElement(interfaceDeclaration);
                return exportedInterfaceDeclaration.ToSingleEnumerable();
            }

            return interfaceDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        public override IEnumerable<IAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ITsIdentifier functionName = Factory.Identifier(node.Identifier.Text);

            // create the call signature
            ITsTypeParameters typeParameters = Factory.TypeParameters();
            ITsParameterList parameters = Factory.ParameterList();
            ITsType returnType = Factory.VoidType;

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);

            ITsMethodSignature functionDeclaration = Factory.MethodSignature(
                functionName,
                isOptional: false,
                callSignature: callSignature);

            return functionDeclaration.ToSingleEnumerable();
        }
    }
}
