// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using Desalt.Core.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) nodes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstNode : IAstNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected AstNode(
            IEnumerable<IAstTriviaNode> leadingTrivia = null,
            IEnumerable<IAstTriviaNode> trailingTrivia = null)
        {
            LeadingTrivia = leadingTrivia?.ToImmutableArray() ?? ImmutableArray<IAstTriviaNode>.Empty;
            TrailingTrivia = trailingTrivia?.ToImmutableArray() ?? ImmutableArray<IAstTriviaNode>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public abstract string CodeDisplay { get; }

        /// <summary>
        /// Gets an array of trivia that appear before this node in the source code.
        /// </summary>
        public ImmutableArray<IAstTriviaNode> LeadingTrivia { get; private set; }

        /// <summary>
        /// Gets an array of trivia that appear after this node in the source code.
        /// </summary>
        public ImmutableArray<IAstTriviaNode> TrailingTrivia { get; private set; }

        /// <summary>
        /// Gets a consise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {CodeDisplay}";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Accepts the visitor by calling into a specific method on the visitor for this type of AST node.
        /// </summary>
        /// <param name="visitor">The visitor to visit.</param>
        public abstract void Accept(TsVisitor visitor);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => CodeDisplay;

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public void Emit(Emitter emitter)
        {
            foreach (IAstTriviaNode trivia in LeadingTrivia)
            {
                trivia.Emit(emitter);
            }

            EmitInternal(emitter);

            foreach (IAstTriviaNode trivia in TrailingTrivia)
            {
                trivia.Emit(emitter);
            }
        }

        /// <summary>
        /// Creates a copy of this node with the specified leading trivia.
        /// </summary>
        public T WithLeadingTrivia<T>(params IAstTriviaNode[] triviaNodes) where T : AstNode
        {
            // when there are no trivia nodes to append, return the original object
            if (triviaNodes == null || triviaNodes.Length == 0)
            {
                return (T)this;
            }

            var copy = (AstNode)MemberwiseClone();
            copy.LeadingTrivia = triviaNodes.ToImmutableArray();
            return (T)copy;
        }

        /// <summary>
        /// Creates a copy of this node with the specified trailing trivia.
        /// </summary>
        public T WithTrailingTrivia<T>(params IAstTriviaNode[] triviaNodes) where T : AstNode
        {
            // when there are no trivia nodes to append, return the original object
            if (triviaNodes == null || triviaNodes.Length == 0)
            {
                return (T)this;
            }

            var copy = (AstNode)MemberwiseClone();
            copy.TrailingTrivia = triviaNodes.ToImmutableArray();
            return (T)copy;
        }

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        protected abstract void EmitInternal(Emitter emitter);
    }
}
