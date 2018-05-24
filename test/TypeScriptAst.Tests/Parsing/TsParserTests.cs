// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.Parsing
{
    using CompilerUtilities.Extensions;
    using FluentAssertions;
    using TypeScriptAst.Ast;
    using TypeScriptAst.Ast.Expressions;
    using TypeScriptAst.Parsing;
    using Xunit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ITsIdentifier s_x = Factory.Identifier("x");
        private static readonly ITsIdentifier s_y = Factory.Identifier("y");
        private static readonly ITsIdentifier s_z = Factory.Identifier("z");

#pragma warning disable IDE1006 // Naming Styles

        // ReSharper disable InconsistentNaming
        private static readonly ITsType s_T = Factory.TypeReference(Factory.Identifier("T"));

        private static readonly ITsType s_T1 = Factory.TypeReference(Factory.Identifier("T1"));
        private static readonly ITsType s_T2 = Factory.TypeReference(Factory.Identifier("T2"));
        // ReSharper restore InconsistentNaming

#pragma warning restore IDE1006 // Naming Styles

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        private static void AssertParseExpression(string code, ITsExpression expected)
        {
            ITsExpression actual = TsParser.ParseExpression(code);
            actual.Should().BeEquivalentTo(expected);
        }

        //// ===========================================================================================================
        //// Primary Expression Parsing Tests
        //// ===========================================================================================================

        [Fact]
        public void TsParser_should_recognize_this()
        {
            AssertParseExpression("this", Factory.This);
        }

        [Fact]
        public void TsParser_should_recognize_identifiers()
        {
            AssertParseExpression("x", s_x);
        }

        [Fact]
        public void TsParser_should_recognize_conditional_expressions()
        {
            AssertParseExpression("x ? y : z", Factory.Conditional(s_x, s_y, s_z));
        }

        [Fact]
        public void TsParser_should_recognize_assignment_expressions()
        {
            AssertParseExpression("x = y", Factory.Assignment(s_x, TsAssignmentOperator.SimpleAssign, s_y));
            AssertParseExpression("x *= y", Factory.Assignment(s_x, TsAssignmentOperator.MultiplyAssign, s_y));
            AssertParseExpression("x /= y", Factory.Assignment(s_x, TsAssignmentOperator.DivideAssign, s_y));
            AssertParseExpression("x %= y", Factory.Assignment(s_x, TsAssignmentOperator.ModuloAssign, s_y));
            AssertParseExpression("x += y", Factory.Assignment(s_x, TsAssignmentOperator.AddAssign, s_y));
            AssertParseExpression("x -= y", Factory.Assignment(s_x, TsAssignmentOperator.SubtractAssign, s_y));
            AssertParseExpression("x <<= y", Factory.Assignment(s_x, TsAssignmentOperator.LeftShiftAssign, s_y));
            AssertParseExpression("x >>= y", Factory.Assignment(s_x, TsAssignmentOperator.SignedRightShiftAssign, s_y));
            AssertParseExpression(
                "x >>>= y",
                Factory.Assignment(s_x, TsAssignmentOperator.UnsignedRightShiftAssign, s_y));
            AssertParseExpression("x &= y", Factory.Assignment(s_x, TsAssignmentOperator.BitwiseAndAssign, s_y));
            AssertParseExpression("x ^= y", Factory.Assignment(s_x, TsAssignmentOperator.BitwiseXorAssign, s_y));
            AssertParseExpression("x |= y", Factory.Assignment(s_x, TsAssignmentOperator.BitwiseOrAssign, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_logical_and_or_expressions()
        {
            AssertParseExpression("x || y", Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalOr, s_y));
            AssertParseExpression("x && y", Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalAnd, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_bitwise_expressions()
        {
            AssertParseExpression("x | y", Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseOr, s_y));
            AssertParseExpression("x ^ y", Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseXor, s_y));
            AssertParseExpression("x & y", Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseAnd, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_equality_expressions()
        {
            AssertParseExpression("x == y", Factory.BinaryExpression(s_x, TsBinaryOperator.Equals, s_y));
            AssertParseExpression("x != y", Factory.BinaryExpression(s_x, TsBinaryOperator.NotEquals, s_y));
            AssertParseExpression("x === y", Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y));
            AssertParseExpression("x !== y", Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_relational_expressions()
        {
            AssertParseExpression("x < y", Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, s_y));
            AssertParseExpression("x > y", Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThan, s_y));
            AssertParseExpression("x <= y", Factory.BinaryExpression(s_x, TsBinaryOperator.LessThanEqual, s_y));
            AssertParseExpression("x >= y", Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, s_y));
            AssertParseExpression("x instanceof y", Factory.BinaryExpression(s_x, TsBinaryOperator.InstanceOf, s_y));
            AssertParseExpression("x in y", Factory.BinaryExpression(s_x, TsBinaryOperator.In, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_shift_expressions()
        {
            AssertParseExpression("x << y", Factory.BinaryExpression(s_x, TsBinaryOperator.LeftShift, s_y));
            AssertParseExpression("x >> y", Factory.BinaryExpression(s_x, TsBinaryOperator.SignedRightShift, s_y));
            AssertParseExpression("x >>> y", Factory.BinaryExpression(s_x, TsBinaryOperator.UnsignedRightShift, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_additive_expressions()
        {
            AssertParseExpression("x + y", Factory.BinaryExpression(s_x, TsBinaryOperator.Add, s_y));
            AssertParseExpression("x - y", Factory.BinaryExpression(s_x, TsBinaryOperator.Subtract, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_multiplicative_expressions()
        {
            AssertParseExpression("x * y", Factory.BinaryExpression(s_x, TsBinaryOperator.Multiply, s_y));
            AssertParseExpression("x / y", Factory.BinaryExpression(s_x, TsBinaryOperator.Divide, s_y));
            AssertParseExpression("x % y", Factory.BinaryExpression(s_x, TsBinaryOperator.Modulo, s_y));
        }

        [Fact]
        public void TsParser_should_recognize_unary_expressions()
        {
            AssertParseExpression("delete x", Factory.UnaryExpression(s_x, TsUnaryOperator.Delete));
            AssertParseExpression("void x", Factory.UnaryExpression(s_x, TsUnaryOperator.Void));
            AssertParseExpression("typeof x", Factory.UnaryExpression(s_x, TsUnaryOperator.Typeof));
            AssertParseExpression("++x", Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixIncrement));
            AssertParseExpression("--x", Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixDecrement));
            AssertParseExpression("+x", Factory.UnaryExpression(s_x, TsUnaryOperator.Plus));
            AssertParseExpression("-x", Factory.UnaryExpression(s_x, TsUnaryOperator.Minus));
            AssertParseExpression("~x", Factory.UnaryExpression(s_x, TsUnaryOperator.BitwiseNot));
            AssertParseExpression("!x", Factory.UnaryExpression(s_x, TsUnaryOperator.LogicalNot));
            AssertParseExpression("<T>x", Factory.Cast(s_T, s_x));
        }

        [Fact]
        public void TsParser_should_recognize_postfix_expressions()
        {
            AssertParseExpression("x++", Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement));
            AssertParseExpression("x--", Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement));
        }

        [Fact]
        public void TsParser_should_recognize_new_expressions()
        {
            AssertParseExpression("new x()", Factory.NewCall(s_x));
            AssertParseExpression(
                "new x<T>(y, ...z)",
                Factory.NewCall(
                    s_x,
                    Factory.ArgumentList(
                        s_T.ToSafeArray(),
                        Factory.Argument(s_y),
                        Factory.Argument(s_z, isSpreadArgument: true))));
        }

        [Fact]
        public void TsParser_should_recognize_super_calls()
        {
            AssertParseExpression("super(x, y)", Factory.SuperCall(Factory.ArgumentList(s_x, s_y)));
        }

        [Fact]
        public void TsParser_should_recognize_call_expressions()
        {
            AssertParseExpression("x(y, z)", Factory.Call(s_x, Factory.ArgumentList(s_y, s_z)));
            AssertParseExpression("x()[y]", Factory.MemberBracket(Factory.Call(s_x), s_y));
            AssertParseExpression("x().y", Factory.MemberDot(Factory.Call(s_x), "y"));
        }

        [Fact]
        public void TsParser_should_recognize_super_properties()
        {
            AssertParseExpression("super[x]", Factory.SuperBracket(s_x));
            AssertParseExpression("super.x", Factory.SuperDot("x"));
        }

        [Fact]
        public void TsParser_should_recognize_member_dot_and_bracket_expressions()
        {
            AssertParseExpression("x[y]", Factory.MemberBracket(s_x, s_y));
            AssertParseExpression("x.y", Factory.MemberDot(s_x, "y"));
        }

        [Fact]
        public void TsParser_should_recognize_parenthesized_expressions()
        {
            AssertParseExpression("(x)", Factory.ParenthesizedExpression(s_x));
        }

        [Fact]
        public void TsParser_should_get_precedence_correct()
        {
            // x() * y << (x+y)-- - -this.x + [1,2,3]"
            // Should be parsed as this:
            // A + [1,2,3]
            // A = B - -this.x
            // B = C << (x+y)--
            // C = x() * y

            var c = Factory.BinaryExpression(Factory.Call(s_x), TsBinaryOperator.Multiply, s_y);
            var b = Factory.BinaryExpression(
                c,
                TsBinaryOperator.LeftShift,
                Factory.UnaryExpression(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.Add, s_y).WithParentheses(),
                    TsUnaryOperator.PostfixDecrement));
            var a = Factory.BinaryExpression(
                b,
                TsBinaryOperator.Subtract,
                Factory.UnaryExpression(Factory.MemberDot(Factory.This, "x"), TsUnaryOperator.Minus));
            var expected = Factory.BinaryExpression(
                a,
                TsBinaryOperator.Add,
                Factory.Array(Factory.Number(1), Factory.Number(2), Factory.Number(3)));

            AssertParseExpression("x() * y << (x+y)-- - -this.x + [1,2,3]", expected);
        }

        [Fact]
        public void TsParser_should_recognize_keywords_as_identifiers()
        {
            AssertParseExpression("string.Empty", Factory.MemberDot(Factory.Identifier("string"), "Empty"));
        }

        [Fact]
        public void TsParser_should_recognize_a_qualified_name_as_an_expression()
        {
            AssertParseExpression("x.y.z()", Factory.Call(Factory.MemberDot(Factory.MemberDot(s_x, "y"), "z")));
            AssertParseExpression(
                "x.y[z].z()",
                Factory.Call(Factory.MemberDot(Factory.MemberBracket(Factory.MemberDot(s_x, "y"), s_z), "z")));
        }

        [Fact]
        public void TsParser_should_recognize_a_logical_or_with_an_arrow_function()
        {
            AssertParseExpression(
                "filter || (() => true)",
                Factory.BinaryExpression(
                    Factory.Identifier("filter"),
                    TsBinaryOperator.LogicalOr,
                    Factory.ArrowFunction(Factory.CallSignature(), Factory.True).WithParentheses()));
        }
    }
}
