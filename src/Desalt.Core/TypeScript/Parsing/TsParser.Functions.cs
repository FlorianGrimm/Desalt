// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Functions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal partial class TsParser
    {
        /// <summary>
        /// Parses a function expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// FunctionExpression: ( Modified )
        ///     function BindingIdentifierOpt CallSignature { FunctionBody }
        /// ]]></code></remarks>
        private ITsFunctionExpression ParseFunctionExpression()
        {
            throw NotYetImplementedException("Function expressions are not yet implemented");
        }

        /// <summary>
        /// Parses a function body, which is a bunch of statements.
        /// </summary>
        /// <param name="withBraces">Indicates whether to read the beginning and ending braces.</param>
        /// <remarks><code><![CDATA[
        /// FunctionBody:
        ///     FunctionStatementList
        ///
        /// FunctionStatementList:
        ///     StatementListOpt
        /// ]]></code></remarks>
        private ITsStatementListItem[] ParseFunctionBody(bool withBraces)
        {
            if (withBraces)
            {
                Read(TsTokenCode.LeftBrace);
                if (_reader.ReadIf(TsTokenCode.RightBrace))
                {
                    return new ITsStatementListItem[0];
                }
            }

            var statements = ParseStatementList();

            if (withBraces)
            {
                Read(TsTokenCode.RightBrace);
            }

            return statements;
        }

        /// <summary>
        /// Parses a ( ParameterListOpt ) production.
        /// </summary>
        private ITsParameterList ParseOptionalParameterListWithParens()
        {
            Read(TsTokenCode.LeftParen);

            ITsParameterList parameterList = Factory.ParameterList();
            if (!_reader.IsNext(TsTokenCode.RightParen))
            {
                parameterList = ParseParameterList();
            }

            Read(TsTokenCode.RightParen);

            return parameterList;
        }

        /// <summary>
        /// Parses a parameter list.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ParameterList:
        ///     RequiredParameterList
        ///     OptionalParameterList
        ///     RestParameter
        ///     RequiredParameterList , OptionalParameterList
        ///     RequiredParameterList , RestParameter
        ///     OptionalParameterList , RestParameter
        ///     RequiredParameterList , OptionalParameterList , RestParameter
        ///
        /// RequiredParameterList:
        ///     RequiredParameter
        ///     RequiredParameterList , RequiredParameter
        ///
        /// RequiredParameter:
        ///     AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt
        ///     BindingIdentifier : StringLiteral
        ///
        /// OptionalParameterList:
        ///     OptionalParameter
        ///     OptionalParameterList , OptionalParameter
        ///
        /// OptionalParameter:
        ///     AccessibilityModifierOpt BindingIdentifierOrPattern ? TypeAnnotationOpt
        ///     AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt Initializer
        ///     BindingIdentifier ? : StringLiteral
        /// ]]></code></remarks>
        private ITsParameterList ParseParameterList()
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            var optionalParameters = new List<ITsOptionalParameter>();
            ITsRestParameter restParameter;

            // RequiredParameterList or OptionalParameterList
            do
            {
                // RestParameter
                restParameter = TryParseRestParameter();
                if (restParameter != null)
                {
                    break;
                }

                bool parsingOptionalParameter = false;
                ITsStringLiteral stringLiteral = null;
                ITsType parameterType = null;

                // RequiredParameter: AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt
                // OptionalParameter: AccessibilityModifierOpt BindingIdentifierOrPattern ? TypeAnnotationOpt
                TsAccessibilityModifier? accessibilityModifier = TryParseAccessibilityModifier();
                ITsBindingIdentifierOrPattern parameterName = ParseBindingIdentifierOrPattern();

                if (_reader.ReadIf(TsTokenCode.Question))
                {
                    parsingOptionalParameter = true;
                }

                // see if we have a TypeAnnotation or BindingIdentifier : StringLiteral
                if (_reader.IsNext(TsTokenCode.Colon, TsTokenCode.StringLiteral))
                {
                    Read(TsTokenCode.Colon);
                    TsToken stringLiteralToken = _reader.Read();
                    stringLiteral = ToStringLiteral(stringLiteralToken);
                }
                else
                {
                    parameterType = TryParseTypeAnnotation();
                }

                // OptionalParameter: AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt Initializer
                ITsExpression initializer = TryParseInitializer();
                if (initializer != null)
                {
                    parsingOptionalParameter = true;
                }

                if (optionalParameters.Count > 0 && !parsingOptionalParameter)
                {
                    throw NewParseException("Cannot have required parameters after optional parameters");
                }

                ITsIdentifier parameterIdentifier = parameterName as ITsIdentifier;
                if (stringLiteral != null && parameterIdentifier == null)
                {
                    throw NewParseException("A string literal parameter must have an identifier");
                }

                if (parsingOptionalParameter)
                {
                    ITsOptionalParameter optionalParameter;

                    if (stringLiteral != null)
                    {
                        optionalParameter = Factory.StringOptionalParameter(parameterIdentifier, stringLiteral);
                    }
                    else
                    {
                        optionalParameter = Factory.BoundOptionalParameter(
                            parameterName,
                            parameterType,
                            initializer,
                            accessibilityModifier);
                    }

                    optionalParameters.Add(optionalParameter);
                }
                else
                {
                    ITsRequiredParameter requiredParameter;

                    if (stringLiteral != null)
                    {
                        requiredParameter = Factory.StringRequiredParameter(parameterIdentifier, stringLiteral);
                    }
                    else
                    {
                        requiredParameter = Factory.BoundRequiredParameter(
                            parameterName,
                            parameterType,
                            accessibilityModifier);
                    }
                    requiredParameters.Add(requiredParameter);
                }
            }
            while (_reader.ReadIf(TsTokenCode.Comma));

            return Factory.ParameterList(requiredParameters, optionalParameters, restParameter);
        }

        /// <summary>
        /// Parses an accessibility modifier.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// AccessibilityModifier:
        ///     public
        ///     private
        ///     protected
        /// ]]></code></remarks>
        private TsAccessibilityModifier? TryParseAccessibilityModifier()
        {
            if (_reader.ReadIf(TsTokenCode.Public))
            {
                return TsAccessibilityModifier.Public;
            }

            if (_reader.ReadIf(TsTokenCode.Private))
            {
                return TsAccessibilityModifier.Private;
            }

            if (_reader.ReadIf(TsTokenCode.Protected))
            {
                return TsAccessibilityModifier.Protected;
            }

            return null;
        }

        /// <summary>
        /// Parses a rest parameter of the form '... id'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// RestParameter:
        ///     ... BindingIdentifier TypeAnnotationOpt
        /// ]]></code></remarks>
        private ITsRestParameter TryParseRestParameter()
        {
            if (!_reader.ReadIf(TsTokenCode.DotDotDot))
            {
                return null;
            }

            ITsIdentifier parameterName = ParseIdentifier();
            ITsType parameterType = TryParseTypeAnnotation();

            return Factory.RestParameter(parameterName, parameterType);
        }

        /// <summary>
        /// Parses a binding identifier or pattern.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BindingIdentifierOrPattern:
        ///     BindingIdentifier
        ///     BindingPattern
        /// ]]></code></remarks>
        private ITsBindingIdentifierOrPattern ParseBindingIdentifierOrPattern()
        {
            ITsIdentifier identifier = TryParseIdentifier();
            if (identifier != null)
            {
                return identifier;
            }

            return ParseBindingPattern();
        }

        /// <summary>
        /// Parses a binding pattern.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BindingPattern:
        ///     ObjectBindingPattern
        ///     ArrayBindingPattern
        /// ]]></code></remarks>
        private ITsBindingPattern ParseBindingPattern()
        {
            return null;
        }

        /// <summary>
        /// Parses an object binding pattern.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ObjectBindingPattern:
        ///     { }
        ///     { BindingPropertyList }
        ///     { BindingPropertyList , }
        /// ]]></code></remarks>
        private ITsObjectBindingPattern ParseObjectBindingPattern()
        {
            return null;
        }

        /// <summary>
        /// Parses an array binding pattern.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ArrayBindingPattern:
        ///     [ ElisionOpt BindingRestElementOpt ]
        ///     [ BindingElementList ]
        ///     [ BindingElementList , ElisionOpt BindingRestElementOpt ]
        ///
        /// BindingPropertyList:
        ///     BindingProperty
        ///     BindingPropertyList , BindingProperty
        ///
        /// BindingElementList:
        ///     BindingElisionElement
        ///     BindingElementList , BindingElisionElement
        ///
        /// BindingElisionElement:
        ///     ElisionOpt BindingElement
        ///
        /// BindingRestElement:
        ///     ... BindingIdentifier
        /// ]]></code></remarks>
        private ITsArrayBindingPattern ParseArrayBindingPattern()
        {
            return null;
        }

        /// <summary>
        /// Parses a binding property.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BindingProperty:
        ///     SingleNameBinding
        ///     PropertyName : BindingElement
        ///
        /// BindingElement:
        ///     SingleNameBinding
        ///     BindingPattern InitializerOpt
        ///
        /// SingleNameBinding:
        ///     BindingIdentifier InitializerOpt
        /// ]]></code></remarks>
        private ITsBindingProperty ParseBindingProperty()
        {
            return null;
        }
    }
}
