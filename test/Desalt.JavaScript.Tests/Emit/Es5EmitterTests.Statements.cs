// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitterTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Emit
{
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.CodeModels.Expressions;
    using Desalt.JavaScript.CodeModels.Statements;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.CodeModels.Es5ModelFactory;

    public partial class Es5EmitterTests
    {
        [TestMethod]
        public void Emit_block_statements()
        {
            Es5BlockStatement block = Factory.BlockStatement(Factory.DebuggerStatement, Factory.ReturnStatement(s_x));
            VerifyOutput(block, @"{
  debugger;
  return x;
}");
        }

        [TestMethod]
        public void Emit_block_statements_compact()
        {
            Es5BlockStatement block = Factory.BlockStatement(Factory.DebuggerStatement, Factory.ReturnStatement(s_x));
            VerifyOutput(block, "{debugger;return x;}", EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_variable_statements()
        {
            const string expected = @"var x = this, y, z = false;";
            Es5VariableStatement expression = Factory.VariableStatement(
                Factory.VariableDeclaration(s_x, Factory.ThisExpression),
                Factory.VariableDeclaration(s_y),
                Factory.VariableDeclaration(s_z, Factory.FalseLiteral));
            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_variable_statements_compact()
        {
            const string expected = @"var x=this,y,z=false;";
            Es5VariableStatement expression = Factory.VariableStatement(
                Factory.VariableDeclaration(s_x, Factory.ThisExpression),
                Factory.VariableDeclaration(s_y),
                Factory.VariableDeclaration(s_z, Factory.FalseLiteral));
            VerifyOutput(expression, expected, EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_empty_statement()
        {
            VerifyOutput(Factory.EmptyStatement, ";");
        }

        [TestMethod]
        public void Emit_if_statement()
        {
            Es5IfStatement statement = Factory.IfStatement(
                Factory.BinaryExpression(s_x, Es5BinaryOperator.StrictEquals, s_y),
                Factory.BlockStatement(Factory.ReturnStatement(Factory.TrueLiteral)),
                elseStatement: null);

            VerifyOutput(statement, "if (x === y) {\r\n  return true;\r\n}");
            VerifyOutput(statement, "if(x===y){return true;}", EmitOptions.Compact);

            statement = statement.WithElseStatement(Factory.ReturnStatement(Factory.FalseLiteral));
            VerifyOutput(statement, "if (x === y) { return true; } else return false;",
                EmitOptions.Default.WithSimpleBlockOnNewLine(false));
            VerifyOutput(statement, "if(x===y){return true;}else return false;", EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_continue_statements()
        {
            VerifyOutput(Factory.ContinueLabelStatement(), "continue;");
            VerifyOutput(Factory.ContinueLabelStatement(s_x), "continue x;");
        }

        [TestMethod]
        public void Emit_break_statements()
        {
            VerifyOutput(Factory.BreakLabelStatement(), "break;");
            VerifyOutput(Factory.BreakLabelStatement(s_x), "break x;");
        }

        [TestMethod]
        public void Emit_return_statement()
        {
            VerifyOutput(Factory.ReturnStatement(s_x), "return x;");
        }

        [TestMethod]
        public void Emit_with_statement()
        {
            VerifyOutput(Factory.WithStatement(s_x, Factory.ReturnStatement(s_y)), "with (x) return y;");
        }

        [TestMethod]
        public void Emit_with_statement_compact()
        {
            VerifyOutput(Factory.WithStatement(s_x, Factory.ReturnStatement(s_y)), "with(x)return y;", s_compact);
        }

        [TestMethod]
        public void Emit_labelled_statement()
        {
            VerifyOutput(Factory.LabelledStatement(s_x, Factory.DebuggerStatement), "x: debugger;");
        }

        [TestMethod]
        public void Emit_labelled_statement_compact()
        {
            VerifyOutput(Factory.LabelledStatement(s_x, Factory.DebuggerStatement), "x:debugger;", s_compact);
        }

        [TestMethod]
        public void Emit_switch_statement()
        {
            const string expected = @"switch (x) {
  case ""true"":
    x = y;
    break;

  case 4:
    return x;

  default:
    break;
}";
            Es5SwitchStatement statement = Factory.SwitchStatement(
                condition: s_x,
                caseClauses: Factory.CaseClauses(
                    Factory.CaseClause(
                        Factory.StringLiteral("\"true\""),
                        Factory.AssignmentExpression(s_x, Es5AssignmentOperator.SimpleAssign, s_y).ToStatement(),
                        Factory.BreakStatement),
                    Factory.CaseClause(
                        Factory.DecimalLiteral("4"),
                        Factory.ReturnStatement(s_x))),
                defaultClauseStatements: Factory.BreakStatement.ToSafeArray());

            VerifyOutput(statement, expected);
        }

        [TestMethod]
        public void Emit_switch_statement_compact()
        {
            const string expected = @"switch(x){case ""true"":x=y;break;case 4:return x;default:break;}";
            Es5SwitchStatement statement = Factory.SwitchStatement(
                condition: s_x,
                caseClauses: Factory.CaseClauses(
                    Factory.CaseClause(
                        Factory.StringLiteral("\"true\""),
                        Factory.AssignmentExpression(s_x, Es5AssignmentOperator.SimpleAssign, s_y).ToStatement(),
                        Factory.BreakStatement),
                    Factory.CaseClause(
                        Factory.DecimalLiteral("4"),
                        Factory.ReturnStatement(s_x))),
                defaultClauseStatements: Factory.BreakStatement.ToSafeArray());

            VerifyOutput(statement, expected, EmitOptions.Compact);
        }

        [TestMethod]
        public void Emit_throw_statement()
        {
            VerifyOutput(
                Factory.ThrowStatement(
                    Factory.NewCall(Factory.Identifier("Error"), Factory.DecimalLiteral("2"))),
                "throw new Error(2);");
        }

        [TestMethod]
        public void Emit_try_statements()
        {
            // try block only
            const string tryExpected = @"try {
  x += y;
  return x;
}";

            Es5TryStatement tryStatement = Factory.TryStatement(
                Factory.AssignmentExpression(s_x, Es5AssignmentOperator.AddAssign, s_y).ToStatement(),
                Factory.ReturnStatement(s_x));

            VerifyOutput(tryStatement, tryExpected);

            // try/catch blocks
            const string catchExpected = tryExpected + @" catch (err) {
  return y;
}";
            Es5TryStatement catchStatement = tryStatement.WithCatch(
                Factory.Identifier("err"),
                Factory.ReturnStatement(s_y));

            VerifyOutput(catchStatement, catchExpected);

            // set up the finally block
            const string finallyExpected = @" finally {
  console.log('message');
}";
            Es5BlockStatement finallyStatement = Factory.BlockStatement(
                Factory.Call(
                    Factory.MemberDot(Factory.Identifier("console"), Factory.Identifier("log")),
                    Factory.StringLiteral("'message'")).ToStatement());

            // try/finally block
            VerifyOutput(tryStatement.WithFinally(finallyStatement), tryExpected + finallyExpected);

            // try/catch/finally block
            VerifyOutput(catchStatement.WithFinally(finallyStatement), catchExpected + finallyExpected);
        }

        [TestMethod]
        public void Emit_try_statements_compact()
        {
            // try block only
            const string tryExpected = "try{x+=y;return x;}";
            Es5TryStatement tryStatement = Factory.TryStatement(
                Factory.AssignmentExpression(s_x, Es5AssignmentOperator.AddAssign, s_y).ToStatement(),
                Factory.ReturnStatement(s_x));

            VerifyOutput(tryStatement, tryExpected, EmitOptions.Compact);

            // try/catch blocks
            const string catchExpected = tryExpected + "catch(err){return y;}";
            Es5TryStatement catchStatement = tryStatement.WithCatch(
                Factory.Identifier("err"),
                Factory.ReturnStatement(s_y));

            VerifyOutput(catchStatement, catchExpected, EmitOptions.Compact);

            // set up the finally block
            const string finallyExpected = "finally{console.log('message');}";
            Es5BlockStatement finallyStatement = Factory.BlockStatement(
                Factory.Call(
                    Factory.MemberDot(Factory.Identifier("console"), Factory.Identifier("log")),
                    Factory.StringLiteral("'message'")).ToStatement());

            // try/finally block
            VerifyOutput(tryStatement.WithFinally(finallyStatement), tryExpected + finallyExpected, s_compact);

            // try/catch/finally block
            VerifyOutput(catchStatement.WithFinally(finallyStatement), catchExpected + finallyExpected, s_compact);
        }

        [TestMethod]
        public void Emit_debugger_statement()
        {
            VerifyOutput(Factory.DebuggerStatement, "debugger;");
        }
    }
}
