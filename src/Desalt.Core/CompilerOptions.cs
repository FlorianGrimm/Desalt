// ---------------------------------------------------------------------------------------------------------------------
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

        private const WarningLevel DefaultWarningLevel = DiagnosticOptions.DefaultWarningLevel;

        private const ReportDiagnostic DefaultGeneralDiagnosticOption =
            DiagnosticOptions.DefaultGeneralDiagnosticOption;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public CompilerOptions(
            string? outputPath = null,
            WarningLevel warningLevel = DefaultWarningLevel,
            ReportDiagnostic generalDiagnosticOption = DefaultGeneralDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions = null,
            RenameRules? renameRules = null,
            SymbolTableOverrides? symbolTableOverrides = null)
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
            CompilerOptions? instanceToCopy = null,
            string? outputPath = null,
            WarningLevel? warningLevel = null,
            ReportDiagnostic? generalDiagnosticOption = null,
            ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions = null,
            RenameRules? renameRules = null,
            SymbolTableOverrides? symbolTableOverrides = null)
        {
            OutputPath = outputPath ?? instanceToCopy?.OutputPath ?? string.Empty;
            RenameRules = renameRules ?? instanceToCopy?.RenameRules ?? RenameRules.Default;
            SymbolTableOverrides = symbolTableOverrides ??
                instanceToCopy?.SymbolTableOverrides ?? SymbolTableOverrides.Empty;

            // initialize the diagnostic options
            WarningLevel warningLevelToUse = warningLevel ?? instanceToCopy?.WarningLevel ?? DefaultWarningLevel;
            ReportDiagnostic generalDiagnosticOptionToUse = generalDiagnosticOption ??
                instanceToCopy?.GeneralDiagnosticOption ?? DefaultGeneralDiagnosticOption;
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptionsToUse = specificDiagnosticOptions ??
                instanceToCopy?.SpecificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;

            DiagnosticOptions = new DiagnosticOptions(
                warningLevelToUse,
                generalDiagnosticOptionToUse,
                specificDiagnosticOptionsToUse);
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
        public WarningLevel WarningLevel => DiagnosticOptions.WarningLevel;

        /// <summary>
        /// Global warning report option.
        /// </summary>
        [DefaultValue(DefaultGeneralDiagnosticOption)]
        public ReportDiagnostic GeneralDiagnosticOption => DiagnosticOptions.GeneralDiagnosticOption;

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions =>
            DiagnosticOptions.SpecificDiagnosticOptions;

        /// <summary>
        /// Gets the renaming rules to apply during TypeScript translation.
        /// </summary>
        public RenameRules RenameRules { get; }

        /// <summary>
        /// Gets an object containing overrides for various symbols, such as [InlineCode] or
        /// [ScriptName].
        /// </summary>
        public SymbolTableOverrides SymbolTableOverrides { get; }

        internal DiagnosticOptions DiagnosticOptions { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public CompilerOptions WithRenameRules(RenameRules value)
        {
            return RenameRules.Equals(value) ? this : new CompilerOptions(this, renameRules: value);
        }

        public CompilerOptions WithOutputPath(string value)
        {
            return string.Equals(OutputPath, value, StringComparison.Ordinal)
                ? this
                : new CompilerOptions(this, outputPath: value);
        }

        public CompilerOptions WithSymbolTableOverrides(SymbolTableOverrides value)
        {
            return SymbolTableOverrides.Equals(value) ? this : new CompilerOptions(this, symbolTableOverrides: value);
        }

        /// <summary>
        /// Deserializes the options from a JSON file.
        /// </summary>
        /// <param name="path">The file containing the serialized <see cref="CompilerOptions"/>.</param>
        /// <returns>The deserialized <see cref="CompilerOptions"/>.</returns>
        public static IExtendedResult<CompilerOptions?> Deserialize(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open);
                return Deserialize(stream, path);
            }
            catch (Exception e) when (e is IOException || e is JsonException || e is ArgumentException)
            {
                Diagnostic[] diagnostics = { DiagnosticFactory.InvalidOptionsFile(path, e.Message) };
                return new ExtendedResult<CompilerOptions?>(null, diagnostics);
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
        public static IExtendedResult<CompilerOptions?> Deserialize(Stream stream, string? filePath = null)
        {
            CompilerOptions? deserialized = null;
            JsonException? exception = null;

            try
            {
                using var reader = new StreamReader(
                    stream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true);
                using var jsonReader = new JsonTextReader(reader);
                JsonSerializer serializer = CreateSerializer();
                deserialized = serializer.Deserialize<CompilerOptions>(jsonReader);
            }
            catch (JsonException e)
            {
                exception = e;
            }

            if (deserialized == null || exception != null)
            {
                filePath = string.IsNullOrWhiteSpace(filePath) ? string.Empty : Path.GetFullPath(filePath);
                Diagnostic[] diagnostics =
                {
                    DiagnosticFactory.InvalidOptionsFile(filePath, exception?.Message ?? string.Empty)
                };
                return new ExtendedResult<CompilerOptions?>(null, diagnostics);
            }

            return new ExtendedResult<CompilerOptions?>(deserialized);
        }

        public void Serialize(string path)
        {
            using var stream = new FileStream(path, FileMode.Create);
            Serialize(stream);
        }

        public void Serialize(Stream stream)
        {
            using var writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 4096, leaveOpen: true);
            using var jsonWriter = new JsonTextWriter(writer);
            JsonSerializer serializer = CreateSerializer();
            serializer.Serialize(jsonWriter, this);
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
                    new StringEnumConverter(new CamelCaseNamingStrategy())
                    {
                        AllowIntegerValues = false
                    },
                    new SpecificDiagnosticOptionsConverter(),
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

        private sealed class SpecificDiagnosticOptionsConverter
            : JsonConverter<ImmutableDictionary<string, ReportDiagnostic>>
        {
            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The calling serializer.</param>
            public override void WriteJson(
                JsonWriter writer,
                ImmutableDictionary<string, ReportDiagnostic> value,
                JsonSerializer serializer)
            {
                writer.WriteStartObject();

                var orderedPairs = value.OrderBy(pair => pair.Key, StringComparer.Ordinal);
                foreach (var pair in orderedPairs)
                {
                    writer.WritePropertyName(pair.Key, escape: true);
                    serializer.Serialize(writer, pair.Value);
                }

                writer.WriteEndObject();
            }

            /// <summary>
            /// Reads the JSON representation of the object.
            /// </summary>
            /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">
            /// The existing value of object being read. If there is no existing value then <c>null</c> will be used.
            /// </param>
            /// <param name="hasExistingValue">The existing value has a value.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <returns>The object value.</returns>
            public override ImmutableDictionary<string, ReportDiagnostic> ReadJson(
                JsonReader reader,
                Type objectType,
                ImmutableDictionary<string, ReportDiagnostic> existingValue,
                bool hasExistingValue,
                JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null)
                {
                    return ImmutableDictionary<string, ReportDiagnostic>.Empty;
                }

                reader.Read(JsonToken.StartObject);

                var dictionary = ImmutableDictionary.CreateBuilder<string, ReportDiagnostic>();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    reader.VerifyToken(JsonToken.PropertyName);

                    string key = reader.Value!.ToString();
                    reader.Read();

                    var value = serializer.Deserialize<ReportDiagnostic>(reader);
                    reader.Read();

                    dictionary.Add(key, value);
                }

                // don't read the ending object - the caller will do that
                // Read(reader, JsonToken.EndObject);

                return dictionary.ToImmutable();
            }
        }

        private sealed class SymbolTableOverridesConverter : JsonConverter<SymbolTableOverrides>
        {
            /// <summary>
            /// Writes the JSON representation of the object.
            /// </summary>
            /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
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
            /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
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
                    return new SymbolTableOverrides();
                }

                reader.Read(JsonToken.StartObject);

                var dictionary = ImmutableDictionary.CreateBuilder<string, SymbolTableOverride>();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    reader.VerifyToken(JsonToken.PropertyName);

                    string symbolName = reader.Value!.ToString();
                    reader.Read();

                    var symbolOverride = serializer.Deserialize<SymbolTableOverride>(reader);
                    if (symbolOverride != null)
                    {
                        dictionary.Add(symbolName, symbolOverride);
                    }

                    // it is expected that we read the end object, even though the serializer will read the beginning
                    reader.Read(JsonToken.EndObject);
                }

                // don't read the ending object - the caller will do that
                // Read(reader, JsonToken.EndObject);

                return new SymbolTableOverrides(dictionary.ToImmutable());
            }
        }
    }

    internal static class JsonReaderExtensions
    {
        public static void Read(this JsonReader reader, JsonToken expectedToken)
        {
            VerifyToken(reader, expectedToken);
            reader.Read();
        }

        public static void VerifyToken(this JsonReader reader, JsonToken expectedToken)
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
