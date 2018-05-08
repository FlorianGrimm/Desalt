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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AlternateSignatureSymbolTableTests
    {
        private static Task AssertEntriesInTableAsync(
            string codeSnippet,
            string expectedKey,
            params string[] expectedMethods)
        {
            return AssertEntriesInTableAsync(codeSnippet.ToSafeArray(), expectedKey, expectedMethods);
        }

        private static async Task AssertEntriesInTableAsync(
            IReadOnlyList<string> codeSnippetsInSeparateFiles,
            string expectedKey,
            params string[] expectedMethods)
        {
            var projectFiles = new TempProjectFile[codeSnippetsInSeparateFiles.Count];
            for (int i = 0; i < codeSnippetsInSeparateFiles.Count; i++)
            {
                projectFiles[i] = new TempProjectFile(
                    $"File{i}.cs",
                    $"using System.Runtime.CompilerServices;\n{codeSnippetsInSeparateFiles[i]}");
            }

            using (var tempProject = await TempProject.CreateAsync("Test", projectFiles))
            {
                // ReSharper disable once AccessToDisposedClosure
                var contextPromises = projectFiles.Select(file => tempProject.CreateContextForFileAsync(file.FileName));
                var contexts = (await Task.WhenAll(contextPromises)).ToImmutableArray();

                var alternateSignatureTable = AlternateSignatureSymbolTable.Create(contexts);

                var actualEntries = alternateSignatureTable.Entries.ToImmutableArray();
                actualEntries.Length.Should().Be(1);

                var actualMethodSymbols = contexts.SelectMany(
                    context => context.RootSyntax.DescendantNodes()
                        .OfType<BaseMethodDeclarationSyntax>()
                        .Select(methodSyntax => context.SemanticModel.GetDeclaredSymbol(methodSyntax)));

                var expectedMethodSymbols = (from expectedMethod in expectedMethods
                                             let methodSymbol =
                                                 actualMethodSymbols.First(
                                                     methodSymbol =>
                                                         SymbolTableUtils.KeyFromSymbol(methodSymbol) == expectedMethod)
                                             let isAlternateSignature =
                                                 SymbolTableUtils.FindSaltarelleAttribute(
                                                     methodSymbol,
                                                     "AlternateSignature") !=
                                                 null
                                             select new AlternateSignatureMethodInfo(
                                                 methodSymbol,
                                                 isAlternateSignature)).ToImmutableArray();

                var expectedEntry = new KeyValuePair<string, ImmutableArray<AlternateSignatureMethodInfo>>(
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

    public void NoAttrMethod()
    {
    }
}",
                "C.Method",
                "C.Method()",
                "C.Method(int x)",
                "C.Method(int x, string y)");
        }

        [TestMethod]
        public async Task AlternateSignatureSymbolTable_should_work_on_ctors()
        {
            await AssertEntriesInTableAsync(
                @"
class C
{
    [AlternateSignature]
    public extern C();

    public C(int x, string y)
    {
    }

    [AlternateSignature]
    public extern C(int x);
}",
                "C.C",
                "C.C()",
                "C.C(int x)",
                "C.C(int x, string y)");
        }
    }
}
