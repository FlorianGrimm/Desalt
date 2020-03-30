// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.Identifiers.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Desalt.CompilerUtilities;
    using Desalt.CompilerUtilities.Extensions;

    public sealed partial class TsLexer
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<string, TsTokenCode> s_keywords =
            ImmutableDictionary.CreateRange(StringComparer.Ordinal, GetKeywords());

        private static IEnumerable<KeyValuePair<string, TsTokenCode>> GetKeywords()
        {
            var items = from tokenCode in TsTokenCodeExtensions.AllKeywords
                        let keyword = char.ToLowerInvariant(tokenCode.ToString()[0]) + tokenCode.ToString().Substring(1)
                        select new KeyValuePair<string, TsTokenCode>(keyword, tokenCode);
            return items;
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

        /// <remarks><code><![CDATA[
        /// IdentifierName ::
        ///     IdentifierStart
        ///     IdentifierName IdentifierPart
        ///
        /// Keyword :: one of
        ///     see list above taken from the TypeScript spec
        /// ]]></code></remarks>
        private TsToken LexIdentifierNameOrReservedWord()
        {
            TextReaderLocation location = _reader.Location;
            var valueBuilder = new StringBuilder();
            var textBuilder = new StringBuilder();

            // get the first character
            string rawText;
            char value;
            if (ReadIf('\\'))
            {
                (rawText, value) = LexUnicodeEscapeSequence();
                textBuilder.Append('\\').Append(rawText);
                valueBuilder.Append(value);
            }
            else
            {
                value = _reader.IsAtEnd ? '\0' : (char)_reader.Read();
                textBuilder.Append(value);
                valueBuilder.Append(value);
            }

            if (!IsIdentifierStartChar(value))
            {
                throw LexException($"Character '{value}' is not a valid start character for an identifier");
            }

            while (!_reader.IsAtEnd && IsIdentifierPartChar((char)_reader.Peek()))
            {
                string? rest = _reader.ReadWhile(c => IsIdentifierPartChar(c) && c != '\\');
                textBuilder.Append(rest);
                valueBuilder.Append(rest);

                if (ReadIf('\\'))
                {
                    (rawText, value) = LexUnicodeEscapeSequence();
                    textBuilder.Append('\\').Append(rawText);
                    valueBuilder.Append(value);
                }
            }

            string identifier = valueBuilder.ToString();
            return s_keywords.TryGetValue(identifier, out TsTokenCode tokenCode)
                ? new TsToken(tokenCode, identifier, location)
                : TsToken.Identifier(textBuilder.ToString(), identifier, location);
        }
    }
}
