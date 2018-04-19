// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal partial class TsParser
    {
        /// <summary>
        /// Parses a type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Type:
        ///     UnionOrIntersectionOrPrimaryType
        ///     FunctionType                        (starts with '<' or '(')
        ///     ConstructorType                     (starts with 'new')
        /// ]]></code></remarks>
        private ITsType ParseType()
        {
            ITsType ctorType = TryParseConstructorType();
            if (ctorType != null)
            {
                return ctorType;
            }

            if (IsStartOfFunctionType())
            {
                return ParseFunctionType();
            }

            return ParseUnionOrIntersectionOrPrimaryType();
        }

        /// <summary>
        /// Parses a union, intersection, or primary type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// UnionOrIntersectionOrPrimaryType:
        ///     UnionType
        ///     IntersectionOrPrimaryType
        ///
        /// UnionType:
        ///     UnionOrIntersectionOrPrimaryType | IntersectionOrPrimaryType
        ///
        /// IntersectionOrPrimaryType:
        ///     IntersectionType
        ///     PrimaryType
        ///
        /// IntersectionType:
        ///     IntersectionOrPrimaryType & PrimaryType
        /// ]]></code></remarks>
        private ITsType ParseUnionOrIntersectionOrPrimaryType()
        {
            ITsType type = ParsePrimaryType();
            if (_reader.IsNext(TsTokenCode.Ampersand) || _reader.IsNext(TsTokenCode.Pipe))
            {
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                ITsType rightSide = ParseUnionOrIntersectionOrPrimaryType();

                if (tokenCode == TsTokenCode.Ampersand)
                {
                    type = Factory.IntersectionType(type, rightSide);
                }
                else
                {
                    type = Factory.UnionType(type, rightSide);
                }
            }

            return type;
        }

        private bool IsStartOfFunctionType()
        {
            if (_reader.IsNext(TsTokenCode.LessThan))
            {
                return true;
            }

            return _reader.ReadWithSavedState(IsUnambiguouslyStartOfFunctionType);
        }

        private bool IsUnambiguouslyStartOfFunctionType()
        {
            if (!_reader.ReadIf(TsTokenCode.LeftParen))
            {
                return false;
            }

            if (_reader.IsNext(TsTokenCode.RightParen) || _reader.IsNext(TsTokenCode.DotDotDot))
            {
                // ( )
                // ( ...
                return true;
            }

            if (SkipParameterStart())
            {
                // We successfully skipped modifiers (if any) and an identifier or binding pattern,
                // now see if we have something that indicates a parameter declaration.
                if (_reader.Peek()
                    .TokenCode.IsOneOf(TsTokenCode.Colon, TsTokenCode.Comma, TsTokenCode.Question, TsTokenCode.Equals))
                {
                    // ( xxx :
                    // ( xxx ,
                    // ( xxx ?
                    // ( xxx =
                    return true;
                }

                // ( xxx ) =>
                if (_reader.IsNext(TsTokenCode.RightParen, TsTokenCode.EqualsGreaterThan))
                {
                    return true;
                }
            }

            return false;
        }

        private bool SkipParameterStart()
        {
            // skip modifiers
            TryParseAccessibilityModifier();

            // return true if we can parse an array or object binding pattern with no errors
            try
            {
                ParseBindingIdentifierOrPattern();
                return true;
            }
            catch (TsParserException)
            {
                return false;
            }
        }

        /// <summary>
        /// Parses a function type of the form, '&lt;T&gt;(x, y) => Type'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// FunctionType:
        ///     TypeParametersOpt ( ParameterListOpt ) => Type
        /// ]]></code></remarks>
        private ITsFunctionType ParseFunctionType()
        {
            ITsTypeParameters typeParameters = TryParseTypeParameters();
            if (typeParameters == null && !_reader.IsNext(TsTokenCode.LeftParen))
            {
                return null;
            }

            ITsParameterList parameters = ParseOptionalParameterListWithParens();
            Read(TsTokenCode.EqualsGreaterThan);
            ITsType returnType = ParseType();

            return Factory.FunctionType(typeParameters, parameters, returnType);
        }

        /// <summary>
        /// Parses a constructor type of the form, 'new &lt;T&gt;(x, y) => Type'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ConstructorType:
        ///     new TypeParametersOpt ( ParameterListOpt ) => Type
        /// ]]></code></remarks>
        private ITsConstructorType TryParseConstructorType()
        {
            if (!_reader.ReadIf(TsTokenCode.New))
            {
                return null;
            }

            ITsTypeParameters typeParameters = TryParseTypeParameters();
            ITsParameterList parameters = ParseOptionalParameterListWithParens();
            Read(TsTokenCode.EqualsGreaterThan);
            ITsType returnType = ParseType();

            return Factory.ConstructorType(typeParameters, parameters, returnType);
        }

        /// <summary>
        /// Parses a primary type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// PrimaryType:
        ///     PredefinedType      (starts with keyword)
        ///     TypeReference       (starts with identifier)
        ///     ParenthesizedType   (starts with '(')
        ///     ObjectType          (starts with '{')
        ///     ArrayType           (starts recursively)
        ///     TupleType           (starts with '[')
        ///     TypeQuery           (starts with 'typeof')
        ///     ThisType            (starts with 'this')
        ///
        /// ParenthesizedType:
        ///     ( Type )
        ///
        /// ArrayType:
        ///     PrimaryType [no LineTerminator here] [ ]
        ///
        /// ThisType:
        ///     this
        /// ]]></code></remarks>
        private ITsType ParsePrimaryType()
        {
            ITsType type = TryParsePredefinedType() ??
                TryParseObjectType() ?? TryParseTupleType() ?? (ITsType)TryParseTypeQuery();

            // ParenthesizedType
            if (type == null && _reader.ReadIf(TsTokenCode.LeftParen))
            {
                type = ParseType();
                Read(TsTokenCode.RightParen);
            }

            // ThisType
            if (type == null && _reader.ReadIf(TsTokenCode.This))
            {
                type = Factory.ThisType;
            }

            // TypeReference and ArrayType
            if (type == null)
            {
                type = ParseTypeReference();
                if (_reader.ReadIf(TsTokenCode.LeftBracket))
                {
                    type = Factory.ArrayType(type);
                    Read(TsTokenCode.RightBracket);
                }
            }

            if (type == null)
            {
                throw NewParseException($"Unknown token within ParsePrimaryType: {_reader.Peek()}");
            }

            return type;
        }

        /// <remarks><code><![CDATA[
        /// PredefinedType:
        ///     any
        ///     number
        ///     boolean
        ///     string
        ///     symbol
        ///     void
        /// ]]></code></remarks>
        private ITsType TryParsePredefinedType()
        {
            TsToken token = _reader.Peek();
            ITsType type = null;

            switch (token.TokenCode)
            {
                case TsTokenCode.Any:
                    type = Factory.AnyType;
                    break;

                case TsTokenCode.Number:
                    type = Factory.NumberType;
                    break;

                case TsTokenCode.Boolean:
                    type = Factory.BooleanType;
                    break;

                case TsTokenCode.String:
                    type = Factory.StringType;
                    break;

                case TsTokenCode.Symbol:
                    type = Factory.SymbolType;
                    break;

                case TsTokenCode.Void:
                    type = Factory.VoidType;
                    break;
            }

            if (type != null)
            {
                _reader.Skip();
                return type;
            }

            return null;
        }

        /// <summary>
        /// Parses a type reference.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeReference:
        ///     TypeName [no LineTerminator here] TypeArgumentsOpt
        ///
        /// TypeName:
        ///     IdentifierReference
        ///     NamespaceName . IdentifierReference
        ///
        /// NamespaceName:
        ///     IdentifierReference
        ///     NamespaceName . IdentifierReference
        /// ]]></code></remarks>
        private ITsTypeReference ParseTypeReference()
        {
            ITsQualifiedName typeName = ParseQualifiedName();
            ITsType[] typeArguments = TryParseTypeArguments();
            return Factory.TypeReference(typeName, typeArguments);
        }

        /// <summary>
        /// Parses an object type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ObjectType:
        ///     { TypeBodyOpt }
        ///
        /// TypeBody:
        ///     TypeMemberList ;Opt
        ///     TypeMemberList ,Opt
        ///
        /// TypeMemberList:
        ///     TypeMember
        ///     TypeMemberList ; TypeMember
        ///     TypeMemberList , TypeMember
        ///
        /// TypeMember:
        ///     PropertySignature
        ///     CallSignature
        ///     ConstructSignature
        ///     IndexSignature
        ///     MethodSignature
        ///
        /// PropertySignature:
        ///     PropertyName ?Opt TypeAnnotationOpt
        ///
        /// MethodSignature:
        ///     PropertyName ?Opt CallSignature
        /// ]]></code></remarks>
        private ITsObjectType TryParseObjectType()
        {
            if (!_reader.ReadIf(TsTokenCode.LeftBrace))
            {
                return null;
            }

            var typeMembers = new List<ITsTypeMember>();
            do
            {
                if (_reader.IsNext(TsTokenCode.RightBrace))
                {
                    break;
                }

                ITsTypeMember member = TryParseConstructSignature() ??
                    (ITsTypeMember)TryParseIndexSignature() ?? TryParseCallSignature();

                // PropertySignature and MethodSignature start the same way
                if (member == null)
                {
                    ITsPropertyName propertyName = ParsePropertyName();
                    bool isOptional = _reader.ReadIf(TsTokenCode.Question);
                    if (TryParseCallSignature(out ITsCallSignature callSignature))
                    {
                        member = Factory.MethodSignature(propertyName, isOptional, callSignature);
                    }
                    else
                    {
                        ITsType propertyType = TryParseTypeAnnotation();
                        member = Factory.PropertySignature(propertyName, propertyType, isOptional);
                    }
                }

                if (member == null)
                {
                    throw NewParseException($"Unknown token in ParseObjectType: {_reader.Peek()}");
                }

                typeMembers.Add(member);
            }
            while (_reader.ReadIf(tokenCode => tokenCode.IsOneOf(TsTokenCode.Semicolon, TsTokenCode.Comma)));

            Read(TsTokenCode.RightBrace);

            return Factory.ObjectType(typeMembers.ToArray());
        }

        /// <summary>
        /// Parses a construct signature.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ConstructSignature:
        ///     new TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
        /// ]]></code></remarks>
        private ITsConstructSignature TryParseConstructSignature()
        {
            if (!_reader.ReadIf(TsTokenCode.New))
            {
                return null;
            }

            ITsTypeParameters typeParameters = TryParseTypeParameters();
            ITsParameterList parameterList = ParseOptionalParameterListWithParens();
            ITsType returnType = TryParseTypeAnnotation();

            return Factory.ConstructSignature(typeParameters, parameterList, returnType);
        }

        /// <summary>
        /// Parses an index signature.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IndexSignature:
        ///     [ BindingIdentifier : string ] TypeAnnotation
        ///     [ BindingIdentifier : number ] TypeAnnotation
        /// ]]></code></remarks>
        private ITsIndexSignature TryParseIndexSignature()
        {
            if (!_reader.ReadIf(TsTokenCode.LeftBracket))
            {
                return null;
            }

            ITsIdentifier parameterName = ParseIdentifier();
            Read(TsTokenCode.Colon);

            bool isParameterNumberType;
            if (_reader.ReadIf(TsTokenCode.String))
            {
                isParameterNumberType = false;
            }
            else if (_reader.ReadIf(TsTokenCode.Number))
            {
                isParameterNumberType = true;
            }
            else
            {
                throw NewParseException("An index signature needs to either have a string or number type");
            }

            Read(TsTokenCode.RightBracket);

            ITsType returnType = ParseTypeAnnotation();

            return Factory.IndexSignature(parameterName, isParameterNumberType, returnType);
        }

        /// <summary>
        /// Parses a tuple type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TupleType:
        ///     [ TupleElementTypes ]
        ///
        /// TupleElementTypes:
        ///     TupleElementType
        ///     TupleElementTypes , TupleElementType
        ///
        /// TupleElementType:
        ///     Type
        /// ]]></code></remarks>
        private ITsTupleType TryParseTupleType()
        {
            if (!_reader.ReadIf(TsTokenCode.LeftBracket))
            {
                return null;
            }

            var types = new List<ITsType>();
            do
            {
                ITsType type = ParseType();
                types.Add(type);
            }
            while (_reader.ReadIf(TsTokenCode.Comma));

            Read(TsTokenCode.RightBracket);
            return Factory.TupleType(types[0], types.Skip(1).ToArray());
        }

        /// <summary>
        /// Parses a tuple type.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeQuery:
        ///     typeof TypeQueryExpression
        ///
        /// TypeQueryExpression:
        ///     IdentifierReference
        ///     TypeQueryExpression . IdentifierName
        /// ]]></code></remarks>
        private ITsTypeQuery TryParseTypeQuery()
        {
            if (!_reader.ReadIf(TsTokenCode.Typeof))
            {
                return null;
            }

            var qualifiedName = ParseQualifiedName();
            return Factory.TypeQuery(qualifiedName);
        }

        /// <summary>
        /// Parses a call signature.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// CallSignature:
        ///    TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
        /// ]]></code></remarks>
        private bool TryParseCallSignature(out ITsCallSignature callSignature)
        {
            callSignature = TryParseCallSignature();
            return callSignature != null;
        }

        /// <summary>
        /// Parses a call signature.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// CallSignature:
        ///    TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
        /// ]]></code></remarks>
        private ITsCallSignature TryParseCallSignature()
        {
            ITsTypeParameters typeParameters = TryParseTypeParameters();
            if (typeParameters == null && !_reader.IsNext(TsTokenCode.LeftParen))
            {
                return null;
            }

            ITsParameterList parameterList = ParseOptionalParameterListWithParens();
            ITsType returnType = TryParseTypeAnnotation();

            return Factory.CallSignature(typeParameters, parameterList, returnType);
        }

        /// <summary>
        /// Tries to parse type parameters.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeParameters:
        ///     < TypeParameterList >
        ///
        /// TypeParameterList:
        ///     TypeParameter
        ///     TypeParameterList, TypeParameter
        ///
        /// TypeParameter:
        ///     BindingIdentifier ConstraintOpt
        ///
        /// Constraint:
        ///     extends Type
        /// ]]></code></remarks>
        private ITsTypeParameters TryParseTypeParameters()
        {
            if (!_reader.ReadIf(TsTokenCode.LessThan))
            {
                return null;
            }

            var typeParameters = new List<ITsTypeParameter>();
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.GreaterThan))
            {
                ITsIdentifier typeName = ParseIdentifier();
                ITsType constraint = null;
                if (_reader.ReadIf(TsTokenCode.Extends))
                {
                    constraint = ParseType();
                }

                _reader.ReadIf(TsTokenCode.Comma);

                var typeParameter = Factory.TypeParameter(typeName, constraint);
                typeParameters.Add(typeParameter);
            }

            Read(TsTokenCode.GreaterThan);
            return Factory.TypeParameters(typeParameters.ToArray());
        }

        /// <summary>
        /// Tries to parse type arguments.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeArguments:
        ///     < TypeArgumentList >
        ///
        /// TypeArgumentList:
        ///     TypeArgument
        ///     TypeArgumentList , TypeArgument
        ///
        /// TypeArgument:
        ///     Type
        /// ]]></code></remarks>
        private ITsType[] TryParseTypeArguments()
        {
            if (!_reader.ReadIf(TsTokenCode.LessThan))
            {
                return null;
            }

            var typeArguments = new List<ITsType>();
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.GreaterThan))
            {
                ITsType typeArgument = ParseType();
                _reader.ReadIf(TsTokenCode.Comma);
                typeArguments.Add(typeArgument);
            }

            Read(TsTokenCode.GreaterThan);
            return typeArguments.ToArray();
        }

        /// <summary>
        /// Tries to parse a type annotation of the form ': type'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeAnnotation:
        ///     : Type
        /// ]]></code></remarks>
        private ITsType TryParseTypeAnnotation() => _reader.ReadIf(TsTokenCode.Colon) ? ParseType() : null;

        /// <summary>
        /// Parses a type annotation of the form ': type'.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TypeAnnotation:
        ///     : Type
        /// ]]></code></remarks>
        private ITsType ParseTypeAnnotation()
        {
            Read(TsTokenCode.Colon);
            return ParseType();
        }
    }
}
