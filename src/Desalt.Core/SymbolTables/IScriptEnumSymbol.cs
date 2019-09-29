// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptEnumSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    /// <summary>
    /// Represents an enum symbol that can be used in the translation process.
    /// </summary>
    internal interface IScriptEnumSymbol : IScriptTypeSymbol
    {
        /// <summary>
        /// Indicates whether the enumeration type should be generated as a set of names. Rather than
        /// the specific value, the name of the enumeration field is used as a string.
        ///
        /// TODO - Support [NamedValues]
        /// </summary>
        bool NamedValues { get; }

        /// <summary>
        /// Indicates whether the enumeration type should be generated as a set of numeric values.
        /// Rather than the enum field, the value of the enumeration field is used as a literal.
        ///
        /// TODO - Support [NumericValues]
        /// </summary>
        bool NumericValues { get; }
    }
}
