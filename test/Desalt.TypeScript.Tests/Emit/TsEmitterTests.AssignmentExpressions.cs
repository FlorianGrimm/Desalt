// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitterTests.AssignmentExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Emit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;
    using Op = Desalt.TypeScript.Ast.Expressions.TsAssignmentOperator;

    public partial class TsEmitterTests
    {
        [TestMethod]
        public void Emit_all_assignment_expressions()
        {
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SimpleAssign, s_y), "x = y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.AddAssign, s_y), "x += y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SubtractAssign, s_y), "x -= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.MultiplyAssign, s_y), "x *= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.DivideAssign, s_y), "x /= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.ModuloAssign, s_y), "x %= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.LeftShiftAssign, s_y), "x <<= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SignedRightShiftAssign, s_y), "x >>= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.UnsignedRightShiftAssign, s_y), "x >>>= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseAndAssign, s_y), "x &= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseXorAssign, s_y), "x ^= y");
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseOrAssign, s_y), "x |= y");
        }

        [TestMethod]
        public void Emit_all_assignment_expressions_compact()
        {
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SimpleAssign, s_y), "x=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.AddAssign, s_y), "x+=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SubtractAssign, s_y), "x-=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.MultiplyAssign, s_y), "x*=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.DivideAssign, s_y), "x/=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.ModuloAssign, s_y), "x%=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.LeftShiftAssign, s_y), "x<<=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.SignedRightShiftAssign, s_y), "x>>=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.UnsignedRightShiftAssign, s_y), "x>>>=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseAndAssign, s_y), "x&=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseXorAssign, s_y), "x^=y", s_compact);
            VerifyOutput(Factory.AssignmentExpression(s_x, Op.BitwiseOrAssign, s_y), "x|=y", s_compact);
        }
    }
}
