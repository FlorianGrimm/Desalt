// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationCommentTranslatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System.Linq;
    using Desalt.Core.Translation;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Emit;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NUnit.Framework;

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
                    MetadataReference.CreateFromFile(typeof(System.Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Text.StringBuilder).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Collections.Generic.HashSet<int>).Assembly.Location));

            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);

            // find the type symbol for the class member
            MethodDeclarationSyntax methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();
            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

            // get the documentation comment
            DocumentationComment? docComment = methodSymbol.GetDocumentationComment();
            docComment.Should().NotBeNull();

            // translate the documentation comment
            IExtendedResult<ITsJsDocComment> result = DocumentationCommentTranslator.Translate(docComment!);
            result.Diagnostics.Should().BeEmpty();

            ITsJsDocComment jsdocComment = result.Result;
            string actualJsDoc = jsdocComment.EmitAsString(EmitOptions.UnixSpaces);
            string expectedJsDoc = "/**\n" + string.Join("\n", expectedJsDocLines.Select(x => $" * {x}")) + "\n */\n";
            actualJsDoc.Should().Be(expectedJsDoc);
        }

        [Test]
        public void Translate_should_convert_a_summary_section()
        {
            AssertTranslation("///<summary>Test</summary>", "Test");
        }

        [Test]
        public void Translate_should_convert_a_summary_section_with_newlines()
        {
            AssertTranslation("///<summary>\n/// Test\n/// </summary>", "Test");
        }

        [Test]
        public void Translate_should_convert_a_remarks_section()
        {
            AssertTranslation("///<remarks>Remarks</remarks>", "Remarks");
        }

        [Test]
        public void Translate_should_convert_a_remarks_section_with_newlines()
        {
            AssertTranslation("///<remarks>\n/// Remarks\n/// </remarks>", "Remarks");
        }

        [Test]
        public void Translate_should_convert_an_example_section()
        {
            AssertTranslation("///<example>Example</example>", "@example Example");
        }

        [Test]
        public void Translate_should_convert_an_example_section_with_newlines()
        {
            AssertTranslation("///<example>\n/// Example\n/// </example>", "@example Example");
        }

        [Test]
        public void Translate_should_convert_type_param_tags()
        {
            AssertTranslation(
                "///<typeparam name=\"T\">TypeParam1</typeparam><typeparam name=\"TResult\">TypeParam2</typeparam>",
                "typeparam T TypeParam1",
                "typeparam TResult TypeParam2");
        }

        [Test]
        public void Translate_should_convert_param_tags()
        {
            AssertTranslation(
                "///<param name=\"p2\">This is p2</param><param name=\"p1\">This is p1</param>",
                "@param p2 This is p2",
                "@param p1 This is p1");
        }

        [Test]
        public void Translate_should_convert_the_returns_tag()
        {
            AssertTranslation("///<returns>A value</returns>", "@returns A value");
        }

        [Test]
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

        [Test]
        public void Translate_should_convert_see_langword_to_markdown_back_ticks()
        {
            AssertTranslation("///<summary><see langword=\"null\"/></summary>", "`null`");
        }

        [Test]
        public void Translate_should_convert_c_elements_to_markdown_back_ticks()
        {
            AssertTranslation("///<summary><c>some code</c></summary>", "`some code`");
        }

        [Test]
        public void Translate_should_convert_see_references()
        {
            AssertTranslation(@"///<summary><see cref=""Console""/></summary>", "{@link Console}");
            AssertTranslation(@"///<summary><see cref=""StringBuilder""/></summary>", "{@link StringBuilder}");
        }

        [Test]
        public void Translate_should_convert_a_seealso_reference_to_a_JSDoc_see_reference()
        {
            AssertTranslation(@"///<summary><seealso cref=""Console""/></summary>", "{@link Console}");
            AssertTranslation(@"///<summary><seealso cref=""StringBuilder""/></summary>", "{@link StringBuilder}");
        }

        [Test]
        public void Translate_should_convert_alink_tags()
        {
            AssertTranslation("///<summary><a href=\"http://somewhere\"/></summary>", "{@link http://somewhere}");
            AssertTranslation("///<summary><a href=\"http://somewhere\">Text</a></summary>", "[Text]{@link http://somewhere}");
        }

        [Test]
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
                "@param p2 P2",
                "@param p1 P1",
                "@returns Returns",
                "@throws {Exception} Error 1",
                "@throws {Exception} Error 2",
                "@example Example"
            };

            AssertTranslation(csharpComment, jsDocLines);
        }

        [Test]
        public void Translate_should_convert_crefs_of_types_to_a_shortened_name()
        {
            AssertTranslation("///<summary><see cref=\"System.IDisposable\"/></summary>", "{@link IDisposable}");
        }

        [Test]
        public void Translate_should_convert_crefs_of_methods_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"IDisposable.Dispose\"/></summary>",
                "{@link IDisposable.Dispose}");
        }

        [Test]
        public void Translate_should_convert_crefs_of_methods_with_a_full_signature_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"Console.WriteLine(char[], int, int)\"/></summary>",
                "{@link Console.WriteLine}");
        }

        [Test]
        public void Translate_should_convert_crefs_of_properties_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"Environment.NewLine\"/></summary>",
                "{@link Environment.NewLine}");
        }

        [Test]
        public void Translate_should_convert_crefs_of_events_to_a_shortened_name()
        {
            AssertTranslation(
                "///<summary><see cref=\"Console.CancelKeyPress\"/></summary>",
                "{@link Console.CancelKeyPress}");
        }

        [Ignore("TODO")]
        [Test]
        public void Translate_should_convert_nested_types_without_shortening_the_containing_type()
        {
            AssertTranslation(
                "///<summary><see cref=\"Environment.SpecialFolder\"/></summary>",
                "{@link Environment.SpecialFolder}");
        }

        [Ignore("TODO")]
        [Test]
        public void
            Translate_should_not_show_the_type_name_if_referencing_a_member_within_the_same_type_for_cref_references()
        {
            AssertTranslation("///<summary><see cref=\"Foo.Prop\"/></summary>", "{@link Prop}");
        }

        [Test]
        public void Translate_should_recognize_text_within_a_see_or_seealso_tag()
        {
            AssertTranslation(
                "///<summary><see cref=\"Console\">the console</see></summary>",
                "[the console]{@link Console}");
            AssertTranslation(
                "///<summary><seealso cref=\"Console\">the console</seealso></summary>",
                "[the console]{@link Console}");
        }

        [Test]
        public void Translate_should_recognize_see_with_href_as_empty_element()
        {
            AssertTranslation("///<summary><see href=\"http://something\"/></summary>", "{@link http://something}");
        }

        [Test]
        public void Translate_should_recognize_see_with_href_as_element()
        {
            AssertTranslation(
                "///<summary><see href=\"http://something\">Text</see></summary>",
                "[Text]{@link http://something}");
        }

        [Test]
        public void Translate_should_add_spaces_around_translated_link_tags()
        {
            AssertTranslation(
                "///<summary>This is a <see cref=\"Console\"/> tag</summary>",
                "This is a {@link Console} tag");
        }
    }
}
