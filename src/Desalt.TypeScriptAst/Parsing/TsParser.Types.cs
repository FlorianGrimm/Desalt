// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParser
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
            if (_reader.IsNext(TsTokenCode.New))
            {
                ITsType ctorType = ParseConstructorType();
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
            ParseOptionalAccessibilityModifier();

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
            ITsTypeParameters? typeParameters = ParseOptionalTypeParameters();
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
        private ITsConstructorType ParseConstructorType()
        {
            Read(TsTokenCode.New);
            ITsTypeParameters? typeParameters = ParseOptionalTypeParameters();
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
            ITsType? type = null;
            switch (_reader.Peek().TokenCode)
            {
                // PredefinedType
                case TsTokenCode.Any:
                case TsTokenCode.Number:
                case TsTokenCode.Boolean:
                case TsTokenCode.String:
                case TsTokenCode.Symbol:
                case TsTokenCode.Void:
                    type = ParsePredefinedType();
                    break;

                // TypeReference
                // ReSharper disable once PatternAlwaysMatches
                case TsTokenCode tc when IsStartOfIdentifier(tc, isTypeDeclaration: false):
                    type = ParseTypeReference();
                    break;

                // ParenthesizedType
                case TsTokenCode.LeftParen:
                    _reader.Read();
                    type = ParseType();
                    Read(TsTokenCode.RightParen);
                    break;

                // ObjectType
                case TsTokenCode.LeftBrace:
                    type = ParseObjectType();
                    break;

                // TupleType
                case TsTokenCode.LeftBracket:
                    type = ParseTupleType();
                    break;

                // TypeQuery
                case TsTokenCode.Typeof:
                    type = ParseTypeQuery();
                    break;

                // ThisType
                case TsTokenCode.This:
                    type = Factory.ThisType;
                    break;
            }

            // ArrayType
            if (type != null && _reader.ReadIf(TsTokenCode.LeftBracket))
            {
                type = Factory.ArrayType(type);
                Read(TsTokenCode.RightBracket);
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
        private ITsType ParsePredefinedType()
        {
            TsToken token = _reader.Read();
            switch (token.TokenCode)
            {
                case TsTokenCode.Any:
                    return Factory.AnyType;

                case TsTokenCode.Number:
                    return Factory.NumberType;

                case TsTokenCode.Boolean:
                    return Factory.BooleanType;

                case TsTokenCode.String:
                    return Factory.StringType;

                case TsTokenCode.Symbol:
                    return Factory.SymbolType;

                case TsTokenCode.Void:
                    return Factory.VoidType;

                default:
                    throw NewParseException($"Token '{token}' is not a predefined type", token.Location);
            }
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
            ITsType[]? typeArguments = ParseOptionalTypeArguments();
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
        ///     PropertySignature   (starts with PropertyName)
        ///     CallSignature       (starts with either '<' or '(')
        ///     ConstructSignature  (starts with 'new')
        ///     IndexSignature      (starts with '[')
        ///     MethodSignature     (starts with PropertyName)
        ///
        /// PropertySignature:
        ///     PropertyName ?Opt TypeAnnotationOpt
        ///
        /// MethodSignature:
        ///     PropertyName ?Opt CallSignature
        /// ]]></code></remarks>
        private ITsObjectType ParseObjectType()
        {
            Read(TsTokenCode.LeftBrace);

            var typeMembers = new List<ITsTypeMember>();
            do
            {
                ITsTypeMember member;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_reader.Peek().TokenCode)
                {
                    // ConstructSignature
                    case TsTokenCode.New:
                        member = ParseConstructSignature();
                        break;

                    // IndexSignature
                    case TsTokenCode.LeftBracket:
                        member = ParseIndexSignature();
                        break;

                    // CallSignature
                    case TsTokenCode.LessThan:
                    case TsTokenCode.LeftParen:
                        member = ParseCallSignature();
                        break;

                    // PropertySignature and MethodSignature start the same way
                    default:
                        ITsPropertyName propertyName = ParsePropertyName();
                        bool isOptional = _reader.ReadIf(TsTokenCode.Question);
                        if (_reader.Peek().TokenCode.IsOneOf(TsTokenCode.LessThan, TsTokenCode.LeftParen))
                        {
                            ITsCallSignature callSignature = ParseCallSignature();
                            member = Factory.MethodSignature(propertyName, isOptional, callSignature);
                        }
                        else
                        {
                            ITsType? propertyType = ParseOptionalTypeAnnotation();
                            member = Factory.PropertySignature(propertyName, propertyType, isOptional);
                        }

                        break;
                }

                typeMembers.Add(member);
                _reader.ReadIf(tokenCode => tokenCode.IsOneOf(TsTokenCode.Semicolon, TsTokenCode.Comma));
            }
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.RightBrace));

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
        private ITsConstructSignature ParseConstructSignature()
        {
            Read(TsTokenCode.New);
            ITsTypeParameters? typeParameters = ParseOptionalTypeParameters();
            ITsParameterList? parameterList = ParseOptionalParameterListWithParens();
            ITsType? returnType = ParseOptionalTypeAnnotation();

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
        private ITsIndexSignature ParseIndexSignature()
        {
            Read(TsTokenCode.LeftBracket);

            ITsIdentifier parameterName = ParseIdentifier(isTypeDeclaration: false);
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
        private ITsTupleType ParseTupleType()
        {
            Read(TsTokenCode.LeftBracket);

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
        private ITsTypeQuery ParseTypeQuery()
        {
            Read(TsTokenCode.Typeof);
            ITsQualifiedName qualifiedName = ParseQualifiedName();
            return Factory.TypeQuery(qualifiedName);
        }

        /// <summary>
        /// Parses a call signature.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// CallSignature:
        ///    TypeParametersOpt ( ParameterListOpt ) TypeAnnotationOpt
        /// ]]></code></remarks>
        private ITsCallSignature ParseCallSignature()
        {
            ITsTypeParameters? typeParameters = ParseOptionalTypeParameters();
            ITsParameterList? parameterList = ParseOptionalParameterListWithParens();
            ITsType? returnType = ParseOptionalTypeAnnotation();

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
        private ITsTypeParameters? ParseOptionalTypeParameters()
        {
            if (!_reader.ReadIf(TsTokenCode.LessThan))
            {
                return null;
            }

            var typeParameters = new List<ITsTypeParameter>();
            do
            {
                ITsIdentifier typeName = ParseIdentifier(isTypeDeclaration: false);
                ITsType? constraint = null;
                if (_reader.ReadIf(TsTokenCode.Extends))
                {
                    constraint = ParseType();
                }

                ITsTypeParameter typeParameter = Factory.TypeParameter(typeName, constraint);
                typeParameters.Add(typeParameter);
            }
            while (!_reader.IsAtEnd && _reader.ReadIf(TsTokenCode.Comma));

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
        private ITsType[]? ParseOptionalTypeArguments()
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
        private ITsType? ParseOptionalTypeAnnotation()
        {
            return _reader.ReadIf(TsTokenCode.Colon) ? ParseType() : null;
        }

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
