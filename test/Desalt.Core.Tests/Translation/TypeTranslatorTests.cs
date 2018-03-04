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
        private static void AssertTypeTranslation(Type csharpType, ITsType expectedType)
        {
            // parse the C# code and get the root syntax node
            string csharpCode = $"class Foo {{ private {csharpType} x; }}";
            var syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(csharpCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            // compile it and get a semantic model
            CSharpCompilation compilation = CSharpCompilation.Create("TestAssembly").AddSyntaxTrees(syntaxTree);
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

            // find the type symbol for the class member
            VariableDeclarationSyntax variableDeclaration =
                root.DescendantNodes().OfType<VariableDeclarationSyntax>().First();
            ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(variableDeclaration.Type).Type;

            TypeTranslator.TranslateSymbol(typeSymbol, new HashSet<string>()).Should().Be(expectedType);
        }

        [TestMethod]
        public void Basic_CSharp_types_should_be_translated_to_the_corresponding_basic_TypeScript_types()
        {
            AssertTypeTranslation(typeof(void), Factory.VoidType);
            AssertTypeTranslation(typeof(bool), Factory.BooleanType);
            AssertTypeTranslation(typeof(string), Factory.StringType);
        }

        [TestMethod]
        public void Number_CSharp_types_should_be_translated_to_the_Number_TypeScript_type()
        {
            AssertTypeTranslation(typeof(byte), Factory.NumberType);
            AssertTypeTranslation(typeof(sbyte), Factory.NumberType);
            AssertTypeTranslation(typeof(short), Factory.NumberType);
            AssertTypeTranslation(typeof(ushort), Factory.NumberType);
            AssertTypeTranslation(typeof(int), Factory.NumberType);
            AssertTypeTranslation(typeof(uint), Factory.NumberType);
            AssertTypeTranslation(typeof(long), Factory.NumberType);
            AssertTypeTranslation(typeof(ulong), Factory.NumberType);
            AssertTypeTranslation(typeof(decimal), Factory.NumberType);
            AssertTypeTranslation(typeof(float), Factory.NumberType);
            AssertTypeTranslation(typeof(double), Factory.NumberType);
        }
    }
}
