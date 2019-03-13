// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptNamer.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Determines names for the generated script given C# symbols.
    /// </summary>
    internal interface IScriptNamer
    {
        /// <summary>
        /// Determines the name a symbol should have in the generated script.
        /// </summary>
        /// <param name="symbol">The symbol for which to discover the script name.</param>
        /// <returns>The name the specified symbol should have in the generated script.</returns>
        string DetermineScriptNameForSymbol(ISymbol symbol);
    }
}
