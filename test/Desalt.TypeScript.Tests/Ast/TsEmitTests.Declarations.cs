// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Declarations.cs" company="Justin Rockwood">
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

        [TestMethod]
        public void Emit_constructor_declaration_with_no_body()
        {
            VerifyOutput(
                Factory.ConstructorDeclaration(
                    TsAccessibilityModifier.Public,
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, modifier: TsAccessibilityModifier.Protected),
                        Factory.StringRequiredParameter(s_y, Factory.String("str")))),
                "public constructor(protected x, y: 'str');\n");
        }

        [TestMethod]
        public void Emit_constructor_declaration_with_body()
        {
            VerifyOutput(
                Factory.ConstructorDeclaration(
                    TsAccessibilityModifier.Public,
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, modifier: TsAccessibilityModifier.Protected),
                        Factory.StringRequiredParameter(s_y, Factory.String("str"))),
                    Factory.Assignment(
                            Factory.MemberDot(Factory.This, "_y"),
                            TsAssignmentOperator.SimpleAssign,
                            s_y)
                        .ToStatement()
                        .ToSafeArray()),
                "public constructor(protected x, y: 'str') {\n  this._y = y;\n}\n");
        }

        [TestMethod]
        public void Emit_variable_member_declaration()
        {
            VerifyOutput(
                Factory.VariableMemberDeclaration(
                    s_x,
                    TsAccessibilityModifier.Public,
                    isStatic: true,
                    typeAnnotation: Factory.NumberType,
                    initializer: Factory.Number(10)),
                "public static x: number = 10;\n");

            VerifyOutput(
                Factory.VariableMemberDeclaration(
                    s_x,
                    isStatic: true,
                    typeAnnotation: Factory.NumberType,
                    initializer: Factory.Number(10)),
                "static x: number = 10;\n");

            VerifyOutput(Factory.VariableMemberDeclaration(s_x), "x;\n");
        }

        [TestMethod]
        public void Emit_function_member_declarations_with_no_body()
        {
            VerifyOutput(
                Factory.FunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType),
                    TsAccessibilityModifier.Private,
                    isStatic: true),
                "private static myMethod(x: number): void;\n");

            VerifyOutput(
                Factory.FunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    isStatic: true,
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType)),
                "static myMethod(x: number): void;\n");

            VerifyOutput(
                Factory.FunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType)),
                "myMethod(x: number): void;\n");
        }

        [TestMethod]
        public void Emit_function_member_declarations_with_a_body()
        {
            VerifyOutput(
                Factory.FunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.BooleanType)),
                        Factory.VoidType),
                    TsAccessibilityModifier.Protected,
                    functionBody: Factory.Return(Factory.Zero).ToSafeArray()),
                "protected myMethod(x: boolean): void {\n  return 0;\n}\n");
            VerifyOutput(
                Factory.FunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    Factory.CallSignature(),
                    functionBody: Factory.Return(Factory.Zero).ToSafeArray()),
                "myMethod() {\n  return 0;\n}\n");
        }

        [TestMethod]
        public void Emit_get_and_set_accessor_member_declarations()
        {
            VerifyOutput(
                Factory.GetAccessorMemberDeclaration(
                    Factory.GetAccessor(Factory.Identifier("field"), Factory.NumberType),
                    TsAccessibilityModifier.Public,
                    isStatic: true),
                "public static get field(): number { }\n");

            VerifyOutput(
                Factory.GetAccessorMemberDeclaration(
                    Factory.GetAccessor(Factory.Identifier("field"), Factory.NumberType)),
                "get field(): number { }\n");

            VerifyOutput(
                Factory.SetAccessorMemberDeclaration(
                    Factory.SetAccessor(Factory.Identifier("field"), Factory.Identifier("value"), Factory.NumberType),
                    TsAccessibilityModifier.Public,
                    isStatic: true),
                "public static set field(value: number) { }\n");

            VerifyOutput(
                Factory.SetAccessorMemberDeclaration(
                    Factory.SetAccessor(Factory.Identifier("field"), Factory.Identifier("value"), Factory.NumberType)),
                "set field(value: number) { }\n");
        }
    }
}
