// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ScriptSymbolTableTests
    {
        private static IScriptNamer CreateFakeScriptNamer()
        {
            var fakeScriptNamer = new Mock<IScriptNamer>();
            fakeScriptNamer.Setup(mock => mock.DetermineScriptNameForSymbol(It.IsAny<ISymbol>()))
                .Returns("ComputedScriptName");

            return fakeScriptNamer.Object;
        }

        private static async Task AssertDocumentEntriesInSymbolTable(
            string code,
            params string[] expectedEntries)
        {
            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var symbolTable = ScriptSymbolTable.Create(
                    contexts,
                    CreateFakeScriptNamer(),
                    SymbolDiscoveryKind.OnlyDocumentTypes);

                symbolTable.DocumentSymbols
                    .Select(pair => pair.Key.ToHashDisplay())
                    .Should()
                    .BeEquivalentTo(expectedEntries);
            }
        }

        [TestMethod]
        public async Task Create_should_find_all_of_the_types_and_members_in_the_document()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[BindThisToFirstParameter]
delegate void D();

class C
{
    public string Field;
    static C() {} // skipped
    public C(int x) {}
    public string Prop { get; }
    public void Method() {}
}

interface I
{
    string Prop { get; }
    void Method();
}

struct S
{
    public S(int field) { Field = field; Prop = ""Hi""; }
    public int Field;
    public string Prop { get; }
    public void Method() {}
}";

            await AssertDocumentEntriesInSymbolTable(
                code,
                "D",
                "C",
                "C.Field",
                "C.C(int x)",
                "C.Prop",
                "C.Method()",
                "I",
                "I.Prop",
                "I.Method()",
                "S",
                "S.S(int field)",
                "S.Field",
                "S.Prop",
                "S.Method()");
        }

        [TestMethod]
        public async Task Create_should_find_all_of_the_types_and_members_in_external_references()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    public void Method()
    {
        Script.IsValue(null);
    }
}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var symbolTable = ScriptSymbolTable.Create(contexts, CreateFakeScriptNamer());

                // check a directly-reference symbol
                InvocationExpressionSyntax invocationExpressionSyntax =
                    context.RootSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
                ISymbol scriptIsValueSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;
                symbolTable.DirectlyReferencedExternalSymbols.Should().ContainKey(scriptIsValueSymbol);

                // check an implicitly referenced symbol
                var stringBuilderSymbol =
                    scriptIsValueSymbol.ContainingAssembly.GetTypeByMetadataName("System.Text.StringBuilder");
                symbolTable.IndirectlyReferencedExternalSymbols.Should().ContainKey(stringBuilderSymbol);
            }
        }

        [TestMethod]
        public async Task All_attributes_should_be_correctly_read_from_all_references()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[Imported]
class C
{
    public void Method()
    {
        Script.IsNull(null);
    }
}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var symbolTable = ScriptSymbolTable.Create(contexts, CreateFakeScriptNamer());

                // make sure we read the [Imported] from class C
                ClassDeclarationSyntax classDeclarationSyntax =
                    context.RootSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
                ISymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

                symbolTable.TryGetValue(classSymbol, out IScriptTypeSymbol scriptClassSymbol).Should().BeTrue();
                scriptClassSymbol.Imported.Should().BeTrue();

                // make sure we read the [InlineCode] from Script.IsNull
                InvocationExpressionSyntax invocationExpressionSyntax =
                    context.RootSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
                ISymbol scriptIsNullSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol;

                symbolTable.TryGetValue(scriptIsNullSymbol, out IScriptMethodSymbol scriptIsNullScriptSymbol)
                    .Should()
                    .BeTrue();

                scriptIsNullScriptSymbol.InlineCode.Should().NotBeNullOrWhiteSpace();

                // make sure we read the [IgnoreNamespace] from System.Boolean
                ISymbol boolSymbol = scriptIsNullSymbol.ContainingAssembly.GetTypeByMetadataName("System.Boolean");

                symbolTable.TryGetValue(boolSymbol, out IScriptTypeSymbol scriptBooleanSymbol).Should().BeTrue();
                scriptBooleanSymbol.IgnoreNamespace.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task TryGetValue_should_use_the_overrides_if_present()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    public void Method() {}
}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var overrides = new SymbolTableOverrides(
                    new KeyValuePair<string, SymbolTableOverride>(
                        "C.Method()",
                        new SymbolTableOverride(inlineCode: "OVERRIDE")));
                var options = tempProject.Options.WithSymbolTableOverrides(overrides);

                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync(options: options);
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var symbolTable = ScriptSymbolTable.Create(
                    contexts,
                    CreateFakeScriptNamer(),
                    SymbolDiscoveryKind.OnlyDocumentTypes);

                // get the C.Method() symbol
                ISymbol methodSymbol = context.SemanticModel.Compilation.Assembly.GetTypeByMetadataName("C")
                    .GetMembers("Method")
                    .Single();

                symbolTable.Get<IScriptMethodSymbol>(methodSymbol).InlineCode.Should().Be("OVERRIDE");
            }
        }
    }
}
