// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGrammarExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using Desalt.Core.Utility;
    using Desalt.TypeScript.Ast.Types;

    /// <summary>
    /// Contains static extension methods for <see cref="ITsAstNode"/> objects.
    /// </summary>
    public static class TsGrammarExtensions
    {
        /// <summary>
        /// Creates a new <see cref="ITsParenthesizedType"/> wrapping the specified type.
        /// </summary>
        /// <param name="type">The type to wrap inside of parentheses.</param>
        /// <returns>A new <see cref="ITsParenthesizedType"/> wrapping the specified type.</returns>
        public static ITsParenthesizedType WithParentheses(this ITsType type)
        {
            return new TsParenthesizedType(type);
        }

        /// <summary>
        /// Writes out a ": type" type annotation if the type is not null.
        /// </summary>
        /// <param name="type">The type annotation to write.</param>
        public static string ToTypeAnnotationCodeDisplay(this ITsType type)
        {
            return type != null ? $": {type.ToCodeDisplay()}" : string.Empty;
        }

        public static void WriteTypeAnnotation(this ITsType type, IndentedTextWriter writer)
        {
            if (type != null)
            {
                writer.Write(": ");
                type.WriteFullCodeDisplay(writer);
            }
        }
    }
}
