// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.TypeScriptAst.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public class TypeTranslatorTests
    {
        private static async Task AssertTypeTranslation(
            string csharpType,
            ITsType expectedType,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.OnlyDocumentTypes)
        {
            // Parse the C# code and get the root syntax node.
            string code = $@"
using System;
using System.Collections;
using System.Collections.Generic;

class Foo
{{
    private {csharpType} x;
}}
";

            using TempProject tempProject = await TempProject.CreateAsync(code);
            DocumentTranslationContextWithSymbolTables context =
                await tempProject.CreateContextWithSymbolTablesForFileAsync(discoveryKind: discoveryKind);

            // Find the type symbol for the class member.
            VariableDeclarationSyntax variableDeclaration =
                context.RootSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>().First();

            ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
            if (typeSymbol == null)
            {
                throw new InvalidOperationException($"Cannot find symbol for {variableDeclaration.Type}");
            }

            var translationContext = new TranslationContext(context);
            ITsType actualTranslation = TypeTranslator.TranslateTypeSymbol(
                translationContext,
                typeSymbol,
                getLocationFunc: variableDeclaration.Type.GetLocation);

            translationContext.Diagnostics.Should().BeEmpty();
            actualTranslation.Should().BeEquivalentTo(expectedType);
        }

        [Test]
        public async Task TypeTranslator_should_translate_basic_CSharp_types()
        {
            await AssertTypeTranslation("bool", Factory.BooleanType);
            await AssertTypeTranslation("string", Factory.StringType);
            await AssertTypeTranslation("char", Factory.NumberType);
            await AssertTypeTranslation("object", Factory.AnyType);
        }

        [Test]
        public async Task TypeTranslator_should_translate_number_CSharp_types()
        {
            await AssertTypeTranslation("byte", Factory.NumberType);
            await AssertTypeTranslation("sbyte", Factory.NumberType);
            await AssertTypeTranslation("short", Factory.NumberType);
            await AssertTypeTranslation("ushort", Factory.NumberType);
            await AssertTypeTranslation("int", Factory.NumberType);
            await AssertTypeTranslation("uint", Factory.NumberType);
            await AssertTypeTranslation("long", Factory.NumberType);
            await AssertTypeTranslation("ulong", Factory.NumberType);
            await AssertTypeTranslation("decimal", Factory.NumberType);
            await AssertTypeTranslation("float", Factory.NumberType);
            await AssertTypeTranslation("double", Factory.NumberType);
        }

        [Test]
        public async Task TypeTranslator_should_translate_native_JavaScript_objects()
        {
            await AssertTypeTranslation("JsDate", Factory.TypeReference(Factory.Identifier("Date")));
        }

        [Test]
        public async Task TypeTranslator_should_translate_array_types()
        {
            await AssertTypeTranslation("string[]", Factory.ArrayType(Factory.StringType));
        }

        [Test]
        public async Task TypeTranslator_should_translate_nested_array_types()
        {
            await AssertTypeTranslation("int[][]", Factory.ArrayType(Factory.ArrayType(Factory.NumberType)));
        }

        [Test]
        public async Task TypeTranslator_should_translate_function_types()
        {
            await AssertTypeTranslation("Func<int>", Factory.FunctionType(Factory.NumberType));
            await AssertTypeTranslation(
                "Func<string, byte, int>",
                Factory.FunctionType(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(Factory.Identifier("string"), Factory.StringType),
                        Factory.BoundRequiredParameter(Factory.Identifier("byte"), Factory.NumberType)),
                    Factory.NumberType));
        }

        [Test]
        public async Task TypeTranslator_should_TypeTranslator_should_translate_Action_types()
        {
            await AssertTypeTranslation(
                "Action<int, string>",
                Factory.FunctionType(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(Factory.Identifier("int32"), Factory.NumberType),
                        Factory.BoundRequiredParameter(Factory.Identifier("string"), Factory.StringType)),
                    Factory.VoidType));
        }

        [Test]
        public async Task TypeTranslator_should_translate_concrete_generic_types()
        {
            await AssertTypeTranslation(
                "Lazy<string>",
                Factory.TypeReference(Factory.QualifiedName("ss", "Lazy"), Factory.StringType),
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task TypeTranslator_should_translate_a_symbol_that_gets_translated_to_an_array_to_an_array()
        {
            await AssertTypeTranslation(
                "List<string>",
                Factory.ArrayType(Factory.StringType),
                SymbolDiscoveryKind.DocumentAndReferencedTypes);
        }

        [Test]
        public async Task TypeTranslator_should_handle_Nullable_T_types()
        {
            await AssertTypeTranslation("int?", Factory.UnionType(Factory.NumberType, Factory.NullType));
        }

        [Test]
        public async Task TypeTranslator_should_handle_non_generic_JsDictionary_correctly()
        {
            await AssertTypeTranslation(
                "JsDictionary",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: false,
                        returnType: Factory.AnyType)));
        }

        [Test]
        public async Task TypeTranslator_should_handle_JsDictionary_with_string_type_keys_correctly()
        {
            await AssertTypeTranslation(
                "JsDictionary<string, JsDate>",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: false,
                        returnType: Factory.TypeReference(Factory.Identifier("Date")))));
        }

        [Test]
        public async Task TypeTranslator_should_handle_JsDictionary_with_number_type_keys_correctly()
        {
            // jQueryApi.EffectEasing is defined as [NamedValues]
            await AssertTypeTranslation(
                "JsDictionary<char, double>",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: true,
                        returnType: Factory.NumberType)));
        }

        [Test]
        public async Task TypeTranslator_should_handle_JsDictionary_with_NamedValues_enum_type_keys_correctly()
        {
            // jQueryApi.EffectEasing is defined as [NamedValues]
            await AssertTypeTranslation(
                "JsDictionary<jQueryApi.EffectEasing, double>",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: false,
                        returnType: Factory.NumberType)));
        }

        [Test]
        public async Task TypeTranslator_should_handle_JsDictionary_with_NumericValues_enum_type_keys_correctly()
        {
            // System.DayOfWeek is implicitly defined as [NumericValues]
            await AssertTypeTranslation(
                "JsDictionary<DayOfWeek, string>",
                Factory.ObjectType(
                    Factory.IndexSignature(
                        Factory.Identifier("key"),
                        isParameterNumberType: true,
                        returnType: Factory.StringType)));
        }
    }
}
