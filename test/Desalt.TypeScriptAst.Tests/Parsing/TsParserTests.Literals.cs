// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Literals.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Parsing
{
    using Desalt.TypeScriptAst.Ast;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        [Test]
        public void TsParser_should_parse_literals()
        {
            AssertParseExpression("null", Factory.Null);
            AssertParseExpression("true", Factory.True);
            AssertParseExpression("false", Factory.False);
            AssertParseExpression("123.456", Factory.Number(123.456));
            AssertParseExpression("0b1101", Factory.Number(13));
            AssertParseExpression("0o10", Factory.Number(8));
            AssertParseExpression("0x10", Factory.Number(16));
            AssertParseExpression("'str'", Factory.String("str"));
            AssertParseExpression("\"str\"", Factory.String("str", StringLiteralQuoteKind.DoubleQuote));
        }

        [Test]
        public void TsParser_should_parse_array_literals()
        {
            AssertParseExpression("[]", Factory.Array());
            AssertParseExpression(
                "[123, 'a', true]",
                Factory.Array(Factory.Number(123), Factory.String("a"), Factory.True));
            AssertParseExpression("[,]", Factory.Array());
            AssertParseExpression("[,,,]", Factory.Array());
            AssertParseExpression(
                "[1, ...x]",
                Factory.Array(
                    Factory.ArrayElement(Factory.Number(1)),
                    Factory.ArrayElement(s_x, isSpreadElement: true)));
        }

        //// ===========================================================================================================
        //// Object Literals
        //// ===========================================================================================================

        [Test]
        public void TsParser_should_parse_object_literals_with_a_single_element()
        {
            AssertParseExpression("{}", Factory.Object());
            AssertParseExpression("{ x }", Factory.Object(s_x));
            AssertParseExpression("{ x: y }", Factory.Object(Factory.PropertyAssignment(s_x, s_y)));
            AssertParseExpression("{ x = 10 }", Factory.Object(Factory.CoverInitializedName(s_x, Factory.Number(10))));
        }

        [Test]
        public void TsParser_should_parse_object_literals_with_string_or_number_keys()
        {
            AssertParseExpression(
                "{ 'continue': x }",
                Factory.Object(Factory.PropertyAssignment(Factory.String("continue"), s_x)));
            AssertParseExpression("{ 123: x }", Factory.Object(Factory.PropertyAssignment(Factory.Number(123), s_x)));
            AssertParseExpression(
                "{ [x]: 123 }",
                Factory.Object(Factory.PropertyAssignment(Factory.ComputedPropertyName(s_x), Factory.Number(123))));
        }

        [Test]
        public void TsParser_should_pares_an_object_literal_with_multiple_elements()
        {
            AssertParseExpression(
                "{ x: 'str', y: 10 }",
                Factory.Object(
                    Factory.PropertyAssignment(s_x, Factory.String("str")),
                    Factory.PropertyAssignment(s_y, Factory.Number(10))));
        }

        [Test]
        public void TsParser_should_parse_object_literals_with_a_getter()
        {
            AssertParseExpression(
                "{ get prop(): string { return 's'; } }",
                Factory.Object(
                    Factory.GetAccessor(
                        Factory.Identifier("prop"),
                        Factory.StringType,
                        Factory.Return(Factory.String("s")))));
        }

        [Test]
        public void TsParser_should_parse_object_literals_with_a_setter()
        {
            AssertParseExpression(
                "{ set prop(value: string) { this._prop = value; } }",
                Factory.Object(
                    Factory.SetAccessor(
                        Factory.Identifier("prop"),
                        Factory.Identifier("value"),
                        Factory.StringType,
                        Factory.Assignment(
                                Factory.MemberDot(Factory.This, "_prop"),
                                TsAssignmentOperator.SimpleAssign,
                                Factory.Identifier("value"))
                            .ToStatement())));
        }

        [Test]
        public void TsParser_should_parse_object_literals_with_methods()
        {
            AssertParseExpression(
                "{ myFunc<T extends string, TResult extends boolean>(x: T): TResult { return false; } }",
                Factory.Object(
                    Factory.PropertyFunction(
                        Factory.Identifier("myFunc"),
                        Factory.CallSignature(
                            Factory.TypeParameters(Factory.TypeParameter(Factory.Identifier("T"), Factory.StringType)),
                            Factory.ParameterList(s_x),
                            Factory.BooleanType),
                        Factory.Return(Factory.False))));
        }
    }
}
