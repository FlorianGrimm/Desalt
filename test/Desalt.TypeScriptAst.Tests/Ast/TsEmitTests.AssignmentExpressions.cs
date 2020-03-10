// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.AssignmentExpressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;
    using Op = TypeScriptAst.Ast.Expressions.TsAssignmentOperator;

    public partial class TsEmitTests
    {
        [Test]
        public void Emit_all_assignment_expressions()
        {
            VerifyOutput(Factory.Assignment(s_x, Op.SimpleAssign, s_y), "x = y");
            VerifyOutput(Factory.Assignment(s_x, Op.AddAssign, s_y), "x += y");
            VerifyOutput(Factory.Assignment(s_x, Op.SubtractAssign, s_y), "x -= y");
            VerifyOutput(Factory.Assignment(s_x, Op.MultiplyAssign, s_y), "x *= y");
            VerifyOutput(Factory.Assignment(s_x, Op.DivideAssign, s_y), "x /= y");
            VerifyOutput(Factory.Assignment(s_x, Op.ModuloAssign, s_y), "x %= y");
            VerifyOutput(Factory.Assignment(s_x, Op.LeftShiftAssign, s_y), "x <<= y");
            VerifyOutput(Factory.Assignment(s_x, Op.SignedRightShiftAssign, s_y), "x >>= y");
            VerifyOutput(Factory.Assignment(s_x, Op.UnsignedRightShiftAssign, s_y), "x >>>= y");
            VerifyOutput(Factory.Assignment(s_x, Op.BitwiseAndAssign, s_y), "x &= y");
            VerifyOutput(Factory.Assignment(s_x, Op.BitwiseXorAssign, s_y), "x ^= y");
            VerifyOutput(Factory.Assignment(s_x, Op.BitwiseOrAssign, s_y), "x |= y");
        }

        [Test]
        public void Emit_comma_expressions()
        {
            VerifyOutput(Factory.CommaExpression(s_x, s_y), "x, y");
        }
    }
}
