// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TextReaderLocation.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Utility
{
    using System;
    using System.Globalization;

    public struct TextReaderLocation : IEquatable<TextReaderLocation>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TextReaderLocation(int line, int column, string source = null)
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

        public static bool operator ==(TextReaderLocation x, TextReaderLocation y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(TextReaderLocation x, TextReaderLocation y)
        {
            return !(x == y);
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

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", Source, Line, Column);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextReaderLocation))
            {
                return false;
            }

            return Equals((TextReaderLocation)obj);
        }

        public bool Equals(TextReaderLocation other)
        {
            return Source.Equals(other.Source, StringComparison.Ordinal) &&
                Line == other.Line &&
                Column == other.Column;
        }

        public override int GetHashCode()
        {
            return CombineHashCodes(Source.GetHashCode(), Line.GetHashCode(), Column.GetHashCode());
        }

        /// <summary>
        /// Combines multiple hash codes into one.
        /// </summary>
        /// <param name="hash1">The first hash code.</param>
        /// <param name="hash2">The second hash code.</param>
        /// <param name="hashes">More hash codes.</param>
        /// <returns>A combined hash code.</returns>
        /// <remarks>Taken from the .NET Framework code for Tuple and Array,
        /// both having a function with the same name as this.</remarks>
        private static int CombineHashCodes(int hash1, int hash2, params int[] hashes)
        {
            int combinedHash = ((hash1 << 5) + hash1) ^ hash2;
            foreach (int hash in hashes)
            {
                combinedHash = ((combinedHash << 5) + combinedHash) ^ hash;
            }

            return combinedHash;
        }
    }
}
