// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Contains utility methods for working with lists, collections, and other enumerables.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an item into an sequence of one item.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToSingleEnumerable<T>(this T item)
        {
            if (item is IEnumerable<T> enumerable)
            {
                foreach (T element in enumerable)
                {
                    yield return element;
                }
            }
            else
            {
                yield return item;
            }
        }

        /// <summary>
        /// Converts an item to an array if it's not already.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="item">The item to convert to an array.</param>
        /// <returns>
        /// If <paramref name="item"/> is already an array it is returned. Otherwise, a new array is
        /// created with <paramref name="item"/> as the only element. If <paramref name="item"/> is
        /// null, and empty array is returned.
        /// </returns>
        public static T[] ToSafeArray<T>(this T item)
        {
            if (item is null)
            {
                return Array.Empty<T>();
            }

            var enumerable = item as IEnumerable<T>;
            return enumerable?.ToArray() ?? new[] { item };
        }

        /// <summary>
        /// Converts an item to an array if it's not already.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="item">The item to convert to an array.</param>
        /// <returns>
        /// If <paramref name="item"/> is already an array it is returned. Otherwise, a new array is
        /// created with <paramref name="item"/> as the only element. If <paramref name="item"/> is
        /// null, and empty array is returned.
        /// </returns>
        public static T[] ToSafeArray<T>(this IEnumerable<T> item)
        {
            return item as T[] ?? item?.ToArray() ?? Array.Empty<T>();
        }

        /// <summary>
        /// Converts the specified dictionary to a read only dictionary. If the specified dictionary
        /// already implements <see cref="IReadOnlyDictionary{TKey,TValue}"/> it is returned.
        /// </summary>
        /// <typeparam name="TKey">The type of the key for the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the value for the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to wrap as a read-only instance.</param>
        /// <returns>A read-only dictionary that wraps the specified dictionary.</returns>
        public static IReadOnlyDictionary<TKey, TValue> ToReadOnly<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
        {
            var readOnly = dictionary as IReadOnlyDictionary<TKey, TValue>;
            return readOnly ?? new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Adds a range of values to the end of the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection is List<T> list)
            {
                list.AddRange(items);
            }
            else
            {
                foreach (T item in items)
                {
                    collection.Add(item);
                }
            }
        }

        /// <summary>
        /// Returns the specified sequence with nulls filtered out.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="sequence">The sequence to filter.</param>
        /// <returns>The same sequence as the original with only non-null values.</returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> sequence) where T : class
        {
            return sequence.Where(x => !(x is null)).Select(x => x!);
        }

        /// <summary>
        /// Returns the specified sequence of <see cref="KeyValuePair{TKey, TPair}"/> pairs with null values filtered out.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the sequence.</typeparam>
        /// <typeparam name="TValue">The type of the values in the sequence.</typeparam>
        /// <param name="sequence">The sequence to filter.</param>
        /// <returns>The same sequence as the original with only non-null values.</returns>
        public static IEnumerable<KeyValuePair<TKey, TValue>> WhereValueNotNull<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue?>> sequence)
            where TValue : class
        {
            return sequence.Where(pair => pair.Value != null)
                .Select(pair => new KeyValuePair<TKey, TValue>(pair.Key, pair.Value!));
        }
    }
}
