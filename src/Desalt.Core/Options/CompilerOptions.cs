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
    using Desalt.Core.Diagnostics;
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

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public CompilerOptions(
            string? outputPath = null,
            DiagnosticOptions? diagnosticOptions = null,
            RenameRules? renameRules = null,
            SymbolTableOverrides? symbolTableOverrides = null)
            : this(
                instanceToCopy: null,
                outputPath: outputPath,
                diagnosticOptions: diagnosticOptions,
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
            DiagnosticOptions? diagnosticOptions = null,
            RenameRules? renameRules = null,
            SymbolTableOverrides? symbolTableOverrides = null)
        {
            OutputPath = outputPath ?? instanceToCopy?.OutputPath;
            DiagnosticOptions = diagnosticOptions ?? instanceToCopy?.DiagnosticOptions ?? DiagnosticOptions.Default;
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
        public string? OutputPath { get; }

        /// <summary>
        /// Gets the options that relate to how errors and warnings are handled during a compile.
        /// </summary>
        public DiagnosticOptions DiagnosticOptions { get; }

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

        public CompilerOptions WithDiagnosticOptions(DiagnosticOptions value)
        {
            return DiagnosticOptions.Equals(value) ? this : new CompilerOptions(this, diagnosticOptions: value);
        }

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
