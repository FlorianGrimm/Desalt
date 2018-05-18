// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.UnaryAndBinaryExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.TypeScript.Ast
{
    using TypeScriptAst.TypeScript.Ast.Expressions;
    using Xunit;
    using Factory = TypeScriptAst.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [Fact]
        public void Emit_unary_expressions()
        {
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement), "x++");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement), "x--");

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Delete), "delete x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Void), "void x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Typeof), "typeof x");

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixIncrement), "++x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixDecrement), "--x");

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Plus), "+x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Minus), "-x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.BitwiseNot), "~x");
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.LogicalNot), "!x");
        }

        [Fact]
        public void Emit_cast_unary_expression()
        {
            VerifyOutput(
                Factory.Cast(Factory.NumberType, Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixIncrement)),
                "<number>++x");
        }

        [Fact]
        public void Emit_binary_expressions()
        {
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Multiply, s_y), "x * y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Divide, s_y), "x / y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Modulo, s_y), "x % y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Add, s_y), "x + y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Subtract, s_y), "x - y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LeftShift, s_y), "x << y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.SignedRightShift, s_y), "x >> y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.UnsignedRightShift, s_y), "x >>> y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, s_y), "x < y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThan, s_y), "x > y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LessThanEqual, s_y), "x <= y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, s_y), "x >= y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.InstanceOf, s_y), "x instanceof y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.In, s_y), "x in y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Equals, s_y), "x == y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.NotEquals, s_y), "x != y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y), "x === y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y), "x !== y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseAnd, s_y), "x & y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseXor, s_y), "x ^ y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseOr, s_y), "x | y");

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalAnd, s_y), "x && y");
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalOr, s_y), "x || y");
        }

        [Fact]
        public void Emit_conditional_expressions()
        {
            VerifyOutput(Factory.Conditional(s_x, s_y, s_z), "x ? y : z");
        }
    }
}
