// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionsValidatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using NUnit.Framework;

    public class CliOptionsValidatorTests
    {
        [Test]
        public void Validate_should_return_true_if_the_options_are_valid()
        {
            var result = CliOptionsValidator.Validate(new CliOptions { ProjectFile = "Project.csproj" });
            result.Success.Should().BeTrue();
            result.Result.Should().BeTrue();
        }

        [Test]
        public void Validate_should_return_an_error_for_missing_required_options()
        {
            var result = CliOptionsValidator.Validate(new CliOptions());
            result.Result.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingRequiredOption("--project"));
        }

        [Test]
        public void Validate_should_return_true_if_version_or_help_is_specified()
        {
            var result = CliOptionsValidator.Validate(new CliOptions { ShouldShowVersion = true });
            result.Result.Should().BeTrue();

            result = CliOptionsValidator.Validate(new CliOptions { ShouldShowHelp = true });
            result.Result.Should().BeTrue();
        }

        [Test]
        public void Validate_should_return_an_error_for_warning_level_out_of_range()
        {
            var result = CliOptionsValidator.Validate(new CliOptions { ProjectFile = "project", WarningLevel = -1 });
            result.Result.Should().BeFalse();
            result.Diagnostics.Should().ContainSingle().And.BeEquivalentTo(DiagnosticFactory.WarningLevelMustBeInRange());

            result = CliOptionsValidator.Validate(new CliOptions { ProjectFile = "project", WarningLevel = 5 });
            result.Result.Should().BeFalse();
            result.Diagnostics.Should().ContainSingle().And.BeEquivalentTo(DiagnosticFactory.WarningLevelMustBeInRange());
        }
    }
}
