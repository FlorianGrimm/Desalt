// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptNameSymbolTable.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains mappings of defined types in a C# project to the translated names (what they will be
    /// called in the TypeScript file). By default, Saltarelle converts type members to `camelCase`
    /// names. It can also be changed using the [PreserveName] and [ScriptName] attributes.
    /// </summary>
    /// <remarks>This type is thread-safe and is able to be accessed concurrently.</remarks>
    internal class ScriptNameSymbolTable : SymbolTable<string>
    {
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
            var allTypeDeclarations = context.RootSyntax.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();
            foreach (BaseTypeDeclarationSyntax node in allTypeDeclarations)
            {
                INamedTypeSymbol symbol = context.SemanticModel.GetDeclaredSymbol(node, cancellationToken);
                string typeName = symbol.Name;
                AddOrUpdate(symbol, typeName);

                // add all of the members of the declared type
                foreach (ISymbol member in symbol.GetMembers())
                {
                    string memberName = member.Name;
                    string scriptName = char.ToLowerInvariant(memberName[0]) + memberName.Substring(1);
                    AddOrUpdate(member, scriptName);
                }
            }
        }
    }
}
