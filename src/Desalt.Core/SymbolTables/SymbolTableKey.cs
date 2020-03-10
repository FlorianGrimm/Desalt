// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableKey.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.SymbolTables
{
    using System;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a key into the <see cref="ScriptSymbolTable"/>.
    /// </summary>
    internal sealed class SymbolTableKey : IEquatable<SymbolTableKey>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private SymbolTableKey(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Empty string", nameof(key));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SymbolTableKey"/> class from the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol from which to extract a key.</param>
        /// <returns>A key, appropriate for a symbol table.</returns>
        public static SymbolTableKey Create(ISymbol symbol)
        {
            return new SymbolTableKey(symbol.ToHashDisplay() ?? throw new ArgumentNullException(nameof(symbol)));
        }

        public static SymbolTableKey UnsafeCreate(string key)
        {
            return new SymbolTableKey(key);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Key { get; }

        //// ===========================================================================================================
        //// Operator Overloads
        //// ===========================================================================================================

        public static implicit operator string(SymbolTableKey value)
        {
            return value.Key;
        }

        public static bool operator ==(SymbolTableKey left, SymbolTableKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SymbolTableKey left, SymbolTableKey right)
        {
            return !Equals(left, right);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public bool Equals(SymbolTableKey other)
        {
            return !(other is null) && (ReferenceEquals(this, other) || string.Equals(Key, other.Key));
        }

        public override bool Equals(object obj)
        {
            return !(obj is null) && (ReferenceEquals(this, obj) || (obj is SymbolTableKey other && Equals(other)));
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return Key;
        }
    }
}
