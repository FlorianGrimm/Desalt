// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Desalt.Core.Extensions;
    using Desalt.Core.Utility;

    /// <summary>
    /// Parses TypeScript code into tokens.
    /// </summary>
    internal sealed class TsLexer
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<string, TsToken> s_keywords =
            ImmutableDictionary.CreateRange(StringComparer.Ordinal, GetKeywords());

        private readonly string _code;
        private PeekingTextReader _reader;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsLexer(string code)
        {
            _code = code;
        }

        private static IEnumerable<KeyValuePair<string, TsToken>> GetKeywords()
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

            var items = from keyword in keywords
                        let tokenCode = (TsTokenCode)Enum.Parse(typeof(TsTokenCode), keyword, ignoreCase: true)
                        let token = new TsToken(tokenCode, keyword)
                        select new KeyValuePair<string, TsToken>(keyword, token);
            return items;
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ImmutableArray<TsToken> Lex(string code)
        {
            var lexer = new TsLexer(code);
            return lexer.Lex();
        }

        private ImmutableArray<TsToken> Lex()
        {
            var builder = ImmutableArray.CreateBuilder<TsToken>();

            using (_reader = new PeekingTextReader(_code))
            {
                while (!_reader.IsAtEnd)
                {
                    _reader.SkipWhitespace();
                    builder.Add(LexCommonToken());
                }
            }

            return builder.ToImmutable();
        }

        /// <remarks><code>
        /// CommonToken ::
        ///     IdentifierName
        ///     Punctuator
        ///     NumericLiteral
        ///     StringLiteral
        ///     Template
        /// </code></remarks>>
        private TsToken LexCommonToken()
        {
            switch ((char)_reader.Peek())
            {
                case char c when IsIdentifierStartChar(c):
                    return LexIdentifierNameOrReservedWord();

                case char c when IsPunctuatorStartChar(c):
                    return LexPunctuator();

                default:
                    throw LexException($"Unknown character '{_reader.Peek()}.");
            }
        }

        /// <summary>
        /// Returns a value indicating if the character is a valid IdentifierStart character.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IdentifierStart ::
        ///     UnicodeIDStart
        ///     $
        ///     _
        ///     \ UnicodeEscapeSequence
        ///
        /// UnicodeIDStart ::
        ///     any Unicode code point with the Unicode property “ID_Start”
        ///
        /// (from http://www.unicode.org/reports/tr31/)
        /// ID_Start characters are derived from the Unicode General_Category of uppercase letters,
        /// lowercase letters, titlecase letters, modifier letters, other letters, letter numbers,
        /// plus Other_ID_Start, minus Pattern_Syntax and Pattern_White_Space code points.
        /// ]]></code></remarks>
        private static bool IsIdentifierStartChar(char c)
        {
            return c.IsOneOf('$', '_', '\\') ||
                char.GetUnicodeCategory(c)
                    .IsOneOf(
                        UnicodeCategory.UppercaseLetter,
                        UnicodeCategory.LowercaseLetter,
                        UnicodeCategory.TitlecaseLetter,
                        UnicodeCategory.ModifierLetter,
                        UnicodeCategory.OtherLetter,
                        UnicodeCategory.LetterNumber);
        }

        /// <summary>
        /// Returns a value indicating whether the character is a valid IdentifierPart character.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IdentifierPart ::
        ///     UnicodeIDContinue
        ///     $
        ///     _
        ///     \ UnicodeEscapeSequence
        ///     <ZWNJ> - U+200C (ZERO WIDTH NON-JOINER)
        ///     <ZWJ> - U+200D (ZERO WIDTH JOINER)
        ///
        /// UnicodeIDContinue ::
        ///     any Unicode code point with the Unicode property “ID_Continue”
        ///
        /// (from http://www.unicode.org/reports/tr31/)
        /// ID_Continue characters include ID_Start characters, plus characters having the Unicode
        /// General_Category of nonspacing marks, spacing combining marks, decimal number, connector
        /// punctuation, plus Other_ID_Continue , minus Pattern_Syntax and Pattern_White_Space code
        /// points.
        /// ]]></code></remarks>
        private static bool IsIdentifierPartChar(char c)
        {
            return IsIdentifierStartChar(c) ||
                char.GetUnicodeCategory(c)
                    .IsOneOf(
                        UnicodeCategory.NonSpacingMark,
                        UnicodeCategory.SpacingCombiningMark,
                        UnicodeCategory.DecimalDigitNumber,
                        UnicodeCategory.ConnectorPunctuation) ||
                c == '\u200c' ||
                c == '\u200d';
        }

        /// <remarks><code>
        /// IdentifierName ::
        ///     IdentifierStart
        ///     IdentifierName IdentifierPart
        ///
        /// Keyword :: one of
        ///     see list above taken from the TypeScript spec
        /// </code></remarks>
        private TsToken LexIdentifierNameOrReservedWord()
        {
            var builder = new StringBuilder();

            // get the first character
            char firstC;
            if (ReadIf('\\'))
            {
                firstC = LexUnicodeEscapeSequence();
            }
            else
            {
                firstC = (char)_reader.Read();
            }

            if (!IsIdentifierStartChar(firstC))
            {
                throw LexException($"Character '{firstC}' is not a valid start character for an identifier");
            }

            builder.Append(firstC);

            while (!_reader.IsAtEnd && IsIdentifierPartChar((char)_reader.Peek()))
            {
                string rest = _reader.ReadWhile(c => IsIdentifierPartChar(c) && c != '\\');
                builder.Append(rest);

                if (ReadIf('\\'))
                {
                    char c = LexUnicodeEscapeSequence();
                    builder.Append(c);
                }
            }

            string identifier = builder.ToString();
            return s_keywords.TryGetValue(identifier, out TsToken token) ? token : TsToken.Identifier(identifier);
        }

        private static bool IsPunctuatorStartChar(char c)
        {
            return c.IsOneOf(
                '{', '}', '(', ')', '[', ']', '.', ';', ',', '<', '>', '=', '!', '+', '-', '*', '/',
                '%', '&', '|', '^', '~', '?', ':');
        }

        /// <remarks><code><![CDATA[
        /// Punctuator :: one of
        ///     {    (    )    [    ]    .
        ///     ...  ;    ,    <    >    <=
        ///     >=   ==   !=   ===  !==
        ///     +    -    *    %    ++   --
        ///     <<   >>   >>>  &    |    ^
        ///     !    ~    &&   ||   ?    :
        ///     =    +=   -=   *=   %=   <<=
        ///     >>=  >>>= &=   |=   ^=   =>
        ///
        /// DivPunctuator::
        ///     /
        ///     /=
        ///
        /// RightBracePunctuator::
        ///     }
        /// ]]></code></remarks>
        private TsToken LexPunctuator()
        {
            if (_reader.Peek(4) == ">>>=")
            {
                return new TsToken(TsTokenCode.GreaterThanGreaterThanGreaterThanEquals, _reader.Read(4));
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(3))
            {
                case "...":
                    return new TsToken(TsTokenCode.DotDotDot, _reader.Read(3));

                case "===":
                    return new TsToken(TsTokenCode.EqualsEqualsEquals, _reader.Read(3));

                case "!==":
                    return new TsToken(TsTokenCode.ExclamationEqualsEquals, _reader.Read(3));

                case ">>>":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThanGreaterThan, _reader.Read(3));

                case "<<=":
                    return new TsToken(TsTokenCode.LessThanLessThanEquals, _reader.Read(3));

                case ">>=":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThanEquals, _reader.Read(3));
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(2))
            {
                case "<=":
                    return new TsToken(TsTokenCode.LessThanEquals, _reader.Read(2));

                case ">=":
                    return new TsToken(TsTokenCode.GreaterThanEquals, _reader.Read(2));

                case "==":
                    return new TsToken(TsTokenCode.EqualsEquals, _reader.Read(2));

                case "!=":
                    return new TsToken(TsTokenCode.ExclamationEquals, _reader.Read(2));

                case "++":
                    return new TsToken(TsTokenCode.PlusPlus, _reader.Read(2));

                case "--":
                    return new TsToken(TsTokenCode.MinusMinus, _reader.Read(2));

                case "<<":
                    return new TsToken(TsTokenCode.LessThanLessThan, _reader.Read(2));

                case ">>":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThan, _reader.Read(2));

                case "&&":
                    return new TsToken(TsTokenCode.AmpersandAmpersand, _reader.Read(2));

                case "||":
                    return new TsToken(TsTokenCode.PipePipe, _reader.Read(2));

                case "+=":
                    return new TsToken(TsTokenCode.PlusEquals, _reader.Read(2));

                case "-=":
                    return new TsToken(TsTokenCode.MinusEquals, _reader.Read(2));

                case "*=":
                    return new TsToken(TsTokenCode.AsteriskEquals, _reader.Read(2));

                case "/=":
                    return new TsToken(TsTokenCode.SlashEquals, _reader.Read(2));

                case "%=":
                    return new TsToken(TsTokenCode.PercentEquals, _reader.Read(2));

                case "&=":
                    return new TsToken(TsTokenCode.AmpersandEquals, _reader.Read(2));

                case "|=":
                    return new TsToken(TsTokenCode.PipeEquals, _reader.Read(2));

                case "^=":
                    return new TsToken(TsTokenCode.CaretEquals, _reader.Read(2));

                case "=>":
                    return new TsToken(TsTokenCode.EqualsGreaterThan, _reader.Read(2));
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(1))
            {
                case "{":
                    return new TsToken(TsTokenCode.LeftBrace, _reader.Read(1));

                case "}":
                    return new TsToken(TsTokenCode.RightBrace, _reader.Read(1));

                case "(":
                    return new TsToken(TsTokenCode.LeftParen, _reader.Read(1));

                case ")":
                    return new TsToken(TsTokenCode.RightParen, _reader.Read(1));

                case "[":
                    return new TsToken(TsTokenCode.LeftBracket, _reader.Read(1));

                case "]":
                    return new TsToken(TsTokenCode.RightBracket, _reader.Read(1));

                case ".":
                    return new TsToken(TsTokenCode.Dot, _reader.Read(1));

                case ";":
                    return new TsToken(TsTokenCode.Semicolon, _reader.Read(1));

                case ",":
                    return new TsToken(TsTokenCode.Comma, _reader.Read(1));

                case "<":
                    return new TsToken(TsTokenCode.LessThan, _reader.Read(1));

                case ">":
                    return new TsToken(TsTokenCode.GreaterThan, _reader.Read(1));

                case "+":
                    return new TsToken(TsTokenCode.Plus, _reader.Read(1));

                case "-":
                    return new TsToken(TsTokenCode.Minus, _reader.Read(1));

                case "*":
                    return new TsToken(TsTokenCode.Asterisk, _reader.Read(1));

                case "/":
                    return new TsToken(TsTokenCode.Slash, _reader.Read(1));

                case "%":
                    return new TsToken(TsTokenCode.Percent, _reader.Read(1));

                case "&":
                    return new TsToken(TsTokenCode.Ampersand, _reader.Read(1));

                case "|":
                    return new TsToken(TsTokenCode.Pipe, _reader.Read(1));

                case "^":
                    return new TsToken(TsTokenCode.Caret, _reader.Read(1));

                case "!":
                    return new TsToken(TsTokenCode.Exclamation, _reader.Read(1));

                case "~":
                    return new TsToken(TsTokenCode.Tilde, _reader.Read(1));

                case "?":
                    return new TsToken(TsTokenCode.Question, _reader.Read(1));

                case ":":
                    return new TsToken(TsTokenCode.Colon, _reader.Read(1));

                case "=":
                    return new TsToken(TsTokenCode.Equals, _reader.Read(1));
            }

            throw LexException($"Unknown punctuator '{(char)_reader.Read()}'");
        }

        /// <summary>
        /// Lexes a unicode escape sequence.
        /// </summary>
        /// <returns>The character representing the escape sequence.</returns>
        /// <remarks>
        /// <code>
        /// UnicodeEscapeSequence ::
        ///     u Hex4Digits
        ///     u { HexDigits }
        ///
        /// Hex4Digits ::
        ///     HexDigit HexDigit HexDigit HexDigit
        ///
        /// HexDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9 a b c d e f A B C D E F
        /// </code>
        /// </remarks>
        private char LexUnicodeEscapeSequence()
        {
            var builder = new StringBuilder();

            Read('u');
            var location = _reader.Location;

            bool IsHexChar(char c) => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';

            void ReadHexChar()
            {
                if (_reader.IsAtEnd)
                {
                    throw LexException($"Invalid Unicode escape sequence '{builder}'", location);
                }

                char c = (char)_reader.Peek();
                if (!IsHexChar(c))
                {
                    throw LexException(
                        $"'{c}' is not a valid hexidecimal character as part of a Unicode escape sequence");
                }

                _reader.Read();
                builder.Append(c);
            }

            if (ReadIf('{'))
            {
                while (_reader.Peek() != '}')
                {
                    ReadHexChar();
                }

                Read('}');
            }
            else
            {
                ReadHexChar();
                ReadHexChar();
                ReadHexChar();
                ReadHexChar();
            }

            int charValue = int.Parse(builder.ToString(), NumberStyles.HexNumber);
            string converted = char.ConvertFromUtf32(charValue);
            if (converted.Length > 1)
            {
                throw LexException($"Unicode escape sequence '{builder}' is out of range", location);
            }

            return converted[0];
        }

        private void Read(char expectedChar)
        {
            if (_reader.Read() != expectedChar)
            {
                throw LexException($"Expected '{expectedChar}' as the next character");
            }
        }

        private char Read(Func<char, bool> expectedCharFunc)
        {
            char c = (char)_reader.Read();
            if (!expectedCharFunc(c))
            {
                throw LexException($"Did not expect '{c}' as the next character");
            }

            return c;
        }

        private bool ReadIf(char expectedChar)
        {
            if (_reader.Peek() == expectedChar)
            {
                _reader.Read();
                return true;
            }

            return false;
        }

        private Exception LexException(string message, TextReaderLocation? location = null)
        {
            message = $"{location ?? _reader.Location}: error: {message}";
            return new Exception(message);
        }
    }
}
