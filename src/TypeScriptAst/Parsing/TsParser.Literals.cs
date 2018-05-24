// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Literals.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Parsing
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using CompilerUtilities.Extensions;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParser
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
        ///     IdentifierReference                             (starts with Identifier)
        ///     CoverInitializedName                            (starts with Identifier)
        ///     PropertyName : AssignmentExpression             (starts with Identifier, StringLiteral, or NumberLiteral)
        ///     PropertyName CallSignature { FunctionBody }     (starts with Identifier, StringLiteral, or NumberLiteral)
        ///     GetAccessor                                     (starts with 'get')
        ///     SetAccessor                                     (starts with 'set')
        ///
        /// CoverInitializedName:
        ///     IdentifierReference Initializer
        /// ]]></code></remarks>
        private ITsObjectLiteral ParseObjectLiteral()
        {
            var propertyDefinitions = new List<ITsPropertyDefinition>();

            Read(TsTokenCode.LeftBrace);

            // read each property definition
            while (!_reader.IsNext(TsTokenCode.RightBrace))
            {
                ITsPropertyDefinition propertyDefinition;

                // GetAccessor
                if (_reader.IsNext(TsTokenCode.Get))
                {
                    propertyDefinition = ParseGetAccessor();
                }
                // SetAccessor
                else if (_reader.IsNext(TsTokenCode.Set))
                {
                    propertyDefinition = ParseSetAccessor();
                }
                else
                {
                    ITsPropertyName propertyName = ParsePropertyName();
                    ITsIdentifier identifier = propertyName as ITsIdentifier;

                    // PropertyName : AssignmentExpression
                    if (_reader.ReadIf(TsTokenCode.Colon))
                    {
                        ITsExpression initializer = ParseAssignmentExpression();
                        propertyDefinition = Factory.PropertyAssignment(propertyName, initializer);
                    }

                    // CoverInitializedName
                    else if (identifier != null && _reader.IsNext(TsTokenCode.Equals))
                    {
                        ITsExpression initializer = ParseInitializer();
                        propertyDefinition = Factory.CoverInitializedName(identifier, initializer);
                    }

                    // PropertyName CallSignature { FunctionBody }
                    else if (TryParse(ParseCallSignature, out ITsCallSignature callSignature))
                    {
                        ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);
                        propertyDefinition = Factory.PropertyFunction(propertyName, callSignature, functionBody);
                    }

                    // IdentifierReference
                    else if (identifier != null)
                    {
                        propertyDefinition = identifier;
                    }
                    else
                    {
                        throw NewParseException($"Unknown token in ParseObjectLiteral: {_reader.Peek()}");
                    }
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
        private ITsExpression ParseInitializer()
        {
            Read(TsTokenCode.Equals);
            return ParseAssignmentExpression();
        }

        private ITsExpression ParseOptionalInitializer()
        {
            if (_reader.IsNext(TsTokenCode.Equals))
            {
                return ParseInitializer();
            }

            return null;
        }

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

            ITsType propertyType = ParseOptionalTypeAnnotation();
            ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);

            return Factory.GetAccessor(propertyName, propertyType, functionBody);
        }

        /// <summary>
        /// Parses a set accessor of the form 'set name(value: string) { body }'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// SetAccessor:
        ///     set PropertyName ( BindingIdentifierOrPattern TypeAnnotationOpt ) { FunctionBody }
        /// ]]></code></remarks>
        private ITsSetAccessor ParseSetAccessor()
        {
            Read(TsTokenCode.Set);
            ITsPropertyName propertyName = ParsePropertyName();

            Read(TsTokenCode.LeftParen);
            ITsBindingIdentifierOrPattern parameterName = ParseBindingIdentifierOrPattern();
            ITsType parameterType = ParseOptionalTypeAnnotation();
            Read(TsTokenCode.RightParen);

            ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);

            return Factory.SetAccessor(propertyName, parameterName, parameterType, functionBody);
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
