// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;

    /// <summary>
    /// Contains extension methods for working with <see cref="ITsAstNode"/> objects.
    /// </summary>
    public static class TsAstNodeExtensions
    {
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
