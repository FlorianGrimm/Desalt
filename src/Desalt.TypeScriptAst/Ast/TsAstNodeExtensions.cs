// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Immutable;

    /// <summary>
    /// Contains extension methods for working with <see cref="ITsAstNode"/> objects.
    /// </summary>
    public static class TsAstNodeExtensions
    {
        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T PrependTo<T>(this ITsAstTriviaNode trivia, T node)
            where T : ITsAstNode
        {
            return (T)node.ShallowCopy(node.LeadingTrivia.Insert(0, trivia), node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithLeadingTrivia<T>(this T node, ImmutableArray<ITsAstTriviaNode> value)
            where T : ITsAstNode
        {
            return node.LeadingTrivia == value ? node : (T)node.ShallowCopy(value, node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public static T WithLeadingTrivia<T>(this T node, params ITsAstTriviaNode[] trivia)
            where T : ITsAstNode
        {
            return (T)node.ShallowCopy(trivia.ToImmutableArray(), node.TrailingTrivia);
        }

        /// <summary>
        /// Creates a copy of this node with the specified trailing trivia.
        /// </summary>
        public static T WithTrailingTrivia<T>(this T node, ImmutableArray<ITsAstTriviaNode> value)
            where T : ITsAstNode
        {
            return node.TrailingTrivia == value ? node : (T)node.ShallowCopy(node.LeadingTrivia, value);
        }

        /// <summary>
        /// Creates a copy of this node with the specified trailing trivia.
        /// </summary>
        public static T WithTrailingTrivia<T>(this T node, params ITsAstTriviaNode[] trivia)
            where T : ITsAstNode
        {
            return (T)node.ShallowCopy(node.LeadingTrivia, trivia.ToImmutableArray());
        }
    }
}
