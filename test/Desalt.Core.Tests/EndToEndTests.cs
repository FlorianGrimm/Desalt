// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Options;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using NUnit.Framework;
    using DiagnosticId = Core.Diagnostics.DiagnosticFactory.DiagnosticId;

    public class EndToEndTests
    {
        private static string RootDirectory
        {
            get
            {
                string directory = TestContext.CurrentContext.TestDirectory;
                while (Path.GetFileName(directory) != "Desalt")
                {
                    directory = Path.GetDirectoryName(directory)!;
                }

                return directory;
            }
        }

        private static string OutputDirectory => Path.Combine(RootDirectory, "E2ETestResults", "CoreSubset");

        private static string ProjectFilePath =>
            Path.Combine(RootDirectory, "test", "SaltarelleProjectTests", "CoreSubset", "CoreSubset.csproj");

        private static string OptionsFilePath => Path.Combine(OutputDirectory, "desaltOptions.json");

        [Category("SkipWhenLiveUnitTesting")]
        [Test]
        public async Task E2E_Compiling_a_Saltarelle_Core_project()
        {
            string outputPath = OutputDirectory;
            string projectFilePath = ProjectFilePath;

            // for now, turn off some errors and warnings until we implement rewriters
            var ignoredDiagnostics =
                (from id in new[]
                 {
                     DiagnosticId.ClassWithDuplicateFieldAndPropertyName,
                     DiagnosticId.InterfaceWithDefaultParameter,
                     DiagnosticId.UnsupportedAccessibility,
                     DiagnosticId.GetterAndSetterAccessorsDoNotAgreeInVisibility,
                 }
                 select new KeyValuePair<string, ReportDiagnostic>(
                     DiagnosticFactory.IdFromDiagnosticId(id),
                     ReportDiagnostic.Suppress)).ToImmutableDictionary();

            // for now, add a $ prefix to private fields until we implement rewriters
            RenameRules renameRules =
                RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName);

            // create the symbol table overrides
            var overrides = new SymbolTableOverrides(
                new KeyValuePair<string, SymbolTableOverride>(
                    "Tableau.JavaScript.Vql.Core.ScriptEx.Value<T>(T a, T b)",
                    new SymbolTableOverride(inlineCode: "({a}) || ({b})")));

            var options = new CompilerOptions(
                outputPath: outputPath,
                specificDiagnosticOptions: ignoredDiagnostics,
                renameRules: renameRules,
                symbolTableOverrides: overrides);

            // serialize the options, just so we can see it and use it elsewhere if needed
            // (but change the output path to be '.'
            options.WithOutputPath(".").Serialize(OptionsFilePath);

            var request = new CompilationRequest(projectFilePath, options);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);

            result.Diagnostics.Should().BeEmpty();
        }
    }
}
