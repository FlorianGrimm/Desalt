// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableOverrides.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
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
    public sealed class SymbolTableOverrides
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly SymbolTableOverrides Empty = new SymbolTableOverrides();

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableDictionary<string, SymbolTableOverride> Overrides { get; }

        public ImmutableDictionary<string, string> InlineCodeOverrides { get; }
        public ImmutableDictionary<string, string> ScriptNameOverrides { get; }

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
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.InlineCode))
                .ToImmutableDictionary();

            ScriptNameOverrides = Overrides
                .Where(pair => pair.Value.ScriptName != null)
                .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value.ScriptName))
                .ToImmutableDictionary();
        }
    }

    public sealed class SymbolTableOverride
    {
        public SymbolTableOverride(string inlineCode = null, string scriptName = null)
        {
            InlineCode = inlineCode;
            ScriptName = scriptName;
        }

        public string InlineCode { get; }

        public string ScriptName { get; }
    }
}
