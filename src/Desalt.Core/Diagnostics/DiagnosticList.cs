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
    internal sealed class DiagnosticList : ICollection<Diagnostic>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly DiagnosticList Empty = new DiagnosticList(null);

        private readonly List<Diagnostic> _diagnostics;
        private readonly DiagnosticOptions _options;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private DiagnosticList(DiagnosticOptions options, IEnumerable<Diagnostic> diagnostics = null)
        {
            _options = options;
            _diagnostics = new List<Diagnostic>(Filter(options, diagnostics));
        }

        public static DiagnosticList Create(CompilerOptions options, params Diagnostic[] diagnostics)
        {
            return new DiagnosticList(options?.DiagnosticOptions ?? throw new ArgumentNullException(nameof(options)), diagnostics);
        }

        public static DiagnosticList From(CompilerOptions options, IEnumerable<Diagnostic> diagnostics)
        {
            return new DiagnosticList(
                options?.DiagnosticOptions ?? throw new ArgumentNullException(nameof(options)),
                diagnostics ?? throw new ArgumentNullException(nameof(diagnostics)));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public int Count => _diagnostics.Count;

        bool ICollection<Diagnostic>.IsReadOnly => false;

        public ImmutableArray<Diagnostic> FilteredDiagnostics => _diagnostics.ToImmutableArray();

        public bool HasErrors => _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);

        /// <summary>
        /// Determines whether to throw an exception when a filtered error is added. Mainly used in
        /// unit tests - should not normally be used.
        /// </summary>
        public bool ThrowOnErrors { get; set; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        bool ICollection<Diagnostic>.Contains(Diagnostic item) => _diagnostics.Contains(item);

        void ICollection<Diagnostic>.CopyTo(Diagnostic[] array, int arrayIndex) =>
            _diagnostics.CopyTo(array, arrayIndex);

        bool ICollection<Diagnostic>.Remove(Diagnostic item) => _diagnostics.Remove(item);

        void ICollection<Diagnostic>.Add(Diagnostic item) => Add(item);

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
            // don't add anything if we're Empty
            if (_options == null)
            {
                return diagnostic;
            }

            diagnostic = Filter(_options, diagnostic);
            if (diagnostic != null)
            {
                _diagnostics.Add(diagnostic);
                if (ThrowOnErrors)
                {
                    throw new InvalidOperationException(diagnostic.ToString());
                }
            }

            return diagnostic;
        }

        /// <summary>
        /// Adds a range of diagnostics, after filtering them based on the compiler options.
        /// </summary>
        /// <param name="diagnostics">The diagnostics to add.</param>
        public void AddRange(IEnumerable<Diagnostic> diagnostics)
        {
            // don't add anything if we're Empty
            if (_options == null)
            {
                return;
            }

            _diagnostics.AddRange(Filter(_options, diagnostics));
        }

        /// <summary>
        /// Clears the list of diagnostics.
        /// </summary>
        public void Clear()
        {
            _diagnostics.Clear();
        }

        private static Diagnostic Filter(DiagnosticOptions options, Diagnostic diagnostic)
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
            DiagnosticOptions options,
            IEnumerable<Diagnostic> diagnostics = null)
        {
            diagnostics = diagnostics ?? Enumerable.Empty<Diagnostic>();
            return diagnostics.Select(d => Filter(options, d)).Where(d => d != null);
        }
    }
}
