﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CompileOptionsTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Text;
    using CompilerUtilities;
    using Desalt.Core.Diagnostics;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CompileOptionsTests
    {
        private const string JsonContents = @"{
  ""outputPath"": ""outDir"",
  ""warningLevel"": ""minor"",
  ""generalDiagnosticOption"": ""error"",
  ""specificDiagnosticOptions"": {
    ""DSC1003"": ""error"",
    ""DSC1005"": ""info"",
    ""DSC1012"": ""suppress""
  },
  ""renameRules"": {
    ""fieldRule"": ""dollarPrefixOnlyForDuplicateName""
  },
  ""symbolTableOverrides"": {
    ""System.Script"": {
      ""scriptName"": ""ss""
    },
    ""System.String.ToString()"": {
      ""inlineCode"": ""console.log({this})""
    }
  }
}";

        private static readonly CompilerOptions s_overridesObject = new CompilerOptions(
            outputPath: "outDir",
            warningLevel: WarningLevel.Minor,
            generalDiagnosticOption: ReportDiagnostic.Error,
            specificDiagnosticOptions: new[]
            {
                new KeyValuePair<string, ReportDiagnostic>("DSC1005", ReportDiagnostic.Info),
                new KeyValuePair<string, ReportDiagnostic>("DSC1003", ReportDiagnostic.Error),
                new KeyValuePair<string, ReportDiagnostic>("DSC1012", ReportDiagnostic.Suppress),
            }.ToImmutableDictionary(),
            renameRules: RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName),
            symbolTableOverrides: new SymbolTableOverrides(
                new KeyValuePair<string, SymbolTableOverride>(
                    "System.Script",
                    new SymbolTableOverride(scriptName: "ss")),
                new KeyValuePair<string, SymbolTableOverride>(
                    "System.String.ToString()",
                    new SymbolTableOverride(inlineCode: "console.log({this})"))));

        [TestMethod]
        public void CompilerOptions_should_serialize_to_a_json_stream()
        {
            using (var stream = new MemoryStream())
            {
                s_overridesObject.Serialize(stream);
                stream.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    reader.ReadToEnd().Should().Be(JsonContents);
                }
            }
        }

        [TestMethod]
        public void CompilerOptions_should_deserialize_from_a_json_stream()
        {
            using (var stream = new UnicodeStringStream(JsonContents))
            {
                IExtendedResult<CompilerOptions> deserialized = CompilerOptions.Deserialize(stream);
                deserialized.Diagnostics.Should().BeEmpty();
                deserialized.Result.Should().BeEquivalentTo(s_overridesObject);
            }
        }

        [TestMethod]
        public void CompilerOptions_should_add_a_diagnostic_message_if_the_file_path_contains_invalid_chars()
        {
            string invalidPath = @"N:\InvalidPath" + Path.GetInvalidFileNameChars()[0];
            var result = CompilerOptions.Deserialize(invalidPath);
            result.Diagnostics.Should()
                .BeEquivalentTo(DiagnosticFactory.InvalidOptionsFile(invalidPath, "Illegal characters in path."));
        }

        [TestMethod]
        public void CompilerOptions_should_add_a_diagnostic_message_if_the_file_path_cannot_be_read()
        {
            const string invalidPath = @"N:\NotExistingPath";
            var result = CompilerOptions.Deserialize(invalidPath);
            result.Diagnostics.Should()
                .BeEquivalentTo(
                    DiagnosticFactory.InvalidOptionsFile(
                        invalidPath,
                        $"Could not find a part of the path '{invalidPath}'."));
        }

        [TestMethod]
        public void CompilerOptions_should_add_a_diagnostic_message_if_the_json_file_is_invalid()
        {
            using (var stream = new UnicodeStringStream("{ invalidJson {} }"))
            {
                string file = Path.GetFullPath("file");
                var result = CompilerOptions.Deserialize(stream, file);
                result.Diagnostics.Should()
                    .BeEquivalentTo(
                        DiagnosticFactory.InvalidOptionsFile(
                            file,
                            "Invalid character after parsing property name. Expected ':' but got: {. " +
                            "Path '', line 1, position 14."));
            }
        }

        [TestMethod]
        public void CompilerOptions_should_use_the_default_options_when_reading_an_empty_JSON_file()
        {
            using (var stream = new UnicodeStringStream("{}"))
            {
                var result = CompilerOptions.Deserialize(stream);
                result.Diagnostics.Should().BeEmpty();
                result.Result.Should().BeEquivalentTo(CompilerOptions.Default);
            }
        }
    }
}