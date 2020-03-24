// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerOptions.Builder.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    public partial class CompilerOptions
    {
        public Builder ToBuilder()
        {
            return new Builder(this);
        }

        public sealed class Builder
        {
            //// =======================================================================================================
            //// Constructors
            //// =======================================================================================================

            internal Builder(CompilerOptions copy)
            {
                OutputPath = copy.OutputPath;
                WarningLevel = copy.WarningLevel;
                GeneralDiagnosticOption = copy.GeneralDiagnosticOption;
                SpecificDiagnosticOptions = new Dictionary<string, ReportDiagnostic>(copy.SpecificDiagnosticOptions);
                RenameRules = copy.RenameRules;
                SymbolTableOverrides = copy.SymbolTableOverrides;
            }

            //// =======================================================================================================
            //// Properties
            //// =======================================================================================================

            /// <summary>
            /// Gets the directory where the compiled TypeScript files will be generated.
            /// </summary>
            public string OutputPath { get; set; }

            /// <summary>
            /// Gets the global warning level.
            /// </summary>
            public WarningLevel WarningLevel { get; set; }

            /// <summary>
            /// Global warning report option.
            /// </summary>
            public ReportDiagnostic GeneralDiagnosticOption { get; set; }

            /// <summary>
            /// Warning report option for each warning.
            /// </summary>
            public IDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; set; }

            /// <summary>
            /// Gets the renaming rules to apply during TypeScript translation.
            /// </summary>
            public RenameRules RenameRules { get; set; }

            /// <summary>
            /// Gets an object containing overrides for various symbols, such as [InlineCode] or
            /// [ScriptName].
            /// </summary>
            public SymbolTableOverrides SymbolTableOverrides { get; set; }

            //// =======================================================================================================
            //// Methods
            //// =======================================================================================================

            public CompilerOptions ToImmutable()
            {
                return new CompilerOptions(
                    outputPath: OutputPath,
                    warningLevel: WarningLevel,
                    generalDiagnosticOption: GeneralDiagnosticOption,
                    specificDiagnosticOptions: SpecificDiagnosticOptions.ToImmutableDictionary(),
                    renameRules: RenameRules,
                    symbolTableOverrides: SymbolTableOverrides);
            }
        }
    }
}
