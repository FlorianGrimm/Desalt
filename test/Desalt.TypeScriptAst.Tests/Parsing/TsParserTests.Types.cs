// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParserTests.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Parsing
{
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Parsing;
    using FluentAssertions;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParserTests
    {
        private static void AssertParseType(string code, ITsType expected)
        {
            ITsType actual = TsParser.ParseType(code);
            actual.EmitAsString().Should().BeEquivalentTo(expected.EmitAsString());
        }

        //// ===========================================================================================================
        //// ParseType Tests
        //// ===========================================================================================================

        [Test]
        public void TsParser_ParseType_should_recognize_predefined_types()
        {
            AssertParseType("any", Factory.AnyType);
            AssertParseType("number", Factory.NumberType);
            AssertParseType("boolean", Factory.BooleanType);
            AssertParseType("string", Factory.StringType);
            AssertParseType("symbol", Factory.SymbolType);
            AssertParseType("void", Factory.VoidType);
        }

        [Test]
        public void TsParser_ParseType_should_parse_the_this_type()
        {
            AssertParseType("this", Factory.ThisType);
        }

        [Test]
        public void TsParser_ParseType_should_recognize_parenthesized_types()
        {
            AssertParseType("(number)", Factory.NumberType);
        }

        [Test]
        public void TsParser_ParseType_should_recognize_type_references()
        {
            AssertParseType(
                "Ns1.Ns2.Type<T1, T2>",
                Factory.TypeReference(Factory.QualifiedName("Ns1.Ns2.Type"), s_T1, s_T2));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_array_types()
        {
            AssertParseType(
                "Ns1.Ns2.Type<T1, T2>[]",
                Factory.ArrayType(Factory.TypeReference(Factory.QualifiedName("Ns1.Ns2.Type"), s_T1, s_T2)));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_tuple_types()
        {
            AssertParseType(
                "[boolean, Promise<number>]",
                Factory.TupleType(
                    Factory.BooleanType,
                    Factory.TypeReference(Factory.Identifier("Promise"), Factory.NumberType)));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_type_queries()
        {
            AssertParseType("typeof A.B.C", Factory.TypeQuery(Factory.QualifiedName("A.B.C")));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_object_types_with_construct_signatures()
        {
            AssertParseType(
                "{ new <T>(x: number, x1: 'str', y?: any, z = 10, s?: 'str'): Type; }",
                Factory.ObjectType(
                    Factory.ConstructSignature(
                        Factory.TypeParameters(Factory.TypeParameter(Factory.Identifier("T"))),
                        Factory.ParameterList(
                            new ITsRequiredParameter[]
                            {
                                Factory.BoundRequiredParameter(s_x, Factory.NumberType),
                                Factory.StringRequiredParameter(Factory.Identifier("x1"), Factory.String("str"))
                            },
                            new ITsOptionalParameter[]
                            {
                                Factory.BoundOptionalParameter(s_y, Factory.AnyType),
                                Factory.BoundOptionalParameter(s_z, initializer: Factory.Number(10)),
                                Factory.StringOptionalParameter(Factory.Identifier("s"), Factory.String("str"))
                            }),
                        Factory.TypeReference(Factory.Identifier("Type")))));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_object_types_with_index_signatures()
        {
            AssertParseType(
                "{ [key: string]: boolean; [key: number]: any }",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: false,
                        returnType: Factory.BooleanType),
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: true,
                        returnType: Factory.AnyType)));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_object_types_with_call_signatures()
        {
            AssertParseType(
                "{ <T extends X>(public x: boolean, y?): void, () }",
                Factory.ObjectType(
                    Factory.CallSignature(
                        Factory.TypeParameters(
                            Factory.TypeParameter(Factory.Identifier("T"), Factory.TypeReference(s_x))),
                        Factory.ParameterList(
                            new[]
                            {
                                Factory.BoundRequiredParameter(s_x, Factory.BooleanType, TsAccessibilityModifier.Public)
                            },
                            new[] { Factory.BoundOptionalParameter(s_y) }),
                        Factory.VoidType),
                    Factory.CallSignature()));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_object_types_with_property_signatures()
        {
            AssertParseType(
                "{ basic, opt?, typed: boolean, optTyped?: any, readonly getOnly }",
                Factory.ObjectType(
                    Factory.PropertySignature(Factory.Identifier("basic")),
                    Factory.PropertySignature(Factory.Identifier("opt"), isOptional: true),
                    Factory.PropertySignature(Factory.Identifier("typed"), Factory.BooleanType),
                    Factory.PropertySignature(Factory.Identifier("optTyped"), Factory.AnyType, isOptional: true),
                    Factory.PropertySignature(Factory.Identifier("getOnly"), isReadOnly: true)));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_object_types_with_method_signatures()
        {
            AssertParseType(
                "{ call<T extends X>(public x: boolean, y?): void, opt?() }",
                Factory.ObjectType(
                    Factory.MethodSignature(
                        Factory.Identifier("call"),
                        isOptional: false,
                        callSignature: Factory.CallSignature(
                            Factory.TypeParameters(
                                Factory.TypeParameter(Factory.Identifier("T"), Factory.TypeReference(s_x))),
                            Factory.ParameterList(
                                new[]
                                {
                                    Factory.BoundRequiredParameter(
                                        s_x,
                                        Factory.BooleanType,
                                        TsAccessibilityModifier.Public)
                                },
                                new[] { Factory.BoundOptionalParameter(s_y) }),
                            Factory.VoidType)),
                    Factory.MethodSignature(
                        Factory.Identifier("opt"),
                        isOptional: true,
                        callSignature: Factory.CallSignature())));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_a_constructor_type()
        {
            AssertParseType(
                "new<T extends X>(x: any, y?: 'str') => T",
                Factory.ConstructorType(
                    Factory.TypeParameters(Factory.TypeParameter(Factory.Identifier("T"), Factory.TypeReference(s_x))),
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, Factory.AnyType).ToSafeArray(),
                        Factory.StringOptionalParameter(s_y, Factory.String("str")).ToSafeArray()),
                    Factory.TypeReference(Factory.Identifier("T"))));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_a_function_type()
        {
            AssertParseType(
                "<T extends X>(x: any, y?: 'str') => T",
                Factory.FunctionType(
                    Factory.TypeParameters(Factory.TypeParameter(Factory.Identifier("T"), Factory.TypeReference(s_x))),
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, Factory.AnyType).ToSafeArray(),
                        Factory.StringOptionalParameter(s_y, Factory.String("str")).ToSafeArray()),
                    Factory.TypeReference(Factory.Identifier("T"))));

            AssertParseType("() => void", Factory.FunctionType(Factory.ParameterList(), Factory.VoidType));
        }

        [Test]
        public void TsParser_ParseType_should_recognize_a_union_and_intersection_type()
        {
            AssertParseType(
                "string | boolean & number",
                Factory.UnionType(
                    Factory.StringType,
                    Factory.IntersectionType(Factory.BooleanType, Factory.NumberType)));
        }
    }
}
