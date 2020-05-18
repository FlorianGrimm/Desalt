// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public class AlternateSignatureTranslatorTests
    {
        private static readonly ITsIdentifier s_x = Factory.Identifier("x");
        private static readonly ITsIdentifier s_y = Factory.Identifier("y");
        private static readonly ITsIdentifier s_z = Factory.Identifier("z");

        private static async Task AssertTryAdjustParameterListTypesAsync(
            string codeSnippet,
            string methodKey,
            ITsParameterList translatedParameterList,
            bool expectedResult,
            ITsParameterList expectedParameterList)
        {
            string code = $@"
using System.Runtime.CompilerServices;

{codeSnippet}
";
            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContextWithSymbolTables context = await tempProject.CreateContextWithSymbolTablesForFileAsync();

            IMethodSymbol methodSymbol = context.RootSyntax.DescendantNodes()
                .OfType<BaseMethodDeclarationSyntax>()
                .Select(methodSyntax => context.SemanticModel.GetDeclaredSymbol(methodSyntax))
                .First(symbol => symbol.ToHashDisplay() == methodKey);

            var translationContext = new TranslationContext(context);
            bool result = AlternateSignatureTranslator.TryAdjustParameterListTypes(
                translationContext,
                methodSymbol,
                translatedParameterList,
                out ITsParameterList actualParameterList);

            translationContext.Diagnostics.Should().BeEmpty();

            result.Should().Be(expectedResult, because: "the parameter list should not have changed");
            actualParameterList.Should().BeEquivalentTo(expectedParameterList);
        }

        [Test]
        public async Task
            TryAdjustParameterList_should_return_false_for_a_method_that_does_not_have_an_AlternateSignature()
        {
            ITsParameterList translatedParameterList =
                Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType));

            await AssertTryAdjustParameterListTypesAsync(
                "class C { void Method(int x) {} }",
                "C.Method(int x)",
                translatedParameterList,
                expectedResult: false,
                expectedParameterList: translatedParameterList);
        }

        [Test]
        public async Task TryAdjustParameterList_should_leave_the_types_the_same_if_the_overloads_are_compatible()
        {
            const string code = @"
class C
{
    [AlternateSignature]
    public extern void Method(int x);

    public void Method(double x)
    {
    }
}";

            await AssertTryAdjustParameterListTypesAsync(
                code,
                "C.Method(double x)",
                translatedParameterList: Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                expectedResult: false,
                expectedParameterList: Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)));
        }

        [Test]
        public async Task
            TryAdjustParameterList_should_make_all_trailing_parameters_optional_in_the_alternate_signature()
        {
            const string code = @"
class C
{
    [AlternateSignature]
    public extern void Method(int x);
    public void Method(int x, string y, double z)
    {
    }
}
";

            await AssertTryAdjustParameterListTypesAsync(
                code,
                "C.Method(int x, string y, double z)",
                translatedParameterList: Factory.ParameterList(
                    Factory.BoundRequiredParameter(s_x, Factory.NumberType),
                    Factory.BoundRequiredParameter(s_y, Factory.StringType),
                    Factory.BoundRequiredParameter(s_z, Factory.NumberType)),
                expectedResult: true,
                expectedParameterList: Factory.ParameterList(
                    requiredParameters: Factory.BoundRequiredParameter(s_x, Factory.NumberType).ToSafeArray(),
                    optionalParameters: new[]
                    {
                        Factory.BoundOptionalParameter(s_y, Factory.StringType),
                        Factory.BoundOptionalParameter(s_z, Factory.NumberType)
                    }));
        }

        [Test]
        public async Task TryAdjustParameterList_should_create_type_unions_for_the_parameters()
        {
            const string code = @"
class C
{
    [AlternateSignature]
    public extern void Method(int x, bool y);

    public void Method(string x, int y)
    {
    }
}
";
            await AssertTryAdjustParameterListTypesAsync(
                code,
                "C.Method(string x, int y)",
                translatedParameterList: Factory.ParameterList(
                    Factory.BoundRequiredParameter(s_x, Factory.StringType),
                    Factory.BoundRequiredParameter(s_y, Factory.NumberType)),
                expectedResult: true,
                expectedParameterList: Factory.ParameterList(
                    Factory.BoundRequiredParameter(s_x, Factory.UnionType(Factory.StringType, Factory.NumberType)),
                    Factory.BoundRequiredParameter(s_y, Factory.UnionType(Factory.NumberType, Factory.BooleanType))));
        }

        [Test]
        public async Task
            TryAdjustParameterList_should_add_optional_params_when_the_implementing_method_does_not_have_enough_parameters()
        {
            const string code = @"
using System.Runtime.CompilerServices;

class C
{
    public void Method(int x)
    {
    }

    [AlternateSignature]
    public extern void Method(int x, bool y, string z);
}
";
            await AssertTryAdjustParameterListTypesAsync(
                code,
                "C.Method(int x)",
                translatedParameterList: Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                expectedResult: true,
                expectedParameterList: Factory.ParameterList(
                    requiredParameters: Factory.BoundRequiredParameter(s_x, Factory.NumberType).ToSingleEnumerable(),
                    optionalParameters: new[]
                    {
                        Factory.BoundOptionalParameter(s_y, Factory.BooleanType),
                        Factory.BoundOptionalParameter(s_z, Factory.StringType),
                    }));
        }
    }
}
