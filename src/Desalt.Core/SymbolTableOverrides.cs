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
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Contains overrides to the internal symbol tables for specific symbols.
    /// </summary>
    /// <remarks>
    /// This allows a user to specify exceptions for the Saltarelle assemblies without having to
    /// change the source code. Serializing and deserializing to a JSON file is supported to allow
    /// specifying a file name on the command line.
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class SymbolTableOverrides
    {
        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        [JsonProperty]
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

        [JsonConstructor]
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

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static SymbolTableOverrides Deserialize(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return Deserialize(stream);
            }
        }

        public static SymbolTableOverrides Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: true))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = CreateSerializer();
                var deserialized = serializer.Deserialize<SymbolTableOverrides>(jsonReader);
                return deserialized;
            }
        }

        public void Serialize(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Serialize(stream);
            }
        }

        public void Serialize(Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                var serializer = CreateSerializer();
                serializer.Serialize(jsonWriter, this);
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            var settings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return JsonSerializer.Create(settings);
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
