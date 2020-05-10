// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationContext.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Contains data structures and helper methods needed for translation of C# to TypeScript.
    /// </summary>
    /// <remarks>
    /// Many translation functions are split between multiple sub-translators, for example <see
    /// cref="ExpressionTranslator"/> and <see cref="StatementTranslator"/>, but they all might participate in a larger
    /// translation context. Having this shared information in a single class allows the sub-translators to add
    /// information during a translation, for example the diagnostics or types to import.
    ///
    /// NOTE: This class is mutable and not thread-safe. This is by design since translation of a single document can't
    ///       really be done in parallel.
    /// </remarks>
    internal class TranslationContext
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TranslationContext(
            DocumentTranslationContextWithSymbolTables documentContext,
            DiagnosticList? diagnostics = null,
            ISet<ITypeSymbol>? typesToImport = null,
            CancellationToken cancellationToken = default)
            : this(
                documentContext.SemanticModel,
                documentContext.ScriptSymbolTable,
                documentContext.AlternateSignatureSymbolTable,
                documentContext.Options.RenameRules,
                diagnostics ?? new DiagnosticList(documentContext.Options.DiagnosticOptions),
                cancellationToken,
                typesToImport)
        {
        }

        public TranslationContext(
            SemanticModel semanticModel,
            ScriptSymbolTable scriptSymbolTable,
            AlternateSignatureSymbolTable alternateSignatureSymbolTable,
            RenameRules renameRules,
            DiagnosticList diagnostics,
            CancellationToken cancellationToken,
            ISet<ITypeSymbol>? typesToImport = null)
        {
            SemanticModel = semanticModel;
            ScriptSymbolTable = scriptSymbolTable;
            AlternateSignatureSymbolTable = alternateSignatureSymbolTable;
            RenameRules = renameRules;
            Diagnostics = diagnostics;
            CancellationToken = cancellationToken;
            TypesToImport = typesToImport ?? new HashSet<ITypeSymbol>();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the <see cref="AlternateSignatureSymbolTable"/> used during translation.
        /// </summary>
        public AlternateSignatureSymbolTable AlternateSignatureSymbolTable { get; }

        /// <summary>
        /// Gets the cancellation token that controls the whole translation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a <see cref="DiagnosticList"/> to use for logging errors, warnings, and other diagnostics while translating.
        /// </summary>
        public DiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the <see cref="RenameRules"/> to use when translating C# that may override what is specified in the
        /// original C# source code.
        /// </summary>
        public RenameRules RenameRules { get; }

        /// <summary>
        /// Gets the <see cref="ScriptSymbolTable"/> containing information about how all of the C# symbols should be
        /// translated to TypeScript.
        /// </summary>
        public ScriptSymbolTable ScriptSymbolTable { get; }

        /// <summary>
        /// Gets the <see cref="SemanticModel"/> containing information about all of the C# symbols.
        /// </summary>
        public SemanticModel SemanticModel { get; }

        /// <summary>
        /// Gets a <see cref="TemporaryVariableAllocator"/> to use when the translation needs to allocate temporary
        /// variables to hold transient calculations or objects.
        /// </summary>
        public TemporaryVariableAllocator TemporaryVariableAllocator { get; } = new TemporaryVariableAllocator();

        /// <summary>
        /// Gets a set of <see cref="ITypeSymbol"/> that are directly referenced in the translated TypeScript code and
        /// need to be imported at the top of the translated TypeScript file.
        /// </summary>
        public ISet<ITypeSymbol> TypesToImport { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new InternalError diagnostic, adds it to the diagnostics list, and then throws an exception so we
        /// can get a stack trace in debug mode and returns an empty enumerable.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="node">The node where the error occurs.</param>
        [DoesNotReturn]
        public void ReportInternalError(string message, SyntaxNode? node = null)
        {
            ReportInternalError(message, node?.GetLocation());
        }

        /// <summary>
        /// Creates a new InternalError diagnostic, adds it to the diagnostics list, and then throws an exception so we
        /// can get a stack trace in debug mode and returns an empty enumerable.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="nodeLocation">The location in the source code where the error occurs.</param>
        [DoesNotReturn]
        public void ReportInternalError(string message, Location? nodeLocation = null)
        {
            var diagnostic = DiagnosticFactory.InternalError(message, nodeLocation);
            Diagnostics.Add(diagnostic);
            throw new InvalidOperationException(diagnostic.ToString());
        }

        /// <summary>
        /// Translates an identifier name represented by the symbol, taking into account static vs. instance references.
        /// </summary>
        /// <param name="symbol">The symbol to translate.</param>
        /// <param name="node">The start of the syntax node where this symbol was located.</param>
        /// <param name="forcedScriptName">
        /// If present, this name will be used instead of looking it up in the symbol table.
        /// </param>
        /// <returns>An <see cref="ITsIdentifier"/> or <see cref="ITsMemberDotExpression"/>.</returns>
        public ITsExpression TranslateIdentifierName(
            ISymbol symbol,
            SyntaxNode node,
            string? forcedScriptName = null)
        {
            ITsIdentifier scriptName = forcedScriptName != null
                ? Factory.Identifier(forcedScriptName)
                : symbol.GetScriptName(ScriptSymbolTable, symbol.Name);

            // Get the containing type of the symbol.
            INamedTypeSymbol? containingType = symbol.ContainingType;

            // Get the containing type of the syntax node (usually an identifier).
            INamedTypeSymbol? containingTypeOfSyntaxNode = SemanticModel
                .GetEnclosingSymbol(node.GetLocation().SourceSpan.Start)
                ?.ContainingType;

            // See if the identifier is declared within this type.
            bool belongsToThisType = containingType != null &&
                containingTypeOfSyntaxNode != null &&
                SymbolEqualityComparer.Default.Equals(containingType, containingTypeOfSyntaxNode);

            ITsExpression expression;

            // In TypeScript, static references need to be fully qualified with the type name.
            if (symbol.IsStatic && containingType != null)
            {
                ITsIdentifier containingTypeScriptName = containingType.GetScriptName(
                    ScriptSymbolTable,
                    containingType.Name);
                expression = Factory.MemberDot(containingTypeScriptName, scriptName);
            }

            // Add a "this." prefix if it's an instance symbol within our same type.
            else if (!symbol.IsStatic &&
                belongsToThisType &&
                !symbol.Kind.IsOneOf(SymbolKind.Parameter, SymbolKind.Local, SymbolKind.Label))
            {
                expression = Factory.MemberDot(Factory.This, scriptName);
            }
            else
            {
                expression = scriptName;
            }

            // Add this type to the import list if it doesn't belong to us.
            if (!belongsToThisType)
            {
                var typeToImport = symbol as ITypeSymbol ?? containingType;
                if (typeToImport == null)
                {
                    ReportInternalError(
                        $"Cannot find the type to import for symbol '{symbol.ToHashDisplay()}'",
                        node.GetLocation());
                }
                else
                {
                    TypesToImport.Add(typeToImport);
                }
            }

            return expression;
        }

        /// <summary>
        /// Gets an expected symbol from the semantic model and calls <c>ReportInternalError</c> if the symbol is not found.
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> from which to get a symbol.</param>
        /// <returns>The symbol associated with the syntax node.</returns>
        public TSymbol GetExpectedDeclaredSymbol<TSymbol>(SyntaxNode node)
            where TSymbol : class, ISymbol
        {
            var symbol = SemanticModel.GetDeclaredSymbol(node) as TSymbol;
            if (symbol == null)
            {
                ReportInternalError($"Node '{node}' should have an expected declared symbol.", node);
            }

            return symbol;
        }

        /// <summary>
        /// Gets an expected symbol and associated <see cref="IScriptSymbol"/> and calls <c>ReportInternalError</c> if
        /// either is not found.
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> from which to get a symbol.</param>
        /// <returns>The symbol and script symbol associated with the syntax node.</returns>
        public (TSymbol symbol, TScriptSymbol scriptSymbol) GetExpectedDeclaredScriptSymbol<TSymbol, TScriptSymbol>(
            SyntaxNode node)
            where TSymbol : class, ISymbol
            where TScriptSymbol : class, IScriptSymbol
        {
            TSymbol symbol = GetExpectedDeclaredSymbol<TSymbol>(node);

            if (!ScriptSymbolTable.TryGetValue(symbol, out TScriptSymbol? scriptSymbol))
            {
                ReportInternalError($"Node should have been added to the ScriptSymbolTable: {node}", node);
            }

            return (symbol, scriptSymbol);
        }

        /// <summary>
        /// Gets an expected symbol and associated <see cref="IScriptSymbol"/> and calls <c>ReportInternalError</c> if
        /// either is not found.
        /// </summary>
        /// <param name="node">The <see cref="SyntaxNode"/> from which to get a symbol.</param>
        /// <returns>The symbol and script symbol associated with the syntax node.</returns>
        public TScriptSymbol GetExpectedDeclaredScriptSymbol<TScriptSymbol>(SyntaxNode node)
            where TScriptSymbol : class, IScriptSymbol
        {
            return GetExpectedDeclaredScriptSymbol<ISymbol, TScriptSymbol>(node).scriptSymbol;
        }

        /// <summary>
        /// Gets an expected script symbol and calls <c>ReportInternalError</c> if not found.
        /// </summary>
        public TScriptSymbol GetExpectedScriptSymbol<TScriptSymbol>(ISymbol symbol, SyntaxNode node)
            where TScriptSymbol : class, IScriptSymbol
        {
            if (!ScriptSymbolTable.TryGetValue(symbol, out TScriptSymbol? scriptSymbol))
            {
                ReportInternalError(
                    $"Symbol should have been added to the {nameof(ScriptSymbolTable)}: {symbol.ToHashDisplay()}",
                    node);
            }

            return scriptSymbol;
        }

        /// <summary>
        /// Extracts the semantic type symbol from the specified type syntax node.
        /// </summary>
        public ITypeSymbol GetExpectedTypeSymbol(TypeSyntax typeSyntax)
        {
            ITypeSymbol? typeSymbol = SemanticModel.GetTypeInfo(typeSyntax).Type;
            if (typeSymbol == null)
            {
                ReportInternalError($"Type symbol for syntax node '{typeSyntax}' should have been defined", typeSyntax);
            }

            return typeSymbol;
        }

        /// <summary>
        /// Translates an identifier used in a declaration (class, interface, method, etc.) by looking up the symbol
        /// and the associated script name.
        /// </summary>
        /// <param name="node">The node to translate.</param>
        /// <returns>An <see cref="ITsIdentifier"/>.</returns>
        public ITsIdentifier TranslateDeclarationIdentifier(MemberDeclarationSyntax node)
        {
            var scriptSymbol = GetExpectedDeclaredScriptSymbol<IScriptSymbol>(node);
            return Factory.Identifier(scriptSymbol.ComputedScriptName);
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
        public T AddDocumentationComment<T>(T translatedNode, SyntaxNode node, SyntaxNode? symbolNode = null)
            where T : ITsAstNode
        {
            if (!node.HasStructuredTrivia)
            {
                return translatedNode;
            }

            ISymbol? symbol = SemanticModel.GetDeclaredSymbol(symbolNode ?? node);
            if (symbol == null)
            {
                return translatedNode;
            }

            DocumentationComment? documentationComment = symbol.GetDocumentationComment(
                preferredCulture: CultureInfo.InvariantCulture,
                expandIncludes: true,
                cancellationToken: CancellationToken);

            if (documentationComment == null)
            {
                return translatedNode;
            }

            var result = DocumentationCommentTranslator.Translate(documentationComment);
            Diagnostics.AddRange(result.Diagnostics);

            return translatedNode.WithLeadingTrivia(result.Result);
        }

        public TsAccessibilityModifier GetAccessibilityModifier(SyntaxNode node)
        {
            ISymbol symbol = SemanticModel.GetDeclaredSymbol(node);
            return GetAccessibilityModifier(symbol, node.GetLocation);
        }

        public TsAccessibilityModifier GetAccessibilityModifier(ISymbol symbol, Func<Location> getLocationFunc)
        {
            TsAccessibilityModifier? translated = symbol.DeclaredAccessibility switch
            {
                Accessibility.Private => TsAccessibilityModifier.Private,
                Accessibility.Protected => TsAccessibilityModifier.Protected,
                Accessibility.Public => TsAccessibilityModifier.Public,
                _ => null,
            };

            if (translated == null)
            {
                Diagnostics.Add(
                    DiagnosticFactory.UnsupportedAccessibility(
                        symbol.DeclaredAccessibility.ToString(),
                        "public",
                        getLocationFunc()));
            }

            return translated ?? TsAccessibilityModifier.Public;
        }
    }
}
