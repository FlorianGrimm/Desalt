// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    /// <summary>
    /// A set of initialization methods for instances of <see cref="TsAstNodeList{T}"/>.
    /// </summary>
    internal static class TsAstNodeList
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Represents the separator used between elements when it isn't explicitly specified.
        /// </summary>
        public const string DefaultSeparator = ", ";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates an empty <see cref="TsAstNodeList{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <param name="separator">The separator to use between elements.</param>
        /// <returns>An empty <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> Create<T>(string separator = DefaultSeparator)
            where T : ITsAstNode
        {
            return separator == DefaultSeparator
                ? TsAstNodeList<T>.Empty
                : new TsAstNodeList<T>(ImmutableArray<T>.Empty, separator);
        }

        /// <summary>
        /// Creates an <see cref="TsAstNodeList{T}"/> with the specified element as its only member.
        /// </summary>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <param name="item">The element to store in the array.</param>
        /// <param name="separator">The separator to use between elements.</param>
        /// <returns>An immutable <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> Create<T>(T item, string separator = DefaultSeparator)
            where T : ITsAstNode
        {
            var array = ImmutableArray.Create(item);
            return new TsAstNodeList<T>(array, separator);
        }

        /// <summary>
        /// Creates an <see cref="TsAstNodeList{T}"/> with the specified elements and using <see
        /// cref="DefaultSeparator"/> as the separator.
        /// </summary>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <param name="items">The elements to store in the array.</param>
        /// <returns>An immutable <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> Create<T>(params T[] items)
            where T : ITsAstNode
        {
            return items.Length == 0 ? TsAstNodeList<T>.Empty : new TsAstNodeList<T>(items.ToImmutableArray());
        }

        /// <summary>
        /// Creates an <see cref="TsAstNodeList{T}"/> with the specified elements and using <see
        /// cref="DefaultSeparator"/> as the separator.
        /// </summary>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <param name="separator">The separator to use between elements.</param>
        /// <param name="items">The elements to store in the array.</param>
        /// <returns>An immutable <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> Create<T>(string separator, params T[] items)
            where T : ITsAstNode
        {
            return separator == DefaultSeparator && items.Length == 0
                ? TsAstNodeList<T>.Empty
                : new TsAstNodeList<T>(items.ToImmutableArray(), separator);
        }

        /// <summary>
        /// Creates an <see cref="TsAstNodeList{T}"/> populated with the contents of the specified sequence.
        /// </summary>
        /// <typeparam name="T">The type of element stored in the array.</typeparam>
        /// <param name="items">The elements to store in the array.</param>
        /// <param name="separator">The separator to use between elements.</param>
        /// <returns>An immutable <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> CreateRange<T>(IEnumerable<T> items, string separator = DefaultSeparator)
            where T : ITsAstNode
        {
            return new TsAstNodeList<T>(items.ToImmutableArray(), separator);
        }

        /// <summary>
        /// Enumerates a sequence exactly once and produces an immutable <see cref="TsAstNodeList{T}"/> of its contents.
        /// </summary>
        /// <typeparam name="T">The type of element in the sequence.</typeparam>
        /// <param name="items">The sequence to enumerate.</param>
        /// <param name="separator">The separator to use between elements.</param>
        /// <returns>An immutable <see cref="TsAstNodeList{T}"/>.</returns>
        public static TsAstNodeList<T> ToNodeList<T>(this IEnumerable<T> items, string separator = DefaultSeparator)
            where T : ITsAstNode
        {
            return items is TsAstNodeList<T> list ? list : CreateRange(items, separator);
        }
    }
}
