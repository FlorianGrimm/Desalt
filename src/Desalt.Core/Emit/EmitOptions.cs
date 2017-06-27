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
        public static readonly EmitOptions Default = new EmitOptions(
            newline: Environment.NewLine,
            indentationPrefix: "  ",
            simpleBlockOnNewLine: true,
            newlineBetweenPropertyAssignments: true,
            spaceWithinSimpleBlockBraces: true);

        /// <summary>
        /// Represents the set of options that produce compact code, with extraneous whitespace removed.
        /// </summary>
        public static readonly EmitOptions Compact = new EmitOptions();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new instance of the <see cref="EmitOptions"/> class. Values are set using the
        /// following precedence:
        /// * The named parameter, if specified
        /// * Otherwise, the value of <paramref name="instanceToCopy"/>, if specified
        /// * Otherwise, the default value of the parameter for compact code generation
        /// </summary>
        private EmitOptions(
            EmitOptions instanceToCopy = null,
            string newline = null,
            string indentationPrefix = null,
            bool? simpleBlockOnNewLine = null,
            bool? newlineBetweenPropertyAssignments = null,
            bool? spaceWithinSimpleBlockBraces = null)
        {
            Newline = newline ?? instanceToCopy?.Newline ?? "\n";
            IndentationPrefix = indentationPrefix ?? instanceToCopy?.IndentationPrefix ?? "\t";

            SimpleBlockOnNewLine =
                simpleBlockOnNewLine ?? instanceToCopy?.SimpleBlockOnNewLine ?? false;

            NewlineBetweenPropertyAssignments = newlineBetweenPropertyAssignments ??
                instanceToCopy?.NewlineBetweenPropertyAssignments ?? false;

            SpaceWithinSimpleBlockBraces = spaceWithinSimpleBlockBraces ??
                instanceToCopy?.SpaceWithinSimpleBlockBraces ?? false;
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
        /// Indicates whether a block of one statement should be on the same line within the braces
        /// or on a separate line.
        /// </summary>
        public bool SimpleBlockOnNewLine { get; }

        public EmitOptions WithSimpleBlockOnNewLine(bool value) =>
            new EmitOptions(this, simpleBlockOnNewLine: value);

        /// <summary>
        /// Indicates whether a new line should be inserted between property assignments in an object initializer.
        /// </summary>
        public bool NewlineBetweenPropertyAssignments { get; }

        public EmitOptions WithNewlineBetweenPropertyAssignments(bool value) =>
            new EmitOptions(this, newlineBetweenPropertyAssignments: value);

        /// <summary>
        /// Indicates whether a space should be between simple block braces and the content of the block.
        /// </summary>
        public bool SpaceWithinSimpleBlockBraces { get; }

        public EmitOptions WithSpaceWithinSimpleBlockBraces(bool value) =>
            new EmitOptions(this, spaceWithinSimpleBlockBraces: value);
    }
}
