﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains extension methods for working with <see cref="Diagnostic"/> objects.
    /// </summary>
    internal static class DiagnosticExtensions
    {
        /// <summary>
        /// Calculates the severity of a diagnostic based on the compiler options.
        /// </summary>
        /// <param name="diagnostic">The diagnostic information from the attribute on the enum member.</param>
        /// <param name="options">The compiler options to use to determine whether to report the specified diagnostic.</param>
        /// <remarks>Much of this code has been adapted from the Roslyn source at <see href="http://source.roslyn.codeplex.com/#Microsoft.CodeAnalysis.CSharp/Compilation/CSharpDiagnosticFilter.cs"/>.</remarks>
        ///// <param name="hasPragmaSuppression">
        ///// Indicates whether the diagnostic is suppressed in the source with a `#pragma warning
        ///// disable` preprocessor instruction.
        ///// </param>
        public static ReportDiagnostic CalculateReportAction(
            this Diagnostic diagnostic,
            CompilerOptions options /*,
            out bool hasPragmaSuppression*/)
        {
            if (diagnostic == null)
            {
                throw new ArgumentNullException(nameof(diagnostic));
            }

            //hasPragmaSuppression = false;

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
        /// Creates a copy of the specified diagnostic and changes the severity to match the
        /// specified report action. If the severity level is the same, no copy is made. If the
        /// report action is <see cref="ReportDiagnostic.Suppress"/>, null is returned.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to copy.</param>
        /// <param name="reportAction">The severity level in which the diagnostic should be reported.</param>
        /// <returns>
        /// Either a copy or the original <see cref="Diagnostic"/>, or null if the diagnostic should
        /// be suppressed.
        /// </returns>
        public static Diagnostic WithReportDiagnostic(this Diagnostic diagnostic, ReportDiagnostic reportAction)
        {
            if (diagnostic == null)
            {
                return null;
            }

            switch (reportAction)
            {
                case ReportDiagnostic.Default:
                    return diagnostic;

                case ReportDiagnostic.Error:
                    return diagnostic.WithSeverity(DiagnosticSeverity.Error);

                case ReportDiagnostic.Warn:
                    return diagnostic.WithSeverity(DiagnosticSeverity.Warning);

                case ReportDiagnostic.Info:
                    return diagnostic.WithSeverity(DiagnosticSeverity.Info);

                case ReportDiagnostic.Hidden:
                    return diagnostic.WithSeverity(DiagnosticSeverity.Hidden);

                case ReportDiagnostic.Suppress:
                    return null;

                default:
                    throw new ArgumentOutOfRangeException(nameof(reportAction), reportAction, null);
            }
        }

        /// <summary>
        /// Creates a copy of the specified diagnostic with the specified severity level. If the
        /// severity level is the same, no copy is made.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to copy.</param>
        /// <param name="severity">The new severity level.</param>
        /// <returns></returns>
        public static Diagnostic WithSeverity(this Diagnostic diagnostic, DiagnosticSeverity severity)
        {
            if (diagnostic == null)
            {
                return null;
            }

            if (diagnostic.Severity == severity)
            {
                return diagnostic;
            }

            return Diagnostic.Create(
                diagnostic.Id,
                diagnostic.Descriptor.Category,
                diagnostic.GetMessage(CultureInfo.InvariantCulture),
                severity,
                diagnostic.DefaultSeverity,
                diagnostic.Descriptor.IsEnabledByDefault,
                diagnostic.WarningLevel,
                diagnostic.Descriptor.Title,
                diagnostic.Descriptor.Description,
                diagnostic.Descriptor.HelpLinkUri,
                diagnostic.Location,
                diagnostic.AdditionalLocations,
                diagnostic.Descriptor.CustomTags,
                diagnostic.Properties);
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
        private static bool IsNotConfigurable(this Diagnostic diagnostic) =>
            diagnostic.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable);
    }
}
