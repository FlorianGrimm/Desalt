// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Literals.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal partial class TsParser
    {
        private static bool IsNumericLiteral(TsTokenCode tokenCode) =>
            tokenCode.IsOneOf(
                TsTokenCode.DecimalLiteral,
                TsTokenCode.BinaryIntegerLiteral,
                TsTokenCode.OctalIntegerLiteral,
                TsTokenCode.HexIntegerLiteral);

        /// <summary>
        /// Parses a literal.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Literal:
        ///     NullLiteral
        ///     BooleanLiteral
        ///     NumericLiteral
        ///     StringLiteral
        ///
        /// NullLiteral: null
        /// BooleanLiteral: true | false
        /// NumericLiteral:
        ///     DecimalLiteral
        ///     BinaryIntegerLiteral
        ///     OctalIntegerLiteral
        ///     HexIntegerLiteral
        ///
        /// StringLiteral:
        ///     " DoubleStringCharacters "
        ///     ' SingleStringCharacters '
        /// ]]></code></remarks>
        private ITsExpression ParseLiteral()
        {
            TsToken token = _reader.Read();

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (token.TokenCode)
            {
                case TsTokenCode.Null:
                    return Factory.Null;

                case TsTokenCode.True:
                    return Factory.True;

                case TsTokenCode.False:
                    return Factory.False;

                case TsTokenCode.DecimalLiteral:
                case TsTokenCode.BinaryIntegerLiteral:
                case TsTokenCode.OctalIntegerLiteral:
                case TsTokenCode.HexIntegerLiteral:
                    return Factory.Number((double)token.Value);

                case TsTokenCode.StringLiteral:
                    return ToStringLiteral(token);

                default:
                    throw NewParseException($"Unknown token inside of ParseLiteral: {token}");
            }
        }

        private static ITsStringLiteral ToStringLiteral(TsToken stringLiteralToken)
        {
            Debug.Assert(stringLiteralToken.TokenCode == TsTokenCode.StringLiteral);

            StringLiteralQuoteKind quoteKind = stringLiteralToken.Text[0] == '\''
                ? StringLiteralQuoteKind.SingleQuote
                : StringLiteralQuoteKind.DoubleQuote;

            return Factory.String((string)stringLiteralToken.Value, quoteKind);
        }

        /// <summary>
        /// Parses an array literal of the form '[ elements ]'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ArrayLiteral:
        ///     [ ElisionOpt ]
        ///     [ ElementList ]
        ///     [ ElementList, ElisionOpt ]
        ///
        /// ElementList:
        ///     ElisionOpt AssignmentExpression
        ///     ElisionOpt SpreadElement
        ///     ElementList , ElisionOpt AssignmentExpression
        ///     ElementList , ElisionOpt SpreadElement
        ///
        /// Elision:
        ///     ,
        ///     Elision ,
        ///
        /// SpreadElement:
        ///     ... AssignmentExpression
        /// ]]></code></remarks>
        private ITsArrayLiteral ParseArrayLiteral()
        {
            var elements = new List<ITsArrayElement>();

            Read(TsTokenCode.LeftBracket);

            while (!_reader.IsNext(TsTokenCode.RightBracket))
            {
                while (_reader.ReadIf(TsTokenCode.Comma))
                {
                    // read all of the consecutive commas
                }

                // it's possible to have a bunch of commas with no contents
                if (_reader.IsNext(TsTokenCode.RightBracket))
                {
                    break;
                }

                // check for a spread element (...expression)
                if (_reader.ReadIf(TsTokenCode.DotDotDot))
                {
                    ITsExpression element = ParseAssignmentExpression();
                    elements.Add(Factory.ArrayElement(element, isSpreadElement: true));
                }
                else
                {
                    ITsExpression element = ParseAssignmentExpression();
                    elements.Add(Factory.ArrayElement(element));
                }
            }

            Read(TsTokenCode.RightBracket);

            return Factory.Array(elements.ToArray());
        }

        /// <summary>
        /// Parses an object literal of the form '{ elements }'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ObjectLiteral:
        ///     { }
        ///     { PropertyDefinition }
        ///     { PropertyDefinitionList , PropertyDefinition }
        ///
        /// PropertyDefinitionList:
        ///     PropertyDefinition
        ///     PropertyDefinitionList , PropertyDefinition
        ///
        /// PropertyDefinition:
        ///     IdentifierReference
        ///     CoverInitializedName
        ///     PropertyName : AssignmentExpression
        ///     PropertyName CallSignature { FunctionBody }
        ///     GetAccessor
        ///     SetAccessor
        ///
        /// CoverInitializedName:
        ///     IdentifierReference Initializer
        /// ]]></code></remarks>
        private ITsObjectLiteral ParseObjectLiteral()
        {
            var propertyDefinitions = new List<ITsPropertyDefinition>();

            Read(TsTokenCode.LeftBrace);

            while (!_reader.IsNext(TsTokenCode.RightBrace))
            {
                ITsPropertyDefinition propertyDefinition =
                    TryParseGetAccessor() ?? (ITsPropertyDefinition)TryParseSetAccessor();

                if (propertyDefinition == null)
                {
                    ITsPropertyName propertyName = ParsePropertyName();
                    ITsIdentifier identifier = propertyName as ITsIdentifier;

                    // PropertyName : AssignmentExpression
                    if (_reader.ReadIf(TsTokenCode.Colon))
                    {
                        ITsExpression initializer = ParseAssignmentExpression();
                        propertyDefinition = Factory.PropertyAssignment(propertyName, initializer);
                    }

                    // PropertyName CallSignature { FunctionBody }
                    else if (TryParseCallSignature(out ITsCallSignature callSignature))
                    {
                        ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);
                        propertyDefinition = Factory.PropertyFunction(propertyName, callSignature, functionBody);
                    }

                    // CoverInitializedName
                    else if (identifier != null && TryParseInitializer(out ITsExpression initializer))
                    {
                        propertyDefinition = Factory.CoverInitializedName(identifier, initializer);
                    }

                    // IdentifierReference
                    else if (identifier != null)
                    {
                        propertyDefinition = identifier;
                    }
                }

                if (propertyDefinition == null)
                {
                    throw NewParseException($"Unknown token in ParseObjectLiteral: {_reader.Peek()}");
                }

                propertyDefinitions.Add(propertyDefinition);

                _reader.ReadIf(TsTokenCode.Comma);
            }

            Read(TsTokenCode.RightBrace);

            return Factory.Object(propertyDefinitions.ToArray());
        }

        /// <summary>
        /// Parses an initializer.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Initializer:
        ///     = AssignmentExpression
        /// ]]></code></remarks>
        private ITsExpression TryParseInitializer() =>
            _reader.ReadIf(TsTokenCode.Equals) ? ParseAssignmentExpression() : null;

        private bool TryParseInitializer(out ITsExpression initializer)
        {
            initializer = TryParseInitializer();
            return initializer != null;
        }

        private ITsGetAccessor TryParseGetAccessor() =>
            _reader.Peek().TokenCode == TsTokenCode.Get ? ParseGetAccessor() : null;

        /// <summary>
        /// Parses a get accessor of the form 'get name(): string { body }'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// GetAccessor:
        ///     get PropertyName() TypeAnnotationOpt { FunctionBody }
        /// ]]></code></remarks>
        private ITsGetAccessor ParseGetAccessor()
        {
            Read(TsTokenCode.Get);

            ITsPropertyName propertyName = ParsePropertyName();
            Read(TsTokenCode.LeftParen);
            Read(TsTokenCode.RightParen);

            ITsType propertyType = TryParseTypeAnnotation();
            ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);

            return Factory.GetAccessor(propertyName, propertyType, functionBody);
        }

        private ITsSetAccessor TryParseSetAccessor()
        {
            return _reader.Peek().TokenCode == TsTokenCode.Set ? ParseSetAccessor() : null;
        }

        /// <summary>
        /// Parses a set accessor of the form 'set name(value: string) { body }'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// SetAccessor:
        ///     set PropertyName(BindingIdentifierOrPattern TypeAnnotationOpt) { FunctionBody }
        /// ]]></code></remarks>
        private ITsSetAccessor ParseSetAccessor()
        {
            Read(TsTokenCode.Set);
            return null;
        }

        /// <summary>
        /// Parses a regular expression literal.
        /// </summary>
        private ITsRegularExpressionLiteral ParseRegularExpressionLiteral()
        {
            throw NotYetImplementedException("Regular expression literals are not yet supported");
        }

        /// <summary>
        /// Parses a template literal.
        /// </summary>
        private ITsExpression ParseTemplateLiteral()
        {
            throw NotYetImplementedException("Template literals are not yet supported");
        }
    }
}
