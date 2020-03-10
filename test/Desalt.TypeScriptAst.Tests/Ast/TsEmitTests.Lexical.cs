// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Lexical.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Ast
{
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Emit;
    using Xunit;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [Fact]
        public void Emit_single_line_comment()
        {
            VerifyOutput(Factory.SingleLineComment("my comment"), "// my comment\n");
            VerifyOutput(Factory.SingleLineComment("my comment", omitNewLineAtEnd: true), "// my comment");
        }

        [Fact]
        public void Emit_a_multi_line_comment_with_no_lines()
        {
            VerifyOutput(Factory.MultiLineComment(), "/**/");
            VerifyOutput(Factory.MultiLineComment(isJsDoc: true), "/***/");
        }

        [Fact]
        public void Emit_a_multi_line_comment_with_a_single_line()
        {
            VerifyOutput(Factory.MultiLineComment("line 1"), "/* line 1 */");
        }

        [Fact]
        public void Emit_a_multi_line_comment_with_multiple_lines()
        {
            VerifyOutput(Factory.MultiLineComment("line 1", "line 2"), "/* line 1\n * line 2\n */\n");
        }

        [Fact]
        public void Emit_a_single_line_JsDoc_comment_on_three_lines()
        {
            VerifyOutput(Factory.MultiLineComment(isJsDoc: true, lines: "line 1"), "/**\n * line 1\n */\n");
        }

        //// ===========================================================================================================
        //// ITsJsDocLinkTag
        //// ===========================================================================================================

        [Fact]
        public void Emit_a_JSDoc_link_with_no_text()
        {
            VerifyOutput(Factory.JsDocLinkTag("http"), "{@link http}");
        }

        [Fact]
        public void Emit_a_JSDoc_link_with_text()
        {
            VerifyOutput(Factory.JsDocLinkTag("http", "Text"), "[Text]{@link http}");
        }

        [Fact]
        public void Emit_should_strip_out_illegal_characters_for_JSDoc_links()
        {
            VerifyOutput(Factory.JsDocLinkTag("{ht{tp}", "[Te][xt]"), "[Text]{@link http}");
        }

        //// ===========================================================================================================
        //// ITsJsDocComment
        //// ===========================================================================================================

        [Fact]
        public void Emit_a_JSDoc_comment_with_no_content_on_one_line()
        {
            VerifyOutput(
                Factory.JsDocComment(),
                "/** */",
                EmitOptions.UnixSpaces.WithSingleLineJsDocCommentsOnOneLine(true));
        }

        [Fact]
        public void Emit_a_single_line_JSDoc_comment_on_multiple_lines_by_default()
        {
            VerifyOutput(Factory.JsDocComment("Description"), "/**\n * Description\n */\n");
        }

        [Fact]
        public void Emit_a_single_line_JSDoc_comment_on_one_line_if_the_options_specify()
        {
            EmitOptions options = EmitOptions.UnixSpaces.WithSingleLineJsDocCommentsOnOneLine(true);

            VerifyOutput(Factory.JsDocComment(fileTag: Factory.JsDocBlock("File")), "/** @file File */", options);
            VerifyOutput(
                Factory.JsDocComment(copyrightTag: Factory.JsDocBlock("Copyright")),
                "/** @copyright Copyright */",
                options);

            VerifyOutput(Factory.JsDocComment(isPackagePrivate: true), "/** @package */", options);

            VerifyOutput(
                Factory.JsDocComment(paramsTags: ("p1", Factory.JsDocBlock("Param")).ToSafeArray()),
                "/** @param p1 Param */",
                options);

            VerifyOutput(
                Factory.JsDocComment(returnsTag: Factory.JsDocBlock("Returns")),
                "/** @returns Returns */",
                options);

            VerifyOutput(
                Factory.JsDocComment(throwsTags: ("Error", Factory.JsDocBlock("Throws")).ToSafeArray()),
                "/** @throws {Error} Throws */",
                options);

            VerifyOutput(
                Factory.JsDocComment(exampleTags: Factory.JsDocBlock("Example").ToSafeArray()),
                "/** @example Example */",
                options);

            VerifyOutput(Factory.JsDocComment("Description"), "/** Description */", options);

            VerifyOutput(
                Factory.JsDocComment(summaryTag: Factory.JsDocBlock("Summary")),
                "/** @summary Summary */",
                options);

            VerifyOutput(
                Factory.JsDocComment(seeTags: Factory.JsDocBlock("See").ToSafeArray()),
                "/** @see See */",
                options);
        }

        [Fact]
        public void Emit_a_JSDoc_comment_with_only_a_single_tag_should_write_on_multiple_lines_by_default()
        {
            VerifyOutput(Factory.JsDocComment(fileTag: Factory.JsDocBlock("File")), "/**\n * @file File\n */\n");
            VerifyOutput(
                Factory.JsDocComment(copyrightTag: Factory.JsDocBlock("Copyright")),
                "/**\n * @copyright Copyright\n */\n");

            VerifyOutput(Factory.JsDocComment(isPackagePrivate: true), "/**\n * @package\n */\n");

            VerifyOutput(
                Factory.JsDocComment(paramsTags: ("p1", Factory.JsDocBlock("Param")).ToSafeArray()),
                "/**\n * @param p1 Param\n */\n");

            VerifyOutput(
                Factory.JsDocComment(returnsTag: Factory.JsDocBlock("Returns")),
                "/**\n * @returns Returns\n */\n");

            VerifyOutput(
                Factory.JsDocComment(throwsTags: ("Error", Factory.JsDocBlock("Throws")).ToSafeArray()),
                "/**\n * @throws {Error} Throws\n */\n");

            VerifyOutput(
                Factory.JsDocComment(exampleTags: Factory.JsDocBlock("Example").ToSafeArray()),
                "/**\n * @example Example\n */\n");

            VerifyOutput(Factory.JsDocComment("Description"), "/**\n * Description\n */\n");

            VerifyOutput(
                Factory.JsDocComment(summaryTag: Factory.JsDocBlock("Summary")),
                "/**\n * @summary Summary\n */\n");

            VerifyOutput(
                Factory.JsDocComment(seeTags: Factory.JsDocBlock("See").ToSafeArray()),
                "/**\n * @see See\n */\n");
        }

        [Fact]
        public void Emit_a_JSDoc_comment_ensuring_the_order_of_the_tags()
        {
            VerifyOutput(
                Factory.JsDocComment(
                    description: Factory.JsDocBlock("Description"),
                    summaryTag: Factory.JsDocBlock("Summary"),
                    fileTag: Factory.JsDocBlock("File"),
                    copyrightTag: Factory.JsDocBlock("Copyright"),
                    isPackagePrivate: true,
                    paramsTags: new[] { ("p1", Factory.JsDocBlock("Param 1")), ("p2", Factory.JsDocBlock("Param 2")) },
                    returnsTag: Factory.JsDocBlock("Returns"),
                    throwsTags: new[]
                    {
                        ("type1", Factory.JsDocBlock("Throws 1")),
                        ("type2", Factory.JsDocBlock("Throws 2"))
                    },
                    exampleTags: new[] { Factory.JsDocBlock("Example 1"), Factory.JsDocBlock("Example 2") },
                    seeTags: new[] { Factory.JsDocBlock("See 1"), Factory.JsDocBlock("See 2") }),
                @"/**
 * Description
 * @summary Summary
 * @file File
 * @copyright Copyright
 * @package
 * @param p1 Param 1
 * @param p2 Param 2
 * @returns Returns
 * @throws {type1} Throws 1
 * @throws {type2} Throws 2
 * @example Example 1
 * @example Example 2
 * @see See 1
 * @see See 2
 */
".Replace("\r\n", "\n"));
        }
    }
}
