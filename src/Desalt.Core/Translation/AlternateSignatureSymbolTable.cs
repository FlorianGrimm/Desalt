// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of fully-qualified method names to arrays of <see cref="AlternateSignatureMethodInfo"/> for
    /// all of the methods that are decorated with an <c>[AlternateSignature]</c> attribute and their
    /// associated non-decorated methods.
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

        private readonly ImmutableDictionary<string, ImmutableArray<AlternateSignatureMethodInfo>> _entries;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private AlternateSignatureSymbolTable(ImmutableDictionary<string, ImmutableArray<AlternateSignatureMethodInfo>> entries)
        {
            _entries = entries;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets all of the entries in the table. Mainly used for unit testing.
        /// </summary>
        public IEnumerable<KeyValuePair<string, ImmutableArray<AlternateSignatureMethodInfo>>> Entries => _entries.AsEnumerable();

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
                .SelectMany(context => GetMethodsInDocument(context, cancellationToken));

            // aggregate all of the methods into the dictionary - for partial classes it's possible
            // for methods to be spread out amongst several files
            var dictionary = new Dictionary<string, List<AlternateSignatureMethodInfo>>();
            foreach (KeyValuePair<string, IEnumerable<AlternateSignatureMethodInfo>> entry in entries)
            {
                if (dictionary.TryGetValue(entry.Key, out List<AlternateSignatureMethodInfo> methods))
                {
                    methods.AddRange(entry.Value);
                }
                else
                {
                    dictionary.Add(entry.Key, new List<AlternateSignatureMethodInfo>(entry.Value));
                }
            }

            // turn it into an immutable dictionary and filter out any entries that don't have any
            // methods with at least one [AlternateSignature]
            var immutable = dictionary.Where(pair => pair.Value.Any(info => info.IsAlternateSignature))
                .Select(
                    pair => new KeyValuePair<string, ImmutableArray<AlternateSignatureMethodInfo>>(
                        pair.Key,
                        pair.Value.ToImmutableArray()))
                .ToImmutableDictionary();

            return new AlternateSignatureSymbolTable(immutable);
        }

        /// <summary>
        /// Gets the key given a symbol. Only exposed for unit tests.
        /// </summary>
        private static string GetMethodName(ISymbol symbol) => symbol?.ToDisplayString(s_symbolDisplayFormat);

        private static IEnumerable<KeyValuePair<string, IEnumerable<AlternateSignatureMethodInfo>>> GetMethodsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            // get all of the method declarations, since [AlternateSignature] is only valid on methods
            var methodsByName =
                from methodSyntax in context.RootSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                let methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodSyntax, cancellationToken)
                where methodSymbol != null
                let isAlternateSignature =
                    SymbolTableUtils.FindSaltarelleAttribute(methodSymbol, "AlternateSignature") != null
                group new AlternateSignatureMethodInfo(methodSymbol, isAlternateSignature) by GetMethodName(methodSymbol);

            return methodsByName.Select(
                grouping => new KeyValuePair<string, IEnumerable<AlternateSignatureMethodInfo>>(grouping.Key, grouping));
        }
    }

    internal sealed class AlternateSignatureMethodInfo
    {
        public AlternateSignatureMethodInfo(IMethodSymbol methodSymbol, bool isAlternateSignature)
        {
            MethodSymbol = methodSymbol ?? throw new ArgumentNullException(nameof(methodSymbol));
            IsAlternateSignature = isAlternateSignature;
        }

        public IMethodSymbol MethodSymbol { get; }
        public bool IsAlternateSignature { get; }
    }
}
