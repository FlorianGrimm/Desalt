// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImportSymbolTableTests
    {
        private static async Task AssertOnSymbolTableAsync(
            string csharpCode,
            Action<ImportSymbolTable, DocumentTranslationContext> assertAction)
        {
            using (var tempProject = TempProject.Create("Test", new TempProjectFile("File.cs", csharpCode)))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs");
                var importTable = new ImportSymbolTable();
                importTable.AddDefinedTypesInDocument(context);

                assertAction(importTable, context);
            }
        }

        private static Task AssertHasSymbolsAsync(
            string csharpCode,
            params string[] expectedSymbolNames)
        {
            return AssertOnSymbolTableAsync(
                csharpCode,
                (importTable, context) =>
                {
                    foreach (string expectedSymbolName in expectedSymbolNames)
                    {
                        ISymbol symbol = GetSymbol(expectedSymbolName, context);
                        importTable.HasSymbol(symbol)
                            .Should()
                            .BeTrue(because: $"'{expectedSymbolName}' should have been declared in the file");
                    }
                });
        }

        private static ISymbol GetSymbol(string name, DocumentTranslationContext context)
        {
            return context.RootSyntax.DescendantNodes()
                .Select(
                    node =>
                    {
                        switch (node)
                        {
                            case BaseTypeDeclarationSyntax typeDeclaration:
                                return context.SemanticModel.GetDeclaredSymbol(typeDeclaration);

                            case DelegateDeclarationSyntax delegateDeclaration:
                                return context.SemanticModel.GetDeclaredSymbol(delegateDeclaration);

                            default:
                                return null;
                        }
                    })
                .First(symbol => SymbolTable.KeyFromSymbol(symbol) == name);
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
        public async Task Indexer_should_return_the_file_name_of_the_found_symbol()
        {
            await AssertOnSymbolTableAsync(
                "class MyClass {}",
                (importTable, context) =>
                {
                    var symbol = GetSymbol("MyClass", context);
                    importTable[symbol]
                        .RelativeTypeScriptFilePathOrModuleName.Should()
                        .Be(Path.Combine("outputPath", "File.ts"));
                });
        }
    }
}
