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
    }
}
