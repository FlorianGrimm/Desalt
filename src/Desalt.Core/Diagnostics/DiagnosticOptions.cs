// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticOptions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System.Collections.Immutable;
    using Desalt.Core.Options;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains options that relate to how errors and warnings are handled during a compile.
    /// </summary>
    public class DiagnosticOptions
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly DiagnosticOptions Default = new DiagnosticOptions(instanceToCopy: null);

        public const WarningLevel DefaultWarningLevel = WarningLevel.Informational;
        public const ReportDiagnostic DefaultGeneralDiagnosticOption = ReportDiagnostic.Default;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Default constructor contains the default values of all of the options.
        /// </summary>
        public DiagnosticOptions(
            WarningLevel warningLevel = DefaultWarningLevel,
            ReportDiagnostic generalDiagnosticOption = DefaultGeneralDiagnosticOption,
            ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions = null,
            bool throwOnErrors = false)
            : this(
                instanceToCopy: null,
                warningLevel,
                generalDiagnosticOption,
                specificDiagnosticOptions,
                throwOnErrors)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DiagnosticOptions"/> class. Values are set using the
        /// following precedence:
        /// * The named parameter, if specified
        /// * Otherwise, the value of <paramref name="instanceToCopy"/>, if specified
        /// * Otherwise, the default value of the parameter's type
        /// </summary>
        private DiagnosticOptions(
            DiagnosticOptions? instanceToCopy = null,
            WarningLevel? warningLevel = null,
            ReportDiagnostic? generalDiagnosticOption = null,
            ImmutableDictionary<string, ReportDiagnostic>? specificDiagnosticOptions = null,
            bool? throwOnErrors = null)
        {
            WarningLevel = warningLevel ?? instanceToCopy?.WarningLevel ?? DefaultWarningLevel;
            GeneralDiagnosticOption = generalDiagnosticOption ??
                instanceToCopy?.GeneralDiagnosticOption ?? DefaultGeneralDiagnosticOption;

            SpecificDiagnosticOptions = specificDiagnosticOptions ??
                instanceToCopy?.SpecificDiagnosticOptions ?? ImmutableDictionary<string, ReportDiagnostic>.Empty;

            ThrowOnErrors = throwOnErrors ?? instanceToCopy?.ThrowOnErrors ?? false;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Determines whether to throw an exception when a filtered error is added. Mainly used in unit tests - should
        /// not normally be used.
        /// </summary>
        public bool ThrowOnErrors { get; }

        public DiagnosticOptions WithThrowOnErrors(bool value)
        {
            return value == ThrowOnErrors ? this : new DiagnosticOptions(this, throwOnErrors: value);
        }

        /// <summary>
        /// Gets the global warning level.
        /// </summary>
        public WarningLevel WarningLevel { get; }

        public DiagnosticOptions WithWarningLevel(WarningLevel value)
        {
            return value == WarningLevel ? this : new DiagnosticOptions(this, warningLevel: value);
        }

        /// <summary>
        /// Global warning report option.
        /// </summary>
        public ReportDiagnostic GeneralDiagnosticOption { get; }

        public DiagnosticOptions WithGeneralDiagnosticOptions(ReportDiagnostic value)
        {
            return value == GeneralDiagnosticOption
                ? this
                : new DiagnosticOptions(this, generalDiagnosticOption: value);
        }

        /// <summary>
        /// Warning report option for each warning.
        /// </summary>
        public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        public DiagnosticOptions WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic> value)
        {
            return value == SpecificDiagnosticOptions
                ? this
                : new DiagnosticOptions(this, specificDiagnosticOptions: value);
        }
    }
}
