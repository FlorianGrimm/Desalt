// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Types.cs" company="Justin Rockwood">
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
        public void Emit_type_parameter_with_no_constraint()
        {
            VerifyOutput(Factory.TypeParameter(s_T), "T");
        }

        [TestMethod]
        public void Emit_type_parameter_with_constraint()
        {
            VerifyOutput(Factory.TypeParameter(s_T, s_MyTypeRef), "T extends MyType");
        }

        [TestMethod]
        public void Emit_parenthesized_type()
        {
            VerifyOutput(Factory.SymbolType.WithParentheses(), "(symbol)");
        }

        [TestMethod]
        public void Emit_simple_type_reference_with_no_type_arguments()
        {
            VerifyOutput(s_TRef, "T");
        }

        [TestMethod]
        public void Emit_qualified_name_type_reference_with_no_type_arguments()
        {
            VerifyOutput(Factory.TypeReference(Factory.QualifiedName("Ns.Class")), "Ns.Class");
        }

        [TestMethod]
        public void Emit_type_reference_with_type_arguments()
        {
            VerifyOutput(
                Factory.TypeReference(
                    Factory.Identifier("Sub"),
                    Factory.TypeReference(
                        Factory.Identifier("T1"), Factory.TypeReference(Factory.Identifier("T2")))),
                "Sub<T1<T2>>");
        }

        [TestMethod]
        public void Emit_object_property_signatures()
        {
            VerifyOutput(Factory.PropertySignature(s_x), "x");
            VerifyOutput(Factory.PropertySignature(s_x, isOptional: true), "x?");
            VerifyOutput(Factory.PropertySignature(s_x, isOptional: false, propertyType: Factory.StringType), "x: string");
            VerifyOutput(Factory.PropertySignature(s_x, isOptional: true, propertyType: Factory.StringType), "x?: string");
        }

        [TestMethod]
        public void Emit_full_call_signature()
        {
            VerifyOutput(
                Factory.CallSignature(
                    Factory.TypeParameter(s_T, s_MyTypeRef).ToSafeArray(),
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef, TsAccessibilityModifier.Private)),
                    Factory.AnyType),
                "<T extends MyType>(private x: T): any");
        }

        [TestMethod]
        public void Emit_call_signature_with_no_type_parameters()
        {
            VerifyOutput(
                Factory.CallSignature(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef, TsAccessibilityModifier.Protected)),
                    Factory.AnyType),
                "(protected x: T): any");
        }

        [TestMethod]
        public void Emit_call_signature_with_no_type_parameters_or_return_type()
        {
            VerifyOutput(
                Factory.CallSignature(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef))),
                "(x: T)");
        }

        [TestMethod]
        public void Emit_full_ctor_signature()
        {
            VerifyOutput(
                Factory.ConstructSignature(
                    Factory.TypeParameter(s_T, s_MyTypeRef).ToSafeArray(),
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef, TsAccessibilityModifier.Private)),
                    Factory.AnyType),
                "new <T extends MyType>(private x: T): any");
        }

        [TestMethod]
        public void Emit_ctor_signature_with_no_type_parameters()
        {
            VerifyOutput(
                Factory.ConstructSignature(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef, TsAccessibilityModifier.Public)),
                    Factory.AnyType),
                "new (public x: T): any");
        }

        [TestMethod]
        public void Emit_ctor_signature_with_no_type_parameters_or_return_type()
        {
            VerifyOutput(
                Factory.ConstructSignature(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(s_x, s_TRef))),
                "new (x: T)");
        }

        [TestMethod]
        public void Emit_index_signatures()
        {
            VerifyOutput(
                Factory.IndexSignature(Factory.Identifier("key"), isParameterNumberType: false, returnType: s_MyTypeRef),
                "[key: string]: MyType");

            VerifyOutput(
                Factory.IndexSignature(Factory.Identifier("key"), isParameterNumberType: true, returnType: s_MyTypeRef),
                "[key: number]: MyType");
        }

        [TestMethod]
        public void Emit_method_signature()
        {
            VerifyOutput(
                Factory.MethodSignature(s_x, isOptional: false, callSignature: Factory.CallSignature()),
                "x()");

            VerifyOutput(
                Factory.MethodSignature(
                    s_x,
                    isOptional: true,
                    callSignature: Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_y)))),
                "x?(y)");
        }

        [TestMethod]
        public void Emit_object_type()
        {
            VerifyOutput(
                Factory.ObjectType(
                    Factory.PropertySignature(s_x, propertyType: Factory.StringType),
                    Factory.CallSignature(
                        Factory.ParameterList(Factory.BoundRequiredParameter(s_z), Factory.BoundRequiredParameter(s_p)),
                        Factory.BooleanType),
                    Factory.ConstructSignature(
                        Factory.TypeParameter(s_T, s_MyTypeRef).ToSafeArray(),
                        Factory.ParameterList(Factory.BoundRequiredParameter(Factory.Identifier("arg"), s_TRef))),
                    Factory.IndexSignature(Factory.Identifier("k"), false, Factory.AnyType),
                    Factory.MethodSignature(s_z, true, Factory.CallSignature(Factory.ParameterList(), Factory.VoidType))),
                "{ x: string, (z, p): boolean, new <T extends MyType>(arg: T), [k: string]: any, z?(): void }");
        }

        [TestMethod]
        public void Emit_array_type()
        {
            VerifyOutput(Factory.ArrayType(Factory.StringType), "string[]");
        }

        [TestMethod]
        public void Emit_tuple_type()
        {
            VerifyOutput(
                Factory.TupleType(Factory.BooleanType, Factory.StringType),
                "[boolean, string]");
        }

        [TestMethod]
        public void Emit_union_types()
        {
            VerifyOutput(
                Factory.UnionType(s_TRef, Factory.StringType, Factory.ArrayType(Factory.NumberType)),
                "T | string | number[]");
        }

        [TestMethod]
        public void Emit_intersection_types()
        {
            VerifyOutput(
                Factory.IntersectionType(s_TRef, Factory.StringType, Factory.ArrayType(Factory.NumberType)),
                "T & string & number[]");
        }

        [TestMethod]
        public void Emit_function_type()
        {
            VerifyOutput(
                Factory.FunctionType(
                    Factory.TypeParameter(s_T).ToSafeArray(),
                    Factory.ParameterList(Factory.BoundRequiredParameter(s_x), Factory.BoundRequiredParameter(s_y)),
                    Factory.StringType),
                "<T>(x, y) => string");

            VerifyOutput(
                Factory.FunctionType(
                    Factory.ParameterList(Factory.BoundRequiredParameter(s_x), Factory.BoundRequiredParameter(s_y)),
                    Factory.StringType),
                "(x, y) => string");
        }

        [TestMethod]
        public void Emit_ctor_type()
        {
            VerifyOutput(
                Factory.ConstructorType(
                    Factory.TypeParameter(s_T).ToSafeArray(),
                    Factory.ParameterList(Factory.BoundRequiredParameter(s_x), Factory.BoundRequiredParameter(s_y)),
                    Factory.StringType),
                "new <T>(x, y) => string");

            VerifyOutput(
                Factory.ConstructorType(
                    Factory.ParameterList(Factory.BoundRequiredParameter(s_x), Factory.BoundRequiredParameter(s_y)),
                    Factory.StringType),
                "new (x, y) => string");
        }
    }
}
