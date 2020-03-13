// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticListTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Diagnostics
{
    using System;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public class DiagnosticListTests
    {
        //// ===========================================================================================================
        //// Creation Tests
        //// ===========================================================================================================

        [Test]
        public void Create_and_From_should_throw_on_null_options()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => DiagnosticList.Create(null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");

            action = () => DiagnosticList.From(null, Enumerable.Empty<Diagnostic>());
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void From_should_throw_on_null_diagnostics()
        {
            var options = new CompilerOptions("out");
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => DiagnosticList.From(options, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("diagnostics");
        }

        [Test]
        public void From_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            Diagnostic[] diagnostics = new[]
            {
                DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                DiagnosticsTestFactories.CreateWarning(),
                DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
            };
            var list = DiagnosticList.From(options, diagnostics);

            list.FilteredDiagnostics.Select(d => d.Id).Should().BeEquivalentTo("id1", "id3");
        }

        //// ===========================================================================================================
        //// HasErrors Tests
        //// ===========================================================================================================

        [Test]
        public void HasErrors_should_return_true_if_there_is_at_least_one_error()
        {
            var list = DiagnosticList.From(
                new CompilerOptions("out"),
                new[] { DiagnosticsTestFactories.CreateWarning(), DiagnosticsTestFactories.CreateDiagnostic() });

            list.HasErrors.Should().BeTrue();
        }

        [Test]
        public void HasErrors_should_return_false_if_there_are_no_errors()
        {
            var list = DiagnosticList.From(
                new CompilerOptions("out"),
                new[] { DiagnosticsTestFactories.CreateWarning() });
            list.HasErrors.Should().BeFalse();
        }

        //// ===========================================================================================================
        //// Add/AddRange Tests
        //// ===========================================================================================================

        [Test]
        public void Add_should_do_nothing_if_the_diagnostic_is_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            var list = DiagnosticList.Create(options);
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();

            list.Add(warning).Should().BeNull();
        }

        [Test]
        public void Add_should_add_the_diagnostic_if_it_passes_the_filter()
        {
            var list = DiagnosticList.Create(new CompilerOptions("out"));
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            list.Add(warning);

            list.FilteredDiagnostics.Single().Severity.Should().Be(DiagnosticSeverity.Warning);
        }

        [Test]
        public void AddRange_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            var list = DiagnosticList.Create(options);
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
            var list = DiagnosticList.Create(new CompilerOptions("out"));
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            list.Add(warning);

            list.Clear();
            list.FilteredDiagnostics.Should().BeEmpty();
        }
    }
}
