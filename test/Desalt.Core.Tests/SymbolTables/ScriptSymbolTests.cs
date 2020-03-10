// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSymbolTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptSymbolTests
    {
        internal static void AssertScriptSymbolDefaultValues(ISymbol symbol, ScriptSymbol scriptSymbol)
        {
            scriptSymbol.Imported.Should().BeFalse();
            scriptSymbol.PreserveCase.Should().BeFalse();
            scriptSymbol.PreserveName.Should().BeFalse();
            scriptSymbol.Reflectable.Should().BeFalse();
            scriptSymbol.ScriptName.Should().BeNull();

            scriptSymbol.Symbol.Should().Be(symbol);
        }

        [TestMethod]
        public async Task ScriptSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"class C { public void Method() {} }";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                IMethodSymbol methodSymbol = context.RootSyntax
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(node => node.Identifier.ValueText == "Method")
                    .Select(methodNode => context.SemanticModel.GetDeclaredSymbol(methodNode))
                    .Single();

                var scriptSymbol = new TestScriptSymbol(methodSymbol);
                AssertScriptSymbolDefaultValues(methodSymbol, scriptSymbol);
            }
        }

        [TestMethod]
        public async Task ScriptSymbol_should_read_the_correct_values_from_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [PreserveCase]
    [PreserveName]
    [Reflectable]
    [ScriptName(""x"")]
    public void Method() {}
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                IMethodSymbol methodSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(node => node.Identifier.ValueText == "Method")
                    .Select(methodNode => context.SemanticModel.GetDeclaredSymbol(methodNode))
                    .Single();

                var scriptSymbol = new TestScriptSymbol(methodSymbol);
                scriptSymbol.PreserveCase.Should().BeTrue();
                scriptSymbol.PreserveName.Should().BeTrue();
                scriptSymbol.Reflectable.Should().BeTrue();
                scriptSymbol.ScriptName.Should().Be("x");
            }
        }

        [TestMethod]
        public async Task ScriptSymbol_should_correctly_use_inheritance_when_reading_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[Imported]
[PreserveName]
[Reflectable]
class C
{
    public void Method() {}
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                IMethodSymbol methodSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Where(node => node.Identifier.ValueText == "Method")
                    .Select(methodNode => context.SemanticModel.GetDeclaredSymbol(methodNode))
                    .Single();

                var scriptSymbol = new TestScriptSymbol(methodSymbol);
                scriptSymbol.Imported.Should().BeTrue();
                scriptSymbol.PreserveName.Should().BeFalse();
                scriptSymbol.Reflectable.Should().BeFalse();
            }
        }
    }

    internal class TestScriptSymbol : ScriptSymbol
    {
        public TestScriptSymbol(ISymbol symbol)
            : base(symbol, "ComputedScriptName")
        {
        }
    }
}
