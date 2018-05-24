// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AlternateSignatureTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    [TestClass]
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
            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync();

                var methodSymbol = context.RootSyntax.DescendantNodes()
                    .OfType<BaseMethodDeclarationSyntax>()
                    .Select(methodSyntax => context.SemanticModel.GetDeclaredSymbol(methodSyntax))
                    .First(symbol => SymbolTableUtils.KeyFromSymbol(symbol) == methodKey);

                var typeTranslator = new TypeTranslator(context.ScriptNameSymbolTable);

                var translator = new AlternateSignatureTranslator(
                    context.AlternateSignatureSymbolTable,
                    typeTranslator);

                bool result = translator.TryAdjustParameterListTypes(
                    methodSymbol,
                    translatedParameterList,
                    out ITsParameterList actualParameterList,
                    out IEnumerable<Diagnostic> diagnostics);

                diagnostics.Should().BeEmpty();

                result.Should().Be(expectedResult, because: "the parameter list should not have changed");
                actualParameterList.Should().BeEquivalentTo(expectedParameterList);
            }
        }

        [TestMethod]
        public async Task
            TryAdjustParameterList_should_return_false_for_a_method_that_does_not_have_an_AlternateSignature()
        {
            var translatedParameterList =
                Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType));

            await AssertTryAdjustParameterListTypesAsync(
                "class C { void Method(int x) {} }",
                "C.Method(int x)",
                translatedParameterList,
                expectedResult: false,
                expectedParameterList: translatedParameterList);
        }

        [TestMethod]
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

        [TestMethod]
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
                    Factory.BoundRequiredParameter(s_x, Factory.NumberType).ToSafeArray(),
                    new[]
                    {
                        Factory.BoundOptionalParameter(s_y, Factory.StringType),
                        Factory.BoundOptionalParameter(s_z, Factory.NumberType)
                    }));
        }

        [TestMethod]
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

        [TestMethod]
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
                    Factory.BoundRequiredParameter(s_x, Factory.NumberType).ToSingleEnumerable(),
                    new[]
                    {
                        Factory.BoundOptionalParameter(s_y, Factory.BooleanType),
                        Factory.BoundOptionalParameter(s_z, Factory.StringType),
                    }));
        }
    }
}
