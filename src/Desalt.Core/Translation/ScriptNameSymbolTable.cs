// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
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
                foreach (ISymbol member in symbol.GetMembers().Where(sym => !sym.IsImplicitlyDeclared))
                {
                    if (member is IMethodSymbol methodMember)
                    {
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (methodMember.MethodKind)
                        {
                            // skip constructors
                            case MethodKind.Constructor:
                            case MethodKind.StaticConstructor:
                                continue;

                            // skip property get/set methods
                            case MethodKind.PropertyGet:
                            case MethodKind.PropertySet:
                                continue;

                            // skip event add/remove
                            case MethodKind.EventAdd:
                            case MethodKind.EventRemove:
                                continue;
                        }
                    }

                    string memberName = member.Name;
                    string scriptName = FindScriptName(member) ?? ToCamelCase(memberName);

                    AddOrUpdate(member, scriptName);
                }
            }
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
    }
}
