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
    using Desalt.Core.Options;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using NUnit.Framework;

    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public class DiagnosticExtensionsTests
    {
        [Test]
        public void CalculateReportAction_should_throw_on_null_parameters()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action action = () => DiagnosticExtensions.CalculateReportAction(null, DiagnosticOptions.Default);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("diagnostic");

            action = () => DiagnosticExtensions.CalculateReportAction(DiagnosticsTestFactories.CreateDiagnostic(), null);
            action.Should().ThrowExactly<ArgumentNullException>().And.ParamName.Should().Be("options");
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        [Test]
        public void CalculateReportAction_should_return_Default_when_not_configurable_and_enabled()
        {
            ReportDiagnostic reportAction = DiagnosticExtensions.CalculateReportAction(
                DiagnosticsTestFactories.CreateDiagnostic(customTags: new[] { WellKnownDiagnosticTags.NotConfigurable }),
                DiagnosticOptions.Default);

            reportAction.Should().Be(ReportDiagnostic.Default);
        }

        [Test]
        public void CalculateReportAction_should_return_Suppress_when_not_configurable_and_disabled()
        {
            ReportDiagnostic reportAction = DiagnosticExtensions.CalculateReportAction(
                DiagnosticsTestFactories.CreateDiagnostic(
                    isEnabledByDefault: false,
                    customTags: new[] { WellKnownDiagnosticTags.NotConfigurable }),
                DiagnosticOptions.Default);

            reportAction.Should().Be(ReportDiagnostic.Suppress);
        }

        [Test]
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
                var dict = ImmutableDictionary.CreateRange(
                    new[] { new KeyValuePair<string, ReportDiagnostic>("id", reportAction) });
                var options = new DiagnosticOptions(specificDiagnosticOptions: dict);

                DiagnosticExtensions.CalculateReportAction(DiagnosticsTestFactories.CreateDiagnostic(), options).Should().Be(reportAction);
            }
        }

        [Test]
        public void CalculateReportAction_should_suppress_warnings_above_the_options_warning_level()
        {
            Diagnostic informationalWarning = DiagnosticsTestFactories.CreateWarning(warningLevel: 4);

            for (int i = 1; i < 4; i++)
            {
                var options = new DiagnosticOptions(warningLevel: (WarningLevel)i);
                DiagnosticExtensions.CalculateReportAction(informationalWarning, options)
                    .Should()
                    .Be(ReportDiagnostic.Suppress);
            }
        }

        [Test]
        public void CalculateReportAction_should_raise_warnings_to_errors_when_GeneralDiagnosticOption_is_Error()
        {
            Diagnostic error = DiagnosticsTestFactories.CreateWarning();
            var options = new DiagnosticOptions(generalDiagnosticOption: ReportDiagnostic.Error);
            DiagnosticExtensions.CalculateReportAction(error, options).Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void
            CalculateReportAction_should_not_raise_Info_or_Hidden_to_errors_when_GeneralDiagnosticOption_is_Error()
        {
            Diagnostic info = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Info, warningLevel: 4);
            var options = new DiagnosticOptions(generalDiagnosticOption: ReportDiagnostic.Error);
            DiagnosticExtensions.CalculateReportAction(info, options).Should().Be(ReportDiagnostic.Default);

            Diagnostic hidden = DiagnosticsTestFactories.CreateDiagnostic(defaultSeverity: DiagnosticSeverity.Hidden, warningLevel: 4);
            DiagnosticExtensions.CalculateReportAction(hidden, options).Should().Be(ReportDiagnostic.Default);
        }

        [Test]
        public void CalculateReportAction_should_suppress_all_warnings_if_GeneralDiagnosticOption_is_Suppress()
        {
            Diagnostic warning = DiagnosticsTestFactories.CreateWarning();
            var options = new DiagnosticOptions(generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticExtensions.CalculateReportAction(warning, options).Should().Be(ReportDiagnostic.Suppress);
        }

        [Test]
        public void CalculateReportAction_should_not_suppress_errors_when_GeneralDiagnosticOption_is_Suppress()
        {
            Diagnostic error = DiagnosticsTestFactories.CreateDiagnostic();
            var options = new DiagnosticOptions(generalDiagnosticOption: ReportDiagnostic.Suppress);
            DiagnosticExtensions.CalculateReportAction(error, options).Should().Be(ReportDiagnostic.Default);
        }

        [Test]
        public void WithReportDiagnostic_should_return_null_on_a_null_diagnostic_parameter()
        {
            DiagnosticExtensions.WithReportDiagnostic(null, ReportDiagnostic.Default).Should().BeNull();
        }

        [Test]
        public void WithReportDiagnostic_should_pass_through_the_original_value_when_Default()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            DiagnosticExtensions.WithReportDiagnostic(diagnostic, ReportDiagnostic.Default)
                .Should()
                .BeSameAs(diagnostic);
        }

        [Test]
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

        [Test]
        public void WithReportDiagnostic_should_return_null_for_Suppress()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            DiagnosticExtensions.WithReportDiagnostic(diagnostic, ReportDiagnostic.Suppress).Should().BeNull();
        }

        [Test]
        public void WithReportDiagnostic_should_throw_an_exception_on_an_invalid_ReportDiagnostic()
        {
            Diagnostic diagnostic = DiagnosticsTestFactories.CreateDiagnostic();
            Action action = () => DiagnosticExtensions.WithReportDiagnostic(diagnostic, (ReportDiagnostic)3000);
            action.Should().ThrowExactly<ArgumentOutOfRangeException>().And.ParamName.Should().Be("reportAction");
        }

        [Test]
        public void WithSeverity_should_create_a_copy_of_the_Diagnostic_with_the_severity_changed()
        {
            var location = Location.Create("file.cs", TextSpan.FromBounds(1, 10), new LinePositionSpan());
            Location[] additionalLocations = new[] { Location.Create("file2.cs", new TextSpan(), new LinePositionSpan()), };
            string[] customTags = { WellKnownDiagnosticTags.EditAndContinue };

            var diagnostic = Diagnostic.Create(
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

            Diagnostic? copy = DiagnosticExtensions.WithSeverity(diagnostic, DiagnosticSeverity.Hidden);
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
