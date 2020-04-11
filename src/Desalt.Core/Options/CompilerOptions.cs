// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Options
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using Desalt.Core.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Contains options that control how to compile C# into TypeScript.
    /// </summary>
    public partial class CompilerOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly CompilerOptions Default = new CompilerOptions(instanceToCopy: null);

        public const WarningLevel DefaultWarningLevel = DiagnosticOptions.DefaultWarningLevel;
        public const ReportDiagnostic DefaultGeneralDiagnosticOption = DiagnosticOptions.DefaultGeneralDiagnosticOption;

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
            OutputPath = outputPath ?? instanceToCopy?.OutputPath;
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
        public string? OutputPath { get; }

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

        public CompilerOptions WithOutputPath(string? value)
        {
            return string.Equals(OutputPath, value, StringComparison.Ordinal)
                ? this
                : new CompilerOptions(this, outputPath: value);
        }

        public CompilerOptions WithSymbolTableOverrides(SymbolTableOverrides value)
        {
            return SymbolTableOverrides.Equals(value) ? this : new CompilerOptions(this, symbolTableOverrides: value);
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
                    new UserDefinedOperatorMethodNamesConverter(),
                },
                DefaultValueHandling = DefaultValueHandling.Populate,
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return JsonSerializer.Create(settings);
        }
    }
}
