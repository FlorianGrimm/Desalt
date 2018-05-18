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
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TypeScriptAst.Emit;
    using TypeScriptAst.TypeScript.Ast;

    [TestClass]
    public partial class TranslationVisitorTests
    {
        private static Task AssertTranslationWithClassCAndMethod(
            string codeSnippet,
            string expectedTypeScriptCode,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.OnlyDocumentTypes)
        {
            return AssertTranslation(
                $@"
class C
{{
    void Method()
    {{
        {codeSnippet}
    }}
}}",
                $@"
class C {{
  private method(): void {{
    {expectedTypeScriptCode.Replace("\r\n", "\n").Trim()}
  }}
}}
",
                discoveryKind);
        }

        private static async Task AssertTranslation(
            string codeSnippet,
            string expectedTypeScriptCode,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.OnlyDocumentTypes)
        {
            string code = $@"
using System;
using System.Collections.Generic;
using System.Html;

{codeSnippet}
";

            // get rid of \r\n sequences in the expected output
            expectedTypeScriptCode = expectedTypeScriptCode.Replace("\r\n", "\n").TrimStart();

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync(discoveryKind: discoveryKind);

                var throwingDiagnosticList = DiagnosticList.Create(tempProject.Options);
                throwingDiagnosticList.ThrowOnErrors = true;

                var visitor = new TranslationVisitor(context, diagnostics: throwingDiagnosticList);
                IAstNode result = visitor.Visit(context.RootSyntax).Single();

                visitor.Diagnostics.Should().BeEmpty();

                // rather than try to implement equality tests for all IAstNodes, just emit both and compare the strings
                string translated = result.EmitAsString(emitOptions: EmitOptions.UnixSpaces);
                translated.Should().Be(expectedTypeScriptCode);
            }
        }

        //// ===========================================================================================================
        //// Interface Declaration Tests
        //// ===========================================================================================================

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

        //// ===========================================================================================================
        //// Enum Declaration Tests
        //// ===========================================================================================================

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

        [TestMethod]
        public async Task Enum_declarations_with_literal_values_should_be_translated()
        {
            await AssertTranslation(
                "enum LoggerLevel { All = 123 }",
                "enum LoggerLevel {\n  all = 123,\n}\n");
        }

        [TestMethod]
        public async Task Enum_declarations_with_hex_values_should_be_translated_as_hex()
        {
            await AssertTranslation(
                "enum LoggerLevel { Hex = 0x100 }",
                "enum LoggerLevel {\n  hex = 0x100,\n}\n");
        }

        //// ===========================================================================================================
        //// Class Declaration Tests
        //// ===========================================================================================================

        [TestMethod]
        public async Task Bare_class_declaration_without_accessibility_should_not_be_exported()
        {
            await AssertTranslation("class C { }", "class C {\n}\n");
        }

        [TestMethod]
        public async Task Public_class_declaration_should_be_exported()
        {
            await AssertTranslation("public class C { }", "export class C {\n}\n");
        }
    }
}
