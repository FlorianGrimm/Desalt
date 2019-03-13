// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptEnumSymbol.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents an enum symbol that can be used in the translation process.
    /// </summary>
    internal sealed class ScriptEnumSymbol : ScriptTypeSymbol, IScriptEnumSymbol
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public ScriptEnumSymbol(INamedTypeSymbol symbol, string computedScriptName)
            : base(symbol, computedScriptName)
        {
            if (symbol.TypeKind != TypeKind.Enum)
            {
                throw new ArgumentException("Symbol is not an enum", nameof(symbol));
            }

            NamedValues = symbol.GetFlagAttribute(SaltarelleAttributeName.NamedValues);
            NumericValues = symbol.GetFlagAttribute(SaltarelleAttributeName.NumericValues);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether the enumeration type should be generated as a set of names. Rather than
        /// the specific value, the name of the enumeration field is used as a string.
        /// </summary>
        public bool NamedValues { get; }

        /// <summary>
        /// Indicates whether the enumeration type should be generated as a set of numeric values.
        /// Rather than the enum field, the value of the enumeration field is used as a literal.
        /// </summary>
        public bool NumericValues { get; }
    }
}
