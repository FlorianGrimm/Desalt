// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexerTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.TypeScript.Parsing
{
    using System;
    using System.Linq;
    using CompilerUtilities;
    using FluentAssertions;
    using TypeScriptAst.TypeScript.Parsing;
    using Xunit;

    public class TsLexerTests
    {
        private static void AssertLex(string code, params TsToken[] expectedTokens)
        {
            TsLexer.Lex(code).Should().ContainInOrder(expectedTokens.AsEnumerable());
        }

        [Fact]
        public void TsLexer_should_lex_identifiers()
        {
            AssertLex("id", TsToken.Identifier("id", "id", new TextReaderLocation(1, 1)));
            AssertLex("$valid", TsToken.Identifier("$valid", "$valid", new TextReaderLocation(1, 1)));
            AssertLex("\\u007a_bc", TsToken.Identifier("\\u007a_bc", "z_bc", new TextReaderLocation(1, 1)));
            AssertLex("\\u{0061}", TsToken.Identifier("\\u{0061}", "a", new TextReaderLocation(1, 1)));
            AssertLex("j\\u{0061}r", TsToken.Identifier("j\\u{0061}r", "jar", new TextReaderLocation(1, 1)));
        }

        [Fact]
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
                .Be("(1,1): error: 'N' is not a valid hexidecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("\\u{12345}");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Unicode escape sequence '\\u{12345}' is out of range");
        }

        [Fact]
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
                    new TsToken(
                        (TsTokenCode)Enum.Parse(typeof(TsTokenCode), keyword, ignoreCase: true),
                        keyword,
                        new TextReaderLocation(1, 1)));
            }
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void Lex_should_throw_an_error_if_the_decimal_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("1234e12345678901234567890");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid decimal literal '1234e12345678901234567890'");
        }

        [Fact]
        public void Lex_should_recognize_binary_integer_literals()
        {
            AssertLex(
                "0b1101",
                TsToken.NumericLiteral(TsTokenCode.BinaryIntegerLiteral, "0b1101", 13, new TextReaderLocation(1, 1)));

            AssertLex(
                "0B1101",
                TsToken.NumericLiteral(TsTokenCode.BinaryIntegerLiteral, "0B1101", 13, new TextReaderLocation(1, 1)));
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void Lex_should_throw_an_error_if_the_octal_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("0o12345670123456701234567012345670");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be(
                    "(1,1): error: Invalid octal integer literal '0o12345670123456701234567012345670'");
        }

        [Fact]
        public void Lex_should_recognize_hex_integer_literals()
        {
            AssertLex(
                "0x0123456789abcdef",
                TsToken.NumericLiteral(
                    TsTokenCode.HexIntegerLiteral,
                    "0x0123456789abcdef",
                    0x0123456789abcdef,
                    new TextReaderLocation(1, 1)));

            AssertLex(
                "0X0123456789ABCDEF",
                TsToken.NumericLiteral(
                    TsTokenCode.HexIntegerLiteral,
                    "0X0123456789ABCDEF",
                    0x0123456789abcdef,
                    new TextReaderLocation(1, 1)));
        }

        [Fact]
        public void Lex_should_throw_an_error_if_the_hex_integer_is_out_of_range()
        {
            Action action = () => TsLexer.Lex("0x0123456789abcdef0123456789abcdef0123456789abcdef");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Invalid hex integer literal '0x0123456789abcdef0123456789abcdef0123456789abcdef'");
        }

        [Fact]
        public void Lex_should_recognize_string_literals()
        {
            AssertLex(
                "'str'",
                TsToken.StringLiteral("'str'", "str", new TextReaderLocation(1, 1)));

            AssertLex(
                "\"str\"",
                TsToken.StringLiteral("\"str\"", "str", new TextReaderLocation(1, 1)));
        }

        [Fact]
        public void Lex_should_throw_an_eerror_if_the_string_is_not_terminated()
        {
            Action action = () => TsLexer.Lex("'ab");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,1): error: Unterminated string literal: 'ab");
        }

        [Theory]
        [InlineData("'ab\\'c'", "ab'c")]
        [InlineData("'ab\\\"c'", "ab\"c")]
        [InlineData("'ab\\\\c'", "ab\\c")]
        [InlineData("'ab\\bc'", "ab\bc")]
        [InlineData("'ab\\fc'", "ab\fc")]
        [InlineData("'ab\\nc'", "ab\nc")]
        [InlineData("'ab\\rc'", "ab\rc")]
        [InlineData("'ab\\tc'", "ab\tc")]
        [InlineData("'ab\\vc'", "ab\vc")]
        public void Lex_should_recognize_string_literals_with_single_character_escape_sequences(string text, string value)
        {
            AssertLex(text, TsToken.StringLiteral(text, value, new TextReaderLocation(1, 1)));
        }

        [Fact]
        public void Lex_should_recognize_string_literals_with_the_null_character()
        {
            AssertLex("'a\\0bc'", TsToken.StringLiteral("'a\\0bc'", "a\0bc", new TextReaderLocation(1, 1)));
            AssertLex("'a\\01b'", TsToken.StringLiteral("'a\\01b'", "a01b", new TextReaderLocation(1, 1)));
        }

        [Fact]
        public void Lex_should_recognize_string_literals_with_hex_escape_sequences()
        {
            AssertLex("'a\\x62c'", TsToken.StringLiteral("'a\\x62c'", "abc", new TextReaderLocation(1, 1)));
        }

        [Fact]
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
                .Be("(1,2): error: 'N' is not a valid hexidecimal character as part of a hex escape sequence");
        }

        [Fact]
        public void Lex_should_recognize_string_literals_with_Unicode_escape_sequences()
        {
            AssertLex(
                "'a\\u0062\\u0063d'",
                TsToken.StringLiteral("'a\\u0062\\u0063d'", "abcd", new TextReaderLocation(1, 1)));
        }

        [Fact]
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
                .Be("(1,2): error: 'N' is not a valid hexidecimal character as part of a Unicode escape sequence");

            action = () => TsLexer.Lex("'\\u{12345}'");
            action.Should()
                .ThrowExactly<Exception>()
                .And.Message.Should()
                .Be("(1,2): error: Unicode escape sequence '\\u{12345}' is out of range");
        }
    }
}
