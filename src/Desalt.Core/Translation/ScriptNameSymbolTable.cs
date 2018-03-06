// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the translated names (what they will be
    /// called in the TypeScript file). By default, Saltarelle converts type members to `camelCase`
    /// names. It can also be changed using the [PreserveName] and [ScriptName] attributes.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ScriptNameSymbolTable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly ConcurrentDictionary<string, string> _symbolToNameMap =
            new ConcurrentDictionary<string, string>();

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public string this[string symbolName]
        {
            get
            {
                if (!_symbolToNameMap.TryGetValue(symbolName, out string scriptName))
                {
                    throw new KeyNotFoundException(
                        $"There is no symbol '{symbolName}' defined in the script name symbol table");
                }

                return scriptName;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Adds all of the defined types in the document to the mapping.
        /// </summary>
        public void AddDefinedTypesInDocument(DocumentTranslationContext context)
        {
            var allTypeDeclarations = context.RootSyntax.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();
            foreach (BaseTypeDeclarationSyntax node in allTypeDeclarations)
            {
                INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(node);
                string typeName = symbol.Name;
                _symbolToNameMap.AddOrUpdate(typeName, _ => typeName, (_, __) => typeName);

                // add all of the members of the declared type
                foreach (ISymbol member in symbol.GetMembers())
                {
                    string memberName = member.Name;
                    string scriptName = char.ToLowerInvariant(memberName[0]) + memberName.Substring(1);
                    _symbolToNameMap.AddOrUpdate(memberName, _ => scriptName, (_, __) => scriptName);
                }
            }
        }

        /// <summary>
        /// Returns a value indicating whether the symbol table contains a definition for the
        /// specified symbol name.
        /// </summary>
        public bool HasSymbol(string symbolName)
        {
            return _symbolToNameMap.TryGetValue(symbolName, out string _);
        }
    }
}
