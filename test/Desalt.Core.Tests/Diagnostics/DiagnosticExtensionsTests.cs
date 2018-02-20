// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticExtensionsTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod"), TestClass]
    public class DiagnosticExtensionsTests
    {
        [TestMethod]
        public void CalculateReportAction_should_throw_on_null_parameters()
        {
            Action action = () => DiagnosticExtensions.CalculateReportAction(null, new CompilerOptions("out"));
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("diagnostic");

            action = () => DiagnosticExtensions.CalculateReportAction(DiagnosticsTestFactories.CreateDiagnostic(), null);
            action.ShouldThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");
        }

        [TestMethod]
        public void CalculateReportAction_should_return_Default_when_not_configurable_and_enabled()
        {
            ReportDiagnostic reportAction = DiagnosticExtensions.CalculateReportAction(
                DiagnosticsTestFactories.CreateDiagnostic(customTags: new[] { WellKnownDiagnosticTags.NotConfigurable }),
                new CompilerOptions("out"));

            reportAction.Should().Be(ReportDiagnostic.Default);
        }

        [TestMethod]
        public void CalculateReportAction_should_return_Suppress_when_not_configurable_and_disabled()
        {
            ReportDiagnostic reportAction = DiagnosticExtensions.CalculateReportAction(
                DiagnosticsTestFactories.CreateDiagnostic(
                    isEnabledByDefault: false,
                    customTags: new[] { WellKnownDiagnosticTags.NotConfigurable }),
                new CompilerOptions("out"));

            reportAction.Should().Be(ReportDiagnostic.Suppress);
        }

        [TestMethod]
        public void
            CalculateReportAction_should_return_the_level_of_whatever_is_specified_in_the_options_for_a_specific_warning()
        {
            foreach (ReportDiagnostic reportAction in new[]
            {
                ReportDiagnostic.Error,
                ReportDiagnostic.Warn,
                ReportDiagnostic.Info,
                ReportDiagnostic.Hidden,
                ReportDiagnostic.Suppress
            })
            {
                ImmutableDictionary<string, ReportDiagnostic> dict = ImmutableDictionary.CreateRange(
                    new[] { new KeyValuePair<string, ReportDiagnostic>("id", reportAction) });
                var options = new CompilerOptions("out", specificDiagnosticOptions: dict);

                DiagnosticExtensions.CalculateReportAction(DiagnosticsTestFactories.CreateDiagnostic(), options).Should().Be(reportAction);
            }
        }

        [TestMethod]
        public void CalculateReportAction_should_suppress_warnings_above_the_options_warning_level()
        {
            Diagnostic informationalWarning = DiagnosticsTestFactories.CreateWarning(warningLevel: 4);

            for (int i = 1; i < 4; i++)
            {
                var options = new CompilerOptions("out", warningLevel: (WarningLevel)i);
                DiagnosticExtensions.CalculateReportAction(informationalWarning, options)
                    .Should()
                    .Be(ReportDiagnostic.Suppress);
            }
        }

        [TestMethod]
        public void CalculateReportAction_should_raise_warnings_to_errors_when_GeneralDiagnosticOption_is_Error()
        {
            Diagnostic error = DiagnosticsTestFactories.CreateWarning();
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Error);
            DiagnosticExtensions.CalculateReportAction(error, options).Should().Be(ReportDiagnostic.Error);
        }

        [TestMethod]
        public void
            CalculateReportAction_should_not_raise_Info_or_Hidden_to_errors_when_GeneralDiagnosticOption_is_Error()
        {
            Diagnostic info = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Info, warningLevel: 4);
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Error);
            DiagnosticExtensions.CalculateReportAction(info, options).Should().Be(ReportDiagnostic.Default);

            Diagnostic hidden = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Hidden, warningLevel: 4);
            DiagnosticExtensions.CalculateReportAction(hidden, options).Should().Be(ReportDiagnostic.Default);
        }

        [TestMethod]
        public void CalculateReportAction_should_suppress_all_warnings_if_GeneralDiagnosticOption_is_Suppress()
        {
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticExtensions.CalculateReportAction(warning, options).Should().Be(ReportDiagnostic.Suppress);
        }

        [TestMethod]
        public void CalculateReportAction_should_not_suppress_errors_when_GeneralDiagnosticOption_is_Suppress()
        {
            Diagnostic error = DiagnosticsTestFactories.CreateDiagnostic();
            var options = new CompilerOptions("out", generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticExtensions.CalculateReportAction(error, options).Should().Be(ReportDiagnostic.Default);
        }

        [TestMethod]
        public void WithReportDiagnostic_should_return_null_on_a_null_diagnostic_parameter()
        {
            DiagnosticExtensions.WithReportDiagnostic(null, ReportDiagnostic.Default).Should().BeNull();
        }

        [TestMethod]
        public void WithReportDiagnostic_should_pass_through_the_original_value_when_Default()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            DiagnosticExtensions.WithReportDiagnostic(diagnostic, ReportDiagnostic.Default)
                .Should()
                .BeSameAs(diagnostic);
        }

        [TestMethod]
        public void
            WithReportDiagnostic_should_return_the_same_diagnostic_for_Error_Warn_Info_and_Hidden_where_the_severity_matches()
        {
            Diagnostic error = DiagnosticsTestFactories.CreateDiagnostic();
            DiagnosticExtensions.WithReportDiagnostic(error, ReportDiagnostic.Error).Should().BeSameAs(error);

            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            DiagnosticExtensions.WithReportDiagnostic(warning, ReportDiagnostic.Warn).Should().BeSameAs(warning);

            Diagnostic info = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Info, warningLevel: 4);
            DiagnosticExtensions.WithReportDiagnostic(info, ReportDiagnostic.Info).Should().BeSameAs(info);

            Diagnostic hidden = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Hidden, warningLevel: 4);
            DiagnosticExtensions.WithReportDiagnostic(hidden, ReportDiagnostic.Hidden).Should().BeSameAs(hidden);
        }

        [TestMethod]
        public void WithReportDiagnostic_should_return_null_for_Suppress()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            DiagnosticExtensions.WithReportDiagnostic(diagnostic, ReportDiagnostic.Suppress).Should().BeNull();
        }

        [TestMethod]
        public void WithReportDiagnostic_should_throw_an_exception_on_an_invalid_ReportDiagnostic()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            Action action = () => DiagnosticExtensions.WithReportDiagnostic(diagnostic, (ReportDiagnostic)3000);
            action.ShouldThrowExactly<ArgumentOutOfRangeException>().And.ParamName.Should().Be("reportAction");
        }

        [TestMethod]
        public void WithSeverity_should_return_null_on_a_null_diagnostic()
        {
            DiagnosticExtensions.WithSeverity(null, DiagnosticSeverity.Error).Should().BeNull();
        }

        [TestMethod]
        public void WithSeverity_should_create_a_copy_of_the_Diagnostic_with_the_severity_changed()
        {
            Location location = Location.Create("file.cs", TextSpan.FromBounds(1, 10), new LinePositionSpan());
            var additionalLocations = new[] { Location.Create("file2.cs", new TextSpan(), new LinePositionSpan()), };
            var customTags = new[] { WellKnownDiagnosticTags.EditAndContinue };

            Diagnostic diagnostic = Diagnostic.Create(
                "id",
                "category",
                "message",
                DiagnosticSeverity.Warning,
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                warningLevel: 3,
                title: "title",
                description: "description",
                helpLink: "helpLink",
                isSuppressed: false,
                location: location,
                additionalLocations: additionalLocations,
                customTags: customTags);

            Diagnostic copy = DiagnosticExtensions.WithSeverity(diagnostic, DiagnosticSeverity.Hidden);
            copy.Should().NotBeSameAs(diagnostic);

            copy.Id.Should().Be("id");
            copy.Descriptor.Category.Should().Be("category");
            copy.GetMessage(CultureInfo.InvariantCulture).Should().Be("message");
            copy.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            copy.Descriptor.IsEnabledByDefault.Should().BeTrue();
            copy.WarningLevel.Should().Be(3);
            copy.Descriptor.Title.ToString().Should().Be("title");
            copy.Descriptor.Description.ToString().Should().Be("description");
            copy.Descriptor.HelpLinkUri.Should().Be("helpLink");
            copy.IsSuppressed.Should().BeFalse();
            copy.Location.Should().BeSameAs(location);
            copy.AdditionalLocations.Should().BeEquivalentTo(additionalLocations.AsEnumerable());
            copy.Descriptor.CustomTags.Should().BeEquivalentTo(customTags.AsEnumerable());

            copy.Severity.Should().Be(DiagnosticSeverity.Hidden);
        }
    }
}
