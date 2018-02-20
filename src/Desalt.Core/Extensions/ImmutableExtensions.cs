// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImmutableExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Extensions
{
    using System.Collections.Immutable;
    using System.Threading;

    /// <summary>
    /// Contains extension methods for working with ImmutableCollections objects.
    /// </summary>
    internal static class ImmutableExtensions
    {
        /// <summary>
        /// Adds an item to the immutable array and replaces the source with the new array. Uses <see
        /// cref="ImmutableInterlocked.InterlockedCompareExchange{T}"/> under the covers and
        /// continues to retry using a lightweight <see cref="SpinWait"/> until it succeeds.
        /// </summary>
        /// <typeparam name="T">The type of element in the array.</typeparam>
        /// <param name="source">A reference to the source array.</param>
        /// <param name="item">The item to add to the array.</param>
        public static void InterlockedAdd<T>(ref ImmutableArray<T> source, T item)
        {
            if (ImmutableInterlocked.InterlockedCompareExchange(ref source, source.Add(item), source) != source)
            {
                // if we fail, go into a spin wait and keep trying until we succeed
                var spinner = new SpinWait();

                do
                {
                    spinner.SpinOnce();
                }
                while (ImmutableInterlocked.InterlockedCompareExchange(ref source, source.Add(item), source) != source);
            }
        }
    }
}
