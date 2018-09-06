// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the translated names (what they will be
    /// called in the TypeScript file). By default, Saltarelle converts type members to `camelCase`
    /// names. It can also be changed using the [PreserveName] and [ScriptName] attributes.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ScriptNameSymbolTable : SymbolTableBase<string>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ScriptNameSymbolTable(
            ImmutableArray<KeyValuePair<string, string>> overrideSymbols,
            ImmutableArray<KeyValuePair<ISymbol, string>> documentSymbols,
            ImmutableArray<KeyValuePair<ISymbol, string>> directlyReferencedExternalSymbols,
            ImmutableArray<KeyValuePair<ISymbol, Lazy<string>>> indirectlyReferencedExternalSymbols)
            : base(
                overrideSymbols,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols)
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
        /// <param name="overrideSymbols">
        /// An array of overrides that takes precedence over any of the other symbols. This is to
        /// allow creating exceptions without changing the Saltarelle assembly source code. The key
        /// is what is returned from <see cref="RoslynExtensions.ToHashDisplay"/>.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for cancellation.</param>
        /// <returns>A new <see cref="ScriptNameSymbolTable"/>.</returns>
        public static ScriptNameSymbolTable Create(
            ImmutableArray<DocumentTranslationContext> contexts,
            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols,
            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols,
            IEnumerable<KeyValuePair<string, string>> overrideSymbols = null,
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
                .SelectMany(symbol => DiscoverScriptNameOnTypeAndMembers(symbol, renameRules))
                .ToImmutableArray();

            // process all of the types and members in referenced assemblies
            var indirectlyReferencedExternalSymbols = indirectlyReferencedExternalTypeSymbols
                .SelectMany(DiscoverTypeAndMembers)
                .Select(
                    symbol => new KeyValuePair<ISymbol, Lazy<string>>(
                        symbol,
                        new Lazy<string>(() => DiscoverScriptNameForSymbol(symbol, renameRules), isThreadSafe: true)))
                .ToImmutableArray();

            return new ScriptNameSymbolTable(
                overrideSymbols?.ToImmutableArray() ?? ImmutableArray<KeyValuePair<string, string>>.Empty,
                documentSymbols,
                directlyReferencedExternalSymbols,
                indirectlyReferencedExternalSymbols);
        }

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        private static IEnumerable<KeyValuePair<ISymbol, string>> ProcessSymbolsInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            return context.RootSyntax

                // get all of the declared types
                .GetAllDeclaredTypes(context.SemanticModel, cancellationToken)

                // except delegates
                .Where(symbol => symbol.TypeKind != TypeKind.Delegate)

                // and get all of the members of the type that can have a script name
                .SelectMany(symbol => DiscoverScriptNameOnTypeAndMembers(symbol, context.Options.RenameRules));
        }

        private static IEnumerable<ISymbol> DiscoverTypeAndMembers(INamespaceOrTypeSymbol typeSymbol) =>
            typeSymbol.ToSingleEnumerable().Concat(typeSymbol.GetMembers().Where(ShouldProcessMember));

        private static IEnumerable<KeyValuePair<ISymbol, string>> DiscoverScriptNameOnTypeAndMembers(
            ITypeSymbol typeSymbol,
            RenameRules renameRules)
        {
            return DiscoverTypeAndMembers(typeSymbol)
                .Select(
                    symbol => new KeyValuePair<ISymbol, string>(
                        symbol,
                        DiscoverScriptNameForSymbol(symbol, renameRules)));
        }

        /// <summary>
        /// Determines the name a symbol should have in the generated script.
        /// </summary>
        /// <param name="symbol">The symbol for which to discover the script name.</param>
        /// <param name="renameRules">Options controlling the way certain symbols are renamed.</param>
        /// <returns>The name the specified symbol should have in the generated script.</returns>
        private static string DiscoverScriptNameForSymbol(ISymbol symbol, RenameRules renameRules)
        {
            string scriptName;

            switch (symbol)
            {
                case ITypeSymbol typeSymbol:
                    scriptName = TypeTranslator.TranslatesToNativeTypeScriptType(typeSymbol)
                        ? TypeTranslator.GetNativeTypeScriptTypeName(typeSymbol)
                        : DetermineScriptNameFromAttributes(typeSymbol) ?? typeSymbol.Name;
                    break;

                case IFieldSymbol fieldSymbol:
                    scriptName = DetermineFieldScriptName(fieldSymbol, renameRules.FieldRule);
                    break;

                case IMethodSymbol methodSymbol:
                    scriptName = DetermineMethodScriptName(methodSymbol);
                    break;

                default:
                    scriptName = DetermineScriptNameFromAttributes(symbol) ?? ToCamelCase(symbol.Name);
                    break;
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

        /// <summary>
        /// Attempts to find the script name for the specified symbol based on attributes defined on
        /// the symbol, containing type, or containing namespace.
        /// </summary>
        /// <param name="symbol">The symbol to determine naming for.</param>
        /// <returns>
        /// The name to use in the generated script for the symbol, or null if there are no
        /// attributes that control the naming.
        /// </returns>
        private static string DetermineScriptNameFromAttributes(ISymbol symbol)
        {
            // Use the following precedence if there are multiple attributes:
            // [ScriptAlias]
            // [ScriptName]
            // [PreserveCase]
            // [PreserveMemberCase]

            // use [ScriptAlias] if available
            string scriptAlias = symbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptAlias);
            if (scriptAlias != null)
            {
                return scriptAlias;
            }

            // use [ScriptName] if available (even if there's also a [PreserveCase])
            string scriptName = symbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptName);
            if (scriptName != null)
            {
                return scriptName;
            }

            // for [PreserveCase], don't touch the original name as given in the C# code
            if (symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveCase))
            {
                return symbol.Name;
            }

            // see if there's a [PreserveMemberCase] on the containing type
            if (symbol.ContainingType?.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase) == true)
            {
                return symbol.Name;
            }

            // see if there's a [PreserveMemberCase] on the containing assembly
            if (symbol.ContainingAssembly?.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase) == true)
            {
                return symbol.Name;
            }

            return null;
        }

        /// <summary>
        /// Determines the name that a field should have in the generated code.
        /// </summary>
        /// <param name="fieldSymbol">The field for which to determine the script name.</param>
        /// <param name="renameRule">Options on how to rename fields.</param>
        /// <returns>The name the field should have in the generated code.</returns>
        private static string DetermineFieldScriptName(IFieldSymbol fieldSymbol, FieldRenameRule renameRule)
        {
            string scriptName;

            switch (renameRule)
            {
                // ReSharper disable once RedundantCaseLabel
                case FieldRenameRule.LowerCaseFirstChar:
                default:
                    scriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? ToCamelCase(fieldSymbol.Name);
                    break;

                // add a $ prefix if the field is private OR when there's a duplicate name
                case FieldRenameRule.PrivateDollarPrefix
                    when fieldSymbol.DeclaredAccessibility == Accessibility.Private:
                case FieldRenameRule.DollarPrefixOnlyForDuplicateName when HasDuplicateFieldName(fieldSymbol):
                    scriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? $"${ToCamelCase(fieldSymbol.Name)}";
                    break;
            }

            return scriptName;
        }

        /// <summary>
        /// Returns a value indicating whether the specified field symbol has a duplicate script
        /// name. For example, a public field named "Field" will be named "field" by default in the
        /// generated script. Another field named "Other" might be marked with a
        /// [ScriptName("field")], in which case there's a conflict.
        /// </summary>
        /// <param name="fieldSymbol">The field to search for.</param>
        /// <returns>True if there is a duplicate script name; otherwise, false.</returns>
        private static bool HasDuplicateFieldName(ISymbol fieldSymbol)
        {
            string fieldScriptName = DetermineScriptNameFromAttributes(fieldSymbol) ?? ToCamelCase(fieldSymbol.Name);

            var query = from member in fieldSymbol.ContainingType.GetMembers()

                            // don't include the field we're searching against
                        where !Equals(member, fieldSymbol)

                        // find the potential script name of the member
                        let scriptName = DetermineScriptNameFromAttributes(member) ?? ToCamelCase(member.Name)

                        // and only take the ones that are equal to the field's script name
                        where string.Equals(scriptName, fieldScriptName, StringComparison.Ordinal)
                        select member;

            return query.Any();
        }

        /// <summary>
        /// Determines the name that a method should have in the generated code.
        /// </summary>
        /// <param name="methodSymbol">The method for which to determine the script name.</param>
        /// <returns>The name the method should have in the generated code.</returns>
        private static string DetermineMethodScriptName(IMethodSymbol methodSymbol)
        {
            string methodName = methodSymbol.Name;
            string defaultName = ToCamelCase(methodName);

            // if the symbol has any attributes that control naming, use those
            string attributedName = DetermineScriptNameFromAttributes(methodSymbol);
            if (attributedName != null)
            {
                return attributedName;
            }

            // check for [Imported] since we don't rename overloads on imported members
            bool imported = methodSymbol.ContainingType.GetFlagAttribute(SaltarelleAttributeName.Imported);
            if (imported)
            {
                return defaultName;
            }

            // check for [AlternateSignature] and then find the name of the other method
            if (methodSymbol.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature))
            {
                IMethodSymbol otherMethod = methodSymbol.ContainingType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Single(
                        m => !Equals(m, methodSymbol) &&
                            m.IsStatic == methodSymbol.IsStatic &&
                            m.Name == methodSymbol.Name &&
                            !m.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature));
                string scriptName = DetermineMethodScriptName(otherMethod);
                return scriptName;
            }

            // check for overloads, since those need to be renamed with a $1, $2, etc. suffix
            if (!CanBeOverloaded(methodSymbol))
            {
                return defaultName;
            }

            // overloads are only renamed if the script name will be the same, so first run through
            // all of the methods and determine their script names
            var allMethodsWithThisScriptName =
               from m in methodSymbol.ContainingType.GetMembers().OfType<IMethodSymbol>()
               where CanBeOverloaded(m)

               // only look at static/instance methods (the same as what this method is), since it's
               // fine to have a static method and an instance method that have the same script name
               where m.IsStatic == methodSymbol.IsStatic

               // take out [AlternateSignature] methods since they use the name of the implementation method
               where !m.GetFlagAttribute(SaltarelleAttributeName.AlternateSignature)

               let sname = DetermineScriptNameFromAttributes(m) ?? ToCamelCase(m.Name)
               where sname.Equals(defaultName, StringComparison.Ordinal)
               select m;

            // find the index of this method (according to the order it was declared)
            int index = allMethodsWithThisScriptName.ToImmutableArray().IndexOf(methodSymbol);

            // if we're the first method in the declared overloads, then we keep our name (using the
            // default camelCase rename) and the other overloaded methods need to add suffixes
            if (index == 0)
            {
                return defaultName;
            }

            // if it's overloaded, just assign it a suffix of $x where x is the index of when it was declared
            return ToCamelCase($"{methodName}${index}");
        }

        private static bool CanBeOverloaded(IMethodSymbol methodSymbol)
        {
            // only normal methods and constructors can be overloaded
            return methodSymbol.MethodKind == MethodKind.Ordinary || methodSymbol.MethodKind == MethodKind.Constructor;
        }
    }
}
