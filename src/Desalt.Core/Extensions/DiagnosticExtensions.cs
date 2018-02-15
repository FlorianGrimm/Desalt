// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Contains extension methods relating to <see cref="Diagnostic"/> instances.
    /// </summary>
    internal static class DiagnosticExtensions
    {
        /// <summary>
        /// Converts all of the specified non-hidden diagnostics to a list of <see cref="DiagnosticMessage"/>.
        /// </summary>
        /// <param name="diagnostics">A list of Roslyn <see cref="Diagnostic"/> messages.</param>
        /// <returns>A converted list of <see cref="DiagnosticMessage"/> messages.</returns>
        public static IEnumerable<DiagnosticMessage> ToDiagnosticMessages(this IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();

            // convert all non-hidden diagnostics to DiagnosticMessage instances
            return diagnostics
                .Where(d => d.Severity != DiagnosticSeverity.Hidden)
                .Select(DiagnosticMessage.FromDiagnostic);
        }

        /// <summary>
        /// Returns a value indicating whether none of the messages are errors.
        /// </summary>
        /// <param name="diagnostics">A list of <see cref="DiagnosticMessage"/> messages.</param>
        /// <returns>
        /// true if none of the messages are errors; false if there is at least one error.
        /// </returns>
        public static bool NoErrors(this IEnumerable<DiagnosticMessage> diagnostics)
        {
            return diagnostics.All(diagnostic => !diagnostic.IsError);
        }

        /// <summary>
        /// Returns a value indicating whether none of the messages are errors or warnings.
        /// </summary>
        /// <param name="diagnostics">A list of <see cref="DiagnosticMessage"/> messages.</param>
        /// <returns>
        /// true if none of the messages are errors or warnings; false if there is at least one error
        /// or warning.
        /// </returns>
        public static bool NoErrorsOrWarnings(this IEnumerable<DiagnosticMessage> diagnostics)
        {
            return diagnostics.All(diagnostic => !diagnostic.IsError && !diagnostic.IsWarning);
        }
    }
}
