// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Parsing
{
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Parsing;
    using FluentAssertions;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        private static void AssertParseStatement(string code, ITsStatement expected)
        {
            ITsStatement actual = TsParser.ParseStatement(code);
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void TsParser_should_recognize_a_debugger_statement()
        {
            AssertParseStatement("debugger;", Factory.Debugger);
        }

        [Test]
        public void TsParser_should_recognize_an_empty_statement()
        {
            AssertParseStatement(";", Factory.EmptyStatement);
        }

        [Test]
        public void TsParser_should_recognize_an_empty_block()
        {
            AssertParseStatement("{}", Factory.Block());
        }

        [Test]
        public void TsParser_ParseStatement_should_recognize_variable_statements()
        {
            AssertParseStatement(
                "var x: string = 'str';",
                Factory.VariableStatement(
                    Factory.SimpleVariableDeclaration(s_x, Factory.StringType, Factory.String("str"))));
        }

        [Test]
        public void TsParser_should_parse_if_statements()
        {
            AssertParseStatement("if (x) debugger;", Factory.IfStatement(s_x, Factory.Debugger));
            AssertParseStatement(
                "if (x) debugger; else {}",
                Factory.IfStatement(s_x, Factory.Debugger, Factory.Block()));
        }

        [Test]
        public void TsParser_should_parse_continue_statements()
        {
            AssertParseStatement("continue;", Factory.Continue());
            AssertParseStatement("continue x;", Factory.Continue(s_x));
        }

        [Test]
        public void TsParser_should_parse_break_statements()
        {
            AssertParseStatement("break;", Factory.Break());
            AssertParseStatement("break x;", Factory.Break(s_x));
        }

        [Test]
        public void TsParser_should_parse_return_statements()
        {
            AssertParseStatement("return;", Factory.Return());
            AssertParseStatement("return x;", Factory.Return(s_x));
        }

        [Test]
        public void TsParser_should_parse_a_with_statement()
        {
            AssertParseStatement("with (x) {}", Factory.With(s_x, Factory.Block()));
        }

        [Test]
        public void TsParser_should_parse_a_throw_statement()
        {
            AssertParseStatement("throw x;", Factory.Throw(s_x));
        }

        [Test]
        public void TsParser_should_parse_try_statements()
        {
            ITsBlockStatement block = Factory.Block();

            AssertParseStatement("try { }", Factory.Try(block));
            AssertParseStatement("try {} catch {}", Factory.TryCatch(block, block));
            AssertParseStatement("try {} catch (x) {}", Factory.TryCatch(block, s_x, block));
            AssertParseStatement("try {} catch (x) {} finally {}", Factory.TryCatchFinally(block, s_x, block, block));
            AssertParseStatement("try {} finally {}", Factory.TryFinally(block, block));
        }

        [Test]
        public void TsParser_should_parse_labeled_statements()
        {
            AssertParseStatement("x: return x;", Factory.LabeledStatement(s_x, Factory.Return(s_x)));
            AssertParseStatement(
                "x: function ();",
                Factory.LabeledStatement(s_x, Factory.FunctionDeclaration(Factory.CallSignature())));
        }

        [Test]
        public void TsParser_should_parse_do_while_statements()
        {
            AssertParseStatement("do {} while (true);", Factory.DoWhile(Factory.Block(), Factory.True));
        }

        [Test]
        public void TsParser_should_parse_switch_statements()
        {
            AssertParseStatement(
                @"
switch (x) {
  case 10:
  case 11:
    break;

  default:
    return y;
}",
                Factory.Switch(
                    s_x,
                    Factory.CaseClause(Factory.Number(10)),
                    Factory.CaseClause(Factory.Number(11), Factory.Break()),
                    Factory.DefaultClause(Factory.Return(s_y))));
        }

        [Test]
        public void TsParser_should_parse_expression_statements()
        {
            AssertParseStatement(
                "x = y;",
                Factory.ExpressionStatement(Factory.Assignment(s_x, TsAssignmentOperator.SimpleAssign, s_y)));
        }
    }
}
