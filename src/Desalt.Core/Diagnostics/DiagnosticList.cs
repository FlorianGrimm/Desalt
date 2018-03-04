// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticList.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Represents a list of <see cref="Diagnostic"/> messages, filtered using a <see
    /// cref="CompilerOptions"/>. This is not thread-safe and designed to be used within a single
    /// thread to gather diagnostics for the running task.
    /// </summary>
    internal sealed class DiagnosticList : IEnumerable<Diagnostic>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly List<Diagnostic> _diagnostics;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DiagnosticList(CompilerOptions options, IEnumerable<Diagnostic> diagnostics = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
            _diagnostics = new List<Diagnostic>(Filter(options, diagnostics));
        }

        public static DiagnosticList Create(CompilerOptions options, params Diagnostic[] diagnostics)
        {
            return new DiagnosticList(options, diagnostics);
        }

        public static DiagnosticList From(CompilerOptions options, IEnumerable<Diagnostic> diagnostics)
        {
            return new DiagnosticList(options, diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<Diagnostic> FilteredDiagnostics => _diagnostics.ToImmutableArray();

        public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

        public CompilerOptions Options { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate
        /// through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        /// <summary>
        /// Adds a new diagnostic to the list, but only if the compiler options don't dictate that it
        /// should be suppressed.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to add.</param>
        /// <returns>
        /// The diagnostic that was added, which could potentially be at a different severity than
        /// the supplied diagnostic if the compiler options dictate it. For example, if warnings are
        /// treated as errors then a warning diagnostic will be converted to an error diagnostic. If
        /// the diagnostic was suppressed or hidden, null is returned.
        /// </returns>
        public Diagnostic Add(Diagnostic diagnostic)
        {
            diagnostic = Filter(Options, diagnostic);
            if (diagnostic != null)
            {
                _diagnostics.Add(diagnostic);
            }

            return diagnostic;
        }

        /// <summary>
        /// Adds a range of diagnostics, after filtering them based on the compiler options.
        /// </summary>
        /// <param name="diagnostics">The diagnostics to add.</param>
        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            _diagnostics.AddRange(Filter(Options, diagnostics));
        }

        private static Diagnostic Filter(CompilerOptions options, Diagnostic diagnostic)
        {
            ReportDiagnostic reportAction = diagnostic.CalculateReportAction(options);
            Diagnostic filteredDiagnostic = diagnostic.WithReportDiagnostic(reportAction);

            // don't add hidden diagnostics
            if (filteredDiagnostic?.Severity == DiagnosticSeverity.Hidden)
            {
                filteredDiagnostic = null;
            }

            return filteredDiagnostic;
        }

        private static IEnumerable<Diagnostic> Filter(
            CompilerOptions options,
            IEnumerable<Diagnostic> diagnostics = null)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();
            return diagnostics.Select(d => Filter(options, d)).Where(d => d != null);
        }
    }
}
