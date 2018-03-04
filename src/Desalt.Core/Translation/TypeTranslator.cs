// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;
    using Factory = TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Translates known types in C# to known TypeScript types.
    /// </summary>
    internal static class TypeTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<string, ITsType> s_nativeTypeMap = new Dictionary<string, ITsType>
        {
            ["System.Void"] = Factory.VoidType,
            ["System.Boolean"] = Factory.BooleanType,
            ["System.String"] = Factory.StringType,
            ["System.Byte"] = Factory.NumberType,
            ["System.SByte"] = Factory.NumberType,
            ["System.UInt16"] = Factory.NumberType,
            ["System.Int16"] = Factory.NumberType,
            ["System.UInt32"] = Factory.NumberType,
            ["System.Int32"] = Factory.NumberType,
            ["System.UInt64"] = Factory.NumberType,
            ["System.Int64"] = Factory.NumberType,
            ["System.Decimal"] = Factory.NumberType,
            ["System.Single"] = Factory.NumberType,
            ["System.Double"] = Factory.NumberType,
            ["System.Object"] = Factory.AnyType,
        }.ToImmutableDictionary();

        private static readonly SymbolDisplayFormat s_displayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsType TranslateSymbol(ITypeSymbol symbol)
        {
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                ITsType elementType = TranslateSymbol(arrayTypeSymbol.ElementType);
                return Factory.ArrayType(elementType);
            }

            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            if (s_nativeTypeMap.ContainsKey(fullTypeName))
            {
                return s_nativeTypeMap[fullTypeName];
            }

            if (fullTypeName == "System.Func")
            {
                return TranslateFunc((INamedTypeSymbol)symbol);
            }

            return Factory.TypeReference(Factory.Identifier(symbol.Name));
        }

        /// <summary>
        /// Translates a type of <c>Func{T1, T2, TRsult}</c> to a TypeScript function type of the
        /// form <c>(t1: T1, t2: T2) =&gt; TResult</c>.
        /// </summary>
        private static ITsFunctionType TranslateFunc(INamedTypeSymbol symbol)
        {
            var requiredParameters = new List<ITsRequiredParameter>();

            foreach (ITypeSymbol typeArgument in symbol.TypeArguments.Take(symbol.TypeArguments.Length - 1))
            {
                string parameterName = typeArgument.Name;
                parameterName = char.ToLowerInvariant(parameterName[0]) + parameterName.Substring(1);

                var parameterType = TranslateSymbol(typeArgument);
                ITsBoundRequiredParameter requiredParameter = Factory.BoundRequiredParameter(Factory.Identifier(parameterName), parameterType);
                requiredParameters.Add(requiredParameter);
            }

            ITsParameterList parameterList = Factory.ParameterList(requiredParameters: requiredParameters);
            ITsType returnType = TranslateSymbol(symbol.TypeArguments.Last());
            return Factory.FunctionType(parameterList, returnType);
        }
    }
}
