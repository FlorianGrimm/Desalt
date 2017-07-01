// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitTests.UnaryAndBinaryExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Ast
{
    using Desalt.JavaScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;

    public partial class Es5EmitTests
    {
        [TestMethod]
        public void Emit_unary_expressions()
        {
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixIncrement), "x++");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.PostfixDecrement), "x--");

            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.Delete), "delete x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.Void), "void x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.Typeof), "typeof x");

            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.PrefixIncrement), "++x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.PrefixDecrement), "--x");

            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.Plus), "+x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.Minus), "-x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.BitwiseNot), "~x");
            VerifyOutput(Factory.UnaryExpression(s_x, Es5UnaryOperator.LogicalNot), "!x");
        }

        [TestMethod]
        public void Emit_binary_expressions()
        {
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Multiply, s_y), "x * y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Divide, s_y), "x / y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Modulo, s_y), "x % y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Add, s_y), "x + y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Subtract, s_y), "x - y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.LeftShift, s_y), "x << y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.SignedRightShift, s_y), "x >> y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.UnsignedRightShift, s_y), "x >>> y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.LessThan, s_y), "x < y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.GreaterThan, s_y), "x > y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.LessThanEqual, s_y), "x <= y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.GreaterThanEqual, s_y), "x >= y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.InstanceOf, s_y), "x instanceof y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.In, s_y), "x in y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.Equals, s_y), "x == y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.NotEquals, s_y), "x != y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.StrictEquals, s_y), "x === y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.StrictNotEquals, s_y), "x !== y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.BitwiseAnd, s_y), "x & y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.BitwiseXor, s_y), "x ^ y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.BitwiseOr, s_y), "x | y");

            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.LogicalAnd, s_y), "x && y");
            VerifyOutput(Factory.BinaryExpression(s_x, Es5BinaryOperator.LogicalOr, s_y), "x || y");
        }

        [TestMethod]
        public void Emit_conditional_expressions()
        {
            VerifyOutput(Factory.ConditionalExpression(s_x, s_y, s_z), "x ? y : z");
        }
    }
}
