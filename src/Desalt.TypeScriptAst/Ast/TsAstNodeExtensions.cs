// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Desalt.CompilerUtilities;

    /// <summary>
    /// Contains extension methods for working with <see cref="ITsAstNode"/> objects.
    /// </summary>
    public static class TsAstNodeExtensions
    {
        /// <summary>
        /// Creates a delimiter-separated list of elements, up to and not exceeding the specified
        /// maximum string length.
        /// </summary>
        /// <typeparam name="T">The type of element in the list.</typeparam>
        /// <param name="list">The list of items to concatenate.</param>
        /// <param name="delimiter">The delimiter to use between list elements.</param>
        /// <param name="maxStringLength">
        /// The maximum length of the string before it gets elided with ellipses. If the string is
        /// elided, it will have a length of <paramref name="maxStringLength"/> + 3.
        /// </param>
        /// <returns>
        /// A delimiter-separated list of elements, up to and not exceeding the specified maximum
        /// string length.
        /// </returns>
        public static string ToElidedList<T>(
            this IEnumerable<T?> list,
            string delimiter = ", ",
            int maxStringLength = 32)
            where T : class, ITsAstNode
        {
            Param.VerifyGreaterThanOrEqualTo(maxStringLength, nameof(maxStringLength), 0);

            var builder = new StringBuilder();
            foreach (T? item in list)
            {
                if (builder.Length + delimiter.Length >= maxStringLength)
                {
                    builder.Append("...");
                    break;
                }

                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }

                string itemStr = item?.CodeDisplay ?? string.Empty;
                if (builder.Length + itemStr.Length >= maxStringLength)
                {
                    builder.Append("...");
                    break;
                }

                builder.Append(itemStr);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T PrependTo<T>(this ITsAstTriviaNode trivia, T node) where T : ITsAstNode
        {
            TsAstNode classNode = node as TsAstNode ?? throw new InvalidCastException();
            return (T)(object)classNode.WithLeadingTrivia<TsAstNode>(trivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithLeadingTrivia<T>(this T node, params ITsAstTriviaNode[] triviaNodes) where T : ITsAstNode
        {
            TsAstNode classNode = node as TsAstNode ?? throw new InvalidCastException();
            return (T)(object)classNode.WithLeadingTrivia<TsAstNode>(triviaNodes);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithTrailingTrivia<T>(this T node, params ITsAstTriviaNode[] triviaNodes)
            where T : ITsAstNode
        {
            TsAstNode classNode = node as TsAstNode ?? throw new InvalidCastException();
            return (T)(object)classNode.WithTrailingTrivia<TsAstNode>(triviaNodes);
        }
    }
}
