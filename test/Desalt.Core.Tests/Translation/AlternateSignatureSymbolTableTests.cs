// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AlternateSignatureSymbolTableTests
    {
        private static async Task AssertEntriesInTableAsync(
            string codeSnippet,
            string expectedKey,
            params string[] expectedMethods)
        {
            string code = $@"
using System.Runtime.CompilerServices;

{codeSnippet}
";

            using (var tempProject = await TempProject.CreateAsync("Test", new TempProjectFile("File.cs", code)))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs");
                var contexts = context.ToSingleEnumerable().ToImmutableArray();
                var importTable = AlternateSignatureSymbolTable.Create(contexts);

                var actualEntries = importTable.Entries.ToImmutableArray();
                actualEntries.Length.Should().Be(1);

                var actualMethodSymbols = context.RootSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(methodSyntax => context.SemanticModel.GetDeclaredSymbol(methodSyntax))
                    .ToImmutableArray();

                var expectedMethodSymbols = expectedMethods.Select(
                        expectedMethod => actualMethodSymbols.First(
                            methodSymbol => SymbolTableUtils.KeyFromSymbol(methodSymbol) == expectedMethod))
                    .ToImmutableArray();

                var expectedEntry = new KeyValuePair<string, ImmutableArray<IMethodSymbol>>(
                    expectedKey,
                    expectedMethodSymbols);

                actualEntries[0].Should().BeEquivalentTo(expectedEntry);
            }
        }

        [TestMethod]
        public async Task AlternateSignatureSymbolTable_should_find_all_instances_of_a_method_with_the_attribute()
        {
            await AssertEntriesInTableAsync(
                @"
class C
{
    [AlternateSignature]
    public extern void Method();

    public void Method(int x, string y)
    {
    }

    [AlternateSignature]
    public extern void Method(int x);
}",
                "C.Method",
                "C.Method()",
                "C.Method(int x)");
        }
    }
}
