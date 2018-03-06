// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TranslationVisitorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Emit;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TranslationVisitorTests
    {
        private static async Task AssertTranslation(string csharpCode, string expectedTypeScriptCode)
        {
            using (var tempProject = TempProject.Create("TestProject", new TempProjectFile("File", csharpCode)))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync("File");
                var visitor = new TranslationVisitor(context);
                IAstNode result = visitor.Visit(context.RootSyntax).Single();

                visitor.Diagnostics.Should().BeEmpty();

                // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
                string translated = result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
                translated.Should().Be(expectedTypeScriptCode);
            }
        }

        [TestClass]
        public class InterfaceDeclarationTests
        {
            [TestMethod]
            public async Task Bare_interface_declaration_without_accessibility_should_not_be_exported()
            {
                await AssertTranslation("interface ITest {}", "interface ITest {\n}\n");
            }

            [TestMethod]
            public async Task Public_interface_declaration_should_be_exported()
            {
                await AssertTranslation("public interface ITest {}", "export interface ITest {\n}\n");
            }

            [TestMethod]
            public async Task A_method_declaration_with_no_parameters_and_a_void_return_type_should_be_translated()
            {
                await AssertTranslation("interface ITest { void Do(); }", "interface ITest {\n  do(): void;\n}\n");
            }

            [TestMethod]
            public async Task A_method_declaration_with_simple_parameters_and_a_void_return_type_should_be_translated()
            {
                await AssertTranslation(
                    "interface ITest { void Do(string x, string y); }",
                    "interface ITest {\n  do(x: string, y: string): void;\n}\n");
            }
        }

        [TestClass]
        public class EnumDeclarationTests
        {
            [TestMethod]
            public async Task Bare_enum_declaration_without_accessibility_should_not_be_exported()
            {
                await AssertTranslation("enum LoggerLevel { All }", "enum LoggerLevel {\n  all,\n}\n");
            }

            [TestMethod]
            public async Task Public_enum_declaration_should_be_exported()
            {
                await AssertTranslation("public enum LoggerLevel { All }", "export enum LoggerLevel {\n  all,\n}\n");
            }
        }
    }
}
