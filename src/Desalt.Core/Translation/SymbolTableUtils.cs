// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableUtils.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table. Pulled out into a separate class
    /// because statics aren't shared between generic instances.
    /// </summary>
    internal static class SymbolTableUtils
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly IEqualityComparer<ISymbol> KeyComparer = new KeyEqualityComparer();

        private static readonly SymbolDisplayFormat s_symbolDisplayFormat = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            SymbolDisplayGenericsOptions.IncludeTypeParameters,
            SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
            SymbolDisplayDelegateStyle.NameOnly,
            SymbolDisplayExtensionMethodStyle.StaticMethod,
            SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeType,
            SymbolDisplayPropertyStyle.NameOnly,
            SymbolDisplayLocalOptions.IncludeType,
            SymbolDisplayKindOptions.IncludeNamespaceKeyword |
            SymbolDisplayKindOptions.IncludeTypeKeyword |
            SymbolDisplayKindOptions.IncludeMemberKeyword,
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static string KeyFromSymbol(ISymbol symbol) => symbol?.ToDisplayString(s_symbolDisplayFormat);

        public static IEqualityComparer<KeyValuePair<ISymbol, T>> GetKeyValueComparer<T>() =>
            new SymbolKeyValuePairComparer<T>();

        /// <summary>
        /// Finds a Saltarelle attribute attached to a specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeNameMinusSuffix">
        /// The name of the attribute to find, minus the "Attribute" suffix. For example,
        /// "InlineCode", which represents the <c>System.Runtime.CompilerServices.InlineCodeAttribute</c>.
        /// </param>
        /// <returns>
        /// The found attribute or null if the symbol does not have an attached attribute of the
        /// given name.
        /// </returns>
        public static AttributeData FindSaltarelleAttribute(ISymbol symbol, string attributeNameMinusSuffix)
        {
            SymbolDisplayFormat format = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullAttributeName = $"System.Runtime.CompilerServices.{attributeNameMinusSuffix}Attribute";
            AttributeData attributeData = symbol?.GetAttributes()
                .FirstOrDefault(x => x.AttributeClass.ToDisplayString(format) == fullAttributeName);

            return attributeData;
        }

        /// <summary>
        /// Gets the value of the Saltarelle attribute attached to a specified symbol or a default
        /// value if the attribute is not present.
        /// </summary>
        /// <param name="symbol">The symbol to query.</param>
        /// <param name="attributeNameMinusSuffix">
        /// The name of the attribute to find, minus the "Attribute" suffix. For example,
        /// "InlineCode", which represents the <c>System.Runtime.CompilerServices.InlineCodeAttribute</c>.
        /// </param>
        /// <param name="defaultValue">
        /// The value to use if the attribute is not present on the symbol.
        /// </param>
        /// <returns>
        /// Either the value of the attribute or the default value if the attribute is not present on
        /// the symbol.
        /// </returns>
        public static string GetSaltarelleAttributeValueOrDefault(
            ISymbol symbol,
            string attributeNameMinusSuffix,
            string defaultValue)
        {
            AttributeData attributeData = FindSaltarelleAttribute(symbol, attributeNameMinusSuffix);
            return attributeData?.ConstructorArguments[0].Value.ToString() ?? defaultValue;
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class KeyEqualityComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals(ISymbol x, ISymbol y) => KeyFromSymbol(x).Equals(KeyFromSymbol(y));

            public int GetHashCode(ISymbol obj) => KeyFromSymbol(obj).GetHashCode();
        }

        private sealed class SymbolKeyValuePairComparer<T> : IEqualityComparer<KeyValuePair<ISymbol, T>>
        {
            public bool Equals(KeyValuePair<ISymbol, T> x, KeyValuePair<ISymbol, T> y) =>
                KeyFromSymbol(x.Key).Equals(KeyFromSymbol(y.Key));

            public int GetHashCode(KeyValuePair<ISymbol, T> obj) => KeyFromSymbol(obj.Key).GetHashCode();
        }
    }
}
