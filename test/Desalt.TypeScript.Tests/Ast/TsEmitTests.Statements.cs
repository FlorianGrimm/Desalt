// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Ast
{
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast;
    using Desalt.TypeScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_debugger_statement()
        {
            VerifyOutput(Factory.Debugger, "debugger;\n");
        }

        [TestMethod]
        public void Emit_block_statements()
        {
            VerifyOutput(Factory.Block(Factory.Debugger, Factory.Debugger), "{\n  debugger;\n  debugger;\n}");
        }

        [TestMethod]
        public void Emit_empty_statement()
        {
            VerifyOutput(Factory.EmptyStatement, ";\n");
        }

        [TestMethod]
        public void Emit_simple_variable_declarations()
        {
            VerifyOutput(Factory.SimpleVariableDeclaration(s_x), "x");
            VerifyOutput(Factory.SimpleVariableDeclaration(s_x, Factory.ArrayType(Factory.BooleanType)), "x: boolean[]");
            VerifyOutput(
                Factory.SimpleVariableDeclaration(
                    s_x,
                    Factory.StringType,
                    Factory.String("hello")),
                "x: string = 'hello'");
        }

        [TestMethod]
        public void Emit_single_name_binding()
        {
            VerifyOutput(Factory.SingleNameBinding(s_z), "z");
            VerifyOutput(Factory.SingleNameBinding(s_x, s_y), "x = y");
        }

        [TestMethod]
        public void Emit_object_binding_pattern()
        {
            VerifyOutput(
                Factory.ObjectBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                "{x, y}");
        }

        [TestMethod]
        public void Emit_object_binding_pattern_with_default_values()
        {
            VerifyOutput(
                Factory.ObjectBindingPattern(
                    Factory.SingleNameBinding(s_x, Factory.Null),
                    Factory.SingleNameBinding(s_y, Factory.Number(10))),
                "{x = null, y = 10}");
        }

        [TestMethod]
        public void Emit_a_recursive_pattern_binding()
        {
            VerifyOutput(
                Factory.PatternBinding(
                    Factory.ObjectBindingPattern(
                        Factory.SingleNameBinding(
                            Factory.Identifier("size"), Factory.String("big")),
                        Factory.SingleNameBinding(
                            Factory.Identifier("cords"),
                            Factory.Object(
                                Factory.PropertyAssignment(s_x, Factory.Zero),
                                Factory.PropertyAssignment(s_y, Factory.Zero))),
                        Factory.SingleNameBinding(Factory.Identifier("radius"), Factory.Number(25))),
                    Factory.EmptyObject),
                "{size = 'big', cords = {\n  x: 0,\n  y: 0\n}, radius = 25} = {}");
        }

        [TestMethod]
        public void Emit_array_binding_pattern()
        {
            VerifyOutput(
                Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                "[x, y]");
        }

        [TestMethod]
        public void Emit_array_binding_pattern_with_default_values()
        {
            VerifyOutput(
                Factory.ArrayBindingPattern(
                    Factory.SingleNameBinding(s_x, Factory.Null),
                    Factory.SingleNameBinding(s_y, Factory.Number(10))),
                "[x = null, y = 10]");
        }

        [TestMethod]
        public void Emit_array_binding_pattern_with_a_rest_parameter()
        {
            VerifyOutput(
                Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x).ToSafeArray(), s_y),
                "[x, ... y]");
        }

        [TestMethod]
        public void Emit_destructuring_variable_declaration_with_no_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringVariableDeclaration(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    s_z),
                "[x, y] = z");
        }

        [TestMethod]
        public void Emit_destructuring_variable_declaration_with_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringVariableDeclaration(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    Factory.ArrayType(Factory.NumberType),
                    s_z),
                "[x, y]: number[] = z");
        }

        [TestMethod]
        public void Emit_variable_declaration_statements()
        {
            VerifyOutput(
                Factory.VariableStatement(
                    Factory.SimpleVariableDeclaration(
                        s_x, Factory.BooleanType, Factory.BinaryExpression(s_y, TsBinaryOperator.LogicalAnd, s_z)),
                    Factory.SimpleVariableDeclaration(s_p)),
                "var x: boolean = y && z, p;\n");
        }

        [TestMethod]
        public void Emit_expression_statements()
        {
            VerifyOutput(Factory.EmptyObject.ToStatement(), "{};\n");
            VerifyOutput(
                Factory.ExpressionStatement(Factory.UnaryExpression(s_x, TsUnaryOperator.BitwiseNot)), "~x;\n");
        }

        [TestMethod]
        public void Emit_if_statement_with_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y),
                    Factory.Block(
                        Factory.Assignment(
                                s_z, TsAssignmentOperator.SimpleAssign, Factory.True)
                            .ToStatement())),
                "if (x === y) {\n  z = true;\n}");
        }

        [TestMethod]
        public void Emit_if_statement_without_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y),
                    Factory.Assignment(
                            s_z, TsAssignmentOperator.SimpleAssign, Factory.True)
                        .ToStatement()),
                "if (x === y)\n  z = true;\n");
        }

        [TestMethod]
        public void Emit_if_else_statement_with_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y),
                    Factory.Block(
                        Factory.Assignment(
                            s_z, TsAssignmentOperator.SimpleAssign, Factory.False).ToStatement()),
                    Factory.Block(Factory.UnaryExpression(s_p, TsUnaryOperator.PostfixIncrement).ToStatement())),
                "if (x !== y) {\n  z = false;\n} else {\n  p++;\n}");
        }

        [TestMethod]
        public void Emit_if_else_statement_without_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y),
                    Factory.Assignment(
                        s_z, TsAssignmentOperator.SimpleAssign, Factory.False).ToStatement(),
                    Factory.UnaryExpression(s_p, TsUnaryOperator.PostfixIncrement).ToStatement()),
                "if (x !== y)\n  z = false;\nelse\n  p++;\n");
        }

        [TestMethod]
        public void Emit_try_only_statement()
        {
            VerifyOutput(
                Factory.Try(Factory.Assignment(s_x, TsAssignmentOperator.AddAssign, s_y).ToBlock()),
                "try {\n  x += y;\n}\n");
        }

        [TestMethod]
        public void Emit_try_catch_statement()
        {
            VerifyOutput(
                Factory.TryCatch(
                    Factory.Assignment(
                        s_x,
                        TsAssignmentOperator.SimpleAssign,
                        Factory.NewCall(Factory.Identifier("Widget"))).ToBlock(),
                    Factory.Identifier("err"),
                    Factory.Call(Factory.QualifiedName("console.log"), Factory.Argument(Factory.Identifier("err")))
                        .ToBlock()),
                "try {\n  x = new Widget();\n} catch (err) {\n  console.log(err);\n}\n");
        }

        [TestMethod]
        public void Emit_try_finally_statement()
        {
            VerifyOutput(
                Factory.TryFinally(
                    Factory.Debugger.ToBlock(),
                    Factory.Call(s_x).ToBlock()),
                "try {\n  debugger;\n} finally {\n  x();\n}\n");
        }

        [TestMethod]
        public void Emit_try_catch_finally_statement()
        {
            VerifyOutput(
                Factory.TryCatchFinally(
                    Factory.Debugger.ToBlock(),
                    Factory.Identifier("e"),
                    Factory.SuperCall(Factory.Argument(Factory.Identifier("e"))).ToBlock(),
                    Factory.VariableStatement(
                        Factory.SimpleVariableDeclaration(s_p, initializer: Factory.Number(1.2))).ToBlock()),
                "try {\n  debugger;\n} catch (e) {\n  super(e);\n} finally {\n  var p = 1.2;\n}\n");
        }

        [TestMethod]
        public void Emit_do_while_loop_without_block()
        {
            VerifyOutput(
                Factory.DoWhile(
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement).ToStatement(),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, Factory.Zero)),
                "do\n  x--;\nwhile (x >= 0);\n");
        }

        [TestMethod]
        public void Emit_do_while_loop_with_block()
        {
            VerifyOutput(
                Factory.DoWhile(
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement).ToBlock(),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, Factory.Zero)),
                "do {\n  x--;\n} while (x >= 0);\n");
        }

        [TestMethod]
        public void Emit_while_loop_without_block()
        {
            VerifyOutput(
                Factory.While(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, Factory.Zero),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement).ToStatement()),
                "while (x >= 0)\n  x--;\n");
        }

        [TestMethod]
        public void Emit_while_loop_with_block()
        {
            VerifyOutput(
                Factory.While(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.GreaterThanEqual, Factory.Zero),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixDecrement).ToBlock()),
                "while (x >= 0) {\n  x--;\n}");
        }

        [TestMethod]
        public void Emit_basic_for_loop()
        {
            VerifyOutput(
                Factory.For(
                    Factory.Assignment(s_x, TsAssignmentOperator.SimpleAssign, Factory.Zero),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, Factory.Number(10)),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement),
                    Factory.Debugger),
                "for (x = 0; x < 10; x++)\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_for_loop_with_variable_declaration()
        {
            VerifyOutput(
                Factory.For(
                    Factory.SimpleVariableDeclaration(s_x, initializer: Factory.Zero),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, Factory.Number(10)),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement),
                    Factory.Debugger),
                "for (var x = 0; x < 10; x++)\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_for_loop_with_const_and_let_declarations()
        {
            VerifyOutput(
                Factory.For(
                    Factory.LexicalDeclaration(true, Factory.SimpleLexicalBinding(s_x, Factory.NumberType, Factory.Zero)),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, Factory.Number(10)),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement),
                    Factory.Debugger),
                "for (const x: number = 0; x < 10; x++)\n  debugger;\n");

            VerifyOutput(
                Factory.For(
                    Factory.LexicalDeclaration(false, Factory.SimpleLexicalBinding(s_x, Factory.NumberType, Factory.Zero)),
                    Factory.BinaryExpression(s_x, TsBinaryOperator.LessThan, Factory.Number(10)),
                    Factory.UnaryExpression(s_x, TsUnaryOperator.PostfixIncrement),
                    Factory.Debugger),
                "for (let x: number = 0; x < 10; x++)\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_basic_for_in_loop()
        {
            VerifyOutput(Factory.ForIn(s_x, s_y, Factory.Debugger.ToBlock()), "for (x in y) {\n  debugger;\n}");
        }

        [TestMethod]
        public void Emit_for_in_loop_with_declarations()
        {
            VerifyOutput(
                Factory.ForIn(
                    ForDeclarationKind.Const,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (const x in [1, 2])\n  debugger;\n");

            VerifyOutput(
                Factory.ForIn(
                    ForDeclarationKind.Let,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (let x in [1, 2])\n  debugger;\n");

            VerifyOutput(
                Factory.ForIn(
                    ForDeclarationKind.Var,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (var x in [1, 2])\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_basic_for_of_loop()
        {
            VerifyOutput(Factory.ForOf(s_x, s_y, Factory.Debugger.ToBlock()), "for (x of y) {\n  debugger;\n}");
        }

        [TestMethod]
        public void Emit_for_of_loop_with_declarations()
        {
            VerifyOutput(
                Factory.ForOf(
                    ForDeclarationKind.Const,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (const x of [1, 2])\n  debugger;\n");

            VerifyOutput(
                Factory.ForOf(
                    ForDeclarationKind.Let,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (let x of [1, 2])\n  debugger;\n");

            VerifyOutput(
                Factory.ForOf(
                    ForDeclarationKind.Var,
                    s_x,
                    Factory.Array(Factory.Number(1), Factory.Number(2)),
                    Factory.Debugger),
                "for (var x of [1, 2])\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_case_clause()
        {
            VerifyOutput(
                Factory.CaseClause(s_x, Factory.Debugger),
                "case x:\n  debugger;\n");
        }

        [TestMethod]
        public void Emit_case_clause_with_block()
        {
            VerifyOutput(
                Factory.CaseClause(s_x, Factory.Debugger.ToBlock()),
                "case x:\n  {\n    debugger;\n  }");
        }

        [TestMethod]
        public void Emit_case_clause_with_empty_statements_should_not_indent_next_line()
        {
            VerifyOutput(Factory.CaseClause(s_x), "case x:\n");
        }

        [TestMethod]
        public void Emit_default_clause()
        {
            VerifyOutput(
                Factory.DefaultClause(
                    Factory.Call(
                        s_x,
                        Factory.Argument(s_y),
                        Factory.Argument(s_z))
                    .ToStatement()),
                "default:\n  x(y, z);\n");
        }

        [TestMethod]
        public void Emit_switch_statement()
        {
            VerifyOutput(
                Factory.Switch(
                    s_x,
                    Factory.CaseClause(Factory.String("a"), Factory.Debugger),
                    Factory.CaseClause(Factory.String("b")),
                    Factory.DefaultClause(),
                    Factory.CaseClause(
                        Factory.String("c"),
                        Factory.Assignment(s_y, TsAssignmentOperator.AddAssign, Factory.Number(4))
                        .ToStatement())),
                @"switch (x) {
  case 'a':
    debugger;

  case 'b':
  default:
  case 'c':
    y += 4;
}
".Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void Emit_continue_statement_with_no_label()
        {
            VerifyOutput(Factory.Continue(), "continue;\n");
        }

        [TestMethod]
        public void Emit_continue_statement_with_label()
        {
            VerifyOutput(Factory.Continue(s_x), "continue x;\n");
        }

        [TestMethod]
        public void Emit_break_statement_with_no_label()
        {
            VerifyOutput(Factory.Break(), "break;\n");
        }

        [TestMethod]
        public void Emit_break_statement_with_label()
        {
            VerifyOutput(Factory.Break(s_x), "break x;\n");
        }

        [TestMethod]
        public void Emit_a_return_statement()
        {
            VerifyOutput(
                Factory.Return(Factory.BinaryExpression(s_x, TsBinaryOperator.Divide, s_y)), "return x / y;\n");
        }

        [TestMethod]
        public void Emit_a_return_statement_with_no_expression()
        {
            VerifyOutput(Factory.Return(), "return;\n");
        }

        [TestMethod]
        public void Emit_with_statements()
        {
            VerifyOutput(
                Factory.With(
                    Factory.Object(
                        Factory.PropertyAssignment(Factory.Identifier("foo"), Factory.Number(1)),
                        Factory.PropertyAssignment(Factory.ComputedPropertyName(Factory.String("bar")), Factory.Null)),
                    Factory.Return(Factory.Number(50))),
                "with ({\n  foo: 1,\n  ['bar']: null\n})\n  return 50;\n");
        }

        [TestMethod]
        public void Emit_labelled_statement_with_label_at_same_indentation_level()
        {
            VerifyOutput(
                Factory.LabelledStatement(Factory.Identifier("Label"), Factory.Return()),
                "Label:\nreturn;\n");
        }

        [TestMethod]
        public void Emit_labelled_statement_with_label_on_one_less_indentation_level()
        {
            VerifyOutput(
                Factory.Switch(
                    s_x,
                    Factory.CaseClause(
                        Factory.Number(1),
                        Factory.LabelledStatement(Factory.Identifier("Label"), Factory.Return()))),
                "switch (x) {\n  case 1:\n  Label:\n    return;\n}\n");
        }

        [TestMethod]
        public void Emit_throw_statement()
        {
            VerifyOutput(
                Factory.Throw(
                    Factory.NewCall(Factory.Identifier("Error"), Factory.Argument(Factory.String("message")))),
                "throw new Error('message');\n");
        }
    }
}
