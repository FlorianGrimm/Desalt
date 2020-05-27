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
    using Desalt.TypeScriptAst.Emit;

    internal delegate TSubclass CreateSubclassFunc<T, out TSubclass>(ImmutableArray<ITsNode> nodesAndSeparators)
        where TSubclass : TsAstNodeList<T>
        where T : ITsAstNode;

    /// <summary>
    /// Represents an immutable list of <see cref="ITsAstNode"/> nodes separated with tokens.
    /// </summary>
    /// <typeparam name="T">The specific type of <see cref="ITsAstNode"/> nodes in the list.</typeparam>
    internal abstract class TsAstNodeList<T> : ITsAstNodeList<T>, IEquatable<TsAstNodeList<T>>
        where T : ITsAstNode
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        // Don't mark the struct as readonly to avoid defensive copy behavior
        // (see https://devblogs.microsoft.com/premier-developer/the-in-modifier-and-the-readonly-structs-in-c/).
        private /*readonly*/ ImmutableArray<ITsNode> _nodesAndSeparators;

        private readonly ITsTokenNode _separatorToken;
        private readonly int _separatorCount;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected TsAstNodeList(
            ImmutableArray<ITsNode> nodesAndSeparators,
            ITsTokenNode separatorToken,
            ITsTokenNode openToken,
            ITsTokenNode closeToken)
        {
            _nodesAndSeparators = nodesAndSeparators;
            _separatorToken = separatorToken;

            Count = _nodesAndSeparators.Length == 0 ? 0 : (_nodesAndSeparators.Length / 2) + 1;
            _separatorCount = _nodesAndSeparators.Length / 2;
            OpenToken = openToken;
            CloseToken = closeToken;
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public T this[int index] => (T)_nodesAndSeparators[index * 2];

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
        public IEnumerable<ITsTokenNode> Separators => _nodesAndSeparators.OfType<ITsTokenNode>();

        /// <summary>
        /// Returns a value indicating whether the list is empty.
        /// </summary>
        public bool IsEmpty => _nodesAndSeparators.IsEmpty;

        /// <summary>
        /// Returns the open token for the list, for example '(', '&lt;'.
        /// </summary>
        public ITsTokenNode OpenToken { get; }

        /// <summary>
        /// Returns the close token for the list, for example ')', '&gt;'.
        /// </summary>
        public ITsTokenNode CloseToken { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _nodesAndSeparators.OfType<T>().GetEnumerator();
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

            return _nodesAndSeparators.Equals(other._nodesAndSeparators);
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
            return _nodesAndSeparators.GetHashCode();
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

            return (ITsTokenNode)_nodesAndSeparators[(index * 2) + 1];
        }

        /// <summary>
        /// Emits this AST node into code using the specified <see cref="Emitter"/>.
        /// </summary>
        /// <param name="emitter">The emitter to use.</param>
        public void Emit(Emitter emitter)
        {
            OpenToken.Emit(emitter);

            foreach (ITsNode nodeOrToken in _nodesAndSeparators)
            {
                nodeOrToken.Emit(emitter);
            }

            CloseToken.Emit(emitter);
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
        /// Creates a copy of this list with the specified nodes added to the end.
        /// </summary>
        /// <param name="createSubclassFunc">The function to call to create an instance of the subclass.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes added to the end.</returns>
        protected TSubclass Add<TSubclass>(CreateSubclassFunc<T, TSubclass> createSubclassFunc, params T[] nodes)
            where TSubclass : TsAstNodeList<T>
        {
            return Insert(createSubclassFunc, _nodesAndSeparators.Length, nodes);
        }

        /// <summary>
        /// Creates a copy of this list with the specified nodes inserted at the specified index.
        /// </summary>
        /// <param name="createSubclassFunc">The function to call to create an instance of the subclass.</param>
        /// <param name="index">The index where the nodes should be inserted.</param>
        /// <param name="nodes">The nodes to add to the end.</param>
        /// <returns>A copy of this list with the specified nodes inserted at the specified index.</returns>
        protected TSubclass Insert<TSubclass>(
            CreateSubclassFunc<T, TSubclass> createSubclassFunc,
            int index,
            params T[] nodes)
            where TSubclass : TsAstNodeList<T>
        {
            if (nodes.Length == 0)
            {
                return (TSubclass)this;
            }

            var builder = _nodesAndSeparators.ToBuilder();
            for (int i = 0; i < nodes.Length; i++)
            {
                int insertionIndex = index + i;
                builder.Insert(insertionIndex, nodes[i]);

                if (i < nodes.Length - 1)
                {
                    builder.Insert(insertionIndex + 1, _separatorToken);
                }
            }

            return createSubclassFunc(builder.ToImmutable());
        }

        ///// <summary>
        ///// Creates a copy of this list with the separators replaced with the specified separators.
        ///// </summary>
        ///// <param name="separators">The separators to use in the list.</param>
        ///// <returns>A copy of this list with the separators replaced with the specified separators.</returns>
        //public ITsAstNodeList<T> WithSeparators(IEnumerable<ITsTokenNode> separators)
        //{
        //    var separatorArray = separators.ToArray();

        //    if (separatorArray.Length != _separatorCount)
        //    {
        //        throw new ArgumentException(
        //            "The separators need to have the same length as the existing list minus one.",
        //            nameof(separators));
        //    }

        //    var newList = ImmutableArray.CreateBuilder<ITsNode>(_nodesAndSeparators.Length);

        //    for (int i = 0; i < _nodesAndSeparators.Length; i++)
        //    {
        //        newList.Add(_nodesAndSeparators[i]);
        //        i++;

        //        if (i < _nodesAndSeparators.Length - 1)
        //        {
        //            newList.Add(separatorArray[i / 2]);
        //        }
        //    }

        //    return _createInstanceFunc(newList.MoveToImmutable());
        //}

        ///// <summary>
        ///// Creates a copy of this list with the open token replaced with the specified value.
        ///// </summary>
        ///// <param name="value">
        ///// The new open token to use. Note that this only applies to the trivia nodes. The code verifies that the same
        ///// token string is actually used.
        ///// </param>
        ///// <returns>A copy of this list with the open token replaced with the specified value.</returns>
        //public ITsAstNodeList<T> WithOpenToken(ITsTokenNode value)
        //{
        //    if (OpenToken == value)
        //    {
        //        return this;
        //    }

        //    if (value.Token != OpenToken.Token)
        //    {
        //        throw new ArgumentException(
        //            $"The new open token must have the same token string as the existing one: '{OpenToken.Token}'.");
        //    }

        //    return _createInstanceFunc(_nodesAndSeparators);
        //}

        ///// <summary>
        ///// Creates a copy of this list with the close token replaced with the specified value.
        ///// </summary>
        ///// <param name="value">
        ///// The new open token to use. Note that this only applies to the trivia nodes. The code verifies that the same
        ///// token string is actually used.
        ///// </param>
        ///// <returns>A copy of this list with the close token replaced with the specified value.</returns>
        //public ITsAstNodeList<T> WithCloseToken(ITsTokenNode value)
        //{
        //    if (CloseToken == value)
        //    {
        //        return this;
        //    }

        //    if (value.Token != CloseToken.Token)
        //    {
        //        throw new ArgumentException(
        //            $"The new close token must have the same token string as the existing one: '{CloseToken.Token}'.");
        //    }

        //    return _createInstanceFunc(_nodesAndSeparators);
        //}

        protected static ImmutableArray<ITsNode> CreateList(ImmutableArray<T> nodes, ITsTokenNode separatorToken)
        {
            if (nodes.IsEmpty)
            {
                return nodes.CastArray<ITsNode>();
            }

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
