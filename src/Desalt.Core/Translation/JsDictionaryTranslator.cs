// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="JsDictionaryTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains methods to aid in translating <c>JsDictionary</c> types.
    /// </summary>
    internal class JsDictionaryTranslator
    {
        /// <summary>
        /// Returns a value indicating whether the specified type symbol is an instance of
        /// <c>JsDictionary</c> or <c>JsDictionary{TKey, TValue}</c>.
        /// </summary>
        /// <param name="typeSymbol">The <see cref="ITypeSymbol"/> to examine.</param>
        /// <returns>
        /// True if the specified type symbol is an instance of <c>JsDictionary</c> or
        /// <c>JsDictionary{TKey, TValue}</c>; otherwise, false.
        /// </returns>
        public static bool IsJsDictionary(ITypeSymbol typeSymbol)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            return namedTypeSymbol?.ToHashDisplay() == "System.Collections.JsDictionary" ||
                namedTypeSymbol?.OriginalDefinition?.ToHashDisplay() ==
                "System.Collections.Generic.JsDictionary<TKey, TValue>";
        }
    }
}
