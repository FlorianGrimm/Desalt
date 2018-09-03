// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptStructSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    /// <summary>
    /// Represents a struct symbol that can be used in the translation process.
    /// </summary>
    internal interface IScriptStructSymbol : IScriptTypeSymbol
    {
        /// <summary>
        /// Can be applied to a user-defined value type (struct) to instruct the compiler that it can
        /// be mutated and therefore needs to be copied whenever .net would create a copy of a value type.
        /// </summary>
        bool Mutable { get; }
    }
}
