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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
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
        public void Emit_debugger_statement()
        {
            VerifyOutput(Factory.Debugger, "debugger;\n");
        }
    }
}
