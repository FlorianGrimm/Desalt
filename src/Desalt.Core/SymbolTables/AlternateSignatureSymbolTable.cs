// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Contains mappings of fully-qualified method names to <see
    /// cref="AlternateSignatureMethodGroup"/> for all of the methods that are decorated with an
    /// <c>[AlternateSignature]</c> attribute and their associated non-decorated methods.
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

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private AlternateSignatureSymbolTable(ImmutableDictionary<string, AlternateSignatureMethodGroup> entries)
        {
            Entries = entries;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets all of the entries in the table. Mainly used for unit testing.
        /// </summary>
        public ImmutableDictionary<string, AlternateSignatureMethodGroup> Entries { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="AlternateSignatureSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="AlternateSignatureSymbolTable"/>.</returns>
        public static IExtendedResult<AlternateSignatureSymbolTable> Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // process the types defined in the documents
            var entriesWithErrors = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => GetCtorsAndMethodsInDocument(context, cancellationToken))
                .ToImmutableArray();

            // merge all of the diagnostics for each document
            var allDiagnostics = entriesWithErrors.SelectMany(groupResult => groupResult.Diagnostics);

            // create the entries
            var entries = from entry in entriesWithErrors
                          let key = GetMethodName(entry.Result.ImplementingMethod)
                          select new KeyValuePair<string, AlternateSignatureMethodGroup>(key, entry.Result);

            var table = new AlternateSignatureSymbolTable(entries.ToImmutableDictionary());
            return new ExtendedResult<AlternateSignatureSymbolTable>(table, allDiagnostics);
        }

        public bool TryGetValue(IMethodSymbol symbol, out AlternateSignatureMethodGroup value) =>
            Entries.TryGetValue(GetMethodName(symbol), out value);

        /// <summary>
        /// Gets the key given a symbol. Only exposed for unit tests.
        /// </summary>
        private static string GetMethodName(ISymbol symbol) => symbol?.ToDisplayString(s_symbolDisplayFormat);

        private static ImmutableArray<IExtendedResult<AlternateSignatureMethodGroup>>
            GetCtorsAndMethodsInDocument(DocumentTranslationContext context, CancellationToken cancellationToken)
        {
            // [AlternateSignature] is only valid on methods and ctors
            var methodsByName = from methodSyntax in context.RootSyntax.DescendantNodes()
                                where methodSyntax.Kind()
                                    .IsOneOf(SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration)
                                let methodSymbol =
                                    context.SemanticModel.GetDeclaredSymbol(
                                        methodSyntax,
                                        cancellationToken) as IMethodSymbol
                                where methodSymbol != null
                                group methodSymbol by methodSymbol.Name;

            var validGroups = from grouping in methodsByName
                              let groupingArr = grouping.ToArray()
                              where groupingArr.Length > 1 &&
                                  groupingArr.Any(
                                      methodSymbol =>
                                          methodSymbol.FindAttribute(SaltarelleAttributeName.AlternateSignature) !=
                                          null)
                              select groupingArr;

            var groups = validGroups.Select(AlternateSignatureMethodGroup.Create).ToImmutableArray();
            return groups;
        }
    }
}
