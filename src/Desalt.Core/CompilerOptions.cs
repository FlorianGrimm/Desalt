// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

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
        /// Global warning report option.
        /// </summary>
        public ReportDiagnostic GeneralDiagnosticOption { get; }

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        /// <summary>
        /// Gets the directory where the compiled TypeScript files will be generated.
        /// </summary>
        public string OutputPath { get; }

        /// <summary>
        /// Gets the global warning level (from 0 to 4).
        /// </summary>
        public WarningLevel WarningLevel { get; }

        /// <summary>
        /// Gets the renaming rules to apply during TypeScript translation.
        /// </summary>
        public RenameRules RenameRules { get; }

        /// <summary>
        /// Gets the path to a .json file containing overrides for various symbols, such as
        /// [InlineCode] or [ScriptName] overrides.
        /// </summary>
        public string SymbolTableOverridesFilePath { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        internal CSharpCompilationOptions ToCSharpCompilationOptions()
        {
            return new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                warningLevel: (int)WarningLevel,
                generalDiagnosticOption: GeneralDiagnosticOption,
                specificDiagnosticOptions: SpecificDiagnosticOptions);
        }
    }
}
