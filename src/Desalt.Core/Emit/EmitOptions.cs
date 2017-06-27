// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EmitOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Emit
{
    using System;

    /// <summary>
    /// Contains options for emitting code, mostly around how it gets formatted.
    /// </summary>
    public sealed class EmitOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents the set of options that produce readable code.
        /// </summary>
        public static readonly EmitOptions Default = new EmitOptions(instanceToCopy: null);

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new instance of the <see cref="EmitOptions"/> class using the specified options.
        /// </summary>
        /// <param name="newline">
        /// The character sequence to use for new lines. Defaults to <see cref="Environment.NewLine"/>
        /// </param>
        /// <param name="indentationPrefix">
        /// The prefix to use for indenting blocks. Defaults to two spaces.
        /// </param>
        /// <param name="simpleBlockOnNewLine">
        /// Indicates whether a block of one statement should be on the same line within the braces
        /// or on a separate line. Defaults to false.
        /// </param>
        public EmitOptions(
            string newline = null,
            string indentationPrefix = "  ",
            bool simpleBlockOnNewLine = false)

            : this(null, newline, indentationPrefix, simpleBlockOnNewLine)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EmitOptions"/> class. Values are set using the
        /// following precedence:
        /// * The named parameter, if specified
        /// * Otherwise, the value of <paramref name="instanceToCopy"/>, if specified
        /// * Otherwise, the default value of the parameter's type
        /// </summary>
        private EmitOptions(
            EmitOptions instanceToCopy = null,
            string newline = null,
            string indentationPrefix = null,
            bool? simpleBlockOnNewLine = null)
        {
            Newline = newline ?? instanceToCopy?.Newline ?? Environment.NewLine;
            IndentationPrefix = indentationPrefix ?? instanceToCopy?.IndentationPrefix ?? "  ";
            SimpleBlockOnNewLine = simpleBlockOnNewLine ?? instanceToCopy?.SimpleBlockOnNewLine ?? false;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the character sequence to use for new lines.
        /// </summary>
        public string Newline { get; }

        public EmitOptions WithNewline(string value) => new EmitOptions(this, newline: value);

        /// <summary>
        /// Gets the prefix to use for indenting blocks.
        /// </summary>
        public string IndentationPrefix { get; }

        public EmitOptions WithIndentationPrefix(string value) => new EmitOptions(this, indentationPrefix: value);

        /// <summary>
        /// Gets a value indicating whether a block of one statement should be on the same line
        /// within the braces or on a separate line.
        /// </summary>
        public bool SimpleBlockOnNewLine { get; }

        public EmitOptions WithSimpleBlockOnNewLine(bool value) => new EmitOptions(this, simpleBlockOnNewLine: value);
    }
}
