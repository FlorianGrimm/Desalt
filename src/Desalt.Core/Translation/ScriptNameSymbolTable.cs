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
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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

        private ScriptNameSymbolTable(IEnumerable<KeyValuePair<string, string>> values)
            : base(values)
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static async Task<ScriptNameSymbolTable> CreateAsync(
            IEnumerable<DocumentTranslationContext> documentsContexts,
            bool excludeExternalReferenceTypes = false,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var allSymbols = await DiscoverSymbolsAsync(
                documentsContexts,
                DiscoverSymbolsInDocument,
                excludeExternalReferenceTypes
                    ? (ProcessExternallyReferencedTypeFunc<string>)null
                    : ProcessExternallyReferencedType,
                cancellationToken);

            return new ScriptNameSymbolTable(allSymbols);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, string>> DiscoverSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            IEnumerable<INamedTypeSymbol> allTypeDeclarationSymbols = context.RootSyntax
                 .GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                 .Where(symbol => symbol.TypeKind != TypeKind.Delegate);

            var allScriptNames = new List<KeyValuePair<ISymbol, string>>();
            foreach (INamedTypeSymbol symbol in allTypeDeclarationSymbols)
            {
                var scriptNamesForType = GetScriptNameOnTypeAndMembers(symbol, context.Options);
                allScriptNames.AddRange(scriptNamesForType);
            }

            return allScriptNames;
        }

        /// <summary>
        /// Adds a single type defined in an external assembly.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, string>> ProcessExternallyReferencedType(
            ITypeSymbol typeSymbol,
            CompilerOptions options,
            CancellationToken cancellationToken)
        {
            return GetScriptNameOnTypeAndMembers(typeSymbol, options);
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static IEnumerable<KeyValuePair<ISymbol, string>> GetScriptNameOnTypeAndMembers(
            ITypeSymbol typeSymbol,
            CompilerOptions options)
        {
            string typeName = TypeTranslator.TranslatesToNativeTypeScriptType(typeSymbol)
                ? TypeTranslator.GetNativeTypeScriptTypeName(typeSymbol)
                : (FindScriptName(typeSymbol) ?? typeSymbol.Name);

            yield return new KeyValuePair<ISymbol, string>(typeSymbol, typeName);

            // add all of the members of the declared type, but skip over compiler-generated
            // stuff like auto-property backing fields, event add/remove functions, and property
            // get/set methods.
            var members = typeSymbol.GetMembers().Where(ShouldProcessMember);

            RenameRules renameRules = options.RenameRules;

            foreach (ISymbol member in members)
            {
                string scriptName = FindFieldScriptName(member, renameRules.FieldRule) ??
                    FindScriptName(member) ?? ToCamelCase(member.Name);

                yield return new KeyValuePair<ISymbol, string>(member, scriptName);
            }
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
