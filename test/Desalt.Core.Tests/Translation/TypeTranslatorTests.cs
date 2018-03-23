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
    using System.Linq;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    [TestClass]
    public class TypeTranslatorTests
    {
        private static void AssertTypeTranslation(string csharpType, ITsType expectedType)
        {
            // parse the C# code and get the root syntax node
            string csharpCode = $@"
using System;

class Foo
{{
    private {csharpType} x;
}}
";
            var syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(csharpCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            // compile it and get a semantic model
            CSharpCompilation compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddSaltarelleReferences();

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

            // find the type symbol for the class member
            VariableDeclarationSyntax variableDeclaration =
                root.DescendantNodes().OfType<VariableDeclarationSyntax>().FirstOrDefault();
            if (variableDeclaration == null)
            {
                throw new InvalidOperationException($"Cannot find variable declaration from this code: {csharpCode}");
            }

            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(variableDeclaration.Type).Type;
            if (typeSymbol == null)
            {
                throw new InvalidOperationException($"Cannot find symbol for {variableDeclaration.Type}");
            }

            TypeTranslator.TranslateSymbol(typeSymbol, new HashSet<string>()).Should().BeEquivalentTo(expectedType);
        }

        [TestMethod]
        public void Translate_basic_CSharp_types()
        {
            AssertTypeTranslation("void", Factory.VoidType);
            AssertTypeTranslation("bool", Factory.BooleanType);
            AssertTypeTranslation("string", Factory.StringType);
        }

        [TestMethod]
        public void Translate_number_CSharp_types()
        {
            AssertTypeTranslation("byte", Factory.NumberType);
            AssertTypeTranslation("sbyte", Factory.NumberType);
            AssertTypeTranslation("short", Factory.NumberType);
            AssertTypeTranslation("ushort", Factory.NumberType);
            AssertTypeTranslation("int", Factory.NumberType);
            AssertTypeTranslation("uint", Factory.NumberType);
            AssertTypeTranslation("long", Factory.NumberType);
            AssertTypeTranslation("ulong", Factory.NumberType);
            AssertTypeTranslation("decimal", Factory.NumberType);
            AssertTypeTranslation("float", Factory.NumberType);
            AssertTypeTranslation("double", Factory.NumberType);
        }

        [TestMethod]
        public void Translate_array_types()
        {
            AssertTypeTranslation("string[]", Factory.ArrayType(Factory.StringType));
        }

        [TestMethod]
        public void Translate_nested_array_types()
        {
            AssertTypeTranslation("int[][]", Factory.ArrayType(Factory.ArrayType(Factory.NumberType)));
        }

        [TestMethod]
        public void Translate_function_types()
        {
            AssertTypeTranslation("Func<int>", Factory.FunctionType(Factory.NumberType));
            AssertTypeTranslation(
                "Func<string, byte, int>",
                Factory.FunctionType(
                    Factory.ParameterList(
                        Factory.BoundRequiredParameter(Factory.Identifier("string"), Factory.StringType),
                        Factory.BoundRequiredParameter(Factory.Identifier("byte"), Factory.NumberType)),
                    Factory.NumberType));
        }

        [TestMethod]
        public void Translate_concrete_generic_types()
        {
            AssertTypeTranslation(
                "List<string>",
                Factory.TypeReference(Factory.Identifier("List"), Factory.StringType));
            AssertTypeTranslation(
                "IDictionary<string, double>",
                Factory.TypeReference(Factory.Identifier("IDictionary"), Factory.StringType, Factory.NumberType));
        }
    }
}
