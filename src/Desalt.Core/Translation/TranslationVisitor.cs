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
    using System.Threading;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Visits a C# syntax tree, translating from a C# AST into a TypeScript AST.
    /// </summary>
    internal sealed partial class TranslationVisitor : CSharpSyntaxVisitor<IEnumerable<ITsAstNode>>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ITsIdentifier s_staticCtorName = Factory.Identifier("__ctor");

        private readonly ICollection<Diagnostic> _diagnostics;
        private readonly CancellationToken _cancellationToken;
        private readonly DocumentTranslationContextWithSymbolTables _context;
        private readonly SemanticModel _semanticModel;
        private readonly ScriptNameSymbolTable _scriptNameTable;
        private readonly InlineCodeTranslator _inlineCodeTranslator;
        private readonly TypeTranslator _typeTranslator;
        private readonly AlternateSignatureTranslator _alternateSignatureTranslator;
        private readonly ISet<ISymbol> _typesToImport = new HashSet<ISymbol>(SymbolTableUtils.KeyComparer);
        private readonly TemporaryVariableAllocator _temporaryVariableAllocator = new TemporaryVariableAllocator();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationVisitor"/> class.
        /// </summary>
        /// <param name="context">The context for the document translation.</param>
        /// <param name="cancellationToken">An optional token to control canceling translation.</param>
        /// <param name="diagnostics">
        /// An optional diagnostic collection to use for adding errors. This should normally not be
        /// used since it could make this class not thread safe if access to the collection is not
        /// guarded with thread locking mechanisms. No locking is done within this class. This is
        /// used mainly for unit tests.
        /// </param>
        public TranslationVisitor(
            DocumentTranslationContextWithSymbolTables context,
            CancellationToken cancellationToken = default(CancellationToken),
            ICollection<Diagnostic> diagnostics = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cancellationToken = cancellationToken;
            _semanticModel = context.SemanticModel;
            _scriptNameTable = context.ScriptNameSymbolTable;
            _inlineCodeTranslator = new InlineCodeTranslator(
                context.SemanticModel,
                context.InlineCodeSymbolTable,
                context.ScriptNameSymbolTable);

            _typeTranslator = new TypeTranslator(context.ScriptNameSymbolTable);

            _alternateSignatureTranslator = new AlternateSignatureTranslator(
                context.AlternateSignatureSymbolTable,
                _typeTranslator);

            _diagnostics = diagnostics ?? DiagnosticList.Create(context.Options);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEnumerable<Diagnostic> Diagnostics => _diagnostics.AsEnumerable();

        public IEnumerable<ISymbol> TypesToImport => _typesToImport.AsEnumerable();

        //// ===========================================================================================================
        //// Visit Methods
        //// ===========================================================================================================

        public override IEnumerable<ITsAstNode> DefaultVisit(SyntaxNode node)
        {
            var diagnostic = DiagnosticFactory.TranslationNotSupported(node);
            ReportUnsupportedTranslataion(diagnostic);
            return Enumerable.Empty<ITsAstNode>();
        }

        /// <summary>
        /// Adds the diagnostic to the diagnostics list and then throws an exception so we can get a
        /// stack trace in debug mode and returns an empty enumerable.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to add and report.</param>
        /// <returns>An empty <see cref="IEnumerable{ITsAstNode}"/>.</returns>
        private void ReportUnsupportedTranslataion(Diagnostic diagnostic)
        {
            _diagnostics.Add(diagnostic);
#if DEBUG

            // throwing an exception lets us fail fast and see the problem in the unit test failure window
            throw new Exception(diagnostic.ToString());
#endif
        }

        /// <summary>
        /// Called when the visitor visits a CompilationUnitSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsImplementationModule"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitCompilationUnit(CompilationUnitSyntax node)
        {
            var elements = node.Members.SelectMany(Visit).Cast<ITsImplementationModuleElement>();
            ITsImplementationModule implementationScript = Factory.ImplementationModule(elements.ToArray());

            return implementationScript.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a ParameterListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsParameterList"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitParameterList(ParameterListSyntax node)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter restParameter = null;

            foreach (ParameterSyntax parameterNode in node.Parameters)
            {
                ITsAstNode parameter = Visit(parameterNode).Single();
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
        public override IEnumerable<ITsAstNode> VisitParameter(ParameterSyntax node)
        {
            ITsIdentifier parameterName = Factory.Identifier(node.Identifier.Text);
            ITypeSymbol parameterTypeSymbol = node.Type.GetTypeSymbol(_semanticModel);
            ITsType parameterType = _typeTranslator.TranslateSymbol(
                parameterTypeSymbol,
                _typesToImport,
                _diagnostics,
                () => node.Type.GetLocation());

            ITsAstNode parameter;

            if (node.Default == null)
            {
                parameter = Factory.BoundRequiredParameter(parameterName, parameterType);
            }
            else
            {
                var initializer = (ITsExpression)Visit(node.Default).Single();
                parameter = Factory.BoundOptionalParameter(parameterName, parameterType, initializer);
            }

            return parameter.ToSingleEnumerable();
        }

        /// <summary>
        /// Called when the visitor visits a TypeParameterListSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTypeParameters"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeParameterList(TypeParameterListSyntax node)
        {
            var typeParameters = new List<ITsTypeParameter>();
            foreach (TypeParameterSyntax typeParameterNode in node.Parameters)
            {
                var typeParameter = (ITsTypeParameter)Visit(typeParameterNode).Single();
                typeParameters.Add(typeParameter);
            }

            ITsTypeParameters translated = Factory.TypeParameters(typeParameters.ToArray());
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TypeParameterSyntax node.
        /// </summary>
        /// <returns>An <see cref="ITsTypeParameter"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeParameter(TypeParameterSyntax node)
        {
            ITsIdentifier typeName = Factory.Identifier(node.Identifier.Text);
            ITsTypeParameter translated = Factory.TypeParameter(typeName);
            yield return translated;
        }

        /// <summary>
        /// Called when the visitor visits a TypeArgumentListSyntax node.
        /// </summary>
        /// <returns>An enumerable of <see cref="ITsType"/>.</returns>
        public override IEnumerable<ITsAstNode> VisitTypeArgumentList(TypeArgumentListSyntax node)
        {
            var translated = from typeSyntax in node.Arguments
                             let typeSymbol = typeSyntax.GetTypeSymbol(_semanticModel)
                             where typeSymbol != null
                             select _typeTranslator.TranslateSymbol(
                                 typeSymbol,
                                 _typesToImport,
                                 _diagnostics,
                                 typeSyntax.GetLocation);
            return translated;
        }

        /// <summary>
        /// Translates an identifier used in a declaration (class, interface, method, etc.) by
        /// looking up the symbol and the associated script name.
        /// </summary>
        /// <param name="node">The node to translate.</param>
        /// <returns>An <see cref="ITsIdentifier"/>.</returns>
        private ITsIdentifier TranslateDeclarationIdentifier(MemberDeclarationSyntax node)
        {
            ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
            {
                ReportUnsupportedTranslataion(DiagnosticFactory.IdentifierNotSupported(node));
                return Factory.Identifier("Error");
            }

            if (!_scriptNameTable.TryGetValue(symbol, out string scriptName))
            {
                ReportUnsupportedTranslataion(
                    DiagnosticFactory.InternalError(
                        $"Node should have been added to the ScriptNameSymbolTable: {node}",
                        node.GetLocation()));
                return Factory.Identifier("Error");
            }

            return Factory.Identifier(scriptName);
        }

        /// <summary>
        /// Translates the C# XML documentation comment into a JSDoc comment if there is a
        /// documentation comment on the specified node.
        /// </summary>
        /// <typeparam name="T">The type of the translated node.</typeparam>
        /// <param name="translatedNode">The already-translated TypeScript AST node.</param>
        /// <param name="node">The C# syntax node to get documentation comments from.</param>
        /// <param name="symbolNode">
        /// The C# syntax node to use for retrieving the symbol. If not supplied <paramref
        /// name="node"/> is used.
        /// </param>
        /// <returns>
        /// If there are documentation comments, a new TypeScript AST node with the translated JsDoc
        /// comments prepended. If there are no documentation comments, the same node is returned.
        /// </returns>
        private T AddDocumentationComment<T>(T translatedNode, SyntaxNode node, SyntaxNode symbolNode = null)
            where T : ITsAstNode
        {
            if (!node.HasStructuredTrivia)
            {
                return translatedNode;
            }

            ISymbol symbol = _semanticModel.GetDeclaredSymbol(symbolNode ?? node);
            if (symbol == null)
            {
                return translatedNode;
            }

            DocumentationComment documentationComment = symbol.GetDocumentationComment();
            var result = DocumentationCommentTranslator.Translate(documentationComment);
            _diagnostics.AddRange(result.Diagnostics);

            return translatedNode.WithLeadingTrivia(result.Result);
        }

        /// <summary>
        /// Converts the translated declaration to an exported declaration if the C# declaration is public.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is returned.
        /// </returns>
        private ITsImplementationModuleElement ExportIfNeeded(
            ITsImplementationElement translatedDeclaration,
            BaseTypeDeclarationSyntax node)
        {
            // determine if this declaration should be exported
            INamedTypeSymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                return translatedDeclaration;
            }

            ITsExportImplementationElement exportedInterfaceDeclaration =
                Factory.ExportImplementationElement(translatedDeclaration);
            return exportedInterfaceDeclaration;
        }

        /// <summary>
        /// Calls <see cref="ExportIfNeeded"/> followed by <see cref="AddDocumentationComment{T}"/>.
        /// </summary>
        /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
        /// <param name="node">The C# syntax node to inspect.</param>
        /// <returns>
        /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
        /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is
        /// returned. Whichever element is returned, it includes any documentation comment.
        /// </returns>
        private ITsImplementationModuleElement ExportAndAddDocComment(
            ITsImplementationElement translatedDeclaration,
            BaseTypeDeclarationSyntax node)
        {
            var exportedDeclaration = ExportIfNeeded(translatedDeclaration, node);
            var withDocComment = AddDocumentationComment(exportedDeclaration, node);
            return withDocComment;
        }

        private ITsCallSignature TranslateCallSignature(
            ParameterListSyntax parameterListNode,
            TypeParameterListSyntax typeParameterListNode = null,
            TypeSyntax returnTypeNode = null)
        {
            ITsTypeParameters typeParameters = typeParameterListNode == null
                ? Factory.TypeParameters()
                : (ITsTypeParameters)Visit(typeParameterListNode).Single();

            ITsParameterList parameters = parameterListNode == null
                ? Factory.ParameterList()
                : (ITsParameterList)Visit(parameterListNode).Single();

            ITsType returnType = null;
            if (returnTypeNode != null)
            {
                returnType = _typeTranslator.TranslateSymbol(
                    returnTypeNode.GetTypeSymbol(_semanticModel),
                    _typesToImport,
                    _diagnostics,
                    returnTypeNode.GetLocation);
            }

            ITsCallSignature callSignature = Factory.CallSignature(typeParameters, parameters, returnType);
            return callSignature;
        }

        private TsAccessibilityModifier GetAccessibilityModifier(SyntaxNode node)
        {
            ISymbol symbol = _semanticModel.GetDeclaredSymbol(node);
            return GetAccessibilityModifier(symbol, node.GetLocation);
        }

        private TsAccessibilityModifier GetAccessibilityModifier(ISymbol symbol, Func<Location> getLocationFunc)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    return TsAccessibilityModifier.Private;

                case Accessibility.Protected:
                    return TsAccessibilityModifier.Protected;

                case Accessibility.Public:
                    return TsAccessibilityModifier.Public;

                case Accessibility.NotApplicable:
                case Accessibility.Internal:
                case Accessibility.ProtectedAndInternal:
                case Accessibility.ProtectedOrInternal:
                    _diagnostics.Add(
                        DiagnosticFactory.UnsupportedAccessibility(
                            symbol.DeclaredAccessibility.ToString(),
                            "public",
                            getLocationFunc()));
                    return TsAccessibilityModifier.Public;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
