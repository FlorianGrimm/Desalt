// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) nodes that can contain whitespace and be emitted ( <see
    /// cref="TsAstNode"/> and <see cref="TsTokenNode"/>. Implementations should be immutable.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    internal abstract class TsNode : ITsNode, IEquatable<ITsNode>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected TsNode(
            ImmutableArray<ITsAstTriviaNode>? leadingTrivia = null,
            ImmutableArray<ITsAstTriviaNode>? trailingTrivia = null)
        {
            LeadingTrivia = leadingTrivia ?? ImmutableArray<ITsAstTriviaNode>.Empty;
            TrailingTrivia = trailingTrivia ?? ImmutableArray<ITsAstTriviaNode>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets an array of trivia that appear before this node in the source code.
        /// </summary>
        public ImmutableArray<ITsAstTriviaNode> LeadingTrivia { get; private set; }

        /// <summary>
        /// Gets an array of trivia that appear after this node in the source code.
        /// </summary>
        public ImmutableArray<ITsAstTriviaNode> TrailingTrivia { get; private set; }

        /// <summary>
        /// Gets a concise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {EmitAsString()}";

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsNode"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(ITsNode left, TsNode right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsNode"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(TsNode left, ITsNode right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsNode"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(ITsNode left, TsNode right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsNode"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(TsNode left, ITsNode right)
        {
            return !Equals(left, right);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return EmitAsString();
        }

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public void Emit(Emitter emitter)
        {
            foreach (ITsAstTriviaNode trivia in LeadingTrivia)
            {
                trivia.Emit(emitter);
            }

            EmitContent(emitter);

            foreach (ITsAstTriviaNode trivia in TrailingTrivia)
            {
                trivia.Emit(emitter);
            }
        }

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public virtual string EmitAsString(EmitOptions? emitOptions = null)
        {
            using var stream = new MemoryStream();
            using var emitter = new Emitter(stream, options: emitOptions);
            Emit(emitter);
            stream.Flush();
            stream.Position = 0;

            using var reader = new StreamReader(stream, emitter.Encoding);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>. This will be called after the
        /// leading trivia has been emitted and before the trailing trivia.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        protected abstract void EmitContent(Emitter emitter);

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object.</param>
        public override bool Equals(object obj)
        {
            return Equals(obj as TsNode);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        bool IEquatable<ITsNode>.Equals(ITsNode other)
        {
            return Equals(other as TsNode);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public virtual bool Equals(TsNode? other)
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
        public override int GetHashCode()
        {
            return EmitAsString().GetHashCode();
        }
    }
}
