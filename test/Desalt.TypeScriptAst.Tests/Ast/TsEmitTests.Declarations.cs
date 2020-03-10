// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Declarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using Xunit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [Fact]
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

        [Fact]
        public void Emit_destructuring_lexical_binding_with_no_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    initializer: s_z),
                "[x, y] = z");
        }

        [Fact]
        public void Emit_destructuring_lexical_binding_with_type_annotation()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y)),
                    Factory.ArrayType(Factory.NumberType),
                    s_z),
                "[x, y]: number[] = z");
        }

        [Fact]
        public void Emit_destructuring_lexical_binding_with_no_type_annotation_or_initializer()
        {
            VerifyOutput(
                Factory.DestructuringLexicalBinding(
                    Factory.ArrayBindingPattern(Factory.SingleNameBinding(s_x), Factory.SingleNameBinding(s_y))),
                "[x, y]");
        }

        [Fact]
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

        [Fact]
        public void Emit_anonymous_function_declaration_with_no_body()
        {
            VerifyOutput(
                Factory.FunctionDeclaration(
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)))),
                "function(x: number);\n");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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
                    isAbstract: true,
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType)),
                "static abstract myMethod(x: number): void;\n");

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

        [Fact]
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

        [Fact]
        public void Emit_get_and_set_accessor_member_declarations()
        {
            VerifyOutput(
                Factory.GetAccessorMemberDeclaration(
                    Factory.GetAccessor(Factory.Identifier("field"), Factory.NumberType),
                    TsAccessibilityModifier.Public,
                    isStatic: true,
                    isAbstract: true),
                "public static abstract get field(): number { }\n");

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
                    isStatic: true,
                    isAbstract: true),
                "public static abstract set field(value: number) { }\n");

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

        [Fact]
        public void Emit_index_member_declarations()
        {
            VerifyOutput(
                Factory.IndexMemberDeclaration(
                    Factory.IndexSignature(Factory.Identifier("key"), false, Factory.AnyType)),
                "[key: string]: any;\n");
        }

        [Fact]
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

        [Fact]
        public void Emit_empty_class_declaration()
        {
            VerifyOutput(Factory.ClassDeclaration(), "class {\n}\n");
        }

        [Fact]
        public void Emit_abstract_class_declaration()
        {
            VerifyOutput(Factory.ClassDeclaration(Factory.Identifier("C"), isAbstract: true), "abstract class C {\n}\n");
        }

        [Fact]
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
                    className: Factory.Identifier("AnimalCollection"),
                    typeParameters:
                    Factory.TypeParameters(
                        Factory.TypeParameter(s_T, Factory.TypeReference(Factory.Identifier("IAnimal")))),
                    heritage:
                    Factory.ClassHeritage(
                        Factory.TypeReference(Factory.Identifier("Collection"), s_TRef),
                        Factory.TypeReference(Factory.Identifier("ICollection"), s_TRef).ToSafeArray()),
                    isAbstract: false,
                    classBody: new ITsClassElement[]
                    {
                        Factory.VariableMemberDeclaration(
                            _items,
                            TsAccessibilityModifier.Private,
                            typeAnnotation: Factory.ArrayType(s_TRef)),
                        Factory.ConstructorDeclaration(
                            TsAccessibilityModifier.Public,
                            Factory.ParameterList(
                                requiredParameters: Factory.BoundRequiredParameter(item, s_TRef).ToSafeArray(),
                                restParameter: Factory.RestParameter(items, Factory.ArrayType(s_TRef))),
                            Factory.Assignment(
                                    thisItems,
                                    TsAssignmentOperator.SimpleAssign,
                                    Factory.Array(
                                        Factory.ArrayElement(item),
                                        Factory.ArrayElement(items, isSpreadElement: true)))
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
                                length,
                                value,
                                Factory.NumberType,
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
                                    item)
                                .ToStatement()
                                .ToSafeArray())
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

        [Fact]
        public void Emit_empty_interface_declaration()
        {
            VerifyOutput(Factory.InterfaceDeclaration(s_T, Factory.ObjectType()), "interface T {\n}\n");
        }

        [Fact]
        public void Emit_interface_declaration()
        {
            VerifyOutput(
                Factory.InterfaceDeclaration(
                    Factory.Identifier("ISomething"),
                    typeParameters: Factory.TypeParameters(Factory.TypeParameter(s_T)),
                    extendsClause: Factory.TypeReference(Factory.Identifier("IBase")).ToSafeArray(),
                    body: Factory.ObjectType(
                        Factory.PropertySignature(s_x, Factory.NumberType))),
                "interface ISomething<T> extends IBase {\n  x: number;\n}\n");

            VerifyOutput(
                Factory.InterfaceDeclaration(Factory.Identifier("ISomething"), Factory.ObjectType()),
                "interface ISomething {\n}\n");
        }

        [Fact]
        public void Emit_enum_member()
        {
            VerifyOutput(Factory.EnumMember(s_x), "x");
            VerifyOutput(Factory.EnumMember(s_y, s_z), "y = z");
        }

        [Fact]
        public void Emit_empty_enum_declaration()
        {
            VerifyOutput(Factory.EnumDeclaration(s_T), "enum T {\n}\n");
        }

        [Fact]
        public void Emit_enum_declaration()
        {
            VerifyOutput(
                Factory.EnumDeclaration(
                    true,
                    Factory.Identifier("MyEnum"),
                    Factory.EnumMember(s_x),
                    Factory.EnumMember(s_y, Factory.Number(10))),
                "const enum MyEnum {\n  x,\n  y = 10,\n}\n");
        }

        [Fact]
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

        [Fact]
        public void Emit_exported_variable_statement()
        {
            VerifyOutput(
                Factory.VariableStatement(Factory.SimpleVariableDeclaration(s_x)).ToExported(),
                "export var x;\n");
        }

        [Fact]
        public void Emit_exported_declarations()
        {
            VerifyOutput(
                Factory.ExportedDeclaration(
                    Factory.LexicalDeclaration(
                        true, Factory.SimpleLexicalBinding(s_x, Factory.BooleanType, Factory.True))),
                "export const x: boolean = true;\n");

            VerifyOutput(Factory.ClassDeclaration().ToExported(), "export class {\n}\n");
        }

        [Fact]
        public void Emit_import_alias_declaration()
        {
            VerifyOutput(
                Factory.ImportAliasDeclaration(s_x, Factory.QualifiedName("jQuery.IDeferred")),
                "import x = jQuery.IDeferred;\n");
        }

        [Fact]
        public void Emit_ambient_binding()
        {
            VerifyOutput(Factory.AmbientBinding(s_x), "x");
            VerifyOutput(Factory.AmbientBinding(s_x, Factory.BooleanType), "x: boolean");
        }

        [Fact]
        public void Emit_ambient_variable_declaration()
        {
            VerifyOutput(
                Factory.AmbientVariableDeclaration(VariableDeclarationKind.Const, Factory.AmbientBinding(s_x)),
                "const x;\n");

            VerifyOutput(
                Factory.AmbientVariableDeclaration(
                    VariableDeclarationKind.Let,
                    Factory.AmbientBinding(s_x, Factory.NumberType),
                    Factory.AmbientBinding(s_y)),
                "let x: number, y;\n");
        }

        [Fact]
        public void Emit_ambient_function_declaration()
        {
            VerifyOutput(
                Factory.AmbientFunctionDeclaration(s_x, Factory.CallSignature()),
                "function x();\n");
        }

        [Fact]
        public void Emit_ambient_constructor_declaration()
        {
            VerifyOutput(
                Factory.AmbientConstructorDeclaration(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, modifier: TsAccessibilityModifier.Protected),
                        Factory.StringRequiredParameter(s_y, Factory.String("str")))),
                "constructor(protected x, y: 'str');\n");
        }

        [Fact]
        public void Emit_ambient_variable_member_declarations()
        {
            VerifyOutput(
                Factory.AmbientVariableMemberDeclaration(
                    s_x,
                    TsAccessibilityModifier.Public,
                    isStatic: true,
                    typeAnnotation: Factory.NumberType),
                "public static x: number;\n");

            VerifyOutput(
                Factory.AmbientVariableMemberDeclaration(
                    s_x,
                    isStatic: true,
                    typeAnnotation: Factory.NumberType),
                "static x: number;\n");

            VerifyOutput(Factory.AmbientVariableMemberDeclaration(s_x), "x;\n");
        }

        [Fact]
        public void Emit_ambient_function_member_declarations()
        {
            VerifyOutput(
                Factory.AmbientFunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType),
                    TsAccessibilityModifier.Private,
                    isStatic: true),
                "private static myMethod(x: number): void;\n");

            VerifyOutput(
                Factory.AmbientFunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    isStatic: true,
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType)),
                "static myMethod(x: number): void;\n");

            VerifyOutput(
                Factory.AmbientFunctionMemberDeclaration(
                    Factory.Identifier("myMethod"),
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_x, Factory.NumberType)),
                        Factory.VoidType)),
                "myMethod(x: number): void;\n");
        }

        [Fact]
        public void Emit_ambient_class_declaration_with_all_of_the_possible_elements()
        {
            // ReSharper disable once InconsistentNaming
            ITsIdentifier _items = Factory.Identifier("_items");
            ITsIdentifier item = Factory.Identifier("item");
            ITsIdentifier items = Factory.Identifier("items");

            VerifyOutput(
                Factory.AmbientClassDeclaration(
                    Factory.Identifier("AnimalCollection"),
                    Factory.TypeParameters(Factory.TypeParameter(s_T, Factory.TypeReference(Factory.Identifier("IAnimal")))),
                    Factory.ClassHeritage(
                        Factory.TypeReference(Factory.Identifier("Collection"), s_TRef),
                        Factory.TypeReference(Factory.Identifier("ICollection"), s_TRef).ToSafeArray()),
                    new ITsAmbientClassBodyElement[]
                    {
                        Factory.AmbientVariableMemberDeclaration(
                            _items, TsAccessibilityModifier.Private,typeAnnotation: Factory.ArrayType(s_TRef)),
                        Factory.AmbientConstructorDeclaration(
                            Factory.ParameterList(
                                requiredParameters: Factory.BoundRequiredParameter(item, s_TRef).ToSafeArray(),
                                restParameter: Factory.RestParameter(items, Factory.ArrayType(s_TRef)))),
                        Factory.AmbientIndexMemberDeclaration(
                            Factory.IndexSignature(
                                Factory.Identifier("index"),
                                isParameterNumberType: true,
                                returnType: s_TRef)),
                        Factory.AmbientFunctionMemberDeclaration(
                            Factory.Identifier("add"),
                            Factory.CallSignature(
                                Factory.ParameterList(Factory.BoundRequiredParameter(item, s_TRef)),
                                Factory.VoidType),
                            TsAccessibilityModifier.Protected)
                    }),
                @"class AnimalCollection<T extends IAnimal> extends Collection<T> implements ICollection<T> {
  private _items: T[];
  constructor(item: T, ... items: T[]);
  [index: number]: T;
  protected add(item: T): void;
}
".Replace("\r\n", "\n"));
        }

        [Fact]
        public void Emit_ambient_namespace_elements()
        {
            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.AmbientVariableDeclaration(
                        VariableDeclarationKind.Var,
                        Factory.AmbientBinding(s_x, Factory.AnyType)),
                    hasExportKeyword: true),
                "export var x: any;\n");

            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.AmbientFunctionDeclaration(s_x, Factory.CallSignature()),
                    hasExportKeyword: true),
                "export function x();\n");

            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.AmbientClassDeclaration(s_x),
                    hasExportKeyword: true),
                "export class x {\n}\n");

            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.InterfaceDeclaration(s_x, Factory.ObjectType()), hasExportKeyword: true),
                "export interface x {\n}\n");

            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.AmbientEnumDeclaration(s_x), hasExportKeyword: true),
                "export enum x {\n}\n");

            VerifyOutput(
                Factory.AmbientNamespaceElement(
                    Factory.AmbientNamespaceDeclaration(Factory.QualifiedName("MyNs")), hasExportKeyword: true),
                "export namespace MyNs { }\n");
        }

        [Fact]
        public void Emit_ambient_namespace_declaration()
        {
            VerifyOutput(
                Factory.AmbientNamespaceDeclaration(
                    Factory.QualifiedName("A.B.C"),
                    Factory.AmbientNamespaceElement(Factory.AmbientEnumDeclaration(s_x))),
                "namespace A.B.C {\n  enum x {\n}\n}\n");
        }

        [Fact]
        public void Emit_import_specifier()
        {
            VerifyOutput(Factory.ImportSpecifier(s_x), "x");
            VerifyOutput(Factory.ImportSpecifier(s_x, s_y), "x as y");
        }

        [Fact]
        public void Emit_from_clause()
        {
            VerifyOutput(Factory.FromClause(Factory.String("myModule")), "from 'myModule'");
        }

        [Fact]
        public void Emit_import_clauses()
        {
            // Default bindings
            VerifyOutput(Factory.ImportClause(s_x), "x");
            VerifyOutput(Factory.ImportClause(s_x, s_y), "x, * as y");
            VerifyOutput(Factory.ImportClause(s_x, Factory.ImportSpecifier(s_y)), "x, { y }");
            VerifyOutput(
                Factory.ImportClause(s_x, Factory.ImportSpecifier(s_y), Factory.ImportSpecifier(s_z)),
                "x, { y, z }");

            VerifyOutput(Factory.ImportClauseNamespaceBinding(s_x), "* as x");

            VerifyOutput(
                Factory.ImportClause(Factory.ImportSpecifier(s_x, s_y), Factory.ImportSpecifier(s_z)),
                "{ x as y, z }");
        }

        [Fact]
        public void Emit_import_declarations()
        {
            VerifyOutput(Factory.ImportDeclaration(Factory.String("myModule")), "import 'myModule';\n");
            VerifyOutput(
                Factory.ImportDeclaration(
                    Factory.ImportClause(s_x, Factory.ImportSpecifier(s_y, s_z), Factory.ImportSpecifier(s_p)),
                    Factory.FromClause(Factory.String("./Module"))),
                "import x, { y as z, p } from './Module';\n");
        }

        [Fact]
        public void Emit_import_require_declaration()
        {
            VerifyOutput(
                Factory.ImportRequireDeclaration(s_x, Factory.String("jQuery")),
                "import x = require('jQuery');\n");
        }

        [Fact]
        public void Emit_export_implementation_element()
        {
            VerifyOutput(
                Factory.ExportImplementationElement(
                    Factory.VariableStatement(Factory.SimpleVariableDeclaration(s_x))),
                "export var x;\n");
        }
    }
}
