// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessibilityModifierComparer.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using Desalt.TypeScriptAst.Ast;

    /// <summary>
    /// Contains code to compare <see cref="TsAccessibilityModifier"/> based on code visibility.
    /// </summary>
    internal static class AccessibilityModifierComparer
    {
        /// <summary>
        /// An <see cref="IComparer{T}"/> that orders from most visible (public) to least visible (private).
        /// </summary>
        public static readonly IComparer<TsAccessibilityModifier> MostVisibleToLeastVisible = new MostVisibleToLeast();

        private sealed class MostVisibleToLeast : IComparer<TsAccessibilityModifier>
        {
            public int Compare(TsAccessibilityModifier x, TsAccessibilityModifier y)
            {
                return (x, y) switch
                {
                    (_, _) when x == y => 0,
                    (TsAccessibilityModifier.Public, _) => -1,
                    (_, TsAccessibilityModifier.Public) => 1,
                    (TsAccessibilityModifier.Protected, _) => -1,
                    (_, TsAccessibilityModifier.Protected) => 1,
                    (_, _) => throw new InvalidOperationException(),
                };
            }
        }
    }
}
