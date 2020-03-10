// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Contains useful utility extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns a value indicating whether the specified flags are set in the value.
        /// </summary>
        /// <typeparam name="T">The Enum type.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="bits">The flags to check.</param>
        /// <returns>true if <paramref name="bits"/> are set in
        /// <paramref name="value"/>; otherwise, false.</returns>
        public static bool AreBitsSet<T>(this Enum value, T bits) where T : struct
        {
            int valueInt = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            int bitsInt = Convert.ToInt32(bits, CultureInfo.InvariantCulture);

            return (valueInt & bitsInt) == bitsInt;
        }

        /// <summary>
        /// Returns a value indicating whether the specified value is one of the
        /// values in <paramref name="options"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="options">The set of values to test against.</param>
        /// <returns>true if <paramref name="value"/> is one of the values
        /// in <paramref name="options"/>; false otherwise.</returns>
        public static bool IsOneOf<T>(this T value, params T[] options)
        {
            return IsOneOf(value, EqualityComparer<T>.Default, options);
        }

        /// <summary>
        /// Returns a value indicating whether the specified value is one of the
        /// values in <paramref name="options"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="comparer">The equality comparer to use.</param>
        /// <param name="options">The set of values to test against.</param>
        /// <returns>true if <paramref name="value"/> is one of the values
        /// in <paramref name="options"/>; false otherwise.</returns>
        public static bool IsOneOf<T>(this T value, IEqualityComparer<T> comparer, params T[] options)
        {
            return options.Contains(value, comparer);
        }

        /// <summary>
        /// Returns a value indicating whether the specified value is one of the
        /// values in <paramref name="options"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="options">The set of values to test against.</param>
        /// <returns>true if <paramref name="value"/> is one of the values
        /// in <paramref name="options"/>; false otherwise.</returns>
        public static bool IsOneOf<T>(this T value, IEnumerable<T> options)
        {
            return IsOneOf(value, EqualityComparer<T>.Default, options);
        }

        /// <summary>
        /// Returns a value indicating whether the specified value is one of the
        /// values in <paramref name="options"/>.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="value">The value to test.</param>
        /// <param name="comparer">The equality comparer to use.</param>
        /// <param name="options">The set of values to test against.</param>
        /// <returns>true if <paramref name="value"/> is one of the values
        /// in <paramref name="options"/>; false otherwise.</returns>
        public static bool IsOneOf<T>(this T value, IEqualityComparer<T> comparer, IEnumerable<T> options)
        {
            return options.Contains(value, comparer);
        }
    }
}
