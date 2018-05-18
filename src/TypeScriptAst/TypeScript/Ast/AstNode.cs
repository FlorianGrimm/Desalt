// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) nodes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstNode : IAstNode, IEquatable<AstNode>
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
        //// Operator Overloads
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="AstNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(IAstNode left, AstNode right) => Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="AstNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(AstNode left, IAstNode right) => Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AstNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(IAstNode left, AstNode right) => !Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AstNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(AstNode left, IAstNode right) => !Equals(left, right);

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
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public virtual string EmitAsString(EmitOptions emitOptions = null)
        {
            using (var stream = new MemoryStream())
            using (var emitter = new Emitter(stream, options: emitOptions))
            {
                Emit(emitter);
                stream.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, emitter.Encoding))
                {
                    return reader.ReadToEnd();
                }
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj) => Equals(obj as AstNode);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        bool IEquatable<IAstNode>.Equals(IAstNode other) => Equals(other as AstNode);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public virtual bool Equals(AstNode other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (GetType() != other.GetType())
            {
                return false;
            }

            return EmitAsString().Equals(other.EmitAsString());
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode() => EmitAsString().GetHashCode();
    }
}
