// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableUtils.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains static utility methods for using a symbol table. Pulled out into a separate class
    /// because statics aren't shared between generic instances.
    /// </summary>
    internal static class SymbolTableUtils
    {
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

        public static string KeyFromSymbol(ISymbol symbol) => symbol.ToDisplayString(s_symbolDisplayFormat);

        private sealed class KeyEqualityComparer : IEqualityComparer<ISymbol>
        {
            public bool Equals(ISymbol x, ISymbol y) => KeyFromSymbol(x).Equals(KeyFromSymbol(y));

            public int GetHashCode(ISymbol obj) => KeyFromSymbol(obj).GetHashCode();
        }
    }
}
