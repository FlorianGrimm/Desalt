// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImportSymbolTableTests
    {
        private static async Task<ImportSymbolTable> AssertHasSymbolsAsync(
            string csharpCode,
            params string[] expectedSymbolNames)
        {
            using (var tempProject = TempProject.Create("Test", new TempProjectFile("File.cs", csharpCode)))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs");
                var importTable = new ImportSymbolTable();
                importTable.AddDefinedTypesInDocument(context);

                foreach (string expectedSymbolName in expectedSymbolNames)
                {
                    importTable.HasSymbol(expectedSymbolName)
                        .Should()
                        .BeTrue(because: $"'{expectedSymbolName}' should have been declared in the file");
                }

                return importTable;
            }
        }

        [TestMethod]
        public async Task AddDefinedTypesInDocument_should_add_all_of_the_interfaces_classes_enums_and_delegates()
        {
            const string code = @"
interface IMyInterface {}
class MyClass {}
enum MyEnum {}
delegate void MyDelegate();
";

            await AssertHasSymbolsAsync(code, "IMyInterface", "MyClass", "MyEnum", "MyDelegate");
        }

        [TestMethod]
        public void Indexer_should_throw_when_there_is_no_defined_symbol()
        {
            var importTable = new ImportSymbolTable();
            Action action = () =>
            {
                var x = importTable["Unknown"];
            };

            action.Should().ThrowExactly<KeyNotFoundException>();
        }

        [TestMethod]
        public async Task Indexer_should_return_the_file_name_of_the_found_symbol()
        {
            ImportSymbolTable importTable = await AssertHasSymbolsAsync("class MyClass {}", "MyClass");
            importTable["MyClass"].RelativeTypeScriptFilePathOrModuleName.Should().Be(Path.Combine("outputPath", "File.ts"));
        }
    }
}
