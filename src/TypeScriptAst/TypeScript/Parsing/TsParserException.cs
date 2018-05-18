// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserException.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Parsing
{
    using System;
    using CompilerUtilities;

    public class TsParserException : Exception
    {
        public TsParserException(string message, TextReaderLocation location)
            : base($"{location}: error: {message}")
        {
            Location = location;
        }

        public TextReaderLocation Location { get; }

        /// <summary>
        /// Indicates whether the exception is because a parse feature is not yet implemented.
        /// </summary>
        public bool NotYetImplemented { get; set; }
    }
}
