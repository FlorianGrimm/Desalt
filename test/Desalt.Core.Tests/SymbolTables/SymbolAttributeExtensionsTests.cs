// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolAttributeExtensionsTests.cs" company="Justin Rockwood">
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
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

    public class SymbolAttributeExtensionsTests
    {
        [Test]
        public async Task FindAttribute_should_return_null_when_not_found()
        {
            const string code = @"class C {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Single(m => m.Name == "C");

            classSymbol.FindAttribute(SaltarelleAttributeName.Imported).Should().BeNull();
            classSymbol.HasAttribute(SaltarelleAttributeName.Imported).Should().BeFalse();
        }

        [Test]
        public async Task FindAttribute_should_find_the_named_attribute()
        {
            const string code = @"
using System.Runtime.CompilerServices;

[Imported]
class C {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Single(m => m.Name == "C");

            classSymbol.FindAttribute(SaltarelleAttributeName.Imported).Should().NotBeNull();
            classSymbol.HasAttribute(SaltarelleAttributeName.Imported).Should().BeTrue();
        }

        [Test]
        public async Task Try_and_GetAttributeValue_should_return_false_if_the_attribute_is_not_present()
        {
            const string code = @"class C {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Single(m => m.Name == "C");

            classSymbol.TryGetAttributeValue(SaltarelleAttributeName.ScriptName, out string? value)
                .Should()
                .BeFalse();
            value.Should().BeNull();

            classSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptName, "NotThere")
                .Should()
                .Be("NotThere");
        }

        [Test]
        public async Task Try_and_GetAttributeValue_should_return_true_and_the_value_if_the_attribute_is_present()
        {
            const string code = @"
using System.Runtime.CompilerServices;

[ScriptName(""Success"")]
class C {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Single(m => m.Name == "C");

            classSymbol.TryGetAttributeValue(SaltarelleAttributeName.ScriptName, out string? value)
                .Should()
                .BeTrue();
            value.Should().Be("Success");

            classSymbol.GetAttributeValueOrDefault(SaltarelleAttributeName.ScriptName, "Default")
                .Should()
                .Be("Success");
        }

        [Test]
        public async Task Try_and_GetAttributeValue_should_return_the_value_of_a_named_argument()
        {
            const string code = @"
using System.Runtime.CompilerServices;

[Imported(ObeysTypeSystem=true)]
class C {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            Microsoft.CodeAnalysis.INamedTypeSymbol classSymbol = context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Single(m => m.Name == "C");

            classSymbol.TryGetAttributeValue(
                    SaltarelleAttributeName.Imported,
                    SaltarelleAttributeArgumentName.ObeysTypeSystem,
                    out bool value)
                .Should()
                .BeTrue();
            value.Should().BeTrue();

            classSymbol.GetAttributeValueOrDefault(
                    SaltarelleAttributeName.Imported,
                    propertyName: SaltarelleAttributeArgumentName.ObeysTypeSystem,
                    defaultValue: false)
                .Should()
                .BeTrue();
        }

        [Test]
        public async Task GetFlagAttribute_should_return_the_correct_values_for_flag_type_attributes()
        {
            const string code = @"
using System.Runtime.CompilerServices;

class NoAttribute {}
[PreserveMemberCase] class NoCtorArgs {}
[PreserveMemberCase()] class NoCtorArgs2 {}
[PreserveMemberCase(false)] class WithUnnamedFalseArg {}
[PreserveMemberCase(true)] class WithUnnamedTrueArg {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Select(
                    symbol =>
                    {
                        bool value = symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase);
                        return (ClassName: symbol.ToHashDisplay(), AttributeValue: value);
                    })
                .Should()
                .BeEquivalentTo(
                    ("NoAttribute", false),
                    ("NoCtorArgs", true),
                    ("NoCtorArgs2", true),
                    ("WithUnnamedFalseArg", false),
                    ("WithUnnamedTrueArg", true));
        }

        [Test]
        public async Task TryGetAttributeValue_should_return_the_correct_values_for_flag_type_attributes()
        {
            const string code = @"
using System.Runtime.CompilerServices;

class NoAttribute {}
[PreserveMemberCase] class NoCtorArgs {}
[PreserveMemberCase()] class NoCtorArgs2 {}
[PreserveMemberCase(false)] class WithUnnamedFalseArg {}
[PreserveMemberCase(true)] class WithUnnamedTrueArg {}";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
            context.RootSyntax.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(node => context.SemanticModel.GetDeclaredSymbol(node))
                .Select(
                    symbol =>
                    {
                        bool value = symbol.GetFlagAttribute(SaltarelleAttributeName.PreserveMemberCase);
                        return (ClassName: symbol.ToHashDisplay(), AttributeValue: value);
                    })
                .Should()
                .BeEquivalentTo(
                    ("NoAttribute", false),
                    ("NoCtorArgs", true),
                    ("NoCtorArgs2", true),
                    ("WithUnnamedFalseArg", false),
                    ("WithUnnamedTrueArg", true));
        }
    }
}
