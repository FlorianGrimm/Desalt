// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionParserTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System.Collections.Generic;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
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
        public void Parse_should_use_the_last_argument_for_project()
        {
            var result = CliOptionParser.Parse(new[] { "--project", "Proj.csproj", "--project", "ProjB.csproj" });
            result.Success.Should().BeTrue();
            result.Result.Should().BeEquivalentTo(new CliOptions { ProjectFile = "ProjB.csproj" });
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
        public void Parse_should_use_the_last_argument_for_out()
        {
            var result = CliOptionParser.Parse(new[] { "--out", "A", "--out", "B" });
            result.Success.Should().BeTrue();
            result.Result.Should().BeEquivalentTo(new CliOptions { OutDirectory = "B" });
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
        public void Parse_should_use_the_last_value_for_warn()
        {
            var result = CliOptionParser.Parse(new[] { "--warn", "2", "-w", "1" });
            result.Success.Should().BeTrue();
            result.Result.WarningLevel.Should().Be(1);
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
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(1)
                .And.Contain("CS2008", ReportDiagnostic.Suppress);

            result = CliOptionParser.Parse(new[] { "--nowarn", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Suppress);
        }

        [Test]
        public void Parse_should_combine_the_values_for_nowarn()
        {
            var result = CliOptionParser.Parse(new[] { "--nowarn", "CS2008,CS2009", "--nowarn", "CS2010" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().HaveCount(3).And.ContainKeys("CS2008", "CS2009", "CS2010");
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
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror+" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror-" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Default);

            result = CliOptionParser.Parse(new[] { "--warnaserror+", "--project", "Project.csproj" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_recognize_warnaserror_as_a_list()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "CS2008" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().HaveCount(1).And.Contain("CS2008", ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror", ";CS2008,CS2009 CS2010," });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror", ";CS2008,CS2009 CS2010," });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror-", "CS2008" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().BeEmpty();

            result = CliOptionParser.Parse(new[] { "--warnaserror-", ";CS2008,CS2009;CS2010," });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().BeEmpty();
        }

        [Test]
        public void Parse_should_give_precedence_to_later_arguments_with_warnaserror()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "--warnaserror-" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Default);

            result = CliOptionParser.Parse(new[] { "--warnaserror-", "--warnaserror+" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_aggregate_warnaserror_values()
        {
            var result = CliOptionParser.Parse(
                new[]
                {
                    "--warnaserror", "CS2008;CS2009",
                    "--warnaserror-", "CS123",
                    "--warnaserror-", "CS2008;CS2010;CS2011",
                    "--warnaserror+", "CS2011"
                });

            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(2)
                .And.ContainKeys("CS2009", "CS2011")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_give_nowarn_precedence_over_warnaserror()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "1,2", "--nowarn", "3", "--warnaserror", "3" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.Contain(
                    new KeyValuePair<string, ReportDiagnostic>("1", ReportDiagnostic.Error),
                    new KeyValuePair<string, ReportDiagnostic>("2", ReportDiagnostic.Error),
                    new KeyValuePair<string, ReportDiagnostic>("3", ReportDiagnostic.Suppress));
        }

        [Test]
        public void Parse_should_clear_warnaserror_lists_when_used_as_a_flag()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "1,2", "--warnaserror" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().BeEmpty();

            result = CliOptionParser.Parse(new[] { "--warnaserror", "1,2", "--warnaserror-" });
            result.Success.Should().BeTrue();
            result.Result.SpecificDiagnosticOptions.Should().BeEmpty();
        }

        [Test]
        public void Parse_should_not_set_the_general_diagnostic_option_to_default_when_a_list_is_encountered()
        {
            var result = CliOptionParser.Parse(new[] { "--warnaserror", "--warnaserror", "1,2" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            result = CliOptionParser.Parse(new[] { "--warnaserror", "--warnaserror-", "1,2" });
            result.Success.Should().BeTrue();
            result.Result.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        //// ===========================================================================================================
        //// Symbol Table Overrides Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_inlinecode()
        {
            var result = CliOptionParser.Parse(
                new[]
                {
                    "--project",
                    "p",
                    "--inlinecode",
                    "Tableau.JavaScript.Vql.Core.ScriptEx.Value<T>(T a, T b)",
                    "({a}) || ({b})"
                });

            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.InlineCodeOverrides.Should()
                .HaveCount(1)
                .And.Contain("Tableau.JavaScript.Vql.Core.ScriptEx.Value<T>(T a, T b)", "({a}) || ({b})");
        }

        [Test]
        public void Parse_should_use_the_last_value_of_inlinecode_for_the_same_symbol()
        {
            var result = CliOptionParser.Parse(
                new[] { "--project", "p", "--inlinecode", "A", "a", "--inlinecode", "A", "b" });
            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.InlineCodeOverrides.Should().HaveCount(1).And.Contain("A", "b");
        }

        [Test]
        public void Parse_should_return_an_error_when_inlinecode_is_missing_arguments()
        {
            var result = CliOptionParser.Parse(new[] { "--project", "p", "--inlinecode" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingSymbolForOption("--inlinecode"));

            result = CliOptionParser.Parse(new[] { "--project", "p", "--inlinecode", "A" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should().ContainSingle().And.BeEquivalentTo(DiagnosticFactory.MissingValueForOption("--inlinecode A"));
        }

        [Test]
        public void Parse_should_recognize_scriptname()
        {
            var result = CliOptionParser.Parse(
                new[] { "--project", "p", "--scriptname", "System.Text.StringBuilder", "sb" });

            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.ScriptNameOverrides.Should()
                .HaveCount(1)
                .And.Contain("System.Text.StringBuilder", "sb");
        }

        [Test]
        public void Parse_should_use_the_last_value_of_scriptname_for_the_same_symbol()
        {
            var result = CliOptionParser.Parse(
                new[] { "--project", "p", "--scriptname", "A", "a", "--scriptname", "A", "b" });
            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.ScriptNameOverrides.Should().HaveCount(1).And.Contain("A", "b");
        }

        [Test]
        public void Parse_should_return_an_error_when_scriptname_is_missing_arguments()
        {
            var result = CliOptionParser.Parse(new[] { "--project", "p", "--scriptname" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should()
                .ContainSingle()
                .And.BeEquivalentTo(DiagnosticFactory.MissingSymbolForOption("--scriptname"));

            result = CliOptionParser.Parse(new[] { "--project", "p", "--scriptname", "A" });
            result.Success.Should().BeFalse();
            result.Diagnostics.Should().ContainSingle().And.BeEquivalentTo(DiagnosticFactory.MissingValueForOption("--scriptname A"));
        }

        [Test]
        public void Parse_should_allow_both_an_inlinecode_and_scriptname_for_the_same_symbol()
        {
            var result = CliOptionParser.Parse(
                new[] { "--project", "p", "--inlinecode", "A", "code", "--scriptname", "A", "name" });
            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.Overrides.Should()
                .HaveCount(1)
                .And.Contain("A", new SymbolTableOverride("code", "name"));
        }

        [Test]
        public void Parse_should_use_ordinal_comparisons_for_inlinecode_and_scriptname()
        {
            var result = CliOptionParser.Parse(
                new[] { "--project", "p", "--inlinecode", "A", "code", "--scriptname", "a", "name" });
            result.Success.Should().BeTrue();
            result.Result.SymbolTableOverrides.Overrides.Should()
                .HaveCount(2)
                .And.Contain(
                    new KeyValuePair<string, SymbolTableOverride>("A", new SymbolTableOverride("code")),
                    new KeyValuePair<string, SymbolTableOverride>("a", new SymbolTableOverride(scriptName: "name")));
        }
    }
}
