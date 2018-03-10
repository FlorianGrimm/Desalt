// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Lexical.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TypeScript.Ast
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_single_line_comment()
        {
            VerifyOutput(Factory.SingleLineComment("my comment"), "// my comment\n");
            VerifyOutput(Factory.SingleLineComment("my comment", omitNewLineAtEnd: true), "// my comment");
        }

        [TestMethod]
        public void Emit_a_multi_line_comment_with_no_lines()
        {
            VerifyOutput(Factory.MultiLineComment(), "/**/");
            VerifyOutput(Factory.MultiLineComment(isJsDoc: true), "/***/");
        }

        [TestMethod]
        public void Emit_a_multi_line_comment_with_a_single_line()
        {
            VerifyOutput(Factory.MultiLineComment("line 1"), "/* line 1 */");
        }

        [TestMethod]
        public void Emit_a_multi_line_comment_with_multiple_lines()
        {
            VerifyOutput(Factory.MultiLineComment("line 1", "line 2"), "/* line 1\n * line 2\n */\n");
        }

        [TestMethod]
        public void Emit_a_single_line_JsDoc_comment_on_three_lines()
        {
            VerifyOutput(Factory.MultiLineComment(isJsDoc: true, lines: "line 1"), "/**\n * line 1\n */\n");
        }

        public void Emit_a_JSDoc_link_with_no_text()
        {
            VerifyOutput(Factory.JsDocLinkTag("http"), "{@link http}");
        }

        [TestMethod]
        public void Emit_a_JSDoc_link_with_text()
        {
            VerifyOutput(Factory.JsDocLinkTag("http", "Text"), "[Text]{@link http}");
        }

        [TestMethod]
        public void Emit_should_strip_out_illegal_characters_for_JSDoc_links()
        {
            VerifyOutput(Factory.JsDocLinkTag("{ht{tp}", "[Te][xt]"), "[Text]{@link http}");
        }
    }
}
