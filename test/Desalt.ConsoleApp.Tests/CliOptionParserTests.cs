// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionParserTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public class CliOptionParserTests
    {
        private static CliOptions AssertParse(string[] args, FakeFileContentFetcher? fileFetcher = null)
        {
            fileFetcher ??= new FakeFileContentFetcher();
            var result = CliOptionParser.Parse(args, fileFetcher);
            result.Diagnostics.Should().BeEmpty();
            result.Success.Should().BeTrue();

            return result.Result;
        }

        private static void AssertParseError(
            string[] args,
            Diagnostic expectedDiagnostic,
            FakeFileContentFetcher? fileFetcher = null)
        {
            AssertParseError(args, new[] { expectedDiagnostic }, fileFetcher);
        }

        private static void AssertParseError(
            string[] args,
            Diagnostic[] expectedDiagnostics,
            FakeFileContentFetcher? fileFetcher = null)
        {
            fileFetcher ??= new FakeFileContentFetcher();
            var result = CliOptionParser.Parse(args, fileFetcher);
            result.Diagnostics.Should().BeEquivalentTo(expectedDiagnostics.Cast<object>());
        }

        //// ===========================================================================================================
        //// --help and --version Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_version()
        {
            var options = AssertParse(new[] { "--version" });
            options.ShouldShowVersion.Should().BeTrue();

            options = AssertParse(new[] { "-v" });
            options.ShouldShowVersion.Should().BeTrue();
        }

        [Test]
        public void Parse_should_recognize_help()
        {
            var options = AssertParse(new[] { "--help" });
            options.ShouldShowHelp.Should().BeTrue();

            options = AssertParse(new[] { "-?" });
            options.ShouldShowHelp.Should().BeTrue();
        }

        [Test]
        public void Parse_should_use_version_or_help_and_succeed_even_if_there_are_other_errors()
        {
            var options = AssertParse(new[] { "--unknown", "--version" });
            options.ShouldShowVersion.Should().BeTrue();

            options = AssertParse(new[] { "--unknown", "-?" });
            options.ShouldShowHelp.Should().BeTrue();
        }

        //// ===========================================================================================================
        //// Error Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_return_an_error_on_an_unrecognized_option()
        {
            AssertParseError(new[] { "--unknown" }, DiagnosticFactory.UnrecognizedOption("--unknown"));
        }

        //// ===========================================================================================================
        //// File-based Options Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_project()
        {
            var options = AssertParse(new[] { "--project", "Proj.csproj" });
            options.Should().BeEquivalentTo(new CliOptions { ProjectFile = "Proj.csproj" });
        }

        [Test]
        public void Parse_should_use_the_last_argument_for_project()
        {
            var options = AssertParse(new[] { "--project", "Proj.csproj", "--project", "ProjB.csproj" });
            options.Should().BeEquivalentTo(new CliOptions { ProjectFile = "ProjB.csproj" });
        }

        [Test]
        public void Parse_should_return_an_error_when_project_is_missing_the_value()
        {
            AssertParseError(new[] { "--project" }, DiagnosticFactory.MissingFileSpecification("--project"));
            AssertParseError(
                new[] { "--project", "--nologo" },
                DiagnosticFactory.MissingFileSpecification("--project"));
        }

        [Test]
        public void Parse_should_recognize_out()
        {
            var options = AssertParse(new[] { "--out", "Directory" });
            options.Should().BeEquivalentTo(new CliOptions { OutDirectory = "Directory" });
        }

        [Test]
        public void Parse_should_use_the_last_argument_for_out()
        {
            var options = AssertParse(new[] { "--out", "A", "--out", "B" });
            options.Should().BeEquivalentTo(new CliOptions { OutDirectory = "B" });
        }

        [Test]
        public void Parse_should_return_an_error_when_out_is_missing_the_value()
        {
            AssertParseError(new[] { "--out" }, DiagnosticFactory.MissingFileSpecification("--out"));
            AssertParseError(new[] { "--out", "--nologo" }, DiagnosticFactory.MissingFileSpecification("--out"));
        }

        //// ===========================================================================================================
        //// Other Options Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_nologo()
        {
            var options = AssertParse(new[] { "--nologo" });
            options.NoLogo.Should().BeTrue();
        }

        [Test]
        public void Parse_should_allow_escaped_strings_as_args()
        {
            var options = AssertParse(new[] { "--project", "\"embedded \\\"quote and \\\\ backslash\"" });
            options.ProjectFile.Should().Be("embedded \"quote and \\ backslash");
        }

        //// ===========================================================================================================
        //// Warnings and Errors Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_warn()
        {
            var options = AssertParse(new[] { "--warn", "2" });
            options.WarningLevel.Should().Be(2);

            options = AssertParse(new[] { "-w", "2" });
            options.WarningLevel.Should().Be(2);
        }

        [Test]
        public void Parse_should_use_the_last_value_for_warn()
        {
            var options = AssertParse(new[] { "--warn", "2", "-w", "1" });
            options.WarningLevel.Should().Be(1);
        }

        [Test]
        public void Parse_should_return_an_error_when_warn_has_no_value_or_an_invalid_value()
        {
            AssertParseError(new[] { "--warn" }, DiagnosticFactory.MissingNumberForOption("--warn"));
            AssertParseError(
                new[] { "--warn", "--project", "Project.csproj" },
                DiagnosticFactory.MissingNumberForOption("--warn"));
            AssertParseError(new[] { "--warn", "not-a-number" }, DiagnosticFactory.MissingNumberForOption("--warn"));
        }

        [Test]
        public void Parse_should_recognize_nowarn()
        {
            var options = AssertParse(new[] { "--nowarn", "CS2008" });
            options.SpecificDiagnosticOptions.Should()
                .HaveCount(1)
                .And.Contain("CS2008", ReportDiagnostic.Suppress);

            options = AssertParse(new[] { "--nowarn", ";CS2008,CS2009;CS2010," });
            options.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Suppress);
        }

        [Test]
        public void Parse_should_combine_the_values_for_nowarn()
        {
            var options = AssertParse(new[] { "--nowarn", "CS2008,CS2009", "--nowarn", "CS2010" });
            options.SpecificDiagnosticOptions.Should().HaveCount(3).And.ContainKeys("CS2008", "CS2009", "CS2010");
        }

        [Test]
        public void Parse_should_return_an_error_when_nowarn_has_no_value_or_an_invalid_value()
        {
            AssertParseError(new[] { "--nowarn" }, DiagnosticFactory.MissingValueForOption("--nowarn"));
            AssertParseError(
                new[] { "--nowarn", "--project", "Project.csproj" },
                DiagnosticFactory.MissingValueForOption("--nowarn"));
        }

        [Test]
        public void Parse_should_recognize_warnaserror_as_a_flag()
        {
            var options = AssertParse(new[] { "--warnaserror" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror+" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror-" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Default);

            options = AssertParse(new[] { "--warnaserror+", "--project", "Project.csproj" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_recognize_warnaserror_as_a_list()
        {
            var options = AssertParse(new[] { "--warnaserror", "CS2008" });
            options.SpecificDiagnosticOptions.Should().HaveCount(1).And.Contain("CS2008", ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror", ";CS2008,CS2009 CS2010," });
            options.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror", ";CS2008,CS2009 CS2010," });
            options.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.ContainKeys("CS2008", "CS2009", "CS2010")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror-", "CS2008" });
            options.SpecificDiagnosticOptions.Should().BeEmpty();

            options = AssertParse(new[] { "--warnaserror-", ";CS2008,CS2009;CS2010," });
            options.SpecificDiagnosticOptions.Should().BeEmpty();
        }

        [Test]
        public void Parse_should_give_precedence_to_later_arguments_with_warnaserror()
        {
            var options = AssertParse(new[] { "--warnaserror", "--warnaserror-" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Default);

            options = AssertParse(new[] { "--warnaserror-", "--warnaserror+" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_aggregate_warnaserror_values()
        {
            var options = AssertParse(
                new[]
                {
                    "--warnaserror", "CS2008;CS2009",
                    "--warnaserror-", "CS123",
                    "--warnaserror-", "CS2008;CS2010;CS2011",
                    "--warnaserror+", "CS2011"
                });

            options.SpecificDiagnosticOptions.Should()
                .HaveCount(2)
                .And.ContainKeys("CS2009", "CS2011")
                .And.Subject.Values.Should()
                .AllBeEquivalentTo(ReportDiagnostic.Error);
        }

        [Test]
        public void Parse_should_give_nowarn_precedence_over_warnaserror()
        {
            var options = AssertParse(new[] { "--warnaserror", "1,2", "--nowarn", "3", "--warnaserror", "3" });
            options.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.Contain(
                    new KeyValuePair<string, ReportDiagnostic>("1", ReportDiagnostic.Error),
                    new KeyValuePair<string, ReportDiagnostic>("2", ReportDiagnostic.Error),
                    new KeyValuePair<string, ReportDiagnostic>("3", ReportDiagnostic.Suppress));
        }

        [Test]
        public void Parse_should_clear_warnaserror_lists_when_used_as_a_flag()
        {
            var options = AssertParse(new[] { "--warnaserror", "1,2", "--warnaserror" });
            options.SpecificDiagnosticOptions.Should().BeEmpty();

            options = AssertParse(new[] { "--warnaserror", "1,2", "--warnaserror-" });
            options.SpecificDiagnosticOptions.Should().BeEmpty();
        }

        [Test]
        public void Parse_should_not_set_the_general_diagnostic_option_to_default_when_a_list_is_encountered()
        {
            var options = AssertParse(new[] { "--warnaserror", "--warnaserror", "1,2" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);

            options = AssertParse(new[] { "--warnaserror", "--warnaserror-", "1,2" });
            options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        //// ===========================================================================================================
        //// Symbol Table Overrides Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_inlinecode()
        {
            var options = AssertParse(
                new[]
                {
                    "--project",
                    "p",
                    "--inlinecode",
                    "Tableau.JavaScript.Vql.Core.ScriptEx.Value<T>(T a, T b)",
                    "({a}) || ({b})"
                });

            options.SymbolTableOverrides.InlineCodeOverrides.Should()
                .HaveCount(1)
                .And.Contain("Tableau.JavaScript.Vql.Core.ScriptEx.Value<T>(T a, T b)", "({a}) || ({b})");
        }

        [Test]
        public void Parse_should_use_the_last_value_of_inlinecode_for_the_same_symbol()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--inlinecode", "A", "a", "--inlinecode", "A", "b" });
            options.SymbolTableOverrides.InlineCodeOverrides.Should().HaveCount(1).And.Contain("A", "b");
        }

        [Test]
        public void Parse_should_return_an_error_when_inlinecode_is_missing_arguments()
        {
            AssertParseError(
                new[] { "--project", "p", "--inlinecode" },
                DiagnosticFactory.MissingSymbolForOption("--inlinecode"));
            AssertParseError(
                new[] { "--project", "p", "--inlinecode", "A" },
                DiagnosticFactory.MissingValueForOption("--inlinecode A"));
        }

        [Test]
        public void Parse_should_recognize_scriptname()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--scriptname", "System.Text.StringBuilder", "sb" });

            options.SymbolTableOverrides.ScriptNameOverrides.Should()
                .HaveCount(1)
                .And.Contain("System.Text.StringBuilder", "sb");
        }

        [Test]
        public void Parse_should_use_the_last_value_of_scriptname_for_the_same_symbol()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--scriptname", "A", "a", "--scriptname", "A", "b" });
            options.SymbolTableOverrides.ScriptNameOverrides.Should().HaveCount(1).And.Contain("A", "b");
        }

        [Test]
        public void Parse_should_return_an_error_when_scriptname_is_missing_arguments()
        {
            AssertParseError(
                new[] { "--project", "p", "--scriptname" },
                DiagnosticFactory.MissingSymbolForOption("--scriptname"));
            AssertParseError(
                new[] { "--project", "p", "--scriptname", "A" },
                DiagnosticFactory.MissingValueForOption("--scriptname A"));
        }

        [Test]
        public void Parse_should_allow_both_an_inlinecode_and_scriptname_for_the_same_symbol()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--inlinecode", "A", "code", "--scriptname", "A", "name" });
            options.SymbolTableOverrides.Overrides.Should()
                .HaveCount(1)
                .And.Contain("A", new SymbolTableOverride("code", "name"));
        }

        [Test]
        public void Parse_should_use_ordinal_comparisons_for_inlinecode_and_scriptname()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--inlinecode", "A", "code", "--scriptname", "a", "name" });
            options.SymbolTableOverrides.Overrides.Should()
                .HaveCount(2)
                .And.Contain(
                    new KeyValuePair<string, SymbolTableOverride>("A", new SymbolTableOverride("code")),
                    new KeyValuePair<string, SymbolTableOverride>("a", new SymbolTableOverride(scriptName: "name")));
        }

        //// ===========================================================================================================
        //// Rename Rules Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_recognize_enum_rename_rule()
        {
            var options = AssertParse(new[] { "--project", "p", "--enum-rename-rule", "lower-first" });
            options.RenameRules.EnumRule.Should().Be(EnumRenameRule.LowerCaseFirstChar);

            options = AssertParse(new[] { "--project", "p", "--enum-rename-rule", "match-csharp" });
            options.RenameRules.EnumRule.Should().Be(EnumRenameRule.MatchCSharpName);
        }

        [Test]
        public void Parse_should_use_the_last_value_for_enum_rename_rule()
        {
            var options = AssertParse(
                new[] { "--project", "p", "--enum-rename-rule", "match-csharp", "--enum-rename-rule", "lower-first" });
            options.RenameRules.EnumRule.Should().Be(EnumRenameRule.LowerCaseFirstChar);
        }

        [Test]
        public void Parse_should_return_an_error_when_missing_a_value_for_enum_rename_rule()
        {
            AssertParseError(
                new[] { "--project", "p", "--enum-rename-rule" },
                DiagnosticFactory.MissingValueForOption("--enum-rename-rule"));
        }

        [Test]
        public void Parse_should_return_an_error_for_an_invalid_enum_rename_rule_value()
        {
            AssertParseError(
                new[] { "--project", "p", "--enum-rename-rule", "not-valid" },
                DiagnosticFactory.InvalidValueForOption("--enum-rename-rule", "not-valid"));
        }

        [Test]
        public void Parse_should_recognize_field_rename_rules()
        {
            var options = AssertParse(new[] { "--project", "p", "--field-rename-rule", "lower-first" });
            options.RenameRules.FieldRule.Should().Be(FieldRenameRule.LowerCaseFirstChar);

            options = AssertParse(new[] { "--project", "p", "--field-rename-rule", "private-dollar-prefix" });
            options.RenameRules.FieldRule.Should().Be(FieldRenameRule.PrivateDollarPrefix);

            options = AssertParse(new[] { "--project", "p", "--field-rename-rule", "dollar-prefix-for-duplicates" });
            options.RenameRules.FieldRule.Should().Be(FieldRenameRule.DollarPrefixOnlyForDuplicateName);
        }

        [Test]
        public void Parse_should_use_the_last_value_for_field_rename_rules()
        {
            var options = AssertParse(
                new[]
                {
                    "--project",
                    "p",
                    "--field-rename-rule",
                    "lower-first",
                    "--field-rename-rule",
                    "private-dollar-prefix"
                });
            options.RenameRules.FieldRule.Should().Be(FieldRenameRule.PrivateDollarPrefix);
        }

        [Test]
        public void Parse_should_return_an_error_when_missing_a_value_for_field_rename_rules()
        {
            AssertParseError(
                new[] { "--project", "p", "--field-rename-rule" },
                DiagnosticFactory.MissingValueForOption("--field-rename-rule"));
        }

        [Test]
        public void Parse_should_return_an_error_for_an_invalid_field_rename_rules_value()
        {
            AssertParseError(
                new[] { "--project", "p", "--field-rename-rule", "not-valid" },
                DiagnosticFactory.InvalidValueForOption("--field-rename-rule", "not-valid"));
        }

        //// ===========================================================================================================
        //// Response File Tests
        //// ===========================================================================================================

        [Test]
        public void Parse_should_accept_a_response_file()
        {
            string[] lines =
            {
                "--warn 3",
                "--warnaserror 1,2,3",
                "--inlinecode \"System.Console.WriteLine(char[] buffer, int index)\" \"console.log(\\\"hi\\n\\\")\"",
                "--scriptname \"System.Console\" \"console\"",
                "--enum-rename-rule match-csharp",
                "--field-rename-rule dollar-prefix-for-duplicates"
            };

            var options = AssertParse(
                new[] { "--project", "p", "@response.rsp" },
                new FakeFileContentFetcher("response.rsp", lines));

            options.Should()
                .BeEquivalentTo(
                    new CliOptions
                    {
                        ProjectFile = "p",
                        WarningLevel = 3,
                        SpecificDiagnosticOptions =
                            ImmutableDictionary.CreateRange(
                                new[]
                                {
                                    new KeyValuePair<string, ReportDiagnostic>("1", ReportDiagnostic.Error),
                                    new KeyValuePair<string, ReportDiagnostic>("2", ReportDiagnostic.Error),
                                    new KeyValuePair<string, ReportDiagnostic>("3", ReportDiagnostic.Error),
                                }),
                        SymbolTableOverrides = new SymbolTableOverrides(
                            new KeyValuePair<string, SymbolTableOverride>(
                                "System.Console.WriteLine(char[] buffer, int index)",
                                new SymbolTableOverride(inlineCode: @"console.log(""hi\n"")")),
                            new KeyValuePair<string, SymbolTableOverride>(
                                "System.Console",
                                new SymbolTableOverride(scriptName: "console"))),
                        RenameRules = new RenameRules(
                            EnumRenameRule.MatchCSharpName,
                            FieldRenameRule.DollarPrefixOnlyForDuplicateName),
                    });
        }

        [Test]
        public void Parse_should_return_an_error_if_the_response_file_cannot_be_loaded()
        {
            AssertParseError(
                new[] { "--project", "p", "@nothing.txt" },
                DiagnosticFactory.ErrorOpeningResponseFile("nothing.txt"));
        }
    }
}
