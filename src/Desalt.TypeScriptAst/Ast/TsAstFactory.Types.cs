// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Types.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public static partial class TsAstFactory
    {
        //// ===========================================================================================================
        //// Types
        //// ===========================================================================================================

        public static ITsTypeReference TypeReference(ITsTypeName typeName, params ITsType[] typeArguments)
        {
            return new TsTypeReference(typeName, typeArguments.ToImmutableArray());
        }

        public static ITsObjectType ObjectType(params ITsTypeMember[] typeMembers)
        {
            return new TsObjectType(typeMembers.ToImmutableArray(), forceSingleLine: false);
        }

        public static ITsObjectType ObjectType(bool forceSingleLine, params ITsTypeMember[] typeMembers)
        {
            return new TsObjectType(typeMembers.ToImmutableArray(), forceSingleLine);
        }

        public static ITsArrayType ArrayType(ITsType type)
        {
            return new TsArrayType(type);
        }

        public static ITsTupleType TupleType(ITsType elementType, params ITsType[] elementTypes)
        {
            return new TsTupleType(ImmutableArray.Create(elementType).AddRange(elementTypes));
        }

        /// <summary>
        /// Creates a <see cref="ITsFunctionType"/> of the form '&lt;T&gt;(x: type) => returnType'.
        /// </summary>
        public static ITsFunctionType FunctionType(
            ITsTypeParameters? typeParameters,
            ITsParameterList? parameters,
            ITsType returnType)
        {
            return new TsFunctionType(typeParameters, parameters, returnType);
        }

        /// <summary>
        /// Creates a <see cref="ITsFunctionType"/> of the form '(x: type) => returnType'.
        /// </summary>
        public static ITsFunctionType FunctionType(ITsParameterList parameters, ITsType returnType)
        {
            return new TsFunctionType(typeParameters: null, parameters, returnType);
        }

        /// <summary>
        /// Creates a <see cref="ITsFunctionType"/> of the form '() => returnType'.
        /// </summary>
        public static ITsFunctionType FunctionType(ITsType returnType)
        {
            return new TsFunctionType(typeParameters: null, parameters: null, returnType);
        }

        /// <summary>
        /// Creates a <see cref="ITsConstructorType"/> of the form 'new &lt;T&gt;(x: type) => returnType'.
        /// </summary>
        public static ITsConstructorType ConstructorType(
            ITsTypeParameters? typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            return new TsConstructorType(typeParameters, parameters, returnType);
        }

        /// <summary>
        /// Creates a <see cref="ITsConstructorType"/> of the form 'new (x: type) => returnType'.
        /// </summary>
        public static ITsConstructorType ConstructorType(ITsParameterList parameters, ITsType returnType)
        {
            return new TsConstructorType(typeParameters: null, parameters, returnType);
        }

        /// <summary>
        /// Creates a <see cref="ITsConstructorType"/> of the form 'new () => returnType'.
        /// </summary>
        public static ITsConstructorType ConstructorType(ITsType returnType)
        {
            return new TsConstructorType(typeParameters: null, parameters: null, returnType);
        }

        public static ITsTypeQuery TypeQuery(ITsTypeName query)
        {
            return new TsTypeQuery(query);
        }

        public static ITsPropertySignature PropertySignature(
            ITsPropertyName propertyName,
            ITsType? propertyType = null,
            bool isReadOnly = false,
            bool isOptional = false)
        {
            return new TsPropertySignature(propertyName, isReadOnly: isReadOnly, isOptional: isOptional, propertyType);
        }

        public static ITsCallSignature CallSignature()
        {
            return new TsCallSignature(typeParameters: null, parameters: null, returnType: null);
        }

        public static ITsCallSignature CallSignature(
            ITsParameterList? parameters,
            ITsType? returnType = null)
        {
            return new TsCallSignature(typeParameters: null, parameters: parameters, returnType: returnType);
        }

        public static ITsCallSignature CallSignature(
            ITsTypeParameters? typeParameters,
            ITsParameterList? parameters = null,
            ITsType? returnType = null)
        {
            return new TsCallSignature(typeParameters, parameters, returnType);
        }

        public static ITsParameterList ParameterList()
        {
            return new TsParameterList(
                requiredParameters: ImmutableArray<ITsRequiredParameter>.Empty,
                optionalParameters: ImmutableArray<ITsOptionalParameter>.Empty,
                restParameter: null);
        }

        public static ITsParameterList ParameterList(params ITsIdentifier[] requiredParameters)
        {
            return new TsParameterList(
                requiredParameters.Select(p => BoundRequiredParameter(p))
                    .Cast<ITsRequiredParameter>()
                    .ToImmutableArray(),
                optionalParameters: ImmutableArray<ITsOptionalParameter>.Empty,
                restParameter: null);
        }

        public static ITsParameterList ParameterList(params ITsRequiredParameter[] requiredParameters)
        {
            return new TsParameterList(
                requiredParameters.ToImmutableArray(),
                optionalParameters: ImmutableArray<ITsOptionalParameter>.Empty,
                restParameter: null);
        }

        public static ITsParameterList ParameterList(
            IEnumerable<ITsRequiredParameter>? requiredParameters,
            IEnumerable<ITsOptionalParameter>? optionalParameters = null,
            ITsRestParameter? restParameter = null)
        {
            return new TsParameterList(
                requiredParameters?.ToImmutableArray() ?? ImmutableArray<ITsRequiredParameter>.Empty,
                optionalParameters?.ToImmutableArray() ?? ImmutableArray<ITsOptionalParameter>.Empty,
                restParameter);
        }

        /// <summary>
        /// Creates an empty type parameters list.
        /// </summary>
        public static ITsTypeParameters TypeParameters()
        {
            return new TsTypeParameters(ImmutableArray<ITsTypeParameter>.Empty);
        }

        /// <summary>
        /// Creates a list of type parameters of the form '&lt;type, type&gt;'.
        /// </summary>
        public static ITsTypeParameters TypeParameters(params ITsTypeParameter[] typeParameters)
        {
            return new TsTypeParameters(typeParameters.ToImmutableArray());
        }

        public static ITsTypeParameter TypeParameter(ITsIdentifier typeName, ITsType? constraint = null)
        {
            return new TsTypeParameter(typeName, constraint);
        }

        public static ITsBoundRequiredParameter BoundRequiredParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType? parameterType = null,
            TsAccessibilityModifier? modifier = null)
        {
            return new TsBoundRequiredParameter(modifier, parameterName, parameterType);
        }

        public static ITsStringRequiredParameter StringRequiredParameter(
            ITsIdentifier parameterName,
            ITsStringLiteral stringLiteral)
        {
            return new TsStringRequiredParameter(parameterName, stringLiteral);
        }

        public static ITsBoundOptionalParameter BoundOptionalParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType? parameterType = null,
            ITsExpression? initializer = null,
            TsAccessibilityModifier? modifier = null)
        {
            return new TsBoundOptionalParameter(modifier, parameterName, parameterType, initializer);
        }

        public static ITsStringOptionalParameter StringOptionalParameter(
            ITsIdentifier parameterName,
            ITsStringLiteral stringLiteral)
        {
            return new TsStringOptionalParameter(parameterName, stringLiteral);
        }

        public static ITsRestParameter RestParameter(ITsIdentifier parameterName, ITsType? parameterType = null)
        {
            return new TsRestParameter(parameterName, parameterType);
        }

        public static ITsConstructSignature ConstructSignature(
            ITsParameterList? parameters = null,
            ITsType? returnType = null)
        {
            return new TsConstructSignature(typeParameters: null, parameters: parameters, returnType: returnType);
        }

        public static ITsConstructSignature ConstructSignature(
            ITsTypeParameters? typeParameters = null,
            ITsParameterList? parameterList = null,
            ITsType? returnType = null)
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
        public static ITsUnionType UnionType(ITsType type1, ITsType type2, params ITsType[] types)
        {
            return new TsUnionType(ImmutableArray.Create(type1).Add(type2).AddRange(types));
        }

        /// <summary>
        /// Creates an intersection type of the form 'type1 &amp; type2'.
        /// </summary>
        public static ITsIntersectionType IntersectionType(ITsType type1, ITsType type2, params ITsType[] otherTypes)
        {
            return new TsIntersectionType(ImmutableArray.Create(type1).Add(type2).AddRange(otherTypes));
        }
    }
}
