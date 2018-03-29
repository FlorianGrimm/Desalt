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
        //// Member Variables
        //// ===========================================================================================================

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public override void AddDefinedTypesInDocument(
            DocumentTranslationContext context,
            CancellationToken cancellationToken)
        {
            RenameRules renameRules = context.Options.RenameRules;

            IEnumerable<INamedTypeSymbol> allTypeDeclarationSymbols = context.RootSyntax
                .GetAllDeclaredTypes(context.SemanticModel, cancellationToken)
                .Where(symbol => symbol.TypeKind != TypeKind.Delegate);

            foreach (INamedTypeSymbol symbol in allTypeDeclarationSymbols)
            {
                string typeName = FindScriptName(symbol) ?? symbol.Name;
                AddOrUpdate(symbol, typeName);

                // add all of the members of the declared type, but skip over compiler-generated
                // stuff like auto-property backing fields, event add/remove functions, and property
                // get/set methods.
                var members = symbol.GetMembers().Where(ShouldProcessMember);

                foreach (ISymbol member in members)
                {
                    string scriptName = FindFieldScriptName(member, renameRules.FieldRule) ??
                        FindScriptName(member) ?? ToCamelCase(member.Name);

                    AddOrUpdate(member, scriptName);
                }
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
            AttributeData scriptNameAttributeData = symbol.GetAttributes()
                .FirstOrDefault(
                    attributeData => attributeData.AttributeClass.ToDisplayString(s_symbolDisplayFormat) ==
                        "System.Runtime.CompilerServices.ScriptNameAttribute");

            return scriptNameAttributeData?.ConstructorArguments[0].Value.ToString();
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
