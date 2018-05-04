// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of fully-qualified method names to arrays of <see cref="IMethodSymbol"/>
    /// for all of the methods that are decorated with an <c>[AlternateSignature]</c> attribute.
    /// </summary>
    internal class AlternateSignatureSymbolTable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.None,
            SymbolDisplayMemberOptions.IncludeContainingType,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.StaticMethod,
            SymbolDisplayParameterOptions.None,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.None,
            SymbolDisplayKindOptions.None,
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        private readonly ImmutableDictionary<string, ImmutableArray<IMethodSymbol>> _entries;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private AlternateSignatureSymbolTable(
            ImmutableArray<KeyValuePair<string, ImmutableArray<IMethodSymbol>>> entries)
        {
            _entries = entries.ToImmutableDictionary();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets all of the entries in the table. Mainly used for unit testing.
        /// </summary>
        public IEnumerable<KeyValuePair<string, ImmutableArray<IMethodSymbol>>> Entries => _entries.AsEnumerable();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="AlternateSignatureSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="AlternateSignatureSymbolTable"/>.</returns>
        public static AlternateSignatureSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // process the types defined in the documents
            var entries = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, cancellationToken))
                .ToImmutableArray();

            return new AlternateSignatureSymbolTable(entries);
        }

        /// <summary>
        /// Gets the key given a symbol. Only exposed for unit tests.
        /// </summary>
        private static string GetMethodName(ISymbol symbol) => symbol?.ToDisplayString(s_symbolDisplayFormat);

        private static IEnumerable<KeyValuePair<string, ImmutableArray<IMethodSymbol>>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            var parameterCountsPerMethod =

                // get all of the method declarations, since [AlternateSignature] is only valid on methods
                from methodSyntax in context.RootSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>()

                    // methods have to be declared as extern
                where methodSyntax.Modifiers.Any(SyntaxKind.ExternKeyword)

                // lookup the symbol
                let methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken)
                where methodSymbol != null &&
                    SymbolTableUtils.FindSaltarelleAttribute(methodSymbol, "AlternateSignature") != null

                // get the number of parameters for the method
                group methodSymbol by GetMethodName(methodSymbol);

            return parameterCountsPerMethod.Select(
                grouping => new KeyValuePair<string, ImmutableArray<IMethodSymbol>>(
                    grouping.Key,
                    grouping.ToImmutableArray()));
        }
    }
}
