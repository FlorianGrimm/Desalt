﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Emit;
    using Desalt.Core.Translation;
    using Desalt.Core.TypeScript.Ast;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DocumentationCommentTranslatorTests
    {
        private static void AssertTranslation(string csharpComment, params string[] expectedJsDocLines)
        {
            // parse the C# code and get the root syntax node
            string csharpCode = $@"
using System;
using System.Text;
using System.Collections.Generic;

class Foo
{{
    {csharpComment}
    public int Bar<T>(string p1, double p2) {{ }}

    public string Prop {{ get; }}
}}";

            var syntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(csharpCode);
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            // compile it and get a semantic model
            CSharpCompilation compilation = CSharpCompilation.Create("TestAssembly")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(StringBuilder).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(HashSet<int>).Assembly.Location));

            // find the type symbol for the class member
            var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

            //IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            var docNode = (DocumentationCommentTriviaSyntax)methodDeclaration.GetLeadingTrivia()
                .Single(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia))
                .GetStructure();

            // translate the documentation comment
            var result = DocumentationCommentTranslator.Translate(docNode);
            result.Diagnostics.Should().BeEmpty();

            ITsJsDocComment jsdocComment = result.Result;
            string actualJsDoc = jsdocComment.EmitAsString(EmitOptions.UnixSpaces);
            string expectedJsDoc = "/**\n" + string.Join("\n", expectedJsDocLines.Select(x => $" * {x}")) + "\n */\n";
            actualJsDoc.Should().Be(expectedJsDoc);
        }

        [TestMethod]
        public void Translate_should_convert_a_summary_section()
        {
            AssertTranslation("/// <summary>\n/// Test\n/// </summary>", "Test");
        }

        [TestMethod]
        public void Translate_should_convert_a_remarks_section()
        {
            AssertTranslation("///<remarks>Remarks</remarks>", "Remarks");
        }

        [TestMethod]
        public void Translate_should_convert_an_example_section()
        {
            AssertTranslation("///<example>Example</example>", "@example Example");
        }

        [TestMethod]
        public void Translate_should_convert_type_param_tags()
        {
            AssertTranslation(
                "///<typeparam name=\"T\">TypeParam1</typeparam><typeparam name=\"TResult\">TypeParam2</typeparam>",
                "@typeparam T - TypeParam1",
                "@typeparam TResult - TypeParam2");
        }

        [TestMethod]
        public void Translate_should_convert_param_tags()
        {
            AssertTranslation(
                "///<param name=\"p2\">This is p2</param><param name=\"p1\">This is p1</param>",
                "@param p2 - This is p2",
                "@param p1 - This is p1");
        }

        [TestMethod]
        public void Translate_should_convert_the_returns_tag()
        {
            AssertTranslation("///<returns>A value</returns>", "@returns A value");
        }

        [TestMethod]
        public void Translate_should_convert_exception_tags()
        {
            AssertTranslation(
                "///<exception cref=\"ArgumentNullException\">p1 is null</exception>\n" +
                "///<exception cref=\"ArgumentNullException\">p2 is null</exception>\n" +
                "///<exception cref=\"InvalidOperationException\">Something is wrong</exception>",
                "@throws {ArgumentNullException} p1 is null",
                "@throws {ArgumentNullException} p2 is null",
                "@throws {InvalidOperationException} Something is wrong");
        }

        [TestMethod]
        public void Translate_should_convert_see_langword_to_markdown_backticks()
        {
            AssertTranslation("///<summary><see langword=\"null\"/></summary>", "`null`");
        }

        [TestMethod]
        public void Translate_should_convert_c_elements_to_markdown_backticks()
        {
            AssertTranslation("///<summary><c>some code</c></summary>", "`some code`");
        }

        [TestMethod]
        public void Translate_should_convert_see_references()
        {
            AssertTranslation(@"///<summary><see cref=""Console""/></summary>", "{@link Console}");
            AssertTranslation(@"///<summary><see cref=""StringBuilder""/></summary>", "{@link StringBuilder}");
        }

        [TestMethod]
        public void Translate_should_convert_a_seealso_reference_to_a_JSDoc_see_reference()
        {
            AssertTranslation(@"///<summary><seealso cref=""Console""/></summary>", "{@link Console}");
            AssertTranslation(@"///<summary><seealso cref=""StringBuilder""/></summary>", "{@link StringBuilder}");
        }

        [TestMethod]
        public void Translate_should_write_sections_in_the_correct_order()
        {
            const string csharpComment = @"
/// <remarks>Remarks</remarks>
/// <summary>Summary</summary>
/// <exception cref=""Exception"">Error 1</exception>
/// <param name=""p2"">P2</param>
/// <returns>Returns</returns>
/// <example>Example</example>
/// <exception cref=""Exception"">Error 2</exception>
/// <param name=""p1"">P1</param>";

            string[] jsDocLines =
            {
                "Summary",
                "Remarks",
                "@param p2 - P2",
                "@param p1 - P1",
                "@returns Returns",
                "@throws {Exception} Error 1",
                "@throws {Exception} Error 2",
                "@example Example"
            };

            AssertTranslation(csharpComment, jsDocLines);
        }

        [TestMethod]
        public void Translate_should_preserve_the_cref_attribute()
        {
            AssertTranslation("///<summary><see cref=\"System.IDisposable\"/></summary>", "{@link System.IDisposable}");
        }

        [TestMethod]
        public void Translate_should_convert_crefs_of_methods_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"IDisposable.Dispose\"/></summary>",
                "{@link IDisposable.Dispose}");
        }

        [TestMethod]
        public void Translate_should_convert_crefs_of_properties_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"Environment.NewLine\"/></summary>",
                "{@link Environment.NewLine}");
        }

        [TestMethod]
        public void Translate_should_convert_crefs_of_events_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"Console.CancelKeyPress\"/></summary>",
                "{@link Console.CancelKeyPress}");
        }

        [TestMethod]
        public void Translate_should_convert_nested_types_without_shortening_the_containing_type()
        {
            AssertTranslation(
                "///<summary><see cref=\"Environment.SpecialFolder\"/></summary>",
                "{@link Environment.SpecialFolder}");
        }

        [TestMethod]
        public void Translate_should_recognize_text_within_a_see_or_seealso_tag()
        {
            AssertTranslation(
                "///<summary><see cref=\"Console\">the console</see></summary>",
                "[the console]{@link Console}");
            AssertTranslation(
                "///<summary><seealso cref=\"Console\">the console</seealso></summary>",
                "[the console]{@link Console}");
        }

        [TestMethod]
        public void Translate_should_recognize_see_with_href_as_empty_element()
        {
            AssertTranslation("///<summary><see href=\"http://something\"/></summary>", "{@link http://something}");
        }

        [TestMethod]
        public void Translate_should_recognize_see_with_href_as_element()
        {
            AssertTranslation(
                "///<summary><see href=\"http://something\">Text</see></summary>",
                "[Text]{@link http://something}");
        }

        [TestMethod]
        public void Translate_should_add_spaces_around_translated_link_tags()
        {
            AssertTranslation(
                "///<summary>This is a <see cref=\"Console\"/> tag</summary>",
                "This is a {@link Console} tag");
        }

        [TestMethod]
        public void Translate_should_preserve_multiple_blank_lines()
        {
            const string comment = @"/// <summary>
/// Line 1
///
/// Line 2
/// </summary>";

            AssertTranslation(comment, "Line 1", "", "Line 2");
        }
    }
}
