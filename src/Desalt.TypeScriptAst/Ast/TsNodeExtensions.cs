// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;

    /// <summary>
    /// Contains extension methods for working with <see cref="ITsNode"/> objects.
    /// </summary>
    public static class TsNodeExtensions
    {
        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T PrependTo<T>(this ITsAstTriviaNode trivia, T node)
            where T : ITsNode
        {
            return (T)node.ShallowCopy(node.LeadingTrivia.Insert(0, trivia), node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithLeadingTrivia<T>(this T node, ImmutableArray<ITsAstTriviaNode> value)
            where T : ITsNode
        {
            return node.LeadingTrivia == value ? node : (T)node.ShallowCopy(value, node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithLeadingTrivia<T>(this T node, params ITsAstTriviaNode[] trivia)
            where T : ITsNode
        {
            return (T)node.ShallowCopy(trivia.ToImmutableArray(), node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified trailing trivia.
        /// </summary>
        public static T WithTrailingTrivia<T>(this T node, ImmutableArray<ITsAstTriviaNode> value)
            where T : ITsNode
        {
            return node.TrailingTrivia == value ? node : (T)node.ShallowCopy(node.LeadingTrivia, value);
        }

        /// <summary>
        /// Creates a copy of this node with the specified trailing trivia.
        /// </summary>
        public static T WithTrailingTrivia<T>(this T node, params ITsAstTriviaNode[] trivia)
            where T : ITsNode
        {
            return (T)node.ShallowCopy(node.LeadingTrivia, trivia.ToImmutableArray());
        }
    }
}
