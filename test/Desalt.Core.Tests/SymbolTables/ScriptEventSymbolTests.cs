// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptEventSymbolTests.cs" company="Justin Rockwood">
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
    public class ScriptEventSymbolTests
    {
        [TestMethod]
        public async Task ScriptEventSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"
using System;

class C
{
    public event EventHandler MyEvent;
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                IEventSymbol eventSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<EventFieldDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node.Declaration.Variables[0]))
                    .Cast<IEventSymbol>()
                    .Single();

                var eventScriptSymbol = new ScriptEventSymbol(eventSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(eventSymbol, eventScriptSymbol);

                eventScriptSymbol.BackingFieldName.Should().BeNull();
                eventScriptSymbol.CustomInitialization.Should().BeNull();
                eventScriptSymbol.EventSymbol.Should().Be(eventSymbol);
            }
        }

        [TestMethod]
        public async Task ScriptEventSymbol_should_read_the_right_values_on_the_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

class C
{
    [BackingFieldName(""field"")]
    [CustomInitialization(""code"")]
    public event EventHandler MyEvent;
}";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                IEventSymbol eventSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<EventFieldDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node.Declaration.Variables[0]))
                    .Cast<IEventSymbol>()
                    .Single();

                var eventScriptSymbol = new ScriptEventSymbol(eventSymbol, "ComputedScriptName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(eventSymbol, eventScriptSymbol);

                eventScriptSymbol.BackingFieldName.Should().Be("field");
                eventScriptSymbol.CustomInitialization.Should().Be("code");
                eventScriptSymbol.EventSymbol.Should().Be(eventSymbol);
            }
        }
    }
}
