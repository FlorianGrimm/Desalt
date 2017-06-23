// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsModelFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.TypeScript.CodeModels.Expressions;
    using Desalt.TypeScript.CodeModels.Types;

    /// <summary>
    /// Provides factory methods for creating TypeScript code models.
    /// </summary>
    public static class TsModelFactory
    {
        //// ===========================================================================================================
        //// Singleton Properties
        //// ===========================================================================================================

        public static ITsThis This => TsThis.Instance;
        public static ITsThisType ThisType => TsThisType.Instance;

        public static ITsNullLiteral NullLiteral => TsNullLiteral.Instance;

        public static ITsBooleanLiteral TrueLiteral => TsBooleanLiteral.True;
        public static ITsBooleanLiteral FalseLiteral => TsBooleanLiteral.False;

        public static readonly ITsObjectLiteral EmptyObjectLiteral = new TsObjectLiteral(null);

        public static readonly ITsPredefinedType Any = TsPredefinedType.Any;
        public static readonly ITsPredefinedType Number = TsPredefinedType.Number;
        public static readonly ITsPredefinedType Boolean = TsPredefinedType.Boolean;
        public static readonly ITsPredefinedType String = TsPredefinedType.String;
        public static readonly ITsPredefinedType Symbol = TsPredefinedType.Symbol;
        public static readonly ITsPredefinedType Void = TsPredefinedType.Void;

        //// ===========================================================================================================
        //// Identifiers
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

        public static ITsQualifiedName QualifiedName(string dottedName)
        {
            string[] parts = dottedName.Split('.');
            if (parts.Length > 1)
            {
                return QualifiedName(parts[0], parts.Skip(1).ToArray());
            }

            return new TsQualifiedName(TsIdentifier.Get(parts[0]));
        }

        public static ITsQualifiedName QualifiedName(string name, params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return new TsQualifiedName(TsIdentifier.Get(name));
            }

            var right = TsIdentifier.Get(names.Last());
            IEnumerable<TsIdentifier> left = new[] { name }.Concat(names.Take(names.Length - 1)).Select(TsIdentifier.Get);
            return new TsQualifiedName(right, left);
        }

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsStringLiteral StringLiteral(string value, StringLiteralQuoteKind quoteKind) =>
            new TsStringLiteral(value, quoteKind);

        public static ITsNumericLiteral DecimalLiteral(double value) =>
            new TsNumericLiteral(TsNumericLiteralKind.Decimal, value);

        public static ITsNumericLiteral BinaryIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.BinaryInteger, value);

        public static ITsNumericLiteral OctalIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.OctalInteger, value);

        public static ITsNumericLiteral HexIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.HexInteger, value);

        public static ITsRegularExpressionLiteral RegularExpressionLiteral(string body, string flags) =>
            new TsRegularExpressionLiteral(body, flags);

        public static ITsArrayLiteral ArrayLiteral(params ITsArrayElement[] elements) => new TsArrayLiteral(elements);

        public static ITsArrayElement ArrayElement(ITsAssignmentExpression element, bool isSpreadElement = false) =>
            new TsArrayElement(element, isSpreadElement);

        public static ITsTemplateLiteral TemplateLiteral(params TsTemplatePart[] parts) => new TsTemplateLiteral(parts);

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public static ITsObjectLiteral ObjectLiteral(IEnumerable<ITsPropertyDefinition> propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsObjectLiteral ObjectLiteral(params ITsPropertyDefinition[] propertyDefinitions) =>
            new TsObjectLiteral(propertyDefinitions);

        public static ITsCoverInitializedName CoverInitializedName(
            ITsIdentifier identifier,
            ITsAssignmentExpression initializer)
        {
            return new TsCoverInitializedName(identifier, initializer);
        }

        public static ITsPropertyAssignment PropertyAssignment(
            ITsPropertyName propertyName,
            ITsAssignmentExpression initializer)
        {
            return new TsPropertyAssignment(propertyName, initializer);
        }

        public static ITsComputedPropertyName ComputedPropertyName(ITsAssignmentExpression expression) =>
            new TsComputedPropertyName(expression);

        public static ITsPropertyFunction PropertyFunction(
            ITsPropertyName propertyName,
            ITsCallSignature callSignature,
            params ITsStatementListItem[] functionBody)
        {
            return new TsPropertyFunction(propertyName, callSignature, functionBody);
        }

        public static ITsGetAccessor GetAccessor(
            ITsPropertyName propertyName,
            ITsType propertyType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsGetAccessor(propertyName, propertyType, functionBody);
        }

        public static ITsSetAccessor SetAccessor(
            ITsPropertyName propertyName,
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsSetAccessor(propertyName, parameterName, parameterType, functionBody);
        }

        //// ===========================================================================================================
        //// Expressions
        //// ===========================================================================================================

        public static ITsFunctionExpression FunctionExpression(
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            params ITsStatementListItem[] functionBody)
        {
            return new TsFunctionExpression(callSignature, functionName, functionBody);
        }

        //// ===========================================================================================================
        //// Types
        //// ===========================================================================================================

        public static ITsTypeReference TypeReference(ITsTypeName typeName, params ITsType[] typeArguments) =>
            new TsTypeReference(typeName, typeArguments);

        public static ITsObjectType ObjectType(params ITsTypeMember[] typeMembers) => new TsObjectType(typeMembers);

        public static ITsArrayType ArrayType(ITsPrimaryType type) => new TsArrayType(type);

        public static ITsTupleType TupleType(ITsType elementType, params ITsType[] elementTypes) =>
            new TsTupleType(elementType, elementTypes);

        public static ITsFunctionType FunctionType(
            IEnumerable<ITsTypeParameter> typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            return new TsFunctionOrConstructorType(typeParameters, parameters, returnType, isConstructorType: false);
        }

        public static ITsFunctionType FunctionType(ITsParameterList parameters, ITsType returnType) =>
            new TsFunctionOrConstructorType(parameters, returnType, isConstructorType: false);

        public static ITsFunctionType FunctionType(ITsType returnType) =>
            new TsFunctionOrConstructorType(returnType, isConstructorType: false);

        public static ITsConstructorType ConstructorType(
            IEnumerable<ITsTypeParameter> typeParameters,
            ITsParameterList parameters,
            ITsType returnType)
        {
            return new TsFunctionOrConstructorType(typeParameters, parameters, returnType, isConstructorType: true);
        }

        public static ITsConstructorType ConstructorType(ITsParameterList parameters, ITsType returnType) =>
            new TsFunctionOrConstructorType(parameters, returnType, isConstructorType: true);

        public static ITsConstructorType ConstructorType(ITsType returnType) =>
            new TsFunctionOrConstructorType(returnType, isConstructorType: true);

        public static ITsTypeQuery TypeQuery(ITsTypeQueryExpression query) => new TsTypeQuery(query);

        public static ITsPropertySignature PropertySignature(
            ITsLiteralPropertyName propertyName,
            bool isOptional = false,
            ITsType propertyType = null)
        {
            return new TsPropertySignature(propertyName, isOptional, propertyType);
        }

        public static ITsCallSignature CallSignature(
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsParameterList parameters = null,
            ITsType returnType = null)
        {
            return new TsCallSignature(typeParameters, parameters, returnType);
        }

        public static ITsParameterList ParameterList(
            IEnumerable<ITsRequiredParameter> requiredParameters = null,
            IEnumerable<ITsOptionalParameter> optionalParameters = null,
            ITsRestParameter restParameter = null)
        {
            return new TsParameterList(requiredParameters, optionalParameters, restParameter);
        }

        public static ITsTypeParameter TypeParameter(ITsIdentifier typeName, ITsType constraint = null) =>
            new TsTypeParameter(typeName, constraint);

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
            ITsAssignmentExpression initializer,
            ITsType parameterType = null,
            TsAccessibilityModifier? modifier = null)
        {
            return new TsBoundOptionalParameter(parameterName, initializer, parameterType, modifier);
        }

        public static ITsStringOptionalParameter StringOptionalParameter(
            ITsIdentifier parameterName,
            ITsStringLiteral stringLiteral)
        {
            return new TsStringParameter(parameterName, stringLiteral, isOptional: true);
        }

        public static ITsRestParameter RestParameter(ITsIdentifier parameterName, ITsType parameterType = null) =>
            new TsRestParameter(parameterName, parameterType);

        public static ITsConstructSignature ConstructSignature(
            IEnumerable<ITsTypeParameter> typeParameters = null,
            ITsParameterList parameterList = null,
            ITsType returnType = null)
        {
            return new TsConstructSignature(typeParameters, parameterList, returnType);
        }

        public static ITsIndexSignature IndexSignature(
            ITsIdentifier parameterName,
            bool isParameterNumberType,
            ITsType parameterType)
        {
            return new TsIndexSignature(parameterName, isParameterNumberType, parameterType);
        }

        public static ITsMethodSignature MethodSignature(
            ITsPropertyName propertyName,
            bool isOptional,
            ITsCallSignature callSignature)
        {
            return new TsMethodSignature(propertyName, isOptional, callSignature);
        }

        public static ITsTypeAliasDeclaration TypeAliasDeclaration(
            ITsIdentifier aliasName,
            ITsType type,
            params ITsTypeParameter[] typeParameters)
        {
            return new TsTypeAliasDeclaration(aliasName, type, typeParameters);
        }

        //// ===========================================================================================================
        //// Source Files
        //// ===========================================================================================================

        public static ImplementationSourceFile ImplementationSourceFile(
            IEnumerable<IImplementationScriptElement> scriptElements)
        {
            return new ImplementationSourceFile(scriptElements);
        }

        public static ImplementationSourceFile ImplementationSourceFile(
            params IImplementationScriptElement[] scriptElements)
        {
            return new ImplementationSourceFile(scriptElements);
        }
    }
}
