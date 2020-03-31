// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexerTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Parsing
{
    using System;
    using System.Linq;
    using Desalt.CompilerUtilities;
    using Desalt.TypeScriptAst.Parsing;
    using FluentAssertions;
    using NUnit.Framework;

    public class TsLexerTests
    {
        private static void AssertLex(string code, params TsToken[] expectedTokens)
        {
            TsLexer.Lex(code)
                .Should()
                .HaveCount(expectedTokens.Length)
                .And.ContainInOrder(expectedTokens.AsEnumerable());
        }

        //// ===========================================================================================================
        //// Identifiers and Keywords Tests
        //// ===========================================================================================================

        [Test]
        public void TsLexer_should_lex_identifiers()
        {
            AssertLex("id", TsToken.Identifier("id", "id", new TextReaderLocation(1, 1)));
            AssertLex("$valid", TsToken.Identifier("$valid", "$valid", new TextReaderLocation(1, 1)));
            AssertLex("\\u007a_bc", TsToken.Identifier("\\u007a_bc", "z_bc", new TextReaderLocation(1, 1)));
            AssertLex("\\u{0061}", TsToken.Identifier("\\u{0061}", "a", new TextReaderLocation(1, 1)));
            AssertLex("j\\u{0061}r", TsToken.Identifier("j\\u{0061}r", "jar", new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_on_an_invalid_unicode_escape_sequence()
        {
            Action action = () => TsLexer.Lex("\\u123");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid Unicode escape sequence '\\u123'");

            action = () => TsLexer.Lex("\\uNo");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: 'N' is not a valid hexadecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("\\u{12345}");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Unicode escape sequence '\\u{12345}' is out of range");
        }

        [Test]
        public void Lex_should_recognize_keywords()
        {
            foreach (TsTokenCode tokenCode in TsTokenCodeExtensions.AllKeywords)
            {
                string keyword = tokenCode.ToString().ToLowerInvariant();
                AssertLex(
                    keyword,
                    new TsToken(tokenCode, keyword, new TextReaderLocation(1, 1)));
            }
        }

        //// ===========================================================================================================
        //// Punctuators Tests
        //// ===========================================================================================================

        [Test]
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
                AssertLex(text, new TsToken(tokenCode, text, new TextReaderLocation(1, 1)));
            }
        }

        //// ===========================================================================================================
        //// Number Literals Tests
        //// ===========================================================================================================

        [Test]
        public void Lex_should_recognize_decimal_integer_literals()
        {
            AssertLex(
                "123",
                TsToken.NumericLiteral(TsTokenCode.DecimalLiteral, "123", 123, new TextReaderLocation(1, 1)));

            AssertLex(
                ".123",
                TsToken.NumericLiteral(TsTokenCode.DecimalLiteral, ".123", .123, new TextReaderLocation(1, 1)));

            AssertLex(
                "123.e10",
                TsToken.NumericLiteral(TsTokenCode.DecimalLiteral, "123.e10", 123e10, new TextReaderLocation(1, 1)));

            AssertLex(
                "123.456e10",
                TsToken.NumericLiteral(
                    TsTokenCode.DecimalLiteral,
                    "123.456e10",
                    123.456e10,
                    new TextReaderLocation(1, 1)));

            AssertLex(
                "123.456e+10",
                TsToken.NumericLiteral(
                    TsTokenCode.DecimalLiteral,
                    "123.456e+10",
                    123.456e10,
                    new TextReaderLocation(1, 1)));

            AssertLex(
                "123.456e-10",
                TsToken.NumericLiteral(
                    TsTokenCode.DecimalLiteral,
                    "123.456e-10",
                    123.456e-10,
                    new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_an_error_if_the_decimal_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("1234e12345678901234567890");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid decimal literal '1234e12345678901234567890'");
        }

        [Test]
        public void Lex_should_recognize_binary_integer_literals()
        {
            AssertLex(
                "0b1101",
                TsToken.NumericLiteral(TsTokenCode.BinaryIntegerLiteral, "0b1101", 13, new TextReaderLocation(1, 1)));

            AssertLex(
                "0B1101",
                TsToken.NumericLiteral(TsTokenCode.BinaryIntegerLiteral, "0B1101", 13, new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_an_error_if_the_binary_integer_is_out_of_range()
        {
            Action action = () =>
                TsLexer.Lex("0b111100001111000011110000111100001111000011110000111100001111000011110000");

            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be(
                    "(1,1): error: Invalid binary integer literal " +
                    "'0b111100001111000011110000111100001111000011110000111100001111000011110000'");
        }

        [Test]
        public void Lex_should_recognize_octal_integer_literals()
        {
            AssertLex(
                "0o01234567",
                TsToken.NumericLiteral(
                    TsTokenCode.OctalIntegerLiteral,
                    "0o01234567",
                    342391,
                    new TextReaderLocation(1, 1)));

            AssertLex(
                "0O01234567",
                TsToken.NumericLiteral(
                    TsTokenCode.OctalIntegerLiteral,
                    "0O01234567",
                    342391,
                    new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_an_error_if_the_octal_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("0o12345670123456701234567012345670");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be(
                    "(1,1): error: Invalid octal integer literal '0o12345670123456701234567012345670'");
        }

        [Test]
        public void Lex_should_recognize_hex_integer_literals()
        {
            AssertLex(
                // ReSharper disable once StringLiteralTypo
                "0x0123456789abcdef",
                TsToken.NumericLiteral(
                    TsTokenCode.HexIntegerLiteral,
                    // ReSharper disable once StringLiteralTypo
                    "0x0123456789abcdef",
                    0x0123456789abcdef,
                    new TextReaderLocation(1, 1)));

            AssertLex(
                // ReSharper disable once StringLiteralTypo
                "0X0123456789ABCDEF",
                TsToken.NumericLiteral(
                    TsTokenCode.HexIntegerLiteral,
                    // ReSharper disable once StringLiteralTypo
                    "0X0123456789ABCDEF",
                    0x0123456789abcdef,
                    new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_an_error_if_the_hex_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("0x0123456789abcdef0123456789abcdef0123456789abcdef");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid hex integer literal '0x0123456789abcdef0123456789abcdef0123456789abcdef'");
        }

        //// ===========================================================================================================
        //// String Literal Tests
        //// ===========================================================================================================

        [Test]
        public void Lex_should_recognize_string_literals()
        {
            AssertLex(
                "'str'",
                TsToken.StringLiteral("'str'", "str", new TextReaderLocation(1, 1)));

            AssertLex(
                "\"str\"",
                TsToken.StringLiteral("\"str\"", "str", new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_an_error_if_the_string_is_not_terminated()
        {
            Action action = () => TsLexer.Lex("'ab");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Unterminated string literal: 'ab");
        }

        // ReSharper disable StringLiteralTypo
        [TestCase("'ab\\'c'", "ab'c")]
        [TestCase("'ab\\\"c'", "ab\"c")]
        [TestCase("'ab\\\\c'", "ab\\c")]
        [TestCase("'ab\\bc'", "ab\bc")]
        [TestCase("'ab\\fc'", "ab\fc")]
        [TestCase("'ab\\nc'", "ab\nc")]
        [TestCase("'ab\\rc'", "ab\rc")]
        [TestCase("'ab\\tc'", "ab\tc")]
        [TestCase("'ab\\vc'", "ab\vc")]
        // ReSharper restore StringLiteralTypo
        public void Lex_should_recognize_string_literals_with_single_character_escape_sequences(string text, string value)
        {
            AssertLex(text, TsToken.StringLiteral(text, value, new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_recognize_string_literals_with_the_null_character()
        {
            AssertLex("'a\\0bc'", TsToken.StringLiteral("'a\\0bc'", "a\0bc", new TextReaderLocation(1, 1)));
            AssertLex("'a\\01b'", TsToken.StringLiteral("'a\\01b'", "a01b", new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_recognize_string_literals_with_hex_escape_sequences()
        {
            AssertLex("'a\\x62c'", TsToken.StringLiteral("'a\\x62c'", "abc", new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_on_an_invalid_hex_escape_sequence_in_a_string_literal()
        {
            Action action = () => TsLexer.Lex("'\\x1");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: Invalid hex escape sequence '\\x1'");

            action = () => TsLexer.Lex("'\\xNo'");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: 'N' is not a valid hexadecimal character as part of a hex escape sequence");
        }

        [Test]
        public void Lex_should_recognize_string_literals_with_Unicode_escape_sequences()
        {
            AssertLex(
                "'a\\u0062\\u0063d'",
                // ReSharper disable once StringLiteralTypo
                TsToken.StringLiteral("'a\\u0062\\u0063d'", "abcd", new TextReaderLocation(1, 1)));
        }

        [Test]
        public void Lex_should_throw_on_an_invalid_Unicode_escape_sequence_in_a_string_literal()
        {
            Action action = () => TsLexer.Lex("'\\u123");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: Invalid Unicode escape sequence '\\u123'");

            action = () => TsLexer.Lex("'\\uNo'");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: 'N' is not a valid hexadecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("'\\u{12345}'");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: Unicode escape sequence '\\u{12345}' is out of range");
        }
    }
}
