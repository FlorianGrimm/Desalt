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
    using Desalt.Core.Diagnostics;
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

        private readonly DiagnosticList _diagnostics;
        private readonly DocumentTranslationContextWithSymbolTables _context;
        private readonly ISet<string> _typesToImport = new HashSet<string>();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TranslationVisitor(DocumentTranslationContextWithSymbolTables context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _diagnostics = DiagnosticList.Create(context.Options);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics.AsEnumerable();

        public IEnumerable<string> TypesToImport => _typesToImport.AsEnumerable();

        //// ===========================================================================================================
        //// Visit Methods
        //// ===========================================================================================================

        public override IEnumerable<IAstNode> DefaultVisit(SyntaxNode node)
        {
            _diagnostics.Add(DiagnosticFactory.TranslationNotSupported(node));
            return Enumerable.Empty<IAstNode>();
        }

        /// <summary>
        /// Called when the visitor visits a CompilationUnitSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsImplementationModule"/>.</returns>
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
            ITsIdentifier interfaceName = TranslateIdentifier(node);

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

            INamedTypeSymbol symbol = _context.SemanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility == Accessibility.Public)
            {
                ITsExportImplementationElement exportedInterfaceDeclaration =
                    Factory.ExportImplementationElement(interfaceDeclaration);

                exportedInterfaceDeclaration = AddDocumentationCommentIfNecessary(node, exportedInterfaceDeclaration);
                return exportedInterfaceDeclaration.ToSingleEnumerable();
            }

            interfaceDeclaration = AddDocumentationCommentIfNecessary(node, interfaceDeclaration);
            return interfaceDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a MethodDeclarationSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsMethodSignature"/>.</returns>
        public override IEnumerable<IAstNode> VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            ITsIdentifier functionName = TranslateIdentifier(node);

            // create the call signature
            ITsTypeParameters typeParameters = Factory.TypeParameters();
            var parameters = (ITsParameterList)Visit(node.ParameterList).Single();
            ITsType returnType = TypeTranslator.TranslateSymbol(
                node.ReturnType.GetTypeSymbol(_context.SemanticModel),
                _typesToImport);

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);

            ITsMethodSignature functionDeclaration = Factory.MethodSignature(
                functionName,
                isOptional: false,
                callSignature: callSignature);
            functionDeclaration = AddDocumentationCommentIfNecessary(node, functionDeclaration);

            return functionDeclaration.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterListSyntax node.
        /// </summary>
        /// <returns>A <see cref="ITsParameterList"/>.</returns>
        public override IEnumerable<IAstNode> VisitParameterList(ParameterListSyntax node)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter restParameter = null;

            foreach (ParameterSyntax parameterNode in node.Parameters)
            {
                IAstNode parameter = Visit(parameterNode).Single();
                if (parameter is ITsRequiredParameter requiredParameter)
                {
                    requiredParameters.Add(requiredParameter);
                }
                else
                {
                    optionalParameters.Add((ITsOptionalParameter)parameter);
                }
            }

            // ReSharper disable once ExpressionIsAlwaysNull
            ITsParameterList parameterList = Factory.ParameterList(
                requiredParameters,
                optionalParameters,
                restParameter);
            return parameterList.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterSyntax node.
        /// </summary>
        /// <returns>Either a <see cref="ITsBoundRequiredParameter"/> or a <see cref="ITsBoundOptionalParameter"/>.</returns>
        public override IEnumerable<IAstNode> VisitParameter(ParameterSyntax node)
        {
            ITsIdentifier parameterName = Factory.Identifier(node.Identifier.Text);
            ITsType parameterType = TypeTranslator.TranslateSymbol(
                node.Type.GetTypeSymbol(_context.SemanticModel),
                _typesToImport);
            IAstNode parameter;

            if (node.Default == null)
            {
                parameter = Factory.BoundRequiredParameter(parameterName, parameterType);
            }
            else
            {
                var initializer = (ITsExpression)Visit(node.Default).Single();
                parameter = Factory.BoundOptionalParameter(parameterName, initializer, parameterType);
            }

            return parameter.ToSingleEnumerable();
        }

        private ITsIdentifier TranslateIdentifier(BaseTypeDeclarationSyntax node)
        {
            return Factory.Identifier(node.Identifier.Text);
        }

        private ITsIdentifier TranslateIdentifier(MethodDeclarationSyntax node)
        {
            return Factory.Identifier(node.Identifier.Text);
        }

        /// <summary>
        /// Translates the C# XML documentation comment into a JSDoc comment if there is a
        /// documentation comment on the specified node.
        /// </summary>
        /// <typeparam name="T">The type of the translated node.</typeparam>
        /// <param name="syntaxNode">The C# syntax node to get documentation comments from.</param>
        /// <param name="translatedNode">The already-translated TypeScript AST node.</param>
        /// <returns>
        /// If there are documentation comments, a new TypeScript AST node with the translated JsDoc
        /// comments prepended. If there are no documentation comments, the same node is returned.
        /// </returns>
        private T AddDocumentationCommentIfNecessary<T>(SyntaxNode syntaxNode, T translatedNode) where T : IAstNode
        {
            if (!syntaxNode.HasStructuredTrivia)
            {
                return translatedNode;
            }

            ISymbol symbol = _context.SemanticModel.GetDeclaredSymbol(syntaxNode);
            DocumentationComment documentationComment = symbol.GetDocumentationComment();
            var jsDocComment = DocumentationCommentTranslator.Translate(documentationComment);

            return translatedNode.WithLeadingTrivia(jsDocComment);
        }
    }
}
