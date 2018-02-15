// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Linq;
    using Desalt.Core.Emit;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TranslationVisitorTests
    {
        private static (CSharpSyntaxTree syntaxTree, TranslationVisitor visitor) CreateVisitorFromCode(string code)
        {
            var syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(code);
            CSharpCompilation compilation = CSharpCompilation.Create("TestAssembly").AddSyntaxTrees(syntaxTree);
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            return (syntaxTree, new TranslationVisitor(semanticModel));
        }

        private static void AssertTranslation(string csharpCode, string expectedTypeScriptCode)
        {
            (CSharpSyntaxTree syntaxTree, TranslationVisitor visitor) syntaxAndVisitor =
                CreateVisitorFromCode(csharpCode);
            CompilationUnitSyntax compilationUnit = syntaxAndVisitor.syntaxTree.GetCompilationUnitRoot();
            IAstNode result = syntaxAndVisitor.visitor.Visit(compilationUnit).Single();

            // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
            result.EmitAsString(emitOptions: EmitOptions.UnixSpaces).Should().Be(expectedTypeScriptCode);
        }

        [TestClass]
        public class VisitInterfaceDeclarationTests
        {
            [TestMethod]
            public void Bare_interface_declaration_without_accessibility_should_not_be_exported()
            {
                AssertTranslation("interface ITest {}", "interface ITest {\n}\n");
            }

            [TestMethod]
            public void Public_interface_declaration_should_be_exported()
            {
                AssertTranslation("public interface ITest {}", "export interface ITest {\n}\n");
            }
        }
    }
}
