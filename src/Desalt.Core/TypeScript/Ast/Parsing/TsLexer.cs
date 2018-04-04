// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Parsing
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
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

        private readonly string _code;
        private PeekingTextReader _reader;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsLexer(string code)
        {
            _code = code;
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

                default:
                    throw LexException($"Unknown character '{_reader.Peek()}.");
            }
        }

        /// <remarks><code>
        /// IdentifierName ::
        ///     IdentifierStart
        ///     IdentifierName IdentifierPart
        ///
        /// ReservedWord ::
        ///     Keyword
        ///     FutureReservedWord
        ///     NullLiteral
        ///     BooleanLiteral
        ///
        /// Keyword :: one of
        ///     break do in typeof
        ///     case else instanceof var
        ///     catch export new void
        ///     class extends return while
        ///     const finally super with
        ///     continue for switch yield
        ///     debugger function this
        ///     default if throw
        ///     delete import try
        ///
        /// FutureReservedWord ::
        ///     enum
        ///     await
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

            return TsToken.Identifier(builder.ToString());
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
            return default(TsToken);
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

        private void Read(Func<char, bool> expectedCharFunc)
        {
            char c = (char)_reader.Read();
            if (!expectedCharFunc(c))
            {
                throw LexException($"Did not expect '{c}' as the next character");
            }
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
