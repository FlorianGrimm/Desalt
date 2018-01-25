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
        /// <param name="diagnostics"></param>
        /// <returns></returns>
        public static IEnumerable<DiagnosticMessage> ToDiagnosticMessages(this IEnumerable<Diagnostic> diagnostics)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();

            // convert all non-hidden diagnostics to DiagnosticMessage instances
            return diagnostics
                .Where(d => d.Severity != DiagnosticSeverity.Hidden)
                .Select(DiagnosticMessage.FromDiagnostic);
        }
    }
}
