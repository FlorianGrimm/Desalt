// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineCodeTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TypeScriptAst.Ast;
    using TypeScriptAst.Ast.Expressions;
    using TypeScriptAst.Emit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    [TestClass]
    public class InlineCodeTranslatorTests
    {
        private static readonly ITsIdentifier s_ss = Factory.Identifier("ss");

        private static async Task AssertInlineCodeTranslation(
            string codeSnippet,
            Func<CompilationUnitSyntax, ExpressionSyntax> getMethodSyntaxFunc,
            ITsAstNode expectedResult,
            ITsExpression translatedLeftSide,
            ITsArgumentList translatedArgumentList,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.OnlyDocumentTypes)
        {
            string code = $@"
using System;
using System.Collections.Generic;

class C
{{
    void Method()
    {{
        {codeSnippet};
    }}

    private object eval = Script.Eval(""[]"");
}}";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync(
                    "File.cs",
                    discoveryKind: discoveryKind);

                ExpressionSyntax methodSyntax = getMethodSyntaxFunc(context.RootSyntax);

                var translator = new InlineCodeTranslator(
                    context.SemanticModel,
                    context.InlineCodeSymbolTable,
                    context.ScriptSymbolTable);

                var diagnostics = new List<Diagnostic>();
                translator.TryTranslate(
                        methodSyntax,
                        translatedLeftSide,
                        translatedArgumentList,
                        diagnostics,
                        out ITsAstNode result)
                    .Should()
                    .BeTrue(because: "there should be an [InlineCode] translation");

                diagnostics.Should().BeEmpty();

                // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
                string translated = result.EmitAsString(EmitOptions.UnixSpaces);
                string expectedTypeScriptCode = expectedResult.EmitAsString(EmitOptions.UnixSpaces);
                translated.Should().Be(expectedTypeScriptCode);
            }
        }

        private static ExpressionSyntax GetMethodOfObjectCreationSyntax(CompilationUnitSyntax syntax) =>
            syntax.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();

        private static ExpressionSyntax GetInvocationMethod(CompilationUnitSyntax syntax) =>
            syntax.DescendantNodes().OfType<InvocationExpressionSyntax>().First().Expression;

        [TestMethod]
        public async Task InlineCodeTranslator_should_replace_this_parameters_with_the_left_side_expression()
        {
            // Inline code for List.Clear:
            // {$System.Script}.clear({this})

            ITsIdentifier leftSide = Factory.Identifier("list");
            await AssertInlineCodeTranslation(
                "var list = new List<int>(); list.Clear()",
                GetInvocationMethod,
                expectedResult: Factory.Call(Factory.MemberDot(s_ss, "clear"), Factory.ArgumentList(leftSide)),
                translatedLeftSide: leftSide,
                translatedArgumentList: Factory.ArgumentList(),
                discoveryKind: SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }

        [TestMethod]
        public async Task InlineCodeTranslator_should_replace_arguments_in_an_argument_list()
        {
            // Taken from the Saltarelle String ctor:
            //
            // [InlineCode("{$System.String}.fromCharCode.apply(null, {value}.slice({startIndex}, {startIndex} + {length}))")]
            // public String(char[] value, int startIndex, int length)

            // here's the translated expression: new string(charArray, 1, 10)
            ITsExpression translatedLeftSide = Factory.MemberDot(
                Factory.QualifiedName("string", "fromCharCode"),
                "apply");

            await AssertInlineCodeTranslation(
                "var charArray = new char[0]; new string(charArray, 1, 10)",
                GetMethodOfObjectCreationSyntax,
                expectedResult: Factory.Call(
                    translatedLeftSide,
                    Factory.ArgumentList(
                        Factory.Argument(Factory.Null),
                        Factory.Argument(
                            Factory.Call(
                                Factory.MemberDot(Factory.Identifier("charArray"), "slice"),
                                Factory.ArgumentList(
                                    Factory.Argument(Factory.Number(1)),
                                    Factory.Argument(
                                        Factory.BinaryExpression(
                                            Factory.Number(1),
                                            TsBinaryOperator.Add,
                                            Factory.Number(10)))))))),
                translatedLeftSide: translatedLeftSide,
                translatedArgumentList: Factory.ArgumentList(
                    Factory.Argument(Factory.Identifier("charArray")),
                    Factory.Argument(Factory.Number(1)),
                    Factory.Argument(Factory.Number(10))),
                discoveryKind: SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes);
        }

        [TestMethod]
        public async Task InlineCodeTranslator_should_replace_rest_arguments_in_an_argument_list()
        {
            // Inline code for List.Ctor:
            // [ {first}, {*rest} ]

            await AssertInlineCodeTranslation(
                "var list = new List<int>(1, 2, 3)",
                GetMethodOfObjectCreationSyntax,
                expectedResult: Factory.Array(Factory.Number(1), Factory.Number(2), Factory.Number(3)),
                translatedLeftSide: Factory.GenericTypeName("List", Factory.NumberType),
                translatedArgumentList: Factory.ArgumentList(
                    Factory.Argument(Factory.Number(1)),
                    Factory.Argument(Factory.Number(2)),
                    Factory.Argument(Factory.Number(3))),
                discoveryKind: SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }
    }
}
