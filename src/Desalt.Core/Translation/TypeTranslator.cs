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

        private static readonly ImmutableDictionary<string, (string nativeTypeName, ITsType translatedType)> s_nativeTypeMap = new Dictionary<string,
            (string nativeTypeName, ITsType translatedType)>
        {
            ["System.Void"] = ("void", Factory.VoidType),
            ["System.Boolean"] = ("boolean", Factory.BooleanType),
            ["System.String"] = ("string", Factory.StringType),
            ["System.Byte"] = ("number", Factory.NumberType),
            ["System.SByte"] = ("number", Factory.NumberType),
            ["System.UInt16"] = ("number", Factory.NumberType),
            ["System.Int16"] = ("number", Factory.NumberType),
            ["System.UInt32"] = ("number", Factory.NumberType),
            ["System.Int32"] = ("number", Factory.NumberType),
            ["System.UInt64"] = ("number", Factory.NumberType),
            ["System.Int64"] = ("number", Factory.NumberType),
            ["System.Decimal"] = ("number", Factory.NumberType),
            ["System.Single"] = ("number", Factory.NumberType),
            ["System.Double"] = ("number", Factory.NumberType),
            ["System.Object"] = ("any", Factory.AnyType),
            ["System.Func"] = ("Function", null),
            ["System.Text.RegularExpressions.Regex"] = ("RegExp", null),
        }.ToImmutableDictionary();

        private static readonly SymbolDisplayFormat s_displayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the specified symbol has a native JavaScript type
        /// equivalent. These types don't need to be imported, for example.
        /// </summary>
        /// <param name="symbol">The symbol to inspect.</param>
        public static bool IsNativeTypeScriptType(ITypeSymbol symbol)
        {
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            return s_nativeTypeMap.ContainsKey(fullTypeName);
        }

        /// <summary>
        /// Returns the native type name of the specified symbol. Throws an exception if the symbol
        /// is not a native TypeScript type.
        /// </summary>
        /// <param name="symbol">The symbol to inspect.</param>
        /// <returns>The native type name. For example, "System.String" is a TypeScript "string".</returns>
        public static string GetNativeTypeScriptTypeName(ITypeSymbol symbol)
        {
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            return s_nativeTypeMap[fullTypeName].nativeTypeName;
        }

        public static ITsType TranslateSymbol(ITypeSymbol symbol, ISet<ISymbol> typesToImport)
        {
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                ITsType elementType = TranslateSymbol(arrayTypeSymbol.ElementType, typesToImport);
                return Factory.ArrayType(elementType);
            }

            // native types are easy to translate
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            if (s_nativeTypeMap.TryGetValue(fullTypeName, out (string nativeTypeName, ITsType translatedType) value) &&
                value.translatedType != null)
            {
                return value.translatedType;
            }

            // Func<T1, ...> is a special case
            if (fullTypeName == "System.Func")
            {
                return TranslateFunc((INamedTypeSymbol)symbol, typesToImport);
            }

            // this is a type that we'll need to import since it's not a native type
            typesToImport.Add(symbol);

            // check for generic type arguments
            ITsType[] translatedTypeMembers = null;
            if (symbol is INamedTypeSymbol namedTypeSymbol)
            {
                ImmutableArray<ITypeSymbol> typeMembers = namedTypeSymbol.TypeArguments;
                translatedTypeMembers = typeMembers.Select(typeMember => TranslateSymbol(typeMember, typesToImport))
                    .ToArray();
            }

            return Factory.TypeReference(Factory.Identifier(symbol.Name), translatedTypeMembers);
        }

        /// <summary>
        /// Translates a type of <c>Func{T1, T2, TRsult}</c> to a TypeScript function type of the
        /// form <c>(t1: T1, t2: T2) =&gt; TResult</c>.
        /// </summary>
        private static ITsFunctionType TranslateFunc(INamedTypeSymbol symbol, ISet<ISymbol> typesToImport)
        {
            var requiredParameters = new List<ITsRequiredParameter>();

            foreach (ITypeSymbol typeArgument in symbol.TypeArguments.Take(symbol.TypeArguments.Length - 1))
            {
                string parameterName = typeArgument.Name;
                parameterName = char.ToLowerInvariant(parameterName[0]) + parameterName.Substring(1);

                var parameterType = TranslateSymbol(typeArgument, typesToImport);
                ITsBoundRequiredParameter requiredParameter = Factory.BoundRequiredParameter(
                    Factory.Identifier(parameterName),
                    parameterType);
                requiredParameters.Add(requiredParameter);
            }

            ITsParameterList parameterList = Factory.ParameterList(requiredParameters: requiredParameters);
            ITsType returnType = TranslateSymbol(symbol.TypeArguments.Last(), typesToImport);
            return Factory.FunctionType(parameterList, returnType);
        }
    }
}
