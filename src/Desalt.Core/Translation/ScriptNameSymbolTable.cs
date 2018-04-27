// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
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
    using Desalt.Core.Extensions;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the translated names (what they will be
    /// called in the TypeScript file). By default, Saltarelle converts type members to `camelCase`
    /// names. It can also be changed using the [PreserveName] and [ScriptName] attributes.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ScriptNameSymbolTable : SymbolTable<string>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ScriptNameSymbolTable(
            ImmutableArray<KeyValuePair<string, string>> documentSymbols,
            ImmutableArray<KeyValuePair<string, string>> directlyReferencedExternalSymbols,
            ImmutableArray<KeyValuePair<string, Lazy<string>>> indirectlyReferencedExternalSymbols)
            : base(documentSymbols, directlyReferencedExternalSymbols, indirectlyReferencedExternalSymbols)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new <see cref="ScriptNameSymbolTable"/> for the specified translation contexts.
        /// </summary>
        /// <param name="contexts">The contexts from which to retrieve symbols.</param>
        /// <param name="directlyReferencedExternalTypeSymbols">
        /// An array of symbols that are directly referenced in the documents, but are defined in
        /// external assemblies.
        /// </param>
        /// <param name="indirectlyReferencedExternalTypeSymbols">
        /// An array of symbols that are not directly referenced in the documents and are defined in
        /// external assemblies.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ScriptNameSymbolTable"/>.</returns>
        public static ScriptNameSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // process the types defined in the documents
            var documentSymbols = contexts.AsParallel()
                .WithCancellation(cancellationToken)
                .SelectMany(context => ProcessSymbolsInDocument(context, cancellationToken))
                .ToImmutableArray();

            RenameRules renameRules = contexts.FirstOrDefault()?.Options.RenameRules ?? RenameRules.Default;

            // process the externally referenced types
            var directlyReferencedExternalSymbols = directlyReferencedExternalTypeSymbols
                .SelectMany(symbol => GetScriptNameOnTypeAndMembers(symbol, renameRules))
                .ToImmutableArray();

            // process all of the types and members in referenced assemblies
            var indirectlyReferencedExternalSymbols = indirectlyReferencedExternalTypeSymbols
                .SelectMany(symbol => symbol.ToSingleEnumerable().Concat(DiscoverMembersOfTypeSymbol(symbol)))
                .Select(
                    symbol => new KeyValuePair<string, Lazy<string>>(
                        SymbolTableUtils.KeyFromSymbol(symbol),
                        new Lazy<string>(() => GetScriptNameForSymbol(symbol, renameRules), isThreadSafe: true)))
                .ToImmutableArray();

            return new ScriptNameSymbolTable(
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<string, string>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            return context.RootSyntax

                // get all of the delcared types
                .GetAllDeclaredTypes(context.SemanticModel, cancellationToken)

                // except delegates
                .Where(symbol => symbol.TypeKind != TypeKind.Delegate)

                // and get all of the members of the type that can have a script name
                .SelectMany(symbol => GetScriptNameOnTypeAndMembers(symbol, context.Options.RenameRules));
        }

        private static IEnumerable<ISymbol> DiscoverMembersOfTypeSymbol(INamespaceOrTypeSymbol typeSymbol) =>
            typeSymbol.GetMembers().Where(ShouldProcessMember);

        private static IEnumerable<KeyValuePair<string, string>> GetScriptNameOnTypeAndMembers(
            ITypeSymbol typeSymbol,
            RenameRules renameRules)
        {
            var scriptNames = DiscoverMembersOfTypeSymbol(typeSymbol).ToList();
            scriptNames.Insert(0, typeSymbol);

            return scriptNames.Select(
                symbol => new KeyValuePair<string, string>(
                    SymbolTableUtils.KeyFromSymbol(symbol),
                    GetScriptNameForSymbol(symbol, renameRules)));
        }

        private static string GetScriptNameForSymbol(ISymbol symbol, RenameRules renameRules)
        {
            string scriptName;

            if (symbol is ITypeSymbol typeSymbol)
            {
                scriptName = TypeTranslator.TranslatesToNativeTypeScriptType(typeSymbol)
                    ? TypeTranslator.GetNativeTypeScriptTypeName(typeSymbol)
                    : (FindScriptName(typeSymbol) ?? typeSymbol.Name);
            }
            else
            {
                scriptName = FindFieldScriptName(symbol, renameRules.FieldRule) ??
                    FindScriptName(symbol) ?? ToCamelCase(symbol.Name);
            }

            return scriptName;
        }

        private static bool ShouldProcessMember(ISymbol member)
        {
            // skip over compiler-generated stuff like auto-property backing fields, event add/remove
            // functions, and property get/set methods
            if (member.IsImplicitlyDeclared)
            {
                return false;
            }

            // don't skip non-method members
            if (member.Kind != SymbolKind.Method || !(member is IMethodSymbol methodSymbol))
            {
                return true;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (methodSymbol.MethodKind)
            {
                // skip constructors
                case MethodKind.Constructor:
                case MethodKind.StaticConstructor:
                    return false;

                // skip property get/set methods
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                    return false;

                // skip event add/remove
                case MethodKind.EventAdd:
                case MethodKind.EventRemove:
                    return false;
            }

            return true;
        }

        private static string ToCamelCase(string name) => char.ToLowerInvariant(name[0]) + name.Substring(1);

        private static string FindScriptName(ISymbol symbol)
        {
            // use [ScriptName] if available (even if there's also a [PreserveCase])
            string scriptName = SymbolTableUtils.GetSaltarelleAttributeValueOrDefault(symbol, "ScriptName", null);
            if (scriptName != null)
            {
                return scriptName;
            }

            // use [ScriptAlias] if available
            string scriptAlias = SymbolTableUtils.GetSaltarelleAttributeValueOrDefault(symbol, "ScriptAlias", null);
            if (scriptAlias != null)
            {
                return scriptAlias;
            }

            // for [PreserveCase], don't touch the original name as given in the C# code
            AttributeData preserveCaseAttributeData = SymbolTableUtils.FindSaltarelleAttribute(symbol, "PreserveCase");
            if (preserveCaseAttributeData != null)
            {
                return symbol.Name;
            }

            // see if there's a [PreserveMemberCase] on the containing type
            if (symbol.ContainingType != null)
            {
                AttributeData preserveMemberCaseAttributeData =
                    SymbolTableUtils.FindSaltarelleAttribute(symbol.ContainingType, "PreserveMemberCase");
                if (preserveMemberCaseAttributeData != null)
                {
                    return symbol.Name;
                }
            }

            // see if there's a [PreserveMemberCase] on the containing assembly
            if (symbol.ContainingAssembly != null)
            {
                AttributeData preserveMemberCaseAttributeData =
                    SymbolTableUtils.FindSaltarelleAttribute(symbol.ContainingAssembly, "PreserveMemberCase");
                if (preserveMemberCaseAttributeData != null)
                {
                    return symbol.Name;
                }
            }

            return null;
        }

        private static string FindFieldScriptName(ISymbol member, FieldRenameRule renameRule)
        {
            if (member.Kind != SymbolKind.Field)
            {
                return null;
            }

            string scriptName;

            switch (renameRule)
            {
                // ReSharper disable once RedundantCaseLabel
                case FieldRenameRule.LowerCaseFirstChar:
                default:
                    scriptName = FindScriptName(member) ?? ToCamelCase(member.Name);
                    break;

                // add a $ prefix if the field is private OR when there's a duplicate name
                case FieldRenameRule.PrivateDollarPrefix when member.DeclaredAccessibility == Accessibility.Private:
                case FieldRenameRule.DollarPrefixOnlyForDuplicateName when IsDuplicateName(member):
                    scriptName = FindScriptName(member) ?? $"${ToCamelCase(member.Name)}";
                    break;
            }

            return scriptName;
        }

        private static bool IsDuplicateName(ISymbol fieldSymbol)
        {
            string fieldScriptName = FindScriptName(fieldSymbol) ?? ToCamelCase(fieldSymbol.Name);

            var query = from member in fieldSymbol.ContainingType.GetMembers()

                            // don't include the field we're searching against
                        where !Equals(member, fieldSymbol)

                        // find the potential script name of the member
                        let scriptName = FindScriptName(member) ?? ToCamelCase(member.Name)

                        // and only take the ones that are equal to the field's script name
                        where string.Equals(scriptName, fieldScriptName, StringComparison.Ordinal)
                        select member;

            return query.Any();
        }
    }
}
