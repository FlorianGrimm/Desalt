// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitterTests.UnaryAndBinaryExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Emit
{
    using Desalt.TypeScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitterTests
    {
        [TestMethod]
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

        [TestMethod]
        public void Emit_unary_expressions_compact()
        {
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement), "x++", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement), "x--", s_compact);

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Delete), "delete x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Void), "void x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Typeof), "typeof x", s_compact);

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixIncrement), "++x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.PrefixDecrement), "--x", s_compact);

            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Plus), "+x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.Minus), "-x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.BitwiseNot), "~x", s_compact);
            VerifyOutput(Factory.UnaryExpression(s_x, TsUnaryOperator.LogicalNot), "!x", s_compact);
        }

        [TestMethod]
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

        [TestMethod]
        public void Emit_binary_expressions_compact()
        {
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Multiply, s_y), "x*y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Divide, s_y), "x/y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Modulo, s_y), "x%y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Add, s_y), "x+y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Subtract, s_y), "x-y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LeftShift, s_y), "x<<y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.SignedRightShift, s_y), "x>>y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.UnsignedRightShift, s_y), "x>>>y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, s_y), "x<y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThan, s_y), "x>y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LessThanEqual, s_y), "x<=y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, s_y), "x>=y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.InstanceOf, s_y), "x instanceof y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.In, s_y), "x in y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.Equals, s_y), "x==y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.NotEquals, s_y), "x!=y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y), "x===y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y), "x!==y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseAnd, s_y), "x&y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseXor, s_y), "x^y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.BitwiseOr, s_y), "x|y", s_compact);

            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalAnd, s_y), "x&&y", s_compact);
            VerifyOutput(Factory.BinaryExpression(s_x, TsBinaryOperator.LogicalOr, s_y), "x||y", s_compact);
        }

        [TestMethod]
        public void Emit_conditional_expressions()
        {
            VerifyOutput(Factory.ConditionalExpression(s_x, s_y, s_z), "x ? y : z");
        }

        [TestMethod]
        public void Emit_conditional_expressions_compact()
        {
            VerifyOutput(Factory.ConditionalExpression(s_x, s_y, s_z), "x?y:z", s_compact);
        }
    }
}
