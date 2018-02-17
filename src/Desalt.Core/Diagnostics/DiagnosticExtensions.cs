// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static class DiagnosticExtensions
    {
        public static DiagnosticSeverity ToSeverity(
            this ReportDiagnostic reportDiagnostic,
            DiagnosticSeverity defaultSeverity)
        {
            switch (reportDiagnostic)
            {
                case ReportDiagnostic.Default:
                    return defaultSeverity;

                case ReportDiagnostic.Error:
                    return DiagnosticSeverity.Error;

                case ReportDiagnostic.Warn:
                    return DiagnosticSeverity.Warning;

                case ReportDiagnostic.Info:
                    return DiagnosticSeverity.Info;

                case ReportDiagnostic.Hidden:
                case ReportDiagnostic.Suppress:
                    return DiagnosticSeverity.Hidden;

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportDiagnostic), reportDiagnostic, null);
            }
        }

        /// <summary>
        /// Calculates the severity of a diagnostic based on the compiler options.
        /// </summary>
        /// <param name="diagnostic">The diagnostic information from the attribute on the enum member.</param>
        /// <param name="options">The compiler options to use to determine whether to report the specified diagnostic.</param>
        /// <param name="hasPragmaSuppression">
        /// Indicates whether the diagnostic is suppressed in the source with a `#pragma warning
        /// disable` preprocessor instruction.
        /// </param>
        /// <remarks>Much of this code has been adapted from the Roslyn source at <see href="http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis.CSharp/Compilation/CSharpDiagnosticFilter.cs"/>.</remarks>
        public static ReportDiagnostic CalculateReportAction(
            this Diagnostic diagnostic,
            CompilerOptions options,
            out bool hasPragmaSuppression)
        {
            hasPragmaSuppression = false;

            // diagnostics that are not configurable will be reported based on the enabled state
            if (diagnostic.IsNotConfigurable())
            {
                return diagnostic.Descriptor.IsEnabledByDefault ? ReportDiagnostic.Default : ReportDiagnostic.Suppress;
            }

            // check the compiler options for /nowarn:<n> or /warnaserror:<n>
            bool isSpecified =
                options.SpecificDiagnosticOptions.TryGetValue(diagnostic.Id, out ReportDiagnostic report);
            if (!isSpecified)
            {
                report = diagnostic.Descriptor.IsEnabledByDefault
                    ? ReportDiagnostic.Default
                    : ReportDiagnostic.Suppress;
            }

            // determine whether reporting for this diagnostic should be suppressed
            if (diagnostic.WarningLevel > (int)options.WarningLevel)
            {
                report = ReportDiagnostic.Suppress;
            }

            // if the location is available, check any code pragmas for suppressions
            // TODO-jrockwood-2018-02-16: Implement if #pragma support needed for
            //hasPragmaSuppression =
            //    location?.SourceTree?.GetPragmaDirectiveWarningState(diagnostic.Id, location.SourceSpan.Start) ==
            //    ReportDiagnostic.Suppress;

            // Unless specific warning options are defined (/warnaserror[+|-]:<n> or /nowarn:<n>,
            // follow the global option (/warnaserror[+|-] or /nowarn).
            if (report == ReportDiagnostic.Default &&
                options.GeneralDiagnosticOption == ReportDiagnostic.Error &&
                diagnostic.DefaultSeverity == DiagnosticSeverity.Warning)
            {
                report = ReportDiagnostic.Error;
            }

            return report;
        }

        /// <summary>
        /// Returns a value indicating whether the specified <see cref="Diagnostic"/> is
        /// configurable, i.e. the severity can be changed. A diagnostic is not configurable if it
        /// has the <see cref="WellKnownDiagnosticTags.NotConfigurable"/> custom tag.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to check.</param>
        /// <returns>
        /// true if the Diagnostic is not configurable, meaning that the severity cannot be changed;
        /// otherwise, false.
        /// </returns>
        public static bool IsNotConfigurable(this Diagnostic diagnostic) =>
            diagnostic.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable);
    }
}
