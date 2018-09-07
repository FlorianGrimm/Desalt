// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CompilerUtilities.Extensions;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    [TestClass]
    public class TypeTranslatorTests
    {
        private static async Task AssertTypeTranslation(
            string csharpType,
            ITsType expectedType,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.OnlyDocumentTypes)
        {
            // parse the C# code and get the root syntax node
            string code = $@"
using System;
using System.Collections.Generic;

class Foo
{{
    private {csharpType} x;
}}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                // find the type symbol for the class member
                VariableDeclarationSyntax variableDeclaration =
                    context.RootSyntax.DescendantNodes().OfType<VariableDeclarationSyntax>().First();

                ITypeSymbol typeSymbol = context.SemanticModel.GetTypeInfo(variableDeclaration.Type).Type;
                if (typeSymbol == null)
                {
                    throw new InvalidOperationException($"Cannot find symbol for {variableDeclaration.Type}");
                }

                // create the script name symbol table
                ImmutableArray<ITypeSymbol> directlyReferencedExternalTypes =
                    SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(contexts, discoveryKind);

                var scriptNameTable = ScriptNameSymbolTable.Create(
                    contexts,
                    directlyReferencedExternalTypes,
                    SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                        directlyReferencedExternalTypes,
                        context.SemanticModel.Compilation,
                        discoveryKind: discoveryKind));

                var translator = new TypeTranslator(scriptNameTable);
                var diagnostics = new List<Diagnostic>();

                translator.TranslateSymbol(
                        typeSymbol,
                        typesToImport: null,
                        diagnostics: diagnostics,
                        getLocationFunc: variableDeclaration.Type.GetLocation)
                    .Should()
                    .BeEquivalentTo(expectedType);

                diagnostics.Should().BeEmpty();
            }
        }

        [TestMethod]
        public async Task TypeTranslator_should_translate_basic_CSharp_types()
        {
            await AssertTypeTranslation("bool", Factory.BooleanType);
            await AssertTypeTranslation("string", Factory.StringType);
            await AssertTypeTranslation("char", Factory.NumberType);
        }

        [TestMethod]
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

        [TestMethod]
        public async Task TypeTranslator_should_translate_array_types()
        {
            await AssertTypeTranslation("string[]", Factory.ArrayType(Factory.StringType));
        }

        [TestMethod]
        public async Task TypeTranslator_should_translate_nested_array_types()
        {
            await AssertTypeTranslation("int[][]", Factory.ArrayType(Factory.ArrayType(Factory.NumberType)));
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public async Task TypeTranslator_should_translate_concrete_generic_types()
        {
            await AssertTypeTranslation(
                "Lazy<string>",
                Factory.TypeReference(Factory.Identifier("Lazy"), Factory.StringType),
                SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }

        [TestMethod]
        public async Task TypeTranslator_should_translate_a_symbol_that_gets_translated_to_an_array_to_an_array()
        {
            await AssertTypeTranslation(
                "List<string>",
                Factory.ArrayType(Factory.StringType),
                SymbolTableDiscoveryKind.DocumentAndReferencedTypes);
        }
    }
}
