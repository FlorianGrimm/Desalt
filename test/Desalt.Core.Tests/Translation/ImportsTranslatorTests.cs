// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportsTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ImportsTranslatorTests
    {
        private delegate IEnumerable<ISymbol> GetSymbolsFunc(DocumentTranslationContextWithSymbolTables context);

        private static IEnumerable<ISymbol> GetFirstFieldDeclarationSymbol(
            DocumentTranslationContextWithSymbolTables context)
        {
            FieldDeclarationSyntax fieldDeclaration =
                context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().First();

            yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
        }

        private static async Task AssertImports(string codeSnippet, params string[] expectedImportLines)
        {
            string code = $@"
using System;
using System.Collections.Generic;
using System.Html;
using System.Text.RegularExpressions;

class C
{{
    {codeSnippet}
}}
";

            await AssertImports(code, GetFirstFieldDeclarationSymbol, expectedImportLines);
        }

        private static async Task AssertImports(
            string code,
            GetSymbolsFunc getSymbolsFunc,
            params string[] expectedImportLines)
        {
            await AssertImports(new[] { new TempProjectFile("File.cs", code), }, getSymbolsFunc, expectedImportLines);
        }

        private static async Task AssertImports(
            TempProjectFile[] codeFiles,
            GetSymbolsFunc getSymbolsFunc,
            params string[] expectedImportLinesForFirstFile)
        {
            using (var tempProject = await TempProject.CreateAsync("TestProject", codeFiles))
            {
                DocumentTranslationContextWithSymbolTables context =
                    await tempProject.CreateContextWithSymbolTablesForFileAsync(codeFiles[0].FileName);

                IEnumerable<ISymbol> typesToImport = getSymbolsFunc(context);

                IExtendedResult<IEnumerable<ITsImportDeclaration>> results =
                    ImportsTranslator.TranslateImports(context, typesToImport);

                results.Diagnostics.Should().BeEmpty();

                var actualImportLines = results.Result.Select(import => import.EmitAsString().TrimEnd());
                actualImportLines.Should().BeEquivalentTo(expectedImportLinesForFirstFile);
            }
        }

        [TestMethod]
        public async Task ImportsTranslator_should_skip_native_types()
        {
            await AssertImports("int x = 1; object o = null;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_skip_array_types()
        {
            await AssertImports("int[] array;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_RegExp_types()
        {
            await AssertImports("Regex r;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_Function_types()
        {
            await AssertImports("Func<int, bool> func;");
            await AssertImports("Action<int, bool> action;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Array()
        {
            await AssertImports("List<int> list;");
            await AssertImports("JsArray<int> array;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Object()
        {
            await AssertImports("JsDictionary<string, int> dict;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Error()
        {
            await AssertImports("Error err;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_not_import_types_from_Saltarelle_Web()
        {
            await AssertImports("HtmlElement div;");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_alphabetize_symbol_names_within_a_group()
        {
            IEnumerable<ISymbol> GetSymbols(DocumentTranslationContextWithSymbolTables context)
            {
                IEnumerable<FieldDeclarationSyntax> fieldDeclarations =
                    context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>();

                foreach (FieldDeclarationSyntax fieldDeclaration in fieldDeclarations)
                {
                    yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
                }
            }

            await AssertImports(
                new[]
                {
                    new TempProjectFile("Foo.cs", "class Foo { B b; A a; C c; }"),
                    new TempProjectFile("Classes.cs", "class B {} class A {} class C {}")
                },
                GetSymbols,
                "import { A, B, C } from './Classes';");
        }

        [TestMethod]
        public async Task ImportsTranslator_should_get_all_of_the_referenced_files()
        {
            IEnumerable<ISymbol> GetSymbols(DocumentTranslationContextWithSymbolTables context)
            {
                IEnumerable<FieldDeclarationSyntax> fieldDeclarations =
                    context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>();

                foreach (FieldDeclarationSyntax fieldDeclaration in fieldDeclarations)
                {
                    yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
                }
            }

            await AssertImports(
                new[]
                {
                    new TempProjectFile("Foo.cs", "class Foo { B b; A a; C c; }"),
                    new TempProjectFile("AandB.cs", "class B {} class A {}"),
                    new TempProjectFile("C.cs", "class C {}"),
                },
                GetSymbols,
                "import { A, B } from './AandB';",
                "import { C } from './C';");
        }
    }
}
