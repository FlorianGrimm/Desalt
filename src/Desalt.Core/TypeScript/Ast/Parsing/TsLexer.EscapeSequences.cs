// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.EscapeSequences.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Parsing
{
    using System.Globalization;
    using System.Text;
    using Desalt.Core.Utility;

    internal sealed partial class TsLexer
    {
        /// <summary>
        /// Lexes a unicode escape sequence.
        /// </summary>
        /// <returns>The character representing the escape sequence.</returns>
        /// <remarks><code><![CDATA[
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
                        $"'{c}' is not a valid hexidecimal character as part of Unicode escape sequence " +
                        $"'\\{textBuilder + _reader.ReadUntilWhitespace()}'",
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
