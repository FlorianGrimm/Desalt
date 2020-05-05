// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticListTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Diagnostics
{
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public class DiagnosticListTests
    {
        //// ===========================================================================================================
        //// Creation Tests
        //// ===========================================================================================================

        [Test]
        public void Ctor_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out").WithDiagnosticOptions(
                DiagnosticOptions.Default.WithGeneralDiagnosticOptions(ReportDiagnostic.Suppress));
            Diagnostic[] diagnostics = new[]
            {
                DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                DiagnosticsTestFactories.CreateWarning(),
                DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
            };
            var list = new DiagnosticList(options.DiagnosticOptions, diagnostics);

            list.FilteredDiagnostics.Select(d => d.Id).Should().BeEquivalentTo("id1", "id3");
        }

        //// ===========================================================================================================
        //// HasErrors Tests
        //// ===========================================================================================================

        [Test]
        public void HasErrors_should_return_true_if_there_is_at_least_one_error()
        {
            var list = new DiagnosticList(
                new CompilerOptions("out").DiagnosticOptions,
                new[] { DiagnosticsTestFactories.CreateWarning(), DiagnosticsTestFactories.CreateDiagnostic() });

            list.HasErrors.Should().BeTrue();
        }

        [Test]
        public void HasErrors_should_return_false_if_there_are_no_errors()
        {
            var list = new DiagnosticList(
                new CompilerOptions("out").DiagnosticOptions,
                new[] { DiagnosticsTestFactories.CreateWarning() });
            list.HasErrors.Should().BeFalse();
        }

        //// ===========================================================================================================
        //// Add/AddRange Tests
        //// ===========================================================================================================

        [Test]
        public void Add_should_do_nothing_if_the_diagnostic_is_suppressed()
        {
            var options = new CompilerOptions("out").WithDiagnosticOptions(
                DiagnosticOptions.Default.WithGeneralDiagnosticOptions(ReportDiagnostic.Suppress));
            var list = new DiagnosticList(options.DiagnosticOptions);
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();

            list.Add(warning).Should().BeNull();
        }

        [Test]
        public void Add_should_add_the_diagnostic_if_it_passes_the_filter()
        {
            var list = new DiagnosticList(new CompilerOptions("out").DiagnosticOptions);
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            list.Add(warning);

            list.FilteredDiagnostics.Single().Severity.Should().Be(DiagnosticSeverity.Warning);
        }

        [Test]
        public void AddRange_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out").WithDiagnosticOptions(
                DiagnosticOptions.Default.WithGeneralDiagnosticOptions(ReportDiagnostic.Suppress));
            var list = new DiagnosticList(options.DiagnosticOptions);
            list.AddRange(
                new[]
                {
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                    DiagnosticsTestFactories.CreateWarning(),
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
                });

            list.FilteredDiagnostics.Select(d => d.Id).Should().BeEquivalentTo("id1", "id3");
        }

        //// ===========================================================================================================
        //// Clear Tests
        //// ===========================================================================================================

        [Test]
        public void Clear_should_empty_the_list()
        {
            var list = new DiagnosticList(new CompilerOptions("out").DiagnosticOptions);
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            list.Add(warning);

            list.Clear();
            list.FilteredDiagnostics.Should().BeEmpty();
        }
    }
}
