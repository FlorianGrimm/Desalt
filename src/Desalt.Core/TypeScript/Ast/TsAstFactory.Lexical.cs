﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Lexical.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using Desalt.Core.TypeScript.Ast.Lexical;

    public static partial class TsAstFactory
    {
        /// <summary>
        /// Creates a TypeScript single-line comment of the form '// comment'.
        /// </summary>
        public static ITsSingleLineComment SingleLineComment(
            string comment,
            bool preserveSpacing = false,
            bool omitNewLineAtEnd = false)
        {
            return new TsSingleLineComment(comment, preserveSpacing, omitNewLineAtEnd);
        }

        /// <summary>
        /// Creates a TypeScript multi-line comment of the form '/* lines */'.
        /// </summary>
        public static ITsMultiLineComment MultiLineComment(params string[] lines)
        {
            return new TsMultiLineComment(lines);
        }

        /// <summary>
        /// Creates a TypeScript multi-line comment of the form '/* lines */'.
        /// </summary>
        public static ITsMultiLineComment MultiLineComment(
            bool isJsDoc,
            bool preserveSpacing = false,
            params string[] lines)
        {
            return new TsMultiLineComment(lines, isJsDoc, preserveSpacing);
        }
    }
}