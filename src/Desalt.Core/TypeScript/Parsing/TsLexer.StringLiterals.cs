// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.StringLiterals.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Text;
    using Desalt.Core.Utility;

    internal sealed partial class TsLexer
    {
        private static bool IsStringLiteralStartChar(char c) => c == '\'' || c == '"';

        /// <summary>
        /// Lexes a string literal of the form 'string' or "string".
        /// </summary>
        /// <remarks><code><![CDATA[
        /// StringLiteral ::
        ///     " DoubleStringCharactersOpt "
        ///     ' SingleStringCharactersOpt '
        ///
        /// DoubleStringCharacters ::
        ///     DoubleStringCharacter DoubleStringCharactersOpt
        ///
        /// SingleStringCharacters ::
        ///     SingleStringCharacter SingleStringCharactersOpt
        ///
        /// DoubleStringCharacter ::
        ///     SourceCharacter but not one of " or \ or LineTerminator
        ///     \ EscapeSequence
        ///     LineContinuation
        ///
        /// SingleStringCharacter ::
        ///     SourceCharacter but not one of ' or \ or LineTerminator
        ///     \ EscapeSequence
        ///     LineContinuation
        ///
        /// (We're not supporting LineContinuation)
        /// LineContinuation ::
        ///     \ LineTerminatorSequence
        /// ]]></code></remarks>
        private TsToken LexStringLiteral()
        {
            TextReaderLocation location = _reader.Location;
            var textBuilder = new StringBuilder();
            var valueBuilder = new StringBuilder();

            // read the first quote
            char quoteChar = (char)_reader.Peek();
            textBuilder.Append(Read(IsStringLiteralStartChar));

            while (!_reader.IsAtEnd && _reader.Peek() != quoteChar)
            {
                string nextTextPart = _reader.ReadUntil(c => c == quoteChar || c == '\\');
                textBuilder.Append(nextTextPart);
                valueBuilder.Append(nextTextPart);

                if (ReadIf('\\'))
                {
                    (string rawText, char value) = LexEscapeSequence();
                    textBuilder.Append('\\').Append(rawText);
                    valueBuilder.Append(value);
                }
            }

            // make sure the last quote is there
            if (_reader.Peek() != quoteChar)
            {
                throw LexException($"Unterminated string literal: {textBuilder}", location);
            }

            textBuilder.Append((char)_reader.Read());

            return TsToken.WithValue(
                TsTokenCode.StringLiteral,
                textBuilder.ToString(),
                valueBuilder.ToString(),
                location);
        }
    }
}
