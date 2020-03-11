// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TextReaderLocation.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities
{
    using System;
    using System.Globalization;

    public struct TextReaderLocation : IEquatable<TextReaderLocation>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TextReaderLocation Empty = new TextReaderLocation();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TextReaderLocation(int line, int column, string? source = null)
            : this()
        {
            Line = line;
            Column = column;
            Source = source ?? string.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Source { get; }

        public int Line { get; }

        public int Column { get; }

        //// ===========================================================================================================
        //// Operators
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TextReaderLocation"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the
        /// same value; otherwise, false.
        /// </returns>
        public static bool operator ==(TextReaderLocation left, TextReaderLocation right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="TextReaderLocation"/> objects have
        /// different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(TextReaderLocation left, TextReaderLocation right)
        {
            return !left.Equals(right);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public TextReaderLocation IncrementLine()
        {
            return new TextReaderLocation(Line + 1, 1, Source);
        }

        public TextReaderLocation IncrementColumn()
        {
            return new TextReaderLocation(Line, Column + 1, Source);
        }

        public TextReaderLocation DecrementColumn()
        {
            return Column == 1
                ? throw new InvalidOperationException("Cannot decrement the column before the beginning of the line.")
                : new TextReaderLocation(Line, Column - 1, Source);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", Source, Line, Column);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(TextReaderLocation other)
        {
            return string.Equals(Source, other.Source) && Line == other.Line && Column == other.Column;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and
        /// represent the same value; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is TextReaderLocation location && Equals(location);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Source != null ? Source.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Column;
                return hashCode;
            }
        }
    }
}
