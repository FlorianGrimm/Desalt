// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains options that relate to how errors and warnings are handled during a compile.
    /// </summary>
    internal sealed class DiagnosticOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly DiagnosticOptions Default = new DiagnosticOptions();

        internal const WarningLevel DefaultWarningLevel = WarningLevel.Informational;
        internal const ReportDiagnostic DefaultGeneralDiagnosticOption = ReportDiagnostic.Default;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public DiagnosticOptions(
            WarningLevel warningLevel = DefaultWarningLevel,
            ReportDiagnostic generalDiagnosticOption = DefaultGeneralDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions = null)
        {
            WarningLevel = warningLevel;
            GeneralDiagnosticOption = generalDiagnosticOption;

            SpecificDiagnosticOptions =
                specificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the global warning level.
        /// </summary>
        public WarningLevel WarningLevel { get; }

        /// <summary>
        /// Global warning report option.
        /// </summary>
        public ReportDiagnostic GeneralDiagnosticOption { get; }

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }
    }
}
