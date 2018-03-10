// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="EndToEndTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EndToEndTests
    {
        public TestContext TestContext { get; set; }

        //[Ignore]
        [TestCategory("E2E")]
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

            var options = new CompilerOptions(outputPath);
            var request = new CompilationRequest(projectFilePath, options);
            var compiler = new Compiler();
            IExtendedResult<bool> result = await compiler.ExecuteAsync(request);

            result.Diagnostics.Should().BeEmpty();
        }
    }
}
