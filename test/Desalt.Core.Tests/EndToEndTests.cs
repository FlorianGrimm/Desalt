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
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using DiagnosticId = Core.Diagnostics.DiagnosticFactory.DiagnosticId;

    [TestClass]
    public class EndToEndTests
    {
        public TestContext TestContext { get; set; }

        private string RootDirectory
        {
            get
            {
                string directory = TestContext.TestRunDirectory;
                while (Path.GetFileName(directory) != "Desalt")
                {
                    directory = Path.GetDirectoryName(directory);
                }

                return directory;
            }
        }

        private string OutputDirectory => Path.Combine(RootDirectory, "E2ETestResults", "CoreSubset");

        private string ProjectFilePath =>
            Path.Combine(RootDirectory, "test", "SaltarelleProjectTests", "CoreSubset", "CoreSubset.csproj");

        [TestCategory("SkipWhenLiveUnitTesting")]
        [TestMethod]
        public async Task E2E_Compiling_a_Saltarelle_Core_project()
        {
            string outputPath = OutputDirectory;
            string projectFilePath = ProjectFilePath;

            //foreach (string file in Directory.EnumerateFiles(outputPath))
            //{
            //    File.Delete(file);
            //}

            // for now, turn off some errors and warnings until we implement rewriters
            var ignoredDiagnostics =
                (from id in new[]
                 {
                     DiagnosticId.ClassWithDuplicateFieldAndPropertyName,
                     DiagnosticId.InterfaceWithDefaultParameter
                 }
                 select new KeyValuePair<string, ReportDiagnostic>(
                     DiagnosticFactory.IdFromDiagnosticId(id),
                     ReportDiagnostic.Suppress)).ToImmutableDictionary();

            // for now, add a $ prefix to private fields until we implement rewriters
            var renameRules =
                RenameRules.Default.WithFieldRule(FieldRenameRule.DollarPrefixOnlyForDuplicateName);

            var options = new CompilerOptions(
                outputPath,
                specificDiagnosticOptions: ignoredDiagnostics,
                renameRules: renameRules);

            var request = new CompilationRequest(projectFilePath, options);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);

            result.Diagnostics.Should().BeEmpty();
        }
    }
}
