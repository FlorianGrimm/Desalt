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

        [TestCategory("SkipWhenLiveUnitTesting")]
        [TestMethod]
        public async Task E2E_Compiling_a_Saltarelle_Core_project()
        {
            // TODO: generalize this so that it's not an absolute path
            const string outputPath = @"D:\github\Desalt\TestResults\CoreSubset";
            const string projectFilePath = @"D:\github\Desalt\test\SaltarelleProjectTests\CoreSubset\CoreSubset.csproj";

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
