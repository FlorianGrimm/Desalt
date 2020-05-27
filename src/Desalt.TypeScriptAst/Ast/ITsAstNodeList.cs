// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ITsAstNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Generic;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an immutable list of <see cref="ITsAstNode"/> nodes separated with tokens.
    /// </summary>
    /// <typeparam name="T">The specific type of <see cref="ITsAstNode"/> nodes in the list.</typeparam>
    public interface ITsAstNodeList<T> : IReadOnlyList<T>, IEquatable<ITsAstNodeList<T>>
        where T : ITsAstNode
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the list is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Returns the sequence of just the separator tokens.
        /// </summary>
        IEnumerable<ITsTokenNode> Separators { get; }

        /// <summary>
        /// Returns the open token for the list, for example '(', '&lt;'.
        /// </summary>
        ITsTokenNode OpenToken { get; }

        /// <summary>
        /// Returns the close token for the list, for example ')', '&gt;'.
        /// </summary>
        ITsTokenNode CloseToken { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Gets the separator token at the specified index in this list. For example, if <paramref name="index"/> is
        /// <c>0</c> the first separator is returned (which is after the first <see cref="ITsAstNode"/>).
        /// </summary>
        /// <param name="index">The index within the list of the separator to return.</param>
        /// <returns>The token node representing the separator at the specified index.</returns>
        ITsTokenNode GetSeparator(int index);

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        void Emit(Emitter emitter);

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        string EmitAsString(EmitOptions? emitOptions = null);

        ///// <summary>
        ///// Creates a copy of this list with the open token leading trivia replaced with the specified value.
        ///// </summary>
        //ITsAstNodeList<T> WithOpenTokenLeadingTrivia(IEnumerable<ITsAstTriviaNode> trivia);

        ///// <summary>
        ///// Creates a copy of this list with the open token trailing trivia replaced with the specified value.
        ///// </summary>
        //ITsAstNodeList<T> WithOpenTokenTrailingTrivia(IEnumerable<ITsAstTriviaNode> trivia);

        ///// <summary>
        ///// Creates a copy of this list with the close token's leading trivia replaced with the specified value.
        ///// </summary>
        //ITsAstNodeList<T> WithCloseTokenLeadingTrivia(IEnumerable<ITsAstTriviaNode> trivia);

        ///// <summary>
        ///// Creates a copy of this list with the close token's trailing trivia replaced with the specified value.
        ///// </summary>
        //ITsAstNodeList<T> WithCloseTokenTrailingTrivia(IEnumerable<ITsAstTriviaNode> trivia);

        ///// <summary>
        ///// Creates a copy of this list with the leading trivia of each separator replaced with the specified trivia.
        ///// </summary>
        //ITsAstNodeList<T> WithSeparatorsLeadingTrivia(
        //    IEnumerable<IEnumerable<ITsAstTriviaNode>> triviaNodesForEachSeparator);

        ///// <summary>
        ///// Creates a copy of this list with the trailing trivia of each separator replaced with the specified trivia.
        ///// </summary>
        //ITsAstNodeList<T> WithSeparatorsTrailingTrivia(
        //    IEnumerable<IEnumerable<ITsAstTriviaNode>> triviaNodesForEachSeparator);
    }
}
