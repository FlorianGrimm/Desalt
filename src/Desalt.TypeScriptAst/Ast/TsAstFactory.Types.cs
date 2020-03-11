// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.TypeScriptAst.Ast.Types;

    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Types
        //// ===========================================================================================================

        public static ITsTypeReference TypeReference(ITsTypeName typeName, params ITsType[] typeArguments)
        {
            return new TsTypeReference(typeName, typeArguments);
        }

        public static ITsObjectType ObjectType(params ITsTypeMember[] typeMembers)
        {
            return new TsObjectType(typeMembers);
        }

        public static ITsObjectType ObjectType(bool forceSingleLine, params ITsTypeMember[] typeMembers)
        {
            return new TsObjectType(typeMembers, forceSingleLine);
        }

        public static ITsArrayType ArrayType(ITsType type)
        {
            return new TsArrayType(type);
        }

        public static ITsTupleType TupleType(ITsType elementType, params ITsType[] elementTypes)
        {
            return new TsTupleType(elementType, elementTypes);
        }

        public static ITsFunctionType FunctionType(
            ITsTypeParameters typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            return new TsFunctionOrConstructorType(typeParameters, parameters, returnType, isConstructorType: false);
        }

        public static ITsFunctionType FunctionType(ITsParameterList parameters, ITsType returnType)
        {
            return new TsFunctionOrConstructorType(parameters, returnType, isConstructorType: false);
        }

        public static ITsFunctionType FunctionType(ITsType returnType)
        {
            return new TsFunctionOrConstructorType(returnType, isConstructorType: false);
        }

        public static ITsConstructorType ConstructorType(
            ITsTypeParameters typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            return new TsFunctionOrConstructorType(typeParameters, parameters, returnType, isConstructorType: true);
        }

        public static ITsConstructorType ConstructorType(ITsParameterList parameters, ITsType returnType)
        {
            return new TsFunctionOrConstructorType(parameters, returnType, isConstructorType: true);
        }

        public static ITsConstructorType ConstructorType(ITsType returnType)
        {
            return new TsFunctionOrConstructorType(returnType, isConstructorType: true);
        }

        public static ITsTypeQuery TypeQuery(ITsTypeName query)
        {
            return new TsTypeQuery(query);
        }

        public static ITsPropertySignature PropertySignature(
            ITsPropertyName propertyName,
            ITsType propertyType = null,
            bool isOptional = false)
        {
            return new TsPropertySignature(propertyName, isOptional, propertyType);
        }

        public static ITsCallSignature CallSignature()
        {
            return new TsCallSignature();
        }

        public static ITsCallSignature CallSignature(
            ITsParameterList parameters,
            ITsType returnType = null)
        {
            return new TsCallSignature(typeParameters: null, parameters: parameters, returnType: returnType);
        }

        public static ITsCallSignature CallSignature(
            ITsTypeParameters typeParameters,
            ITsParameterList parameters = null,
            ITsType returnType = null)
        {
            return new TsCallSignature(typeParameters, parameters, returnType);
        }

        public static ITsParameterList ParameterList()
        {
            return new TsParameterList();
        }

        public static ITsParameterList ParameterList(params ITsIdentifier[] requiredParameters)
        {
            return new TsParameterList(requiredParameters.Select(p => BoundRequiredParameter(p)));
        }

        public static ITsParameterList ParameterList(params ITsRequiredParameter[] requiredParameters)
        {
            return new TsParameterList(requiredParameters);
        }

        public static ITsParameterList ParameterList(
            IEnumerable<ITsRequiredParameter> requiredParameters,
            IEnumerable<ITsOptionalParameter> optionalParameters = null,
            ITsRestParameter restParameter = null)
        {
            return new TsParameterList(requiredParameters, optionalParameters, restParameter);
        }

        /// <summary>
        /// Creates an empty type parameters list.
        /// </summary>
        public static ITsTypeParameters TypeParameters()
        {
            return TsTypeParameters.Empty;
        }

        /// <summary>
        /// Creates a list of type parameters of the form '&lt;type, type&gt;'.
        /// </summary>
        public static ITsTypeParameters TypeParameters(params ITsTypeParameter[] typeParameters)
        {
            return new TsTypeParameters(typeParameters);
        }

        public static ITsTypeParameter TypeParameter(ITsIdentifier typeName, ITsType constraint = null)
        {
            return new TsTypeParameter(typeName, constraint);
        }

        public static ITsBoundRequiredParameter BoundRequiredParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            TsAccessibilityModifier? modifier = null)
        {
            return new TsBoundRequiredParameter(parameterName, parameterType, modifier);
        }

        public static ITsStringRequiredParameter StringRequiredParameter(
            ITsIdentifier parameterName,
            ITsStringLiteral stringLiteral)
        {
            return new TsStringParameter(parameterName, stringLiteral, isOptional: false);
        }

        public static ITsBoundOptionalParameter BoundOptionalParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            ITsExpression initializer = null,
            TsAccessibilityModifier? modifier = null)
        {
            return new TsBoundOptionalParameter(parameterName, parameterType, initializer, modifier);
        }

        public static ITsStringOptionalParameter StringOptionalParameter(
            ITsIdentifier parameterName,
            ITsStringLiteral stringLiteral)
        {
            return new TsStringParameter(parameterName, stringLiteral, isOptional: true);
        }

        public static ITsRestParameter RestParameter(ITsIdentifier parameterName, ITsType parameterType = null)
        {
            return new TsRestParameter(parameterName, parameterType);
        }

        public static ITsConstructSignature ConstructSignature(
            ITsParameterList parameterList = null,
            ITsType returnType = null)
        {
            return new TsConstructSignature(typeParameters: null, parameterList: parameterList, returnType: returnType);
        }

        public static ITsConstructSignature ConstructSignature(
            ITsTypeParameters typeParameters = null,
            ITsParameterList parameterList = null,
            ITsType returnType = null)
        {
            return new TsConstructSignature(typeParameters, parameterList, returnType);
        }

        public static ITsIndexSignature IndexSignature(
            ITsIdentifier parameterName,
            bool isParameterNumberType,
            ITsType returnType)
        {
            return new TsIndexSignature(parameterName, isParameterNumberType, returnType);
        }

        public static ITsMethodSignature MethodSignature(
            ITsPropertyName propertyName,
            bool isOptional,
            ITsCallSignature callSignature)
        {
            return new TsMethodSignature(propertyName, isOptional, callSignature);
        }

        /// <summary>
        /// Creates a union type of the form 'type1 | type2'.
        /// </summary>
        public static ITsUnionType UnionType(params ITsType[] types)
        {
            return new TsUnionOrIntersectionType(types[0], types[1], types.Skip(2).ToArray(), isUnion: true);
        }

        /// <summary>
        /// Creates an intersection type of the form 'type1 &amp; type2'.
        /// </summary>
        public static ITsIntersectionType IntersectionType(ITsType type1, ITsType type2, params ITsType[] otherTypes)
        {
            return new TsUnionOrIntersectionType(type1, type2, otherTypes, isUnion: false);
        }
    }
}