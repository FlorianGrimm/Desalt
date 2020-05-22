// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeListT.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Desalt.TypeScriptAst.Emit;
    using Factory = TsAstFactory;

    /// <summary>
    /// Represents an immutable list of <see cref="ITsAstNode"/> nodes separated with tokens.
    /// </summary>
    /// <typeparam name="T">The specific type of <see cref="ITsAstNode"/> nodes in the list.</typeparam>
    internal sealed class TsAstNodeList<T> : ITsAstNodeList<T>, IEquatable<TsAstNodeList<T>>
        where T : ITsAstNode
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents an empty list using a comma as the separator.
        /// </summary>
        public static readonly TsAstNodeList<T> Empty = new TsAstNodeList<T>(ImmutableArray<T>.Empty);

        // Don't mark the struct as readonly to avoid defensive copy behavior
        // (see https://devblogs.microsoft.com/premier-developer/the-in-modifier-and-the-readonly-structs-in-c/).
        private /*readonly*/ ImmutableArray<ITsNode> _nodesAndTokens;

        private readonly int _separatorCount;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal TsAstNodeList(ImmutableArray<T> nodes, string separator = TsAstNodeList.DefaultSeparator)
            : this(CreateList(nodes, separator))
        {
        }

        private TsAstNodeList(ImmutableArray<ITsNode> nodesAndTokens)
        {
            _nodesAndTokens = nodesAndTokens;

            Count = _nodesAndTokens.Length == 0 ? 0 : (_nodesAndTokens.Length / 2) + 1;
            _separatorCount = _nodesAndTokens.Length / 2;
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public T this[int index] => (T)_nodesAndTokens[index * 2];

        //// ===========================================================================================================
        //// Operators
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsAstNodeList{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(ITsAstNodeList<T> left, TsAstNodeList<T>? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsAstNodeList{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(TsAstNodeList<T> left, ITsAstNodeList<T>? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsAstNodeList{T}"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(TsAstNodeList<T>? left, TsAstNodeList<T>? right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsAstNodeList{T}"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(ITsAstNodeList<T>? left, TsAstNodeList<T>? right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsAstNodeList{T}"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(TsAstNodeList<T>? left, ITsAstNodeList<T>? right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TsAstNodeList{T}"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.</returns>
        public static bool operator !=(TsAstNodeList<T>? left, TsAstNodeList<T>? right)
        {
            return !Equals(left, right);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Returns the sequence of just the separator tokens.
        /// </summary>
        public IEnumerable<ITsTokenNode> Separators => _nodesAndTokens.OfType<ITsTokenNode>();

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _nodesAndTokens.OfType<T>().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the separator token at the specified index in this list. For example, if <paramref name="index"/> is
        /// <c>0</c> the first separator is returned (which is after the first <see cref="ITsAstNode"/>).
        /// </summary>
        /// <param name="index">The index within the list of the separator to return.</param>
        /// <returns>The token node representing the separator at the specified index.</returns>
        public ITsTokenNode GetSeparator(int index)
        {
            if (index < 0 || index >= _separatorCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (ITsTokenNode)_nodesAndTokens[(index * 2) + 1];
        }

        /// <summary>
        /// Creates a copy of this list with the separators replaced with the specified separators.
        /// </summary>
        /// <param name="separators">The separators to use in the list.</param>
        /// <returns>A copy of this list with the separators replaced with the specified separators.</returns>
        public ITsAstNodeList<T> WithSeparators(IEnumerable<ITsTokenNode> separators)
        {
            var separatorArray = separators.ToArray();

            if (separatorArray.Length != _separatorCount)
            {
                throw new ArgumentException(
                    "The separators need to have the same length as the existing list minus one.",
                    nameof(separators));
            }

            var newList = ImmutableArray.CreateBuilder<ITsNode>(_nodesAndTokens.Length);

            for (int i = 0; i < _nodesAndTokens.Length; i++)
            {
                newList.Add(_nodesAndTokens[i]);
                i++;

                if (i < _nodesAndTokens.Length - 1)
                {
                    newList.Add(separatorArray[i / 2]);
                }
            }

            return new TsAstNodeList<T>(newList.MoveToImmutable());
        }

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public void Emit(Emitter emitter)
        {
            foreach (ITsNode nodeOrToken in _nodesAndTokens)
            {
                nodeOrToken.Emit(emitter);
            }
        }

        /// <summary>
        /// Emits a node using a string stream. Useful for unit tests and debugging.
        /// </summary>
        /// <param name="emitOptions">The optional emit options.</param>
        /// <returns>The node emitted to a string stream.</returns>
        public string EmitAsString(EmitOptions? emitOptions = null)
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
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(TsAstNodeList<T>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _nodesAndTokens.Equals(other._nodesAndTokens);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(ITsAstNodeList<T>? other)
        {
            return Equals(other as TsAstNodeList<T>);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is TsAstNodeList<T> other && Equals(other));
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _nodesAndTokens.GetHashCode();
        }

        private static ImmutableArray<ITsNode> CreateList(ImmutableArray<T> nodes, string separator)
        {
            if (nodes.IsEmpty)
            {
                return nodes.CastArray<ITsNode>();
            }

            ITsTokenNode separatorToken = Factory.Token(separator);
            var builder = ImmutableArray.CreateBuilder<ITsNode>((nodes.Length * 2) - 1);

            for (int i = 0; i < nodes.Length; i++)
            {
                builder.Add(nodes[i]);

                if (i < nodes.Length - 1)
                {
                    builder.Add(separatorToken);
                }
            }

            return builder.MoveToImmutable();
        }
    }
}
