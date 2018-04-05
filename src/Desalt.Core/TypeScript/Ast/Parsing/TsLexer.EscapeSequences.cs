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
        private char LexUnicodeEscapeSequence()
        {
            var builder = new StringBuilder();

            var location = _reader.Location;
            Read('\\');
            Read('u');

            void ReadHexChar()
            {
                if (_reader.IsAtEnd)
                {
                    throw LexException($"Invalid Unicode escape sequence '{builder}'", location);
                }

                char c = (char)_reader.Peek();
                if (!IsHexDigit(c))
                {
                    throw LexException(
                        $"'{c}' is not a valid hexidecimal character as part of a Unicode escape sequence",
                        location);
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
    }
}
