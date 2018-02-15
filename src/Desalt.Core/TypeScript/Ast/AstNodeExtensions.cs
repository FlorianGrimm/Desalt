// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstNodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.Core.Utility;

    /// <summary>
    /// Contains extension methods for working with <see cref="IAstNode"/> objects.
    /// </summary>
    public static class AstNodeExtensions
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
            this IEnumerable<T> list,
            string delimiter = ", ",
            int maxStringLength = 32)
            where T : IAstNode
        {
            Param.VerifyGreaterThanOrEqualTo(maxStringLength, nameof(maxStringLength), 0);

            var builder = new StringBuilder();
            foreach (T item in list)
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

                string itemStr = item.CodeDisplay;
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
        public static T PrependTo<T>(this IAstTriviaNode trivia, T node) where T : IAstNode
        {
            AstNode classNode = node as AstNode ?? throw new InvalidCastException();
            return (T)(object)classNode.WithLeadingTrivia<AstNode>(trivia);
        }

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="node">The node to emit.</param>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public static string EmitAsString(this IAstNode node, EmitOptions emitOptions = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new Emitter(stream, options: emitOptions))
            {
                node.Emit(emitter);
                stream.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, emitter.Encoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
