﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmitTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Tests.Ast
{
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast.Expressions;
    using Desalt.JavaScript.Ast.Statements;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.JavaScript.Ast.Es5AstFactory;

    public partial class Es5EmitTests
    {
        [TestMethod]
        public void Emit_block_statements()
        {
            Es5BlockStatement block = Factory.BlockStatement(Factory.DebuggerStatement, Factory.ReturnStatement(s_x));
            VerifyOutput(block, "{\n  debugger;\n  return x;\n}");
        }

        [TestMethod]
        public void Emit_variable_statements()
        {
            const string expected = "var x = this, y, z = false;\n";
            Es5VariableStatement expression = Factory.VariableStatement(
                Factory.VariableDeclaration(s_x, Factory.ThisExpression),
                Factory.VariableDeclaration(s_y),
                Factory.VariableDeclaration(s_z, Factory.FalseLiteral));
            VerifyOutput(expression, expected);
        }

        [TestMethod]
        public void Emit_empty_statement()
        {
            VerifyOutput(Factory.EmptyStatement, ";\n");
        }

        [TestMethod]
        public void Emit_if_statement()
        {
            Es5IfStatement statement = Factory.IfStatement(
                Factory.BinaryExpression(s_x, Es5BinaryOperator.StrictEquals, s_y),
                Factory.BlockStatement(Factory.ReturnStatement(Factory.TrueLiteral)),
                elseStatement: null);

            VerifyOutput(statement, "if (x === y) {\n  return true;\n}");

            statement = statement.WithElseStatement(Factory.ReturnStatement(Factory.FalseLiteral));
            VerifyOutput(statement, "if (x === y) {\n  return true;\n} else\n  return false;\n");
        }

        [TestMethod]
        public void Emit_continue_statements()
        {
            VerifyOutput(Factory.ContinueLabelStatement(), "continue;\n");
            VerifyOutput(Factory.ContinueLabelStatement(s_x), "continue x;\n");
        }

        [TestMethod]
        public void Emit_break_statements()
        {
            VerifyOutput(Factory.BreakLabelStatement(), "break;\n");
            VerifyOutput(Factory.BreakLabelStatement(s_x), "break x;\n");
        }

        [TestMethod]
        public void Emit_return_statement()
        {
            VerifyOutput(Factory.ReturnStatement(s_x), "return x;\n");
        }

        [TestMethod]
        public void Emit_with_statement()
        {
            VerifyOutput(Factory.WithStatement(s_x, Factory.ReturnStatement(s_y)), "with (x)\n  return y;\n");
        }

        [TestMethod]
        public void Emit_labelled_statement()
        {
            VerifyOutput(Factory.LabelledStatement(s_x, Factory.DebuggerStatement), "x: debugger;\n");
        }

        [TestMethod]
        public void Emit_switch_statement()
        {
            const string expected = "switch (x) {\n•case \"true\":\n••x = y;\n••break;\n\n•" +
                "case 4:\n••return x;\n\n•default:\n••break;\n}\n";

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

            VerifyOutput(statement, expected, new EmitOptions(newline: "\n", indentationPrefix: "•"));
        }

        [TestMethod]
        public void Emit_throw_statement()
        {
            VerifyOutput(
                Factory.ThrowStatement(
                    Factory.NewCall(Factory.Identifier("Error"), Factory.DecimalLiteral("2"))),
                "throw new Error(2);\n");
        }

        [TestMethod]
        public void Emit_try_statements()
        {
            // try block only
            const string tryExpected = "try {\n  x += y;\n  return x;\n}";

            Es5TryStatement tryStatement = Factory.TryStatement(
                Factory.AssignmentExpression(s_x, Es5AssignmentOperator.AddAssign, s_y).ToStatement(),
                Factory.ReturnStatement(s_x));

            VerifyOutput(tryStatement, tryExpected);

            // try/catch blocks
            const string catchExpected = tryExpected + " catch (err) {\n  return y;\n}";
            Es5TryStatement catchStatement = tryStatement.WithCatch(
                Factory.Identifier("err"),
                Factory.ReturnStatement(s_y));

            VerifyOutput(catchStatement, catchExpected);

            // set up the finally block
            const string finallyExpected = " finally {\n  console.log('message');\n}";
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
        public void Emit_debugger_statement()
        {
            VerifyOutput(Factory.DebuggerStatement, "debugger;\n");
        }
    }
}