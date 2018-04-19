// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.EscapeSequences.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Globalization;
    using System.Text;
    using Desalt.Core.Utility;

    internal sealed partial class TsLexer
    {
        /// <summary>
        /// Lexes a string literal of the form 'string' or "string".
        /// </summary>
        /// <remarks><code><![CDATA[
        /// EscapeSequence ::
        ///     CharacterEscapeSequence
        ///     0 [lookahead ∉ DecimalDigit]
        ///     HexEscapeSequence
        ///     UnicodeEscapeSequence
        ///
        /// CharacterEscapeSequence ::
        ///     SingleEscapeCharacter
        ///     NonEscapeCharacter
        ///
        /// SingleEscapeCharacter :: one of
        ///     ' " \ b f n r t v
        ///
        /// NonEscapeCharacter::
        ///     SourceCharacter but not one of EscapeCharacter or LineTerminator
        ///
        /// EscapeCharacter ::
        ///     SingleEscapeCharacter
        ///     DecimalDigit
        ///     x
        ///     u
        ///
        /// HexEscapeSequence ::
        ///     x HexDigit HexDigit
        /// ]]></code></remarks>
        private (string text, char value) LexEscapeSequence()
        {
            string next2 = _reader.Peek(2);
            char c = (char)_reader.Peek();
            char nextC = next2.Length > 1 ? next2[1] : '\0';

            switch (c)
            {
                case 'b':
                    return (_reader.Read(1), '\b');

                case 'f':
                    return (_reader.Read(1), '\f');

                case 'n':
                    return (_reader.Read(1), '\n');

                case 'r':
                    return (_reader.Read(1), '\r');

                case 't':
                    return (_reader.Read(1), '\t');

                case 'v':
                    return (_reader.Read(1), '\v');

                case '0' when !IsDecimalDigit(nextC):
                    return (_reader.Read(1), '\0');

                case 'x':
                    return LexHexEscapeSequence();

                case 'u':
                    return LexUnicodeEscapeSequence();

                default:
                    return (_reader.Read(1), c);
            }
        }

        /// <summary>
        /// Lexes a hex escape sequence of the form '\x12'. The '\' should have already been read.
        /// </summary>
        /// <returns>The character representing the escape sequence.</returns>
        /// <remarks><code><![CDATA[
        /// HexEscapeSequence ::
        ///     x HexDigit HexDigit
        /// ]]></code></remarks>
        private (string text, char value) LexHexEscapeSequence()
        {
            TextReaderLocation startLocation = _reader.Location.DecrementColumn();
            Read('x');

            string text = "x";
            string valueText = "";

            for (int i = 0; i < 2; i++)
            {
                if (_reader.IsAtEnd)
                {
                    throw LexException($"Invalid hex escape sequence '\\{text}'", startLocation);
                }

                char c = (char)_reader.Peek();
                if (!IsHexDigit(c))
                {
                    throw LexException(
                        $"'{c}' is not a valid hexidecimal character as part of a hex escape sequence",
                        startLocation);
                }

                _reader.Read();
                text += c;
                valueText += c;
            }

            int charValue = int.Parse(valueText, NumberStyles.HexNumber);
            string converted = char.ConvertFromUtf32(charValue);
            return (text, converted[0]);
        }

        /// <summary>
        /// Lexes a unicode escape sequence of the form '\u1234' or '\u{12345678}'. The '\' should
        /// have already been read.
        /// </summary>
        /// <returns>The character representing the escape sequence.</returns>
        /// <remarks>
        /// <code>
        /// <![CDATA[
        /// UnicodeEscapeSequence ::
        ///     u Hex4Digits
        ///     u { HexDigits }
        ///
        /// Hex4Digits ::
        ///     HexDigit HexDigit HexDigit HexDigit
        ///
        /// HexDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9 a b c d e f A B C D E F
        /// ]]></code></remarks>
        private (string text, char value) LexUnicodeEscapeSequence()
        {
            var textBuilder = new StringBuilder();
            var valueBuilder = new StringBuilder();

            TextReaderLocation startLocation = _reader.Location.DecrementColumn();
            Read('u');
            textBuilder.Append('u');

            void ReadHexChar()
            {
                if (_reader.IsAtEnd)
                {
                    throw LexException($"Invalid Unicode escape sequence '\\{textBuilder}'", startLocation);
                }

                char c = (char)_reader.Peek();
                if (!IsHexDigit(c))
                {
                    throw LexException(
                        $"'{c}' is not a valid hexidecimal character as part of a Unicode escape sequence",
                        startLocation);
                }

                _reader.Read();
                textBuilder.Append(c);
                valueBuilder.Append(c);
            }

            if (ReadIf('{'))
            {
                textBuilder.Append('{');
                while (_reader.Peek() != '}')
                {
                    ReadHexChar();
                }

                textBuilder.Append(Read('}'));
            }
            else
            {
                ReadHexChar();
                ReadHexChar();
                ReadHexChar();
                ReadHexChar();
            }

            string rawText = textBuilder.ToString();
            int charValue = int.Parse(valueBuilder.ToString(), NumberStyles.HexNumber);
            string converted = char.ConvertFromUtf32(charValue);
            if (converted.Length > 1)
            {
                throw LexException($"Unicode escape sequence '\\{rawText}' is out of range", startLocation);
            }

            return (rawText, converted[0]);
        }
    }
}
