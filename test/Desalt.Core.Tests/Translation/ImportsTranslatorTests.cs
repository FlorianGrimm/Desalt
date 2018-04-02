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
    using Desalt.Core.Extensions;
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

        private static async Task AssertImports(
            string codeSnippet,
            GetSymbolsFunc getSymbolsFunc,
            params string[] expectedImportLines)
        {
            string code = $@"
using System;

class C
{{
    {codeSnippet}
}}
";

            using (var tempProject = await TempProject.CreateAsync(
                "TestProject",
                new TempProjectFile("File.cs", code)))
            {
                DocumentTranslationContextWithSymbolTables context =
                    await tempProject.CreateContextWithSymbolTablesForFileAsync("File.cs");

                IEnumerable<ISymbol> typesToImport = getSymbolsFunc(context);

                IExtendedResult<IEnumerable<ITsImportDeclaration>> results =
                    ImportsTranslator.TranslateImports(context, typesToImport);

                results.Diagnostics.Should().BeEmpty();

                var actualImportLines = results.Result.Select(import => import.EmitAsString());
                actualImportLines.Should().BeEquivalentTo(expectedImportLines);
            }
        }

        [TestMethod]
        public async Task ImportsTranslator_should_skip_native_types()
        {
            IEnumerable<ISymbol> SymbolsFunc(DocumentTranslationContextWithSymbolTables context) =>
                context.SemanticModel
                    .GetTypeInfo(
                        context.RootSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>().First().Declaration.Type)
                    .Type.ToSingleEnumerable();

            await AssertImports("int x = 1; object o = null;", SymbolsFunc);
        }
    }
}
