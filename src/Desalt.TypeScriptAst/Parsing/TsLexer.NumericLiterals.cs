// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.NumericLiterals.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System;
    using System.Text;
    using Desalt.CompilerUtilities;
    using Desalt.CompilerUtilities.Extensions;

    public sealed partial class TsLexer
    {
        private static bool IsDecimalDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        /// <summary>
        /// Returns a value indicating whether the character is a valid start character for a numeric literal.
        /// </summary>
        private static bool IsNumericLiteralStartChar(char c)
        {
            return c >= '0' && c <= '9' || c == '.';
        }

        /// <summary>
        /// Lexes a numeric literal.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// NumericLiteral ::
        ///     DecimalLiteral
        ///     BinaryIntegerLiteral
        ///     OctalIntegerLiteral
        ///     HexIntegerLiteral
        /// ]]></code></remarks>
        private TsToken LexNumericLiteral()
        {
            switch (_reader.Peek(2))
            {
                case "0b":
                case "0B":
                    return LexBinaryIntegerLiteral();

                case "0o":
                case "0O":
                    return LexOctalIntegerLiteral();

                case "0x":
                case "0X":
                    return LexHexIntegerLiteral();

                default:
                    return LexDecimalLiteral();
            }
        }

        /// <summary>
        /// Lexes a decimal literal, which is a floating point number with an optional exponent.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// DecimalLiteral ::
        ///     DecimalIntegerLiteral . DecimalDigitsOpt ExponentPartOpt
        ///     . DecimalDigits ExponentPartOpt
        ///     DecimalIntegerLiteral ExponentPartOpt
        ///
        /// DecimalIntegerLiteral ::
        ///     0
        ///     NonZeroDigit DecimalDigitsOpt
        ///
        /// DecimalDigits ::
        ///     DecimalDigit
        ///     DecimalDigits DecimalDigit
        ///
        /// DecimalDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9
        ///
        /// NonZeroDigit :: one of
        ///     1 2 3 4 5 6 7 8 9
        ///
        /// ExponentPart ::
        ///     ExponentIndicator SignedInteger
        ///
        /// ExponentIndicator :: one of
        ///     e E
        ///
        /// SignedInteger ::
        ///     DecimalDigits
        ///     +DecimalDigits
        ///     -DecimalDigits
        /// ]]></code></remarks>
        private TsToken LexDecimalLiteral()
        {
            TextReaderLocation location = _reader.Location;

            string ReadDecimalIntegerLiteral()
            {
                string decimalInteger = ReadIf('0') ? "0" : _reader.ReadWhile(IsDecimalDigit);
                if (decimalInteger.Length == 0)
                {
                    throw LexException("Expected a decimal literal");
                }

                return decimalInteger;
            }

            string integerPart;
            string decimalPart = null;
            string exponentPart = null;
            var textBuilder = new StringBuilder();

            // read the decimal part if there's no integer part
            if (ReadIf('.'))
            {
                integerPart = "0";
                decimalPart = Read(IsDecimalDigit) + _reader.ReadWhile(IsDecimalDigit);

                textBuilder.Append('.').Append(decimalPart);
            }
            else
            {
                // read the integer.decimal number
                integerPart = ReadDecimalIntegerLiteral();
                textBuilder.Append(integerPart);

                if (ReadIf('.'))
                {
                    decimalPart = _reader.ReadWhile(IsDecimalDigit);
                    textBuilder.Append('.').Append(decimalPart);
                }
            }

            // read the optional exponent
            if (_reader.Peek().IsOneOf('e', 'E'))
            {
                textBuilder.Append((char)_reader.Read());

                char sign = '+';
                if (_reader.Peek().IsOneOf('+', '-'))
                {
                    sign = (char)_reader.Read();
                    textBuilder.Append(sign);
                }

                exponentPart = _reader.ReadWhile(IsDecimalDigit);
                textBuilder.Append(exponentPart);

                exponentPart = sign + exponentPart;
            }

            string text = textBuilder.ToString();
            string valueStr = integerPart +
                (!string.IsNullOrEmpty(decimalPart) ? $".{decimalPart}" : string.Empty) +
                (!string.IsNullOrEmpty(exponentPart) ? $"e{exponentPart}" : string.Empty);

            if (!double.TryParse(valueStr, out double value) || double.IsInfinity(value) || double.IsNaN(value))
            {
                throw LexException($"Invalid decimal literal '{text}'", location);
            }

            return TsToken.NumericLiteral(TsTokenCode.DecimalLiteral, text, value, location);
        }

        /// <summary>
        /// Lexes a binary integer literal of the form '0b01010'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BinaryIntegerLiteral ::
        ///     0b BinaryDigits
        ///     0B BinaryDigits
        ///
        /// BinaryDigits ::
        ///     BinaryDigit
        ///     BinaryDigits BinaryDigit
        ///
        /// BinaryDigit :: one of
        ///     0 1
        /// ]]></code></remarks>
        private TsToken LexBinaryIntegerLiteral()
        {
            TextReaderLocation location = _reader.Location;

            var textBuilder = new StringBuilder("0");
            Read('0');
            textBuilder.Append(Read(c => c == 'b' || c == 'B'));

            bool IsBinaryDigit(char c) => c == '0' || c == '1';

            string valueText = Read(IsBinaryDigit) + _reader.ReadWhile(IsBinaryDigit);
            textBuilder.Append(valueText);

            try
            {
                double value = Convert.ToDouble(Convert.ToInt64(valueText, 2));
                return TsToken.NumericLiteral(
                    TsTokenCode.BinaryIntegerLiteral,
                    textBuilder.ToString(),
                    value,
                    location);
            }
            catch (Exception)
            {
                throw LexException($"Invalid binary integer literal '{textBuilder}'", location);
            }
        }

        /// <summary>
        /// Lexes an octal integer literal of the form '0o1234'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// OctalIntegerLiteral ::
        ///     0o OctalDigits
        ///     0O OctalDigits
        ///
        /// OctalDigits ::
        ///     OctalDigit
        ///     OctalDigits OctalDigit
        ///
        /// OctalDigit :: one of
        ///     0 1 2 3 4 5 6 7
        /// ]]></code></remarks>
        private TsToken LexOctalIntegerLiteral()
        {
            TextReaderLocation location = _reader.Location;

            var textBuilder = new StringBuilder("0");
            Read('0');
            textBuilder.Append(Read(c => c == 'o' || c == 'O'));

            bool IsOctalDigit(char c) => c >= '0' && c <= '7';

            string valueText = Read(IsOctalDigit) + _reader.ReadWhile(IsOctalDigit);
            textBuilder.Append(valueText);

            try
            {
                double value = Convert.ToDouble(Convert.ToInt64(valueText, 8));
                return TsToken.NumericLiteral(TsTokenCode.OctalIntegerLiteral, textBuilder.ToString(), value, location);
            }
            catch (Exception)
            {
                throw LexException($"Invalid octal integer literal '{textBuilder}'", location);
            }
        }

        /// <summary>
        /// Lexes a hexidecimal integer literal of the form '0x12ab'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// HexIntegerLiteral ::
        ///     0x HexDigits
        ///     0X HexDigits
        ///
        /// HexDigits ::
        ///     HexDigit
        ///     HexDigits HexDigit
        ///
        /// HexDigit :: one of
        ///     0 1 2 3 4 5 6 7 8 9 a b c d e f A B C D E F
        /// ]]></code></remarks>
        private TsToken LexHexIntegerLiteral()
        {
            TextReaderLocation location = _reader.Location;

            var textBuilder = new StringBuilder("0");
            Read('0');
            textBuilder.Append(Read(c => c == 'x' || c == 'X'));

            string valueText = Read(IsHexDigit) + _reader.ReadWhile(IsHexDigit);
            textBuilder.Append(valueText);

            try
            {
                double value = Convert.ToDouble(Convert.ToInt64(valueText, 16));
                return TsToken.NumericLiteral(TsTokenCode.HexIntegerLiteral, textBuilder.ToString(), value, location);
            }
            catch (Exception)
            {
                throw LexException($"Invalid hex integer literal '{textBuilder}'", location);
            }
        }
    }
}
