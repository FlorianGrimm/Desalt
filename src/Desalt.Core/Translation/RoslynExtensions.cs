// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Contains extension methods for working with Roslyn data types.
    /// </summary>
    internal static class RoslynExtensions
    {
        /// <summary>
        /// Extracts the semanic type symbol from the specified type syntax node.
        /// </summary>
        public static ITypeSymbol GetTypeSymbol(this TypeSyntax typeSyntax, SemanticModel semanticModel)
        {
            return semanticModel.GetTypeInfo(typeSyntax).Type;
        }

        public static bool IsInterfaceType(this ITypeSymbol symbol)
        {
            return symbol?.TypeKind == TypeKind.Interface;
        }
    }
}
