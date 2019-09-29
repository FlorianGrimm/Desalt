// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptFieldSymbolTests.cs" company="Justin Rockwood">
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
    public class ScriptFieldSymbolTests
    {
        [TestMethod]
        public async Task ScriptFieldSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"
using System;

class C
{
    public int MyField;
}";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var fieldSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node.Declaration.Variables[0]))
                    .Cast<IFieldSymbol>()
                    .Single();

                var eventScriptSymbol = new ScriptFieldSymbol(fieldSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(fieldSymbol, eventScriptSymbol);

                eventScriptSymbol.CustomInitialization.Should().BeNull();
                eventScriptSymbol.FieldSymbol.Should().Be(fieldSymbol);
                eventScriptSymbol.InlineConstant.Should().BeFalse();
                eventScriptSymbol.NoInline.Should().BeFalse();
            }
        }

        [TestMethod]
        public async Task ScriptFieldSymbol_should_read_the_right_values_on_the_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [CustomInitialization(""code"")]
    [InlineConstant]
    [NoInline]
    public int MyField;
}";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var fieldSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node.Declaration.Variables[0]))
                    .Cast<IFieldSymbol>()
                    .Single();

                var eventScriptSymbol = new ScriptFieldSymbol(fieldSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(fieldSymbol, eventScriptSymbol);

                eventScriptSymbol.CustomInitialization.Should().Be("code");
                eventScriptSymbol.FieldSymbol.Should().Be(fieldSymbol);
                eventScriptSymbol.InlineConstant.Should().BeTrue();
                eventScriptSymbol.NoInline.Should().BeTrue();
            }
        }
    }
}
