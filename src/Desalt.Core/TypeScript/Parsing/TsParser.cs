// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.Utility;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Parses an expression into an abstract syntax tree (AST).
    /// </summary>
    internal partial class TsParser
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly TsTokenReader _reader;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsParser(ImmutableArray<TsToken> tokens)
        {
            _reader = new TsTokenReader(tokens);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Parses a TypeScript expression.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public static ITsExpression ParseExpression(string code)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens);
            return parser.ParseExpression();
        }

        /// <summary>
        /// Parses a TypeScript expression.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <returns>An <see cref="ITsType"/>.</returns>
        public static ITsType ParseType(string code)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens);
            return parser.ParseType();
        }

        /// <summary>
        /// Parses a TypeScript statement.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <returns>An <see cref="ITsStatement"/>.</returns>
        public static ITsStatement ParseStatement(string code)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens);
            return parser.ParseStatement();
        }

        /// <summary>
        /// Parses a property name, which is the key inside of an object literal.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// PropertyName:
        ///     LiteralPropertyName
        ///     ComputedPropertyName
        ///
        /// LiteralPropertyName:
        ///     IdentifierName
        ///     StringLiteral
        ///     NumericLiteral
        ///
        /// ComputedPropertyName:
        ///     [ AssignmentExpression ]
        /// ]]></code></remarks>
        private ITsPropertyName ParsePropertyName()
        {
            if (_reader.ReadIf(TsTokenCode.Identifier, out TsToken identifierToken))
            {
                return Factory.Identifier(identifierToken.Value.ToString());
            }

            if (_reader.ReadIf(TsTokenCode.StringLiteral, out TsToken stringLiteralToken))
            {
                return ToStringLiteral(stringLiteralToken);
            }

            if (_reader.ReadIf(IsNumericLiteral, out TsToken numericLiteralToken))
            {
                return Factory.Number((double)numericLiteralToken.Value);
            }

            Read(TsTokenCode.LeftBracket);
            ITsExpression expression = ParseAssignmentExpression();
            Read(TsTokenCode.RightBracket);

            return Factory.ComputedPropertyName(expression);
        }

        /// <summary>
        /// Parses a namespace name (aka qualified name) for the form 'name.name.name'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// NamespaceName:
        ///     IdentifierReference
        ///     NamespaceName . IdentifierReference
        ///
        /// TypeQueryExpression:
        ///     IdentifierReference
        ///     TypeQueryExpression . IdentifierName
        /// ]]></code></remarks>
        private ITsQualifiedName ParseQualifiedName()
        {
            var identifiers = new List<ITsIdentifier>();

            do
            {
                ITsIdentifier identifier = ParseIdentifierReference();
                identifiers.Add(identifier);
            }
            while (_reader.ReadIf(TsTokenCode.Dot));

            return Factory.QualifiedName(identifiers.ToArray());
        }

        /// <summary>
        /// Parses an identifier expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IdentifierReference:
        ///     Identifier
        ///     [~Yield] yield
        /// ]]></code></remarks>
        private ITsIdentifier ParseIdentifierReference() => ParseIdentifier();

        private ITsIdentifier TryParseIdentifier()
        {
            return _reader.ReadIf(TsTokenCode.Identifier, out TsToken identifierToken)
                ? Factory.Identifier(identifierToken.Value.ToString())
                : null;
        }

        private ITsIdentifier ParseIdentifier()
        {
            TsToken identifierToken;

            // keywords can be identifiers as long as they're not reserved words
            if (_reader.Peek().TokenCode > TsTokenCode.LastReservedWord)
            {
                var keywordAsIdentifierToken = _reader.Read();
                identifierToken = TsToken.Identifier(
                    keywordAsIdentifierToken.Text,
                    keywordAsIdentifierToken.Text,
                    keywordAsIdentifierToken.Location);
            }
            else
            {
                identifierToken = Read(TsTokenCode.Identifier);
            }

            return Factory.Identifier(identifierToken.Value.ToString());
        }

        /// <summary>
        /// Parses either an identifier or a keyword that will be interpreted as an identifier.
        /// </summary>
        /// <returns></returns>
        private ITsIdentifier ParseIdentifierOrKeyword()
        {
            if (_reader.Peek().TokenCode < TsTokenCode.Identifier)
            {
                throw NewParseException(
                    $"Expected an identifier or keyword as the next token, but saw '{_reader.Peek().TokenCode}'");
            }

            TsToken token = _reader.Read();
            return Factory.Identifier(token.Value.ToString());
        }

        private TsToken Read(TsTokenCode expectedTokenCode)
        {
            if (_reader.Peek().TokenCode != expectedTokenCode)
            {
                throw NewParseException($"Expected '{expectedTokenCode}' as the next token");
            }

            return _reader.Read();
        }

        private TsParserException NewParseException(string message, TextReaderLocation? location = null)
        {
            return new TsParserException(message, location ?? _reader.Peek().Location);
        }

        private TsParserException NotYetImplementedException(string message, TextReaderLocation? location = null)
        {
            return new TsParserException(message, location ?? _reader.Peek().Location)
            {
                NotYetImplemented = true
            };
        }
    }
}
