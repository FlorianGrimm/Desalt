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
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public CompilerOptions(
            string outputPath,
            WarningLevel warningLevel = WarningLevel.Informational,
            ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default,
            ImmutableDictionary<string, ReportDiagnostic> specificDiagnosticOptions = null)
        {
            OutputPath = outputPath;
            WarningLevel = warningLevel;
            GeneralDiagnosticOption = generalDiagnosticOption;
            SpecificDiagnosticOptions =
                specificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;
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
