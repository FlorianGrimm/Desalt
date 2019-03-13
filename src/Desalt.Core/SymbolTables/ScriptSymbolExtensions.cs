// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbolExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;

    /// <summary>
    /// Contains extension methods for working with <see cref="IScriptSymbol"/> interfaces.
    /// </summary>
    internal static class ScriptSymbolExtensions
    {
        public static IScriptMethodSymbol WithInlineCode(this IScriptMethodSymbol methodSymbol, string value)
        {
            if (methodSymbol is ScriptMethodSymbol instance)
            {
                return instance.WithInlineCode(value);
            }

            throw new ArgumentException(
                $"The method symbol is not an instance of {typeof(ScriptMethodSymbol)}",
                nameof(methodSymbol));
        }
    }
}
