﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Contains options that control how to compile C# into TypeScript.
    /// </summary>
    public class CompilerOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly CompilerOptions Default = new CompilerOptions(instanceToCopy: null);

        private const WarningLevel DefaultWarningLevel = WarningLevel.Informational;
        private const ReportDiagnostic DefaultGeneralDiagnosticOption = ReportDiagnostic.Default;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public CompilerOptions(
            string outputPath = null,
            WarningLevel warningLevel = DefaultWarningLevel,
            ReportDiagnostic generalDiagnosticOption = DefaultGeneralDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = null,
            RenameRules renameRules = null,
            SymbolTableOverrides symbolTableOverrides = null)
            : this(
                instanceToCopy: null,
                outputPath: outputPath,
                warningLevel: warningLevel,
                generalDiagnosticOption: generalDiagnosticOption,
                specificDiagnosticOptions: specificDiagnosticOptions,
                renameRules: renameRules,
                symbolTableOverrides: symbolTableOverrides)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CompilerOptions"/> class. Values are set using the
        /// following precedence:
        /// * The named parameter, if specified
        /// * Otherwise, the value of <paramref name="instanceToCopy"/>, if specified
        /// * Otherwise, the default value of the parameter's type
        /// </summary>
        private CompilerOptions(
            CompilerOptions instanceToCopy = null,
            string outputPath = null,
            WarningLevel? warningLevel = null,
            ReportDiagnostic? generalDiagnosticOption = null,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = null,
            RenameRules renameRules = null,
            SymbolTableOverrides symbolTableOverrides = null)
        {
            OutputPath = outputPath ?? instanceToCopy?.OutputPath ?? string.Empty;
            WarningLevel = warningLevel ?? instanceToCopy?.WarningLevel ?? DefaultWarningLevel;
            GeneralDiagnosticOption = generalDiagnosticOption ??
                instanceToCopy?.GeneralDiagnosticOption ?? DefaultGeneralDiagnosticOption;

            SpecificDiagnosticOptions = specificDiagnosticOptions ??
                instanceToCopy?.SpecificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;

            RenameRules = renameRules ?? instanceToCopy?.RenameRules ?? RenameRules.Default;
            SymbolTableOverrides = symbolTableOverrides ??
                instanceToCopy?.SymbolTableOverrides ?? SymbolTableOverrides.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the directory where the compiled TypeScript files will be generated.
        /// </summary>
        public string OutputPath { get; }

        /// <summary>
        /// Gets the global warning level.
        /// </summary>
        [DefaultValue(DefaultWarningLevel)]
        public WarningLevel WarningLevel { get; }

        /// <summary>
        /// Global warning report option.
        /// </summary>
        [DefaultValue(DefaultGeneralDiagnosticOption)]
        public ReportDiagnostic GeneralDiagnosticOption { get; }

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        /// <summary>
        /// Gets the renaming rules to apply during TypeScript translation.
        /// </summary>
        public RenameRules RenameRules { get; }

        /// <summary>
        /// Gets an object containing overrides for various symbols, such as [InlineCode] or
        /// [ScriptName].
        /// </summary>
        public SymbolTableOverrides SymbolTableOverrides { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public CompilerOptions WithOutputPath(string value)
        {
            return string.Equals(OutputPath, value, StringComparison.Ordinal)
                ? this
                : new CompilerOptions(this, outputPath: value);
        }

        /// <summary>
        /// Deserializes the options from a JSON file.
        /// </summary>
        /// <param name="path">The file containing the serialized <see cref="CompilerOptions"/>.</param>
        /// <returns>The deserialized <see cref="CompilerOptions"/>.</returns>
        public static IExtendedResult<CompilerOptions> Deserialize(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    return Deserialize(stream, path);
                }
            }
            catch (Exception e) when (e is IOException || e is JsonException || e is ArgumentException)
            {
                var diagnostics = new[] { DiagnosticFactory.InvalidOptionsFile(path, e.Message) };
                return new ExtendedResult<CompilerOptions>(null, diagnostics);
            }
        }

        /// <summary>
        /// Deserializes the options from a JSON stream.
        /// </summary>
        /// <param name="stream">The stream containing the serialized <see cref="CompilerOptions"/>.</param>
        /// <param name="filePath">
        /// An optional file path if the stream was read from disk. This is used when reporting errors.
        /// </param>
        /// <returns>The deserialized <see cref="CompilerOptions"/>.</returns>
        public static IExtendedResult<CompilerOptions> Deserialize(Stream stream, string filePath = null)
        {
            try
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
                    var deserialized = serializer.Deserialize<CompilerOptions>(jsonReader);
                    return new ExtendedResult<CompilerOptions>(deserialized);
                }
            }
            catch (JsonException e)
            {
                filePath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
                var diagnostics = new[] { DiagnosticFactory.InvalidOptionsFile(filePath, e.Message) };
                return new ExtendedResult<CompilerOptions>(null, diagnostics);
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

        internal CSharpCompilationOptions ToCSharpCompilationOptions()
        {
            return new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                warningLevel: (int)WarningLevel,
                generalDiagnosticOption: GeneralDiagnosticOption,
                specificDiagnosticOptions: SpecificDiagnosticOptions);
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
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter(camelCaseText: true)
                    {
                        AllowIntegerValues = false
                    },
                    new SymbolTableOverridesConverter(),
                },
                DefaultValueHandling = DefaultValueHandling.Populate,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return JsonSerializer.Create(settings);
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class SymbolTableOverridesConverter : JsonConverter<SymbolTableOverrides>
        {
            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The calling serializer.</param>
            public override void WriteJson(JsonWriter writer, SymbolTableOverrides value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                writer.WriteStartObject();

                var orderedPairs = value.Overrides.OrderBy(pair => pair.Key, StringComparer.Ordinal);
                foreach (KeyValuePair<string, SymbolTableOverride> pair in orderedPairs)
                {
                    writer.WritePropertyName(pair.Key, escape: true);
                    serializer.Serialize(writer, pair.Value);
                }

                writer.WriteEndObject();
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">
            /// The existing value of object being read. If there is no existing value then
            /// <c>null</c> will be used.
            /// </param>
            /// <param name="hasExistingValue">The existing value has a value.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <returns>The object value.</returns>
            public override SymbolTableOverrides ReadJson(
                JsonReader reader,
                Type objectType,
                SymbolTableOverrides existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return null;
                }

                Read(reader, JsonToken.StartObject);

                var pairs = new List<KeyValuePair<string, SymbolTableOverride>>();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    VerifyToken(reader, JsonToken.PropertyName);

                    string symbolName = reader.Value.ToString();
                    reader.Read();

                    var symbolOverride = serializer.Deserialize<SymbolTableOverride>(reader);
                    pairs.Add(new KeyValuePair<string, SymbolTableOverride>(symbolName, symbolOverride));

                    // it is expected that we read the end object, even though the serializer will read the beginning
                    Read(reader, JsonToken.EndObject);
                }

                // don't read the ending object - the caller will do that
                // Read(reader, JsonToken.EndObject);

                return new SymbolTableOverrides(pairs.ToImmutableDictionary());
            }

            private static void Read(JsonReader reader, JsonToken expectedToken)
            {
                VerifyToken(reader, expectedToken);
                reader.Read();
            }

            private static void VerifyToken(JsonReader reader, JsonToken expectedToken)
            {
                if (reader.TokenType != expectedToken)
                {
                    throw new InvalidOperationException(
                        $"Invalid JSON token. Expecting '{expectedToken}' but found '{reader.TokenType}': " +
                        $"path = {reader.Path}");
                }
            }
        }
    }
}
