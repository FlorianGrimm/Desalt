// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstTriviaNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) trivia node types. A trivia node is a
    /// comment or whitespace.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class TsAstTriviaNode : ITsAstTriviaNode, IEquatable<TsAstTriviaNode>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected TsAstTriviaNode(bool preserveSpacing)
        {
            PreserveSpacing = preserveSpacing;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether to preserve the leading and trailing spacing and not add spaces around
        /// the beginning and ending markers.
        /// </summary>
        public bool PreserveSpacing { get; }

        /// <summary>
        /// Gets a concise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {EmitAsString()}";

        /// <summary>
        /// Returns either a space or an empty string depending on the value of <see cref="PreserveSpacing"/>.
        /// </summary>
        protected string Space => PreserveSpacing ? string.Empty : " ";

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsAstTriviaNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(ITsAstTriviaNode left, TsAstTriviaNode right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsAstTriviaNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(TsAstTriviaNode left, ITsAstTriviaNode right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsAstTriviaNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(ITsAstTriviaNode left, TsAstTriviaNode right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsAstTriviaNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(TsAstTriviaNode left, ITsAstTriviaNode right)
        {
            return !Equals(left, right);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public abstract void Emit(Emitter emitter);

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
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as TsAstTriviaNode);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        bool IEquatable<ITsAstTriviaNode?>.Equals(ITsAstTriviaNode? other)
        {
            return Equals(other as TsAstTriviaNode);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool Equals(TsAstTriviaNode? other)
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
