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
    using Desalt.Core.Utility;

    /// <summary>
    /// Parses TypeScript code into tokens.
    /// </summary>
    internal sealed partial class TsLexer
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

        private static bool IsHexDigit(char c) => c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';

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

        /// <remarks><code><![CDATA[
        /// CommonToken ::
        ///     IdentifierName
        ///     Punctuator
        ///     NumericLiteral
        ///     StringLiteral
        ///     Template (Not supported yet)
        /// ]]></code></remarks>>
        private TsToken LexCommonToken()
        {
            string next2 = _reader.Peek(2);

            // ReSharper disable PatternAlwaysMatches (ReSharper doesn't understand the new case syntax yet)
            switch ((char)_reader.Peek())
            {
                case char c when IsIdentifierStartChar(c):
                    return LexIdentifierNameOrReservedWord();

                // special case: '.' can either be a punctuator or the start of a numeric literal with no integer part
                case '.' when next2.Length == 1:
                case '.' when !IsDecimalDigit(next2[1]):
                    return LexPunctuator();

                case char c when IsNumericLiteralStartChar(c):
                    return LexNumericLiteral();

                case char c when IsPunctuatorStartChar(c):
                    return LexPunctuator();

                case char c when IsStringLiteralStartChar(c):
                    return LexStringLiteral();

                default:
                    throw LexException($"Unknown character '{_reader.Peek()}.");
            }
            // ReSharper restore PatternAlwaysMatches
        }

        private char Read(char expectedChar)
        {
            if (_reader.Read() != expectedChar)
            {
                throw LexException($"Expected '{expectedChar}' as the next character");
            }

            return expectedChar;
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
