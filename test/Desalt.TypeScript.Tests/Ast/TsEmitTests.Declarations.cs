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
                "const x, y: any = z;\n");

            VerifyOutput(
                Factory.LexicalDeclaration(
                    false,
                    Factory.SimpleLexicalBinding(s_x),
                    Factory.SimpleLexicalBinding(s_y, Factory.AnyType, s_z)),
                "let x, y: any = z;\n");
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

        [TestMethod]
        public void Emit_index_member_declarations()
        {
            VerifyOutput(
                Factory.IndexMemberDeclaration(
                    Factory.IndexSignature(Factory.Identifier("key"), false, Factory.AnyType)),
                "[key: string]: any;\n");
        }

        [TestMethod]
        public void Emit_class_heritage()
        {
            VerifyOutput(
                Factory.ClassHeritage(
                    Factory.TypeReference(Factory.Identifier("Foo")),
                    new[]
                    {
                        Factory.TypeReference(Factory.Identifier("IOne")),
                        Factory.TypeReference(Factory.Identifier("ITwo")),
                    }),
                " extends Foo implements IOne, ITwo");

            VerifyOutput(
                Factory.ClassHeritage(Factory.TypeReference(Factory.Identifier("Foo"))),
                " extends Foo");

            VerifyOutput(
                Factory.ClassHeritage(
                    new[]
                    {
                        Factory.TypeReference(Factory.Identifier("IOne")),
                        Factory.TypeReference(Factory.Identifier("ITwo")),
                    }),
                " implements IOne, ITwo");
        }

        [TestMethod]
        public void Emit_empty_class_declaration()
        {
            VerifyOutput(Factory.ClassDeclaration(), "class {\n}\n");
        }

        [TestMethod]
        public void Emit_class_declaration_with_all_of_the_possible_elements()
        {
            // ReSharper disable once InconsistentNaming
            ITsIdentifier _items = Factory.Identifier("_items");
            ITsIdentifier item = Factory.Identifier("item");
            ITsIdentifier items = Factory.Identifier("items");
            ITsIdentifier length = Factory.Identifier("length");
            ITsIdentifier value = Factory.Identifier("value");
            ITsMemberDotExpression thisItems = Factory.MemberDot(Factory.This, "_items");
            ITsMemberDotExpression thisItemsLength = Factory.MemberDot(thisItems, "length");

            VerifyOutput(
                Factory.ClassDeclaration(
                    Factory.Identifier("AnimalCollection"),
                    Factory.TypeParameters(Factory.TypeParameter(s_T, Factory.TypeReference(Factory.Identifier("IAnimal")))),
                    Factory.ClassHeritage(
                        Factory.TypeReference(Factory.Identifier("Collection"), s_TRef),
                        Factory.TypeReference(Factory.Identifier("ICollection"), s_TRef).ToSafeArray()),
                    new ITsClassElement[]
                    {
                        Factory.VariableMemberDeclaration(
                            _items, TsAccessibilityModifier.Private,typeAnnotation: Factory.ArrayType(s_TRef)),
                        Factory.ConstructorDeclaration(
                            TsAccessibilityModifier.Public,
                            Factory.ParameterList(
                                requiredParameters: Factory.BoundRequiredParameter(item, s_TRef).ToSafeArray(),
                                restParameter: Factory.RestParameter(items, Factory.ArrayType(s_TRef))),
                            Factory.Assignment(
                                thisItems,
                                TsAssignmentOperator.SimpleAssign,
                                Factory.Array(
                                    Factory.ArrayElement(item), Factory.ArrayElement(items, isSpreadElement: true)))
                            .ToStatement()
                            .ToSafeArray()),
                        Factory.IndexMemberDeclaration(
                            Factory.IndexSignature(
                                Factory.Identifier("index"),
                                isParameterNumberType: true,
                                returnType: s_TRef)),
                        Factory.GetAccessorMemberDeclaration(
                            Factory.GetAccessor(length, Factory.NumberType, Factory.Return(thisItemsLength)),
                            TsAccessibilityModifier.Public),
                        Factory.SetAccessorMemberDeclaration(
                            Factory.SetAccessor(
                                length, value, Factory.NumberType,
                                Factory.Assignment(thisItemsLength, TsAssignmentOperator.SimpleAssign, value)
                                .ToStatement()),
                            TsAccessibilityModifier.Public),
                        Factory.FunctionMemberDeclaration(
                            Factory.Identifier("add"),
                            Factory.CallSignature(
                                Factory.ParameterList(Factory.BoundRequiredParameter(item, s_TRef)),
                                Factory.VoidType),
                            TsAccessibilityModifier.Public,
                            functionBody: Factory.Assignment(
                                Factory.MemberBracket(thisItems, thisItemsLength),
                                TsAssignmentOperator.SimpleAssign,
                                item).ToStatement().ToSafeArray())
                    }),
                @"class AnimalCollection<T extends IAnimal> extends Collection<T> implements ICollection<T> {
  private _items: T[];

  public constructor(item: T, ... items: T[]) {
    this._items = [item, ... items];
  }

  [index: number]: T;

  public get length(): number {
    return this._items.length;
  }

  public set length(value: number) {
    this._items.length = value;
  }

  public add(item: T): void {
    this._items[this._items.length] = item;
  }
}
".Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void Emit_empty_interface_declaration()
        {
            VerifyOutput(Factory.InterfaceDeclaration(s_T, Factory.ObjectType()), "interface T {\n}\n");
        }

        [TestMethod]
        public void Emit_interface_declaration()
        {
            VerifyOutput(
                Factory.InterfaceDeclaration(
                    Factory.Identifier("ISomething"),
                    typeParameters: Factory.TypeParameters(Factory.TypeParameter(s_T)),
                    extendsClause: Factory.TypeReference(Factory.Identifier("IBase")).ToSafeArray(),
                    body: Factory.ObjectType(
                        Factory.PropertySignature(s_x, Factory.NumberType))),
                "interface ISomething<T> extends IBase {\n  x: number\n}\n");

            VerifyOutput(
                Factory.InterfaceDeclaration(Factory.Identifier("ISomething"), Factory.ObjectType()),
                "interface ISomething {\n}\n");
        }

        [TestMethod]
        public void Emit_enum_member()
        {
            VerifyOutput(Factory.EnumMember(s_x), "x");
            VerifyOutput(Factory.EnumMember(s_y, s_z), "y = z");
        }

        [TestMethod]
        public void Emit_empty_enum_declaration()
        {
            VerifyOutput(Factory.EnumDeclaration(s_T), "enum T {\n}\n");
        }

        [TestMethod]
        public void Emit_enum_declaration()
        {
            VerifyOutput(
                Factory.EnumDeclaration(
                    true,
                    Factory.Identifier("MyEnum"),
                    Factory.EnumMember(s_x),
                    Factory.EnumMember(s_y, Factory.Number(10))),
                "const enum MyEnum {\n  x,\n  y = 10\n}\n");
        }

        [TestMethod]
        public void Emit_namespace_declaration()
        {
            VerifyOutput(
                Factory.NamespaceDeclaration(Factory.QualifiedName("A.B.C")),
                "namespace A.B.C { }\n");

            VerifyOutput(
                Factory.NamespaceDeclaration(
                    Factory.QualifiedName("tab"),
                    Factory.ClassDeclaration(s_T)),
                @"namespace tab {
  class T {
  }
}
".Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void Emit_exported_variable_statement()
        {
            VerifyOutput(
                Factory.VariableStatement(Factory.SimpleVariableDeclaration(s_x)).ToExported(),
                "export var x;\n");
        }

        [TestMethod]
        public void Emit_exported_declarations()
        {
            VerifyOutput(
                Factory.ExportedDeclaration(
                    Factory.LexicalDeclaration(
                        true, Factory.SimpleLexicalBinding(s_x, Factory.BooleanType, Factory.True))),
                "export const x: boolean = true;\n");

            VerifyOutput(Factory.ClassDeclaration().ToExported(), "export class {\n}\n");
        }

        [TestMethod]
        public void Emit_import_alias_declaration()
        {
            VerifyOutput(
                Factory.ImportAliasDeclaration(s_x, Factory.QualifiedName("jQuery.IDeferred")),
                "import x = jQuery.IDeferred;\n");
        }

        [TestMethod]
        public void Emit_ambient_binding()
        {
            VerifyOutput(Factory.AmbientBinding(s_x), "x");
            VerifyOutput(Factory.AmbientBinding(s_x, Factory.BooleanType), "x: boolean");
        }
    }
}
