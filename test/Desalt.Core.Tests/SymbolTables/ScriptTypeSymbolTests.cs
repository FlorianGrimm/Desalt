// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptTypeSymbolTests.cs" company="Justin Rockwood">
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptTypeSymbolTests
    {
        [TestMethod]
        public async Task ScriptTypeSymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"
using System;
using System.Runtime.CompilerServices;

[DefaultMemberReflectability(MemberReflectability.PublicAndProtected)]
[IgnoreNamespace]
[IncludeGenericArguments]
[Mixin(""fx"")]
[ModuleName(""Module"")]
[Imported(ObeysTypeSystem=true, TypeCheckCode=""typeof x === 'string'"")]
[PreserveMemberCase]
[ScriptNamespace(""tab"")]
[GlobalMethods]
public static class C {}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var classSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                    .Single(m => m.Name == "C");

                var scriptSymbol = new ScriptTypeSymbol(classSymbol);
                scriptSymbol.DefaultMemberReflectability.Should().Be(MemberReflectability.PublicAndProtected);
                scriptSymbol.IgnoreNamespace.Should().BeTrue();
                scriptSymbol.IncludeGenericArguments.Should().BeTrue();
                scriptSymbol.MixinExpression.Should().Be("fx");
                scriptSymbol.ModuleName.Should().Be("Module");
                scriptSymbol.Imported.Should().BeTrue();
                scriptSymbol.ObeysTypeSystem.Should().BeTrue();
                scriptSymbol.TypeCheckCode.Should().Be("typeof x === 'string'");
                scriptSymbol.PreserveMemberCase.Should().BeTrue();
                scriptSymbol.ScriptNamespace.Should().Be("tab");
                scriptSymbol.TreatMethodsAsGlobal.Should().BeTrue();
                scriptSymbol.TypeSymbol.Should().Be(classSymbol);
            }
        }
    }
}
