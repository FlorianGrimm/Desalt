// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionParserTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using NUnit.Framework;

    public class CliOptionParserTests
    {
        //// ===========================================================================================================
        //// --help and --version Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_version()
        {
            var result = CliOptionParser.Parse(new[] { "--version" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowVersion.Should().BeTrue();

            result = CliOptionParser.Parse(new[] { "-v" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowVersion.Should().BeTrue();
        }

        [Test]
        public void Parse_should_recognize_help()
        {
            var result = CliOptionParser.Parse(new[] { "--help" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowHelp.Should().BeTrue();

            result = CliOptionParser.Parse(new[] { "-?" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowHelp.Should().BeTrue();
        }

        [Test]
        public void Parse_should_use_version_or_help_and_succeed_even_if_there_are_other_errors()
        {
            var result = CliOptionParser.Parse(new[] { "--unknown", "--version" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowVersion.Should().BeTrue();

            result = CliOptionParser.Parse(new[] { "--unknown", "-?" });
            result.Success.Should().BeTrue();
            result.Result.ShouldShowHelp.Should().BeTrue();
        }

        //// ===========================================================================================================
        //// Error Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_return_an_error_on_an_unrecognized_option()
        {
            var result = CliOptionParser.Parse(new[] { "--unknown" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.UnrecognizedOption("--unknown"));
        }

        //// ===========================================================================================================
        //// File-based Options Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_project()
        {
            var result = CliOptionParser.Parse(new[] { "--project", "Proj.csproj" });
            result.Success.Should().BeTrue();
            result.Result.Should().BeEquivalentTo(new CliOptions { ProjectFile = "Proj.csproj" });
        }

        [Test]
        public void Parse_should_return_an_error_when_project_is_missing_the_value()
        {
            var result = CliOptionParser.Parse(new[] { "--project" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingFileSpecification("--project"));

            result = CliOptionParser.Parse(new[] { "--project", "--nologo" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingFileSpecification("--project"));
        }

        [Test]
        public void Parse_should_recognize_out()
        {
            var result = CliOptionParser.Parse(new[] { "--out", "Directory" });
            result.Success.Should().BeTrue();
            result.Result.Should().BeEquivalentTo(new CliOptions { OutDirectory = "Directory" });
        }

        [Test]
        public void Parse_should_return_an_error_when_out_is_missing_the_value()
        {
            var result = CliOptionParser.Parse(new[] { "--out" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingFileSpecification("--out"));

            result = CliOptionParser.Parse(new[] { "--out", "--nologo" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingFileSpecification("--out"));
        }

        //// ===========================================================================================================
        //// Other Options Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_nologo()
        {
            var result = CliOptionParser.Parse(new[] { "--nologo" });
            result.Success.Should().BeTrue();
            result.Result.NoLogo.Should().BeTrue();
        }

        //// ===========================================================================================================
        //// Warnings and Errors Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_warn()
        {
            var result = CliOptionParser.Parse(new[] { "--warn", "2" });
            result.Success.Should().BeTrue();
            result.Result.WarningLevel.Should().Be(2);

            result = CliOptionParser.Parse(new[] { "-w", "2" });
            result.Success.Should().BeTrue();
            result.Result.WarningLevel.Should().Be(2);
        }

        [Test]
        public void Parse_should_return_an_error_when_warn_has_no_value_or_an_invalid_value()
        {
            var result = CliOptionParser.Parse(new[] { "--warn" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingNumberForOption("--warn"));

            result = CliOptionParser.Parse(new[] { "--warn", "--project", "Project.csproj" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingNumberForOption("--warn"));

            result = CliOptionParser.Parse(new[] { "--warn", "not-a-number" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingNumberForOption("--warn"));
        }

        [Test]
        public void Parse_should_recognize_nowarn()
        {
            var result = CliOptionParser.Parse(new[] { "--nowarn", "CS2008" });
            result.Success.Should().BeTrue();
            result.Result.NoWarn.Should().ContainSingle().And.Equal("CS2008");

            result = CliOptionParser.Parse(new[] { "--nowarn", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.NoWarn.Should().HaveCount(3).And.ContainInOrder("CS2008", "CS2009", "CS2010");
        }

        [Test]
        public void Parse_should_return_an_error_when_nowarn_has_no_value_or_an_invalid_value()
        {
            var result = CliOptionParser.Parse(new[] { "--nowarn" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingValueForOption("--nowarn"));

            result = CliOptionParser.Parse(new[] { "--nowarn", "--project", "Project.csproj" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingValueForOption("--nowarn"));
        }

        [Test]
        public void Parse_should_recognize_warnaserror_as_a_flag()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror" });
            result.Success.Should().BeTrue();
            result.Result.AllWarningsAsErrors.Should().BeTrue();

            result = CliOptionParser.Parse(new[] { "--warnaserror+" });
            result.Success.Should().BeTrue();
            result.Result.AllWarningsAsErrors.Should().BeTrue();

            result = CliOptionParser.Parse(new[] { "--warnaserror-" });
            result.Success.Should().BeTrue();
            result.Result.AllWarningsAsErrors.Should().BeFalse();

            result = CliOptionParser.Parse(new[] { "--warnaserror+", "--project", "Project.csproj" });
            result.Success.Should().BeTrue();
            result.Result.AllWarningsAsErrors.Should().BeTrue();
        }

        [Test]
        public void Parse_should_recognize_warnaserror_as_a_list()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "CS2008" });
            result.Success.Should().BeTrue();
            result.Result.WarningsAsErrors.Should().ContainSingle().And.Equal("CS2008");

            result = CliOptionParser.Parse(new[] { "--warnaserror", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.WarningsAsErrors.Should().HaveCount(3).And.ContainInOrder("CS2008", "CS2009", "CS2010");

            result = CliOptionParser.Parse(new[] { "--warnaserror+", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.WarningsAsErrors.Should().HaveCount(3).And.ContainInOrder("CS2008", "CS2009", "CS2010");

            result = CliOptionParser.Parse(new[] { "--warnaserror-", "CS2008" });
            result.Success.Should().BeTrue();
            result.Result.WarningsNotAsErrors.Should().ContainSingle().And.Equal("CS2008");

            result = CliOptionParser.Parse(new[] { "--warnaserror-", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.WarningsNotAsErrors.Should().HaveCount(3).And.ContainInOrder("CS2008", "CS2009", "CS2010");
        }
    }
}
