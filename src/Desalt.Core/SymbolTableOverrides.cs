// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableOverrides.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Contains overrides to the internal symbol tables for specific symbols.
    /// </summary>
    /// <remarks>
    /// This allows a user to specify exceptions for the Saltarelle assemblies without having to
    /// change the source code. Serializing and deserializing to a JSON file is supported to allow
    /// specifying a file name on the command line.
    /// </remarks>
    public sealed class SymbolTableOverrides : IReadOnlyDictionary<string, SymbolTableOverride>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly SymbolTableOverrides Empty = new SymbolTableOverrides();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public SymbolTableOverrides(params KeyValuePair<string, SymbolTableOverride>[] overrides)
            : this(overrides.ToImmutableDictionary())
        {
        }

        public SymbolTableOverrides(ImmutableDictionary<string, SymbolTableOverride> overrides)
        {
            Overrides = overrides ?? ImmutableDictionary<string, SymbolTableOverride>.Empty;

            InlineCodeOverrides = Overrides
                .Where(pair => pair.Value.InlineCode != null)
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.InlineCode!))
                .ToImmutableDictionary();

            ScriptNameOverrides = Overrides
                .Where(pair => pair.Value.ScriptName != null)
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ScriptName!))
                .ToImmutableDictionary();
        }

        //// ===========================================================================================================
        //// Indexers
        //// ===========================================================================================================

        public SymbolTableOverride this[string key] => Overrides[key];

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableDictionary<string, SymbolTableOverride> Overrides { get; }

        public ImmutableDictionary<string, string> InlineCodeOverrides { get; }
        public ImmutableDictionary<string, string> ScriptNameOverrides { get; }

        public int Count => Overrides.Count;

        public IEnumerable<string> Keys => Overrides.Keys;
        public IEnumerable<SymbolTableOverride> Values => Overrides.Values;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public IEnumerator<KeyValuePair<string, SymbolTableOverride>> GetEnumerator()
        {
            return Overrides.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string key)
        {
            return Overrides.ContainsKey(key);
        }

        public bool TryGetValue(string key, out SymbolTableOverride value)
        {
            return Overrides.TryGetValue(key, out value);
        }
    }

    /// <summary>
    /// Contains information about a symbol table override, which will be used when translating a symbol.
    /// </summary>
    public sealed class SymbolTableOverride : IEquatable<SymbolTableOverride>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public SymbolTableOverride(string? inlineCode = null, string? scriptName = null)
        {
            InlineCode = inlineCode;
            ScriptName = scriptName;
        }

        //// ===========================================================================================================
        //// Operators
        //// ===========================================================================================================

        public static bool operator ==(SymbolTableOverride? left, SymbolTableOverride? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SymbolTableOverride? left, SymbolTableOverride? right)
        {
            return !Equals(left, right);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string? InlineCode { get; }

        public string? ScriptName { get; }

        public SymbolTableOverride WithInlineCode(string? value)
        {
            return InlineCode == value ? this : new SymbolTableOverride(value, ScriptName);
        }

        public SymbolTableOverride WithScriptName(string? value)
        {
            return ScriptName == value ? this : new SymbolTableOverride(InlineCode, value);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public bool Equals(SymbolTableOverride? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return InlineCode == other.InlineCode && ScriptName == other.ScriptName;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is SymbolTableOverride other && Equals(other));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InlineCode, ScriptName);
        }
    }
}
