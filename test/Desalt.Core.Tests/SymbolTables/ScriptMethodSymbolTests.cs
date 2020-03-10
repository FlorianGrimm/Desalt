// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptMethodSymbolTests.cs" company="Justin Rockwood">
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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class ScriptMethodSymbolTests
    {
        [Test]
        public async Task ScriptMethodSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"
using System;

class C
{
    public void Method() {}
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                Microsoft.CodeAnalysis.IMethodSymbol methodSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(methodNode => context.SemanticModel.GetDeclaredSymbol(methodNode))
                    .Single();

                var methodScriptSymbol = new ScriptMethodSymbol(methodSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(methodSymbol, methodScriptSymbol);

                methodScriptSymbol.AlternateSignature.Should().BeFalse();
                methodScriptSymbol.DontGenerate.Should().BeFalse();
                methodScriptSymbol.EnumerateAsArray.Should().BeFalse();
                methodScriptSymbol.ExpandParams.Should().BeFalse();
                methodScriptSymbol.InlineCode.Should().BeNull();
                methodScriptSymbol.InlineCodeGeneratedMethodName.Should().BeNull();
                methodScriptSymbol.InlineCodeNonExpandedFormCode.Should().BeNull();
                methodScriptSymbol.InlineCodeNonVirtualCode.Should().BeNull();
                methodScriptSymbol.InstanceMethodOnFirstArgument.Should().BeFalse();
                methodScriptSymbol.IntrinsicOperator.Should().BeFalse();
                methodScriptSymbol.MethodSymbol.Should().Be(methodSymbol);
                methodScriptSymbol.ObjectLiteral.Should().BeFalse();
                methodScriptSymbol.ScriptAlias.Should().BeNull();
                methodScriptSymbol.ScriptSkip.Should().BeFalse();
            }
        }

        [Test]
        public async Task ScriptMethodSymbol_should_read_the_right_values_on_the_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [AlternateSignature]
    [DontGenerate]
    [EnumerateAsArray]
    [ExpandParams]
    [InlineCode(""code"", GeneratedMethodName=""method"", NonExpandedFormCode=""nonExpanded"", NonVirtualCode=""nonVirtual"")]
    [InstanceMethodOnFirstArgument]
    [IntrinsicOperator]
    // [ObjectLiteral]
    [ScriptAlias(""alias"")]
    [ScriptSkip]
    public void Method() {}
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                Microsoft.CodeAnalysis.IMethodSymbol methodSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .Select(methodNode => context.SemanticModel.GetDeclaredSymbol(methodNode))
                    .Single();

                var methodScriptSymbol = new ScriptMethodSymbol(methodSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(methodSymbol, methodScriptSymbol);

                methodScriptSymbol.AlternateSignature.Should().BeTrue();
                methodScriptSymbol.DontGenerate.Should().BeTrue();
                methodScriptSymbol.EnumerateAsArray.Should().BeTrue();
                methodScriptSymbol.ExpandParams.Should().BeTrue();
                methodScriptSymbol.InlineCode.Should().Be("code");
                methodScriptSymbol.InlineCodeGeneratedMethodName.Should().Be("method");
                methodScriptSymbol.InlineCodeNonExpandedFormCode.Should().Be("nonExpanded");
                methodScriptSymbol.InlineCodeNonVirtualCode.Should().Be("nonVirtual");
                methodScriptSymbol.InstanceMethodOnFirstArgument.Should().BeTrue();
                methodScriptSymbol.IntrinsicOperator.Should().BeTrue();
                methodScriptSymbol.MethodSymbol.Should().Be(methodSymbol);
                //methodScriptSymbol.ObjectLiteral.Should().BeTrue();
                methodScriptSymbol.ScriptAlias.Should().Be("alias");
                methodScriptSymbol.ScriptSkip.Should().BeTrue();
            }
        }
    }
}
