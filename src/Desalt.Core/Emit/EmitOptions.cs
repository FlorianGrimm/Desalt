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
            surroundOperatorsWithSpaces: true,
            spaceWithinEmptyArrayBrackets: true,
            spaceWithinEmptyObjectInitializers: true,
            spaceWithinEmptyFunctionBody: true,
            newlineAfterOpeningBrace: true,
            newlineBeforeClosingBrace: true,
            simpleBlockOnNewLine: true,
            newlineBetweenPropertyAssignments: true,
            newlineBetweenStatements: true,
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
            bool? surroundOperatorsWithSpaces = null,
            bool? spaceWithinEmptyArrayBrackets = null,
            bool? spaceWithinEmptyObjectInitializers = null,
            bool? spaceWithinEmptyFunctionBody = null,
            bool? newlineAfterOpeningBrace = null,
            bool? newlineBeforeClosingBrace = null,
            bool? simpleBlockOnNewLine = null,
            bool? newlineBetweenPropertyAssignments = null,
            bool? newlineBetweenStatements = null,
            bool? spaceWithinSimpleBlockBraces = null)
        {
            Newline = newline ?? instanceToCopy?.Newline ?? "\n";
            IndentationPrefix = indentationPrefix ?? instanceToCopy?.IndentationPrefix ?? "\t";

            SurroundOperatorsWithSpaces =
                surroundOperatorsWithSpaces ?? instanceToCopy?.SurroundOperatorsWithSpaces ?? false;

            SpaceWithinEmptyArrayBrackets =
                spaceWithinEmptyArrayBrackets ?? instanceToCopy?.SpaceWithinEmptyArrayBrackets ?? false;

            SpaceWithinEmptyObjectInitializers =
                spaceWithinEmptyObjectInitializers ?? instanceToCopy?.SpaceWithinEmptyObjectInitializers ?? false;

            SpaceWithinEmptyFunctionBody = spaceWithinEmptyFunctionBody ??
                instanceToCopy?.SpaceWithinEmptyFunctionBody ?? false;

            NewlineAfterOpeningBrace =
                newlineAfterOpeningBrace ?? instanceToCopy?.NewlineAfterOpeningBrace ?? false;

            NewlineBeforeClosingBrace =
                newlineBeforeClosingBrace ?? instanceToCopy?.NewlineBeforeClosingBrace ?? false;

            SimpleBlockOnNewLine =
                simpleBlockOnNewLine ?? instanceToCopy?.SimpleBlockOnNewLine ?? false;

            NewlineBetweenPropertyAssignments = newlineBetweenPropertyAssignments ??
                instanceToCopy?.NewlineBetweenPropertyAssignments ?? false;

            NewlineBetweenStatements = newlineBetweenStatements ?? instanceToCopy?.NewlineBetweenStatements ?? false;

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
        /// Indicates whether operators should be surrouded with spaces.
        /// </summary>
        public bool SurroundOperatorsWithSpaces { get; }

        public EmitOptions WithSurroundOperatorsWithSpaces(bool value) =>
            new EmitOptions(this, surroundOperatorsWithSpaces: value);

        /// <summary>
        /// Indicates whether to include a space between array brackets when the array is empty.
        /// </summary>
        public bool SpaceWithinEmptyArrayBrackets { get; }

        public EmitOptions WithSpaceWithinEmptyArrayBrackets(bool value) =>
            new EmitOptions(this, spaceWithinEmptyArrayBrackets: value);

        /// <summary>
        /// Indicates whether to include a space between object initializer braces if the object
        /// initializer is empty.
        /// </summary>
        public bool SpaceWithinEmptyObjectInitializers { get; }

        public EmitOptions WithSpaceWithinEmptyObjectInitializers(bool value) =>
            new EmitOptions(this, spaceWithinEmptyObjectInitializers: value);

        /// <summary>
        /// Indicates whether to include a space between empty function bodies.
        /// </summary>
        public bool SpaceWithinEmptyFunctionBody { get; }

        public EmitOptions WithSpaceWithinEmptyFunctionBody(bool value) =>
            new EmitOptions(this, spaceWithinEmptyFunctionBody: value);

        /// <summary>
        /// Indicates whether to insert a newline after an opening brace in a block statement.If
        /// there is only a single line in the block, the <see cref="SimpleBlockOnNewLine"/> flag
        /// takes precedence over this setting.
        /// </summary>
        public bool NewlineAfterOpeningBrace { get; }

        public EmitOptions WithNewlineAfterOpeningBrace(bool value) =>
            new EmitOptions(this, newlineAfterOpeningBrace: value);

        /// <summary>
        /// Indicates whether to insert a newline before the closing brace in a block statement. If
        /// there is only a single line in the block, the <see cref="SimpleBlockOnNewLine"/> flag
        /// takes precedence over this setting.
        /// </summary>
        public bool NewlineBeforeClosingBrace { get; }

        public EmitOptions WithNewlineBeforeClosingBrace(bool value) =>
            new EmitOptions(this, newlineBeforeClosingBrace: value);

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
        /// Indicates whether a new line should be placed between multiple statements.
        /// </summary>
        public bool NewlineBetweenStatements { get; }

        public EmitOptions WithNewlineBetweenStatements(bool value) =>
            new EmitOptions(this, newlineBetweenStatements: value);

        /// <summary>
        /// Indicates whether a space should be between simple block braces and the content of the block.
        /// </summary>
        public bool SpaceWithinSimpleBlockBraces { get; }

        public EmitOptions WithSpaceWithinSimpleBlockBraces(bool value) =>
            new EmitOptions(this, spaceWithinSimpleBlockBraces: value);
    }
}
