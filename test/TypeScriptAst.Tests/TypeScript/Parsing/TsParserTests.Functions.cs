// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Functions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.TypeScript.Parsing
{
    using Xunit;
    using Factory = TypeScriptAst.TypeScript.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        [Fact]
        public void TsParser_should_parse_function_parameters_with_object_binding_patterns()
        {
            AssertParseExpression(
                "function ({ x, y = 0, [z]: x }) {}",
                Factory.FunctionExpression(
                    Factory.CallSignature(
                        Factory.ParameterList(
                            Factory.BoundRequiredParameter(
                                Factory.ObjectBindingPattern(
                                    Factory.SingleNameBinding(s_x),
                                    Factory.SingleNameBinding(s_y, Factory.Number(0)),
                                    Factory.PropertyNameBinding(
                                        Factory.ComputedPropertyName(s_z),
                                        Factory.SingleNameBinding(s_x))))))));
        }

        [Fact]
        public void TsParser_should_parse_function_parameters_with_array_binding_patterns()
        {
            AssertParseExpression(
                "function ([, , x, , y = 0, ...z]) {}",
                Factory.FunctionExpression(
                    Factory.CallSignature(
                        Factory.ParameterList(
                            Factory.BoundRequiredParameter(
                                Factory.ArrayBindingPattern(
                                    new[]
                                    {
                                        null,
                                        null,
                                        Factory.SingleNameBinding(s_x),
                                        null,
                                        Factory.SingleNameBinding(s_y, Factory.Number(0))
                                    },
                                    s_z))))));
        }

        [Fact]
        public void TsParser_should_parse_function_parameters_with_nested_binding_patterns()
        {
            AssertParseExpression(
                "function ({ x: [{ y: z }] }) {}",
                Factory.FunctionExpression(
                    Factory.CallSignature(
                        Factory.ParameterList(
                            Factory.BoundRequiredParameter(
                                Factory.ObjectBindingPattern(
                                    Factory.PropertyNameBinding(
                                        s_x,
                                        Factory.PatternBinding(
                                            Factory.ArrayBindingPattern(
                                                Factory.PatternBinding(
                                                    Factory.ObjectBindingPattern(
                                                        Factory.PropertyNameBinding(
                                                            s_y,
                                                            Factory.SingleNameBinding(s_z)))))))))))));
        }
    }
}
