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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DiagnosticListTests
    {
        [TestMethod]
        public void DiagnosticList_Create_and_From_should_throw_on_null_options()
        {
            Action action = () => DiagnosticList.Create(null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");

            action = () => DiagnosticList.From(null, Enumerable.Empty<Diagnostic>());
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");
        }

        [TestMethod]
        public void DiagnosticList_From_should_throw_on_null_diagnostics()
        {
            var options = new CompilerOptions("out");
            Action action = () => DiagnosticList.From(options, null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("diagnostics");
        }

        [TestMethod]
        public void DiagnosticList_From_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            var diagnostics = new[]
            {
                DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                DiagnosticsTestFactories.CreateWarning(),
                DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
            };
            DiagnosticList list = DiagnosticList.From(options, diagnostics);

            list.FilteredDiagnostics.Select(d => d.Id).Should().BeEquivalentTo("id1", "id3");
        }

        [TestMethod]
        public void DiagnosticList_HasErrors_should_return_true_if_there_is_at_least_one_error()
        {
            DiagnosticList list = DiagnosticList.From(
                new CompilerOptions("out"),
                new[] { DiagnosticsTestFactories.CreateWarning(), DiagnosticsTestFactories.CreateDiagnostic() });

            list.HasErrors.Should().BeTrue();
        }

        [TestMethod]
        public void DiagnosticList_HasErrors_should_return_false_if_there_are_no_errors()
        {
            DiagnosticList list = DiagnosticList.From(
                new CompilerOptions("out"),
                new[] { DiagnosticsTestFactories.CreateWarning() });
            list.HasErrors.Should().BeFalse();
        }

        [TestMethod]
        public void DiagnosticList_Add_should_do_nothing_if_the_diagnostic_is_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticList list = DiagnosticList.Create(options);
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();

            list.Add(warning).Should().BeNull();
        }

        [TestMethod]
        public void DiagnosticList_Add_should_add_the_diagnostic_if_it_passes_the_filter()
        {
            DiagnosticList list = DiagnosticList.Create(new CompilerOptions("out"));
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            list.Add(warning);

            list.FilteredDiagnostics.Single().Severity.Should().Be(DiagnosticSeverity.Warning);
        }

        [TestMethod]
        public void DiagnosticList_AddRange_should_add_only_the_errors_if_the_options_have_warnings_suppressed()
        {
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticList list = DiagnosticList.Create(options);
            list.AddRange(
                new[]
                {
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                    DiagnosticsTestFactories.CreateWarning(),
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
                });

            list.FilteredDiagnostics.Select(d => d.Id).Should().BeEquivalentTo("id1", "id3");
        }

        [TestMethod]
        public void DiagnosticList_Empty_should_not_store_anyting_on_Add()
        {
            DiagnosticList.Empty.Add(DiagnosticsTestFactories.CreateDiagnostic(id: "id1"));
            DiagnosticList.Empty.FilteredDiagnostics.Should().BeEmpty();
        }

        [TestMethod]
        public void DiagnosticList_Empty_should_not_store_anyting_on_AddRanage()
        {
            DiagnosticList.Empty.AddRange(
                new[]
                {
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id1"),
                    DiagnosticsTestFactories.CreateWarning(),
                    DiagnosticsTestFactories.CreateDiagnostic(id: "id3")
                });
            DiagnosticList.Empty.FilteredDiagnostics.Should().BeEmpty();
        }
    }
}
