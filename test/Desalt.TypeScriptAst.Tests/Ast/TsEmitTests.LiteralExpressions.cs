// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.LiteralExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    using System;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using FluentAssertions;
    using Xunit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        [Fact]
        public void Emit_null_literal()
        {
            VerifyOutput(Factory.Null, "null");
        }

        [Fact]
        public void Emit_boolean_literals()
        {
            VerifyOutput(Factory.True, "true");
            VerifyOutput(Factory.False, "false");
        }

        [Fact]
        public void Emit_string_literals()
        {
            VerifyOutput(Factory.String("single"), "'single'");
            VerifyOutput(Factory.String("double", StringLiteralQuoteKind.DoubleQuote), "\"double\"");
        }

        [Fact]
        public void Number_literals_should_be_positive()
        {
            Action action = () => Factory.Number(-123);
            action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.BinaryInteger(-123);
            action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.OctalInteger(-123);
            action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");

            action = () => Factory.HexInteger(-123);
            action.Should().ThrowExactly<ArgumentException>().And.ParamName.Should().Be("value");
        }

        [Fact]
        public void Emit_decimal_literals()
        {
            VerifyOutput(Factory.Number(123), "123");
            VerifyOutput(Factory.Number(1.23e4), "12300");
            VerifyOutput(Factory.Number(83e45), "8.3E+46");
            VerifyOutput(Factory.Number(53e-53), "5.3E-52");
        }

        [Fact]
        public void Emit_binary_integer_literals()
        {
            VerifyOutput(Factory.BinaryInteger(17), "0b10001");
        }

        [Fact]
        public void Emit_octal_integer_literals()
        {
            VerifyOutput(Factory.OctalInteger(20), "0o24");
        }

        [Fact]
        public void Emit_hex_integer_literal()
        {
            VerifyOutput(Factory.HexInteger(415), "0x19f");
            VerifyOutput(Factory.HexInteger(48879), "0xbeef");
        }

        [Fact]
        public void Emit_regular_expression_literals()
        {
            VerifyOutput(Factory.RegularExpression("a-z", "g"), "/a-z/g");
            VerifyOutput(Factory.RegularExpression("hello", null), "/hello/");
        }

        [Fact]
        public void Emit_array_literals()
        {
            VerifyOutput(
                Factory.Array(
                    Factory.ArrayElement(s_x),
                    Factory.ArrayElement(Factory.Number(10))),
                "[x, 10]");

            VerifyOutput(
                Factory.Array(
                    Factory.ArrayElement(s_y),
                    Factory.ArrayElement(s_z),
                    Factory.ArrayElement(Factory.String("str"))),
                "[y, z, 'str']");
        }

        [Fact]
        public void Emit_array_literals_with_elisons_ie_empty_elements()
        {
            VerifyOutput(
                Factory.Array(null, Factory.ArrayElement(s_x), null, null, Factory.ArrayElement(s_y)),
                "[, x, , , y]");
        }

        [Fact]
        public void Emit_array_literals_with_spread_operator()
        {
            VerifyOutput(Factory.Array(Factory.ArrayElement(s_y, isSpreadElement: true)), "[... y]");
        }

        [Fact]
        public void Emit_template_literals()
        {
            VerifyOutput(Factory.TemplateString(Factory.TemplatePart(template: "string")), "`string`");
            VerifyOutput(
                Factory.TemplateString(
                    Factory.TemplatePart("xy=", s_x),
                    Factory.TemplatePart(expression: s_y)),
                "`xy=${x}${y}`");
        }

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        [Fact]
        public void Emit_empty_object_literal()
        {
            VerifyOutput(Factory.EmptyObject, "{}");
        }

        [Fact]
        public void Emit_full_object_literal_with_every_type_of_property_definition()
        {
            VerifyOutput(
                Factory.Object(
                    Factory.Identifier("identifier"),
                    Factory.CoverInitializedName(Factory.Identifier("coverInitializedName"), Factory.String("check")),
                    Factory.PropertyAssignment(Factory.Identifier("propName"), Factory.String("assignment")),
                    Factory.PropertyFunction(
                        Factory.Identifier("method"),
                        Factory.CallSignature(
                            Factory.TypeParameters(
                                Factory.TypeParameter(s_T, Factory.TypeReference(Factory.Identifier("IString")))),
                            Factory.ParameterList(
                                Factory.BoundRequiredParameter(s_x, s_TRef),
                                Factory.BoundRequiredParameter(s_y, s_TRef)),
                            s_TRef),
                        Factory.Return(Factory.BinaryExpression(s_x, TsBinaryOperator.Add, s_y))),
                    Factory.GetAccessor(
                        Factory.Identifier("getter"),
                        Factory.NumberType,
                        Factory.Return(Factory.MemberDot(Factory.This, "_field"))),
                    Factory.SetAccessor(
                        Factory.Identifier("setter"),
                        Factory.Identifier("value"),
                        Factory.NumberType,
                        Factory.Assignment(
                            Factory.MemberBracket(Factory.This, Factory.String("_field")),
                            TsAssignmentOperator.SimpleAssign,
                            Factory.Identifier("value"))
                        .ToStatement())),
                @"{
  identifier,
  coverInitializedName = 'check',
  propName: 'assignment',
  method<T extends IString>(x: T, y: T): T {
    return x + y;
  },
  get getter(): number {
    return this._field;
  },
  set setter(value: number) {
    this['_field'] = value;
  }
}".Replace("\r\n", "\n"));
        }
    }
}
