// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexerTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TypeScript.Ast.Parsing
{
    using System;
    using System.Linq;
    using Desalt.Core.TypeScript.Ast.Parsing;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TsLexerTests
    {
        private static void AssertLex(string code, params TsToken[] expectedTokens)
        {
            TsLexer.Lex(code).Should().ContainInOrder(expectedTokens.AsEnumerable());
        }

        [TestMethod]
        public void TsLexer_should_lex_identifiers()
        {
            AssertLex("id", TsToken.Identifier("id"));
            AssertLex("$valid", TsToken.Identifier("$valid"));
            AssertLex("\\u007a_bc", TsToken.Identifier("z_bc"));
            AssertLex("\\u{0061}", TsToken.Identifier("a"));
        }

        [TestMethod]
        public void Lex_should_throw_on_an_invalid_unicode_escape_sequence()
        {
            Action action = () => TsLexer.Lex("\\u123");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,3): error: Invalid Unicode escape sequence '123'");

            action = () => TsLexer.Lex("\\uNo");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,3): error: 'N' is not a valid hexidecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("\\u{12345}");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,3): error: Unicode escape sequence '12345' is out of range");
        }

        [TestMethod]
        public void Lex_should_recognize_keywords()
        {
            string[] keywords =
            {
                // The following keywords are reserved and cannot be used as an Identifier:
                "break", "case", "catch", "class", "const", "continue", "debugger", "default", "delete", "do",
                "else", "enum", "export", "extends", "false", "finally", "for", "function", "if", "import", "in",
                "instanceof", "new", "null", "return", "super", "switch", "this", "throw", "true", "try", "typeof",
                "var", "void", "while", "with",

                // The following keywords cannot be used as identifiers in strict mode code, but are otherwise not restricted:
                "implements", "interface", "let", "package", "private", "protected", "public", "static", "yield",

                // The following keywords cannot be used as user defined type names, but are otherwise not restricted:
                "any", "boolean", "number", "string", "symbol",

                // The following keywords have special meaning in certain contexts, but are valid identifiers:
                "abstract", "as", "async", "await", "constructor", "declare", "from", "get", "is", "module",
                "namespace", "of", "require", "set", "type",
            };

            foreach (string keyword in keywords)
            {
                AssertLex(
                    keyword,
                    new TsToken((TsTokenCode)Enum.Parse(typeof(TsTokenCode), keyword, ignoreCase: true), keyword));
            }
        }
    }
}
