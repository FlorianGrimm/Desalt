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

        /// <summary>
        /// Represents options that use Unix line endings (\n) and tabs for indentation.
        /// </summary>
        public static readonly EmitOptions UnixTabs =
            new EmitOptions(instanceToCopy: null, newline: "\n", indentationPrefix: "\t");

        /// <summary>
        /// Represents options that use Unix line endings (\n) and two spaces for indentation.
        /// </summary>
        public static readonly EmitOptions UnixSpaces =
            new EmitOptions(instanceToCopy: null, newline: "\n", indentationPrefix: "  ");

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
        public EmitOptions(string newline = null, string indentationPrefix = "  ")
            : this(null, newline, indentationPrefix)
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
            string indentationPrefix = null)
        {
            Newline = newline ?? instanceToCopy?.Newline ?? Environment.NewLine;
            IndentationPrefix = indentationPrefix ?? instanceToCopy?.IndentationPrefix ?? "  ";
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
    }
}
