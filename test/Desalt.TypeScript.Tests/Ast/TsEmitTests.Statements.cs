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
            VerifyOutput(Factory.SimpleVariableDeclaration(s_x, Factory.ArrayType(Factory.Boolean)), "x: boolean[]");
            VerifyOutput(
                Factory.SimpleVariableDeclaration(
                    s_x,
                    Factory.String,
                    Factory.StringLiteral("hello", StringLiteralQuoteKind.SingleQuote)),
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
                    Factory.SingleNameBinding(s_x, Factory.NullLiteral),
                    Factory.SingleNameBinding(s_y, Factory.DecimalLiteral(10))),
                "{x = null, y = 10}");
        }

        [TestMethod]
        public void Emit_a_recursive_pattern_binding()
        {
            VerifyOutput(
                Factory.PatternBinding(
                    Factory.ObjectBindingPattern(
                        Factory.SingleNameBinding(
                            Factory.Identifier("size"), Factory.StringLiteral("big", StringLiteralQuoteKind.SingleQuote)),
                        Factory.SingleNameBinding(
                            Factory.Identifier("cords"),
                            Factory.ObjectLiteral(
                                Factory.PropertyAssignment(s_x, Factory.Zero),
                                Factory.PropertyAssignment(s_y, Factory.Zero))),
                        Factory.SingleNameBinding(Factory.Identifier("radius"), Factory.DecimalLiteral(25))),
                    Factory.EmptyObjectLiteral),
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
                    Factory.SingleNameBinding(s_x, Factory.NullLiteral),
                    Factory.SingleNameBinding(s_y, Factory.DecimalLiteral(10))),
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
                    Factory.ArrayType(Factory.Number),
                    s_z),
                "[x, y]: number[] = z");
        }

        [TestMethod]
        public void Emit_variable_declaration_statements()
        {
            VerifyOutput(
                Factory.VariableStatement(
                    Factory.SimpleVariableDeclaration(
                        s_x, Factory.Boolean, Factory.BinaryExpression(s_y, TsBinaryOperator.LogicalAnd, s_z)),
                    Factory.SimpleVariableDeclaration(s_p)),
                "var x: boolean = y && z, p;\n");
        }

        [TestMethod]
        public void Emit_expression_statements()
        {
            VerifyOutput(Factory.EmptyObjectLiteral.ToStatement(), "{};\n");
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
                        Factory.AssignmentExpression(
                                s_z, TsAssignmentOperator.SimpleAssign, Factory.TrueLiteral)
                            .ToStatement())),
                "if (x === y) {\n  z = true;\n}");
        }

        [TestMethod]
        public void Emit_if_statement_without_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictEquals, s_y),
                    Factory.AssignmentExpression(
                            s_z, TsAssignmentOperator.SimpleAssign, Factory.TrueLiteral)
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
                        Factory.AssignmentExpression(
                            s_z, TsAssignmentOperator.SimpleAssign, Factory.FalseLiteral).ToStatement()),
                    Factory.Block(Factory.UnaryExpression(s_p, TsUnaryOperator.PostfixIncrement).ToStatement())),
                "if (x !== y) {\n  z = false;\n} else {\n  p++;\n}");
        }

        [TestMethod]
        public void Emit_if_else_statement_without_blocks()
        {
            VerifyOutput(
                Factory.IfStatement(
                    Factory.BinaryExpression(s_x, TsBinaryOperator.StrictNotEquals, s_y),
                    Factory.AssignmentExpression(
                        s_z, TsAssignmentOperator.SimpleAssign, Factory.FalseLiteral).ToStatement(),
                    Factory.UnaryExpression(s_p, TsUnaryOperator.PostfixIncrement).ToStatement()),
                "if (x !== y)\n  z = false;\nelse\n  p++;\n");
        }

        [TestMethod]
        public void Emit_try_only_statement()
        {
            VerifyOutput(
                Factory.Try(Factory.AssignmentExpression(s_x, TsAssignmentOperator.AddAssign, s_y).ToBlock()),
                "try {\n  x += y;\n}\n");
        }

        [TestMethod]
        public void Emit_try_catch_statement()
        {
            VerifyOutput(
                Factory.TryCatch(
                    Factory.AssignmentExpression(
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
                        Factory.SimpleVariableDeclaration(s_p, initializer: Factory.DecimalLiteral(1.2))).ToBlock()),
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
        public void Emit_simple_lexical_bindings()
        {
            VerifyOutput(Factory.SimpleLexicalBinding(s_x), "x");
            VerifyOutput(Factory.SimpleLexicalBinding(s_x, Factory.ArrayType(Factory.Boolean)), "x: boolean[]");
            VerifyOutput(
                Factory.SimpleLexicalBinding(
                    s_x,
                    Factory.String,
                    Factory.StringLiteral("hello", StringLiteralQuoteKind.SingleQuote)),
                "x: string = 'hello'");
        }

        [TestMethod]
        public void Emit_destructuring_lexical_binding_with_no_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    initializer: s_z),
                "[x, y] = z");
        }

        [TestMethod]
        public void Emit_destructuring_lexical_binding_with_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    Factory.ArrayType(Factory.Number),
                    s_z),
                "[x, y]: number[] = z");
        }

        [TestMethod]
        public void Emit_destructuring_lexical_binding_with_no_type_annotation_or_initializer()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y))),
                "[x, y]");
        }

        [TestMethod]
        public void Emit_lexical_declarations()
        {
            VerifyOutput(
                Factory.LexicalDeclaration(
                    true,
                    Factory.SimpleLexicalBinding(s_x),
                    Factory.SimpleLexicalBinding(s_y, Factory.Any, s_z)),
                "const x, y: any = z;");

            VerifyOutput(
                Factory.LexicalDeclaration(
                    false,
                    Factory.SimpleLexicalBinding(s_x),
                    Factory.SimpleLexicalBinding(s_y, Factory.Any, s_z)),
                "let x, y: any = z;");
        }
    }
}
