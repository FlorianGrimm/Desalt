// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportsTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class ImportsTranslatorTests
    {
        private delegate IEnumerable<ITypeSymbol> GetSymbolsFunc(DocumentTranslationContextWithSymbolTables context);

        private static IEnumerable<ITypeSymbol> GetFirstFieldDeclarationSymbol(
            DocumentTranslationContextWithSymbolTables context)
        {
            FieldDeclarationSyntax fieldDeclaration =
                context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().First();

            yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type ??
                throw new InvalidOperationException(
                    "There's an error in the test code since a valid Type could not be found for the field declaration.");
        }

        private static async Task AssertNoImports(
            string codeSnippet,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes)
        {
            await AssertImports(codeSnippet, discoveryKind);
        }

        private static async Task AssertImports(
            string codeSnippet,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes,
            string[]? expectedImportLines = null)
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

            await AssertImports(
                code,
                GetFirstFieldDeclarationSymbol,
                discoveryKind,
                expectedImportLines ?? Array.Empty<string>());
        }

        private static async Task AssertImports(
            string code,
            GetSymbolsFunc getSymbolsFunc,
            SymbolDiscoveryKind discoveryKind,
            params string[] expectedImportLines)
        {
            await AssertImports(
                new[] { new TempProjectFile("File.cs", code), },
                getSymbolsFunc,
                discoveryKind,
                expectedImportLines);
        }

        private static async Task AssertImports(
            TempProjectFile[] codeFiles,
            GetSymbolsFunc getSymbolsFunc,
            SymbolDiscoveryKind discoveryKind,
            params string[] expectedImportLinesForFirstFile)
        {
            using TempProject tempProject = await TempProject.CreateAsync(codeFiles);
            DocumentTranslationContextWithSymbolTables context =
                await tempProject.CreateContextWithSymbolTablesForFileAsync(
                    codeFiles[0].FileName,
                    discoveryKind: discoveryKind);

            IEnumerable<ITypeSymbol> typesToImport = getSymbolsFunc(context);

            var importsTranslator = new ImportsTranslator(context.ScriptSymbolTable);
            IExtendedResult<IEnumerable<TypeScriptAst.Ast.ITsImportDeclaration>> results =
                importsTranslator.GatherImportDeclarations(context, typesToImport);

            results.Diagnostics.Should().BeEmpty();

            IEnumerable<string> actualImportLines = results.Result.Select(import => import.EmitAsString().TrimEnd());
            actualImportLines.Should().BeEquivalentTo(expectedImportLinesForFirstFile);
        }

        [Test]
        public async Task ImportsTranslator_should_skip_native_types()
        {
            await AssertNoImports("int x = 1; object o = null;");
        }

        [Test]
        public async Task ImportsTranslator_should_skip_array_types()
        {
            await AssertNoImports("int[] array;");
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_RegExp_types()
        {
            await AssertNoImports("Regex r;");
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_Function_types()
        {
            await AssertNoImports("Func<int, bool> func;");
            await AssertNoImports("Action<int, bool> action;");
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Array()
        {
            await AssertNoImports("List<int> list;", SymbolDiscoveryKind.DocumentAndReferencedTypes);
            await AssertNoImports("JsArray<int> array;", SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Object()
        {
            await AssertNoImports("JsDictionary<string, int> dict;", SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_types_that_get_translated_to_Error()
        {
            await AssertNoImports("Error err;", SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task ImportsTranslator_should_not_import_types_from_Saltarelle_Web()
        {
            await AssertNoImports("HtmlElement div;");
        }

        [Test]
        public async Task ImportsTranslator_should_alphabetize_symbol_names_within_a_group()
        {
            static IEnumerable<ITypeSymbol> GetSymbols(DocumentTranslationContextWithSymbolTables context)
            {
                IEnumerable<FieldDeclarationSyntax> fieldDeclarations =
                    context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>();

                foreach (FieldDeclarationSyntax fieldDeclaration in fieldDeclarations)
                {
                    yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type ??
                        throw new InvalidOperationException(
                            "There's an error in the test code since a valid Type could not be found for the field declaration.");
                }
            }

            await AssertImports(
                new[]
                {
                    new TempProjectFile("Foo.cs", "class Foo { B b; A a; C c; }"),
                    new TempProjectFile("Classes.cs", "class B {} class A {} class C {}")
                },
                GetSymbols,
                SymbolDiscoveryKind.OnlyDocumentTypes,
                "import { A, B, C } from './Classes';");
        }

        [Test]
        public async Task ImportsTranslator_should_get_all_of_the_referenced_files()
        {
            static IEnumerable<ITypeSymbol> GetSymbols(DocumentTranslationContextWithSymbolTables context)
            {
                IEnumerable<FieldDeclarationSyntax> fieldDeclarations =
                    context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>();

                foreach (FieldDeclarationSyntax fieldDeclaration in fieldDeclarations)
                {
                    yield return context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type ??
                        throw new InvalidOperationException(
                            "There's an error in the test code since a valid Type could not be found for the field declaration.");
                }
            }

            await AssertImports(
                new[]
                {
                    new TempProjectFile("Foo.cs", "class Foo { B b; A a; C c; }"),
                    new TempProjectFile("AAndB.cs", "class B {} class A {}"),
                    new TempProjectFile("C.cs", "class C {}"),
                },
                GetSymbols,
                SymbolDiscoveryKind.OnlyDocumentTypes,
                "import { A, B } from './AAndB';",
                "import { C } from './C';");
        }
    }
}
