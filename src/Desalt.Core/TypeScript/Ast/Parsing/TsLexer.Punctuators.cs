// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsLexer.Punctuators.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Parsing
{
    using Desalt.Core.Extensions;
    using Desalt.Core.Utility;

    internal sealed partial class TsLexer
    {
        private static bool IsPunctuatorStartChar(char c)
        {
            return c.IsOneOf(
                '{',
                '}',
                '(',
                ')',
                '[',
                ']',
                '.',
                ';',
                ',',
                '<',
                '>',
                '=',
                '!',
                '+',
                '-',
                '*',
                '/',
                '%',
                '&',
                '|',
                '^',
                '~',
                '?',
                ':');
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
        /// DivPunctuator ::
        ///     /
        ///     /=
        ///
        /// RightBracePunctuator ::
        ///     }
        /// ]]></code></remarks>
        private TsToken LexPunctuator()
        {
            TextReaderLocation location = _reader.Location;

            if (_reader.Peek(4) == ">>>=")
            {
                return new TsToken(TsTokenCode.GreaterThanGreaterThanGreaterThanEquals, _reader.Read(4), location);
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(3))
            {
                case "...":
                    return new TsToken(TsTokenCode.DotDotDot, _reader.Read(3), location);

                case "===":
                    return new TsToken(TsTokenCode.EqualsEqualsEquals, _reader.Read(3), location);

                case "!==":
                    return new TsToken(TsTokenCode.ExclamationEqualsEquals, _reader.Read(3), location);

                case ">>>":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThanGreaterThan, _reader.Read(3), location);

                case "<<=":
                    return new TsToken(TsTokenCode.LessThanLessThanEquals, _reader.Read(3), location);

                case ">>=":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThanEquals, _reader.Read(3), location);
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(2))
            {
                case "<=":
                    return new TsToken(TsTokenCode.LessThanEquals, _reader.Read(2), location);

                case ">=":
                    return new TsToken(TsTokenCode.GreaterThanEquals, _reader.Read(2), location);

                case "==":
                    return new TsToken(TsTokenCode.EqualsEquals, _reader.Read(2), location);

                case "!=":
                    return new TsToken(TsTokenCode.ExclamationEquals, _reader.Read(2), location);

                case "++":
                    return new TsToken(TsTokenCode.PlusPlus, _reader.Read(2), location);

                case "--":
                    return new TsToken(TsTokenCode.MinusMinus, _reader.Read(2), location);

                case "<<":
                    return new TsToken(TsTokenCode.LessThanLessThan, _reader.Read(2), location);

                case ">>":
                    return new TsToken(TsTokenCode.GreaterThanGreaterThan, _reader.Read(2), location);

                case "&&":
                    return new TsToken(TsTokenCode.AmpersandAmpersand, _reader.Read(2), location);

                case "||":
                    return new TsToken(TsTokenCode.PipePipe, _reader.Read(2), location);

                case "+=":
                    return new TsToken(TsTokenCode.PlusEquals, _reader.Read(2), location);

                case "-=":
                    return new TsToken(TsTokenCode.MinusEquals, _reader.Read(2), location);

                case "*=":
                    return new TsToken(TsTokenCode.AsteriskEquals, _reader.Read(2), location);

                case "/=":
                    return new TsToken(TsTokenCode.SlashEquals, _reader.Read(2), location);

                case "%=":
                    return new TsToken(TsTokenCode.PercentEquals, _reader.Read(2), location);

                case "&=":
                    return new TsToken(TsTokenCode.AmpersandEquals, _reader.Read(2), location);

                case "|=":
                    return new TsToken(TsTokenCode.PipeEquals, _reader.Read(2), location);

                case "^=":
                    return new TsToken(TsTokenCode.CaretEquals, _reader.Read(2), location);

                case "=>":
                    return new TsToken(TsTokenCode.EqualsGreaterThan, _reader.Read(2), location);
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_reader.Peek(1))
            {
                case "{":
                    return new TsToken(TsTokenCode.LeftBrace, _reader.Read(1), location);

                case "}":
                    return new TsToken(TsTokenCode.RightBrace, _reader.Read(1), location);

                case "(":
                    return new TsToken(TsTokenCode.LeftParen, _reader.Read(1), location);

                case ")":
                    return new TsToken(TsTokenCode.RightParen, _reader.Read(1), location);

                case "[":
                    return new TsToken(TsTokenCode.LeftBracket, _reader.Read(1), location);

                case "]":
                    return new TsToken(TsTokenCode.RightBracket, _reader.Read(1), location);

                case ".":
                    return new TsToken(TsTokenCode.Dot, _reader.Read(1), location);

                case ";":
                    return new TsToken(TsTokenCode.Semicolon, _reader.Read(1), location);

                case ",":
                    return new TsToken(TsTokenCode.Comma, _reader.Read(1), location);

                case "<":
                    return new TsToken(TsTokenCode.LessThan, _reader.Read(1), location);

                case ">":
                    return new TsToken(TsTokenCode.GreaterThan, _reader.Read(1), location);

                case "+":
                    return new TsToken(TsTokenCode.Plus, _reader.Read(1), location);

                case "-":
                    return new TsToken(TsTokenCode.Minus, _reader.Read(1), location);

                case "*":
                    return new TsToken(TsTokenCode.Asterisk, _reader.Read(1), location);

                case "/":
                    return new TsToken(TsTokenCode.Slash, _reader.Read(1), location);

                case "%":
                    return new TsToken(TsTokenCode.Percent, _reader.Read(1), location);

                case "&":
                    return new TsToken(TsTokenCode.Ampersand, _reader.Read(1), location);

                case "|":
                    return new TsToken(TsTokenCode.Pipe, _reader.Read(1), location);

                case "^":
                    return new TsToken(TsTokenCode.Caret, _reader.Read(1), location);

                case "!":
                    return new TsToken(TsTokenCode.Exclamation, _reader.Read(1), location);

                case "~":
                    return new TsToken(TsTokenCode.Tilde, _reader.Read(1), location);

                case "?":
                    return new TsToken(TsTokenCode.Question, _reader.Read(1), location);

                case ":":
                    return new TsToken(TsTokenCode.Colon, _reader.Read(1), location);

                case "=":
                    return new TsToken(TsTokenCode.Equals, _reader.Read(1), location);
            }

            throw LexException($"Unknown punctuator '{(char)_reader.Read()}'", location);
        }
    }
}
