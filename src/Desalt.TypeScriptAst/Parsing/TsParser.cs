// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using Desalt.CompilerUtilities;
    using Desalt.TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Parses an expression into an abstract syntax tree (AST).
    /// </summary>
    public partial class TsParser
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly TsTokenReader _reader;
        private readonly bool _isStrictMode;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsParser(ImmutableArray<TsToken> tokens, bool isStrictMode)
        {
            _reader = new TsTokenReader(tokens);
            _isStrictMode = isStrictMode;
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Parses a TypeScript expression.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="isStrictMode">Indicates whether the parse should be done in strict mode.</param>
        /// <returns>An <see cref="ITsExpression"/>.</returns>
        public static ITsExpression ParseExpression(string code, bool isStrictMode = true)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens, isStrictMode);
            return parser.ParseExpression();
        }

        /// <summary>
        /// Parses a TypeScript expression.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="isStrictMode">Indicates whether the parse should be done in strict mode.</param>
        /// <returns>An <see cref="ITsType"/>.</returns>
        internal static ITsType ParseType(string code, bool isStrictMode = true)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens, isStrictMode);
            return parser.ParseType();
        }

        /// <summary>
        /// Parses a TypeScript statement.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="isStrictMode">Indicates whether the parse should be done in strict mode.</param>
        /// <returns>An <see cref="ITsStatement"/>.</returns>
        internal static ITsStatement ParseStatement(string code, bool isStrictMode = true)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens, isStrictMode);
            return parser.ParseStatement();
        }

        /// <summary>
        /// Parses a TypeScript declaration.
        /// </summary>
        /// <param name="code">The code to parse</param>
        /// <param name="isStrictMode">Indicates whether the parse should be done in strict mode.</param>
        /// <returns>An <see cref="ITsDeclaration"/>.</returns>
        internal static ITsDeclaration ParseDeclaration(string code, bool isStrictMode = true)
        {
            ImmutableArray<TsToken> tokens = TsLexer.Lex(code);
            var parser = new TsParser(tokens, isStrictMode);
            return parser.ParseDeclaration();
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
            if (_reader.ReadIf(TsTokenCode.Identifier, out TsToken? identifierToken))
            {
                return Factory.Identifier(identifierToken.Value.ToString());
            }

            if (_reader.ReadIf(TsTokenCode.StringLiteral, out TsToken? stringLiteralToken))
            {
                return ToStringLiteral(stringLiteralToken);
            }

            if (_reader.ReadIf(IsNumericLiteral, out TsToken? numericLiteralToken))
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
        private ITsIdentifier ParseIdentifierReference()
        {
            return ParseIdentifier(isTypeDeclaration: false);
        }

        /// <summary>
        /// Returns a value indicating whether the specified token code is a valid start of an Identifier, which is
        /// defined as an IdentifierName but not ReservedWord.
        /// </summary>
        /// <param name="tokenCode">The token code to check.</param>
        /// <param name="isTypeDeclaration">
        /// Indicates whether the identifier will be a user-defined type name (class, interface, etc.) since some
        /// keywords are not valid as type name identifiers ('boolean', 'number', etc.).
        /// </param>
        /// <returns>True if the token code is a valid identifier start token; false otherwise.</returns>
        private bool IsStartOfIdentifier(TsTokenCode tokenCode, bool isTypeDeclaration)
        {
            return tokenCode == TsTokenCode.Identifier ||
                (tokenCode.IsKeyword() && tokenCode.IsKeywordAllowedAsIdentifier(isTypeDeclaration, _isStrictMode));
        }

        /// <summary>
        /// Tries to parse an identifier or a keyword that can be made into an identifier.
        /// </summary>
        /// <param name="isTypeDeclaration">
        /// Indicates whether the identifier will be a user-defined type name (class, interface, etc.) since some
        /// keywords are not valid as type name identifiers ('boolean', 'number', etc.).
        /// </param>
        /// <param name="identifier">The parsed identifier (or keyword as identifier) if successful; otherwise, null.</param>
        /// <returns>True if the next token was successfully parsed as an identifier; false otherwise.</returns>
        private bool TryParseIdentifier(bool isTypeDeclaration, [NotNullWhen(true)] out ITsIdentifier? identifier)
        {
            TsToken identifierToken;
            TsTokenCode tokenCode = _reader.Peek().TokenCode;

            // Keywords can be identifiers as long as they're not reserved words.
            if (tokenCode.IsKeyword() && tokenCode.IsKeywordAllowedAsIdentifier(isTypeDeclaration, _isStrictMode))
            {
                TsToken keywordAsIdentifierToken = _reader.Read();
                identifierToken = TsToken.Identifier(
                    keywordAsIdentifierToken.Text,
                    keywordAsIdentifierToken.Text,
                    keywordAsIdentifierToken.Location);
            }
            else if (_reader.IsNext(TsTokenCode.Identifier))
            {
                identifierToken = Read(TsTokenCode.Identifier);
            }
            else
            {
                identifier = null;
                return false;
            }

            identifier = Factory.Identifier(identifierToken.Value.ToString());
            return true;
        }

        /// <summary>
        /// Parses either an identifier or a keyword that will be interpreted as an identifier.
        /// </summary>
        /// <param name="isTypeDeclaration">
        /// Indicates whether the identifier will be a user-defined type name (class, interface, etc.) since some
        /// keywords are not valid as type name identifiers ('boolean', 'number', etc.).
        /// </param>
        private ITsIdentifier ParseIdentifier(bool isTypeDeclaration)
        {
            if (TryParseIdentifier(isTypeDeclaration, out ITsIdentifier? identifier))
            {
                return identifier;
            }

            throw NewParseException(
                $"Expected an identifier or keyword as the next token, but saw '{_reader.Peek().TokenCode}'");
        }

        private TsToken Read(TsTokenCode expectedTokenCode)
        {
            if (!_reader.IsNext(expectedTokenCode))
            {
                throw NewParseException($"Expected '{expectedTokenCode}' as the next token");
            }

            return _reader.Read();
        }

        /// <summary>
        /// Attempts to parse productions without throwing exceptions. If there are failures, false
        /// is returned and the reader is at the same state as before this function was called. If
        /// the parsing succeeded, the reader is at the next token after the parse.
        /// </summary>
        /// <typeparam name="T">The type of parsed TypeScript AST node.</typeparam>
        /// <param name="func">The function to run inside of a saved state context.</param>
        /// <param name="result">
        /// If the parse is successful, the result of the tried parse; otherwise, <c>default(T)</c>.
        /// </param>
        /// <returns>True if the parse succeeded; false otherwise.</returns>
        public bool TryParse<T>(Func<T> func, [NotNullWhen(true)] out T? result) where T : class, ITsAstNode
        {
            try
            {
                result = _reader.ReadWithSavedState(func, shouldCommitReadFunc: x => x != null);
                return result != null;
            }
            catch (TsParserException)
            {
                result = default;
                return false;
            }
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
