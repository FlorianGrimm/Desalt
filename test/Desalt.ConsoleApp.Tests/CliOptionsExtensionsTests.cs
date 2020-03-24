// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CliOptionsExtensionsTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.ConsoleApp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;

    public class CliOptionsExtensionsTests
    {
        [Test]
        public void ToCompilationRequest_should_convert_to_a_valid_CompilationRequest_object()
        {
            var cliOptions = new CliOptions { ProjectFile = "projectFile", OutDirectory = "out", WarningLevel = 3, };
            var expectedOptions = new CompilerOptions("out", WarningLevel.Minor);

            cliOptions.ToCompilationRequest()
                .Should()
                .BeEquivalentTo(new CompilationRequest("projectFile", expectedOptions));
        }

        [Test]
        public void ToCompilationRequest_should_correctly_set_the_general_diagnostic_option()
        {
            var cliOptions = new CliOptions
            {
                ProjectFile = "projectFile",
                GeneralDiagnosticOption = ReportDiagnostic.Error
            };
            cliOptions.ToCompilationRequest().Options.GeneralDiagnosticOption.Should().Be(ReportDiagnostic.Error);
        }

        [Test]
        public void ToCompilationRequest_should_correctly_set_the_specific_diagnostics()
        {
            var cliOptions = new CliOptions
            {
                ProjectFile = "project",
                SpecificDiagnosticOptions = ImmutableDictionary.CreateRange(
                    StringComparer.Ordinal,
                    new[]
                    {
                        new KeyValuePair<string, ReportDiagnostic>("DSC001", ReportDiagnostic.Error),
                        new KeyValuePair<string, ReportDiagnostic>("DSC002", ReportDiagnostic.Error),
                        new KeyValuePair<string, ReportDiagnostic>("DSC123", ReportDiagnostic.Suppress),
                    }),
            };

            cliOptions.ToCompilationRequest()
                .Options.SpecificDiagnosticOptions.Should()
                .HaveCount(3)
                .And.Equal(
                    new Dictionary<string, ReportDiagnostic>
                    {
                        ["DSC001"] = ReportDiagnostic.Error,
                        ["DSC002"] = ReportDiagnostic.Error,
                        ["DSC123"] = ReportDiagnostic.Suppress,
                    });
        }

        [Test]
        public void ToCompilationRequest_should_correctly_set_the_symbol_table_overrides()
        {
            var cliOptions = new CliOptions
            {
                ProjectFile = "project",
                SymbolTableOverrides = new SymbolTableOverrides(
                    new KeyValuePair<string, SymbolTableOverride>(
                        "SymbolA",
                        new SymbolTableOverride("code", "scriptName")),
                    new KeyValuePair<string, SymbolTableOverride>(
                        "SymbolB",
                        new SymbolTableOverride(scriptName: "b")))
            };

            cliOptions.ToCompilationRequest()
                .Options.SymbolTableOverrides.Overrides.Should()
                .HaveCount(2)
                .And.Contain(
                    new KeyValuePair<string, SymbolTableOverride>(
                        "SymbolA",
                        new SymbolTableOverride("code", "scriptName")),
                    new KeyValuePair<string, SymbolTableOverride>("SymbolB", new SymbolTableOverride(scriptName: "b")));
        }

        [Test]
        public void ToCompilationRequest_should_correctly_set_the_rename_rules()
        {
            var cliOptions = new CliOptions
            {
                ProjectFile = "project",
                RenameRules = new RenameRules(
                    EnumRenameRule.LowerCaseFirstChar,
                    FieldRenameRule.PrivateDollarPrefix),
            };

            cliOptions.ToCompilationRequest()
                .Options.RenameRules.Should()
                .BeEquivalentTo(
                    new RenameRules(EnumRenameRule.LowerCaseFirstChar, FieldRenameRule.PrivateDollarPrefix));
        }
    }
}
