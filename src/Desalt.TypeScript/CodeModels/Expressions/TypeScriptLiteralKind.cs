// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptLiteralKind.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    /// <summary>
    /// Represents the different kinds of literal tokens.
    /// </summary>
    public enum TypeScriptLiteralKind
    {
        /// <summary>
        /// The 'null' literal.
        /// </summary>
        Null,

        /// <summary>
        /// The Boolean literal 'true'.
        /// </summary>
        True,

        /// <summary>
        /// The Boolean literal 'false'.
        /// </summary>
        False,

        /// <summary>
        /// The numeric decimal literal, containing a integer number with an optional decimal and exponent.
        /// </summary>
        Decimal,

        /// <summary>
        /// The numeric binary integer literal.
        /// </summary>
        BinaryInteger,

        /// <summary>
        /// The numeric octal integer literal.
        /// </summary>
        OctalInteger,

        /// <summary>
        /// The numeric hexidecimal integer literal.
        /// </summary>
        HexInteger,

        /// <summary>
        /// The string literal, surrounded by either single or double quotes.
        /// </summary>
        String,

        /// <summary>
        /// A regular expression literal.
        /// </summary>
        RegExp,
    }
}
