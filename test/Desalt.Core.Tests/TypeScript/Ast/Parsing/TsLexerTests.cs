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
                .Be("(1,1): error: Invalid Unicode escape sequence '123'");

            action = () => TsLexer.Lex("\\uNo");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: 'N' is not a valid hexidecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("\\u{12345}");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Unicode escape sequence '12345' is out of range");
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

        [TestMethod]
        public void Lex_should_recognize_punctuators()
        {
            (string text, TsTokenCode tokenCode)[] punctuators =
            {
                ("{", TsTokenCode.LeftBrace),
                ("}", TsTokenCode.RightBrace),
                ("(", TsTokenCode.LeftParen),
                (")", TsTokenCode.RightParen),
                ("[", TsTokenCode.LeftBracket),
                ("]", TsTokenCode.RightBracket),
                (".", TsTokenCode.Dot),
                ("...", TsTokenCode.DotDotDot),
                (";", TsTokenCode.Semicolon),
                (",", TsTokenCode.Comma),
                ("<", TsTokenCode.LessThan),
                (">", TsTokenCode.GreaterThan),
                ("<=", TsTokenCode.LessThanEquals),
                (">=", TsTokenCode.GreaterThanEquals),
                ("==", TsTokenCode.EqualsEquals),
                ("!=", TsTokenCode.ExclamationEquals),
                ("===", TsTokenCode.EqualsEqualsEquals),
                ("!==", TsTokenCode.ExclamationEqualsEquals),
                ("+", TsTokenCode.Plus),
                ("-", TsTokenCode.Minus),
                ("*", TsTokenCode.Asterisk),
                ("/", TsTokenCode.Slash),
                ("%", TsTokenCode.Percent),
                ("++", TsTokenCode.PlusPlus),
                ("--", TsTokenCode.MinusMinus),
                ("<<", TsTokenCode.LessThanLessThan),
                (">>", TsTokenCode.GreaterThanGreaterThan),
                (">>>", TsTokenCode.GreaterThanGreaterThanGreaterThan),
                ("&", TsTokenCode.Ampersand),
                ("|", TsTokenCode.Pipe),
                ("^", TsTokenCode.Caret),
                ("!", TsTokenCode.Exclamation),
                ("~", TsTokenCode.Tilde),
                ("&&", TsTokenCode.AmpersandAmpersand),
                ("||", TsTokenCode.PipePipe),
                ("?", TsTokenCode.Question),
                (":", TsTokenCode.Colon),
                ("=", TsTokenCode.Equals),
                ("+=", TsTokenCode.PlusEquals),
                ("-=", TsTokenCode.MinusEquals),
                ("*=", TsTokenCode.AsteriskEquals),
                ("/=", TsTokenCode.SlashEquals),
                ("%=", TsTokenCode.PercentEquals),
                ("<<=", TsTokenCode.LessThanLessThanEquals),
                (">>=", TsTokenCode.GreaterThanGreaterThanEquals),
                (">>>=", TsTokenCode.GreaterThanGreaterThanGreaterThanEquals),
                ("&=", TsTokenCode.AmpersandEquals),
                ("|=", TsTokenCode.PipeEquals),
                ("^=", TsTokenCode.CaretEquals),
                ("=>", TsTokenCode.EqualsGreaterThan),
            };

            foreach ((string text, TsTokenCode tokenCode) in punctuators)
            {
                AssertLex(text, new TsToken(tokenCode, text));
            }
        }

        [TestMethod]
        public void Lex_should_recognize_decimal_integer_literals()
        {
            AssertLex("123", TsToken.WithValue(TsTokenCode.DecimalLiteral, "123", 123));
            AssertLex(".123", TsToken.WithValue(TsTokenCode.DecimalLiteral, ".123", .123));
            AssertLex("123.e10", TsToken.WithValue(TsTokenCode.DecimalLiteral, "123.e10", 123e10));
            AssertLex("123.456e10", TsToken.WithValue(TsTokenCode.DecimalLiteral, "123.456e10", 123.456e10));
            AssertLex("123.456e+10", TsToken.WithValue(TsTokenCode.DecimalLiteral, "123.456e+10", 123.456e10));
            AssertLex("123.456e-10", TsToken.WithValue(TsTokenCode.DecimalLiteral, "123.456e-10", 123.456e-10));
        }

        [TestMethod]
        public void Lex_should_throw_an_error_if_the_literal_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("1234e12345678901234567890");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid decimal literal '1234e12345678901234567890'");
        }
    }
}
