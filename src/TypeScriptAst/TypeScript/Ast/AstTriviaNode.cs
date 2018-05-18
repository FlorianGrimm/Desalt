// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AstTriviaNode.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Abstract base class for all abstract syntax tree (AST) trivia node types. A trivia node is a
    /// comment or whitespace.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class AstTriviaNode : IAstTriviaNode, IEquatable<AstTriviaNode>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected AstTriviaNode(bool preserveSpacing)
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
        /// Returns an abbreviated string representation of the AST node, which is useful for debugging.
        /// </summary>
        /// <value>A string representation of this AST node.</value>
        public abstract string CodeDisplay { get; }

        /// <summary>
        /// Gets a consise string representing the current AST node to show in the debugger
        /// variable window.
        /// </summary>
        protected virtual string DebuggerDisplay => $"{GetType().Name}: {CodeDisplay}";

        /// <summary>
        /// Returns either a space or an empty string depending on the value of <see cref="PreserveSpacing"/>.
        /// </summary>
        protected string Space => PreserveSpacing ? string.Empty : " ";

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="AstTriviaNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(IAstTriviaNode left, AstTriviaNode right) => Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="AstTriviaNode"/>
        /// objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(AstTriviaNode left, IAstTriviaNode right) => Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AstTriviaNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(IAstTriviaNode left, AstTriviaNode right) => !Equals(left, right);

        /// <summary>
        /// Returns a value that indicates whether two <see cref="AstTriviaNode"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(AstTriviaNode left, IAstTriviaNode right) => !Equals(left, right);

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
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj) => Equals(obj as AstTriviaNode);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        bool IEquatable<IAstTriviaNode>.Equals(IAstTriviaNode other) => Equals(other as AstTriviaNode);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool Equals(AstTriviaNode other)
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
