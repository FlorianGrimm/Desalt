// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptEnumSymbolTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptEnumSymbolTests
    {
        [TestMethod]
        public async Task ScriptEnumSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"enum E { }";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                Microsoft.CodeAnalysis.INamedTypeSymbol enumSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<EnumDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                    .Single();

                var enumScriptSymbol = new ScriptEnumSymbol(
                    enumSymbol,
                    "ComputedScriptName",
                    ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath));

                ScriptSymbolTests.AssertScriptSymbolDefaultValues(enumSymbol, enumScriptSymbol);

                enumScriptSymbol.NamedValues.Should().BeFalse();
                enumScriptSymbol.NumericValues.Should().BeFalse();
            }
        }

        [TestMethod]
        public async Task ScriptEnumSymbol_should_give_precedence_to_NamedValues()
        {
            const string code = @"
using System.Runtime.CompilerServices;

[NamedValues]
[NumericValues]
enum E { }";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                Microsoft.CodeAnalysis.INamedTypeSymbol enumSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<EnumDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                    .Single();

                var enumScriptSymbol = new ScriptEnumSymbol(
                    enumSymbol,
                    "ComputedScriptName",
                    ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath));

                ScriptSymbolTests.AssertScriptSymbolDefaultValues(enumSymbol, enumScriptSymbol);

                enumScriptSymbol.NamedValues.Should().BeTrue();
                enumScriptSymbol.NumericValues.Should().BeTrue();
            }
        }

        [TestMethod]
        public async Task ScriptEnumSymbol_ctor_should_throw_if_not_an_enum_symbol()
        {
            const string code = @"
using System.Runtime.CompilerServices;

class E { }";

            using (TempProject tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                    .Single();

                // ReSharper disable once ObjectCreationAsStatement
                Action action = () => new ScriptEnumSymbol(
                    classSymbol,
                    "ComputedScriptName",
                    ImportSymbolInfo.CreateInternalReference(context.TypeScriptFilePath));

                action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("symbol");
            }
        }
    }
}
