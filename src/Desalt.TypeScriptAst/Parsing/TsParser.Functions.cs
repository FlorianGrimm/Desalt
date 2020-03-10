// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Functions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System.Collections.Generic;
    using Desalt.TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParser
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
            Read(TsTokenCode.Function);
            TryParseIdentifier(out ITsIdentifier functionName);
            ITsCallSignature callSignature = ParseCallSignature();
            ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);

            return Factory.FunctionExpression(callSignature, functionName, functionBody);
        }

        /// <summary>
        /// Tries to parse an arrow function. If successful, the arrow function is returned and the
        /// reader is ready for the next production. If not successful, the reader is preserved in
        /// the same location as before the function was called.
        /// </summary>
        /// <returns>true if the next production was an ArrowFunction; otherwise, false.</returns>
        /// <remarks><code><![CDATA[
        /// ArrowFunction:
        ///   ArrowParameters [no LineTerminator here] => ConciseBody
        ///
        /// ArrowParameters:
        ///   BindingIdentifier
        ///   CoverParenthesizedExpressionAndArrowParameterList
        ///
        /// ConciseBody:
        ///   [lookahead != { ] AssignmentExpression
        ///   { FunctionBody }
        ///
        /// When the production `ArrowParameters: CoverParenthesizedExpressionAndArrowParameterList`
        /// is recognized the following grammar is used to refine the interpretation of
        /// CoverParenthesizedExpressionAndArrowParameterList:
        ///
        /// ArrowFormalParameters: ( Modified )
        ///   CallSignature
        /// ]]></code></remarks>
        private ITsArrowFunction ParseArrowFunction()
        {
            ITsExpression bodyExpression;

            // try to parse 'param =>' arrow functions
            if (TryParseIdentifier(out ITsIdentifier singleParameterName))
            {
                Read(TsTokenCode.EqualsGreaterThan);
                if (_reader.IsNext(TsTokenCode.LeftBrace))
                {
                    ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);
                    return Factory.ArrowFunction(singleParameterName, functionBody);
                }

                bodyExpression = ParseAssignmentExpression();
                return Factory.ArrowFunction(singleParameterName, bodyExpression);
            }

            // parse the call signature
            ITsCallSignature callSignature = ParseCallSignature();
            Read(TsTokenCode.EqualsGreaterThan);

            if (_reader.IsNext(TsTokenCode.LeftBrace))
            {
                ITsStatementListItem[] functionBody = ParseFunctionBody(withBraces: true);
                return Factory.ArrowFunction(callSignature, functionBody);
            }

            bodyExpression = ParseAssignmentExpression();
            return Factory.ArrowFunction(callSignature, bodyExpression);
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
            ITsRestParameter restParameter = null;

            // RequiredParameterList or OptionalParameterList
            do
            {
                // RestParameter
                if (_reader.IsNext(TsTokenCode.DotDotDot))
                {
                    restParameter = ParseRestParameter();
                    break;
                }

                bool parsingOptionalParameter = false;
                ITsStringLiteral stringLiteral = null;
                ITsType parameterType = null;

                // RequiredParameter: AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt
                // OptionalParameter: AccessibilityModifierOpt BindingIdentifierOrPattern ? TypeAnnotationOpt
                TsAccessibilityModifier? accessibilityModifier = ParseOptionalAccessibilityModifier();
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
                    parameterType = ParseOptionalTypeAnnotation();
                }

                // OptionalParameter: AccessibilityModifierOpt BindingIdentifierOrPattern TypeAnnotationOpt Initializer
                ITsExpression initializer = null;
                if (_reader.IsNext(TsTokenCode.Equals))
                {
                    initializer = ParseInitializer();
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
        private TsAccessibilityModifier? ParseOptionalAccessibilityModifier()
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
        private ITsRestParameter ParseRestParameter()
        {
            Read(TsTokenCode.DotDotDot);
            ITsIdentifier parameterName = ParseIdentifier();
            ITsType parameterType = ParseOptionalTypeAnnotation();

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
            if (TryParseIdentifier(out ITsIdentifier identifier))
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
            if (_reader.IsNext(TsTokenCode.LeftBrace))
            {
                return ParseObjectBindingPattern();
            }

            return ParseArrayBindingPattern();
        }

        /// <summary>
        /// Parses an object binding pattern.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ObjectBindingPattern:
        ///     { }
        ///     { BindingPropertyList }
        ///     { BindingPropertyList , }
        ///
        /// BindingPropertyList:
        ///     BindingProperty
        ///     BindingPropertyList , BindingProperty
        /// ]]></code></remarks>
        private ITsObjectBindingPattern ParseObjectBindingPattern()
        {
            Read(TsTokenCode.LeftBrace);

            var properties = new List<ITsBindingProperty>();
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.RightBrace))
            {
                ITsBindingProperty property = ParseBindingProperty();
                properties.Add(property);

                _reader.ReadIf(TsTokenCode.Comma);
            }

            Read(TsTokenCode.RightBrace);
            return Factory.ObjectBindingPattern(properties.ToArray());
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
            Read(TsTokenCode.LeftBracket);

            var elements = new List<ITsBindingElement>();
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.RightBracket))
            {
                // read all of the elisons (empty elements)
                while (_reader.ReadIf(TsTokenCode.Comma))
                {
                    elements.Add(null);
                }

                // check for the rest element
                if (_reader.IsNext(TsTokenCode.DotDotDot))
                {
                    break;
                }

                // we could be at the end after reading the empty elements
                if (_reader.IsNext(TsTokenCode.RightBracket))
                {
                    break;
                }

                ITsBindingElement element = ParseBindingElement();
                elements.Add(element);

                _reader.ReadIf(TsTokenCode.Comma);
            }

            ITsIdentifier restElement = null;
            if (_reader.ReadIf(TsTokenCode.DotDotDot))
            {
                restElement = ParseIdentifier();
            }

            Read(TsTokenCode.RightBracket);
            return Factory.ArrayBindingPattern(elements, restElement);
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
            ITsPropertyName propertyName = ParsePropertyName();

            if (propertyName is ITsIdentifier name && !_reader.IsNext(TsTokenCode.Colon))
            {
                ITsExpression defaultValue = ParseOptionalInitializer();
                return Factory.SingleNameBinding(name, defaultValue);
            }

            Read(TsTokenCode.Colon);
            ITsBindingElement bindingElement = ParseBindingElement();
            return Factory.PropertyNameBinding(propertyName, bindingElement);
        }

        /// <summary>
        /// Parses a binding element.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BindingElement:
        ///     SingleNameBinding
        ///     BindingPattern InitializerOpt
        ///
        /// SingleNameBinding:
        ///     BindingIdentifier InitializerOpt
        /// ]]></code></remarks>
        private ITsBindingElement ParseBindingElement()
        {
            if (_reader.IsNext(TsTokenCode.LeftBrace) || _reader.IsNext(TsTokenCode.LeftBracket))
            {
                ITsBindingPattern bindingPattern = ParseBindingPattern();
                ITsExpression initializer = ParseOptionalInitializer();
                return Factory.PatternBinding(bindingPattern, initializer);
            }

            ITsIdentifier name = ParseIdentifier();
            ITsExpression defaultValue = ParseOptionalInitializer();
            return Factory.SingleNameBinding(name, defaultValue);
        }
    }
}
