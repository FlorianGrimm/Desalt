// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using NUnit.Framework;

    public class AlternateSignatureSymbolTableTests
    {
        private static async Task AssertEntriesInTableAsync(
            string codeSnippet,
            string expectedKey,
            string expectedImplementingMethod,
            params string[] expectedAlternateSignatures)
        {
            string code = $@"
using System.Runtime.CompilerServices;

{codeSnippet}
";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            Core.Translation.DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();

            IExtendedResult<AlternateSignatureSymbolTable> result = AlternateSignatureSymbolTable.Create(context.ToSingleEnumerable().ToImmutableArray());
            result.Diagnostics.Should().BeEmpty();
            AlternateSignatureSymbolTable alternateSignatureTable = result.Result;

            var actualEntries = alternateSignatureTable.Entries.ToImmutableArray();
            actualEntries.Length.Should().Be(1);
            actualEntries[0].Key.Should().Be(expectedKey);

            AlternateSignatureMethodGroup actualGroup = actualEntries[0].Value;

            actualGroup.ImplementingMethod.ToHashDisplay().Should().Be(expectedImplementingMethod);
            actualGroup.AlternateSignatureMethods.Select(RoslynExtensions.ToHashDisplay)
                .Should()
                .BeEquivalentTo(expectedAlternateSignatures);
        }

        [Test]
        public async Task AlternateSignatureSymbolTable_should_find_all_instances_of_a_method_with_the_attribute()
        {
            await AssertEntriesInTableAsync(
                @"
class C
{
    [AlternateSignature]
    public extern void Method();

    [AlternateSignature]
    public extern void Method(int x);

    public void Method(int x, string y)
    {
    }

    public void NoAttrMethod()
    {
    }
}",
                "C.Method",
                expectedImplementingMethod: "C.Method(int x, string y)",
                expectedAlternateSignatures: new[] { "C.Method()", "C.Method(int x)" });
        }

        [Test]
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
                expectedImplementingMethod: "C.C(int x, string y)",
                expectedAlternateSignatures: new[] { "C.C()", "C.C(int x)" });
        }
    }
}
