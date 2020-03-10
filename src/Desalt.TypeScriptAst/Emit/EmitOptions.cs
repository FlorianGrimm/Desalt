// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EmitOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Emit
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
        public static readonly EmitOptions UnixTabs = new EmitOptions(
            instanceToCopy: null,
            newline: "\n",
            indentationPrefix: "\t");

        /// <summary>
        /// Represents options that use Unix line endings (\n) and two spaces for indentation.
        /// </summary>
        public static readonly EmitOptions UnixSpaces = new EmitOptions(
            instanceToCopy: null,
            newline: "\n",
            indentationPrefix: "  ");

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
        /// <param name="singleLineJsDocCommentsOnOneLine">
        /// A value indicating whether single-line JSDoc comments should be emitted on a single line
        /// ('/** text */') or multiple lines ('/**\n * text\n*/').
        /// </param>
        public EmitOptions(
            string newline = null,
            string indentationPrefix = "  ",
            bool singleLineJsDocCommentsOnOneLine = false) : this(
            null,
            newline,
            indentationPrefix,
            singleLineJsDocCommentsOnOneLine)
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
            bool? singleLineJsDocCommentsOnOneLine = null)
        {
            Newline = newline ?? instanceToCopy?.Newline ?? Environment.NewLine;
            IndentationPrefix = indentationPrefix ?? instanceToCopy?.IndentationPrefix ?? "  ";
            SingleLineJsDocCommentsOnOneLine =
                singleLineJsDocCommentsOnOneLine.GetValueOrDefault(
                    instanceToCopy?.SingleLineJsDocCommentsOnOneLine ?? false);
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
        /// Gets a value indicating whether single-line JSDoc comments should be emitted on a single
        /// line ('/** text */') or multiple lines ('/**\n * text\n*/').
        /// </summary>
        public bool SingleLineJsDocCommentsOnOneLine { get; }

        public EmitOptions WithSingleLineJsDocCommentsOnOneLine(bool value) =>
            new EmitOptions(this, singleLineJsDocCommentsOnOneLine: value);
    }
}
