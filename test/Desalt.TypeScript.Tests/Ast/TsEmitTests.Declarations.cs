// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Ast
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_simple_lexical_bindings()
        {
            VerifyOutput(Factory.SimpleLexicalBinding(s_x), "x");
            VerifyOutput(Factory.SimpleLexicalBinding(s_x, Factory.ArrayType(Factory.BooleanType)), "x: boolean[]");
            VerifyOutput(
                Factory.SimpleLexicalBinding(
                    s_x,
                    Factory.StringType,
                    Factory.String("hello")),
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
                    Factory.ArrayType(Factory.NumberType),
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
                    Factory.SimpleLexicalBinding(s_y, Factory.AnyType, s_z)),
                "const x, y: any = z;");

            VerifyOutput(
                Factory.LexicalDeclaration(
                    false,
                    Factory.SimpleLexicalBinding(s_x),
                    Factory.SimpleLexicalBinding(s_y, Factory.AnyType, s_z)),
                "let x, y: any = z;");
        }

        [TestMethod]
        public void Emit_anonymous_function_declaration_with_no_body()
        {
            VerifyOutput(
                Factory.FunctionDeclaration(
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)))),
                "function(x: number);\n");
        }

        [TestMethod]
        public void Emit_named_function_declaration_with_no_body()
        {
            VerifyOutput(
                Factory.FunctionDeclaration(
                    Factory.Identifier("func"),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.BooleanType)),
                        Factory.AnyType)),
                "function func(x: boolean): any;\n");
        }

        [TestMethod]
        public void Emit_anonymous_function_declaration_with_body()
        {
            VerifyOutput(
                Factory.FunctionDeclaration(
                    null,
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType))),
                    Factory.Return()),
                "function(x: number) {\n  return;\n}\n");
        }

        [TestMethod]
        public void Emit_named_function_declaration_with_body()
        {
            VerifyOutput(
                Factory.FunctionDeclaration(
                    Factory.Identifier("func"),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.BooleanType)),
                        Factory.AnyType),
                    Factory.Return(Factory.Zero)),
                "function func(x: boolean): any {\n  return 0;\n}\n");
        }
    }
}
