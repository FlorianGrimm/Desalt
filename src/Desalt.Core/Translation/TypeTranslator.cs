// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates known types in C# to known TypeScript types.
    /// </summary>
    internal static class TypeTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly ImmutableDictionary<string, (string nativeTypeName, ITsType? translatedType)>
            s_nativeTypeMap = new Dictionary<string, (string nativeTypeName, ITsType? translatedType)>
            {
                // Number Types
                ["System.Char"] = ("number", Factory.NumberType),
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

                // Object Types
                ["System.Object"] = ("any", Factory.AnyType),
                ["dynamic"] = ("any", Factory.AnyType),

                // Function Types
                ["System.Action"] = ("Function", null),
                ["System.Func"] = ("Function", null),

                // Other Types
                ["System.Array"] = ("Array", null),
                ["System.Boolean"] = ("boolean", Factory.BooleanType),
                ["System.JsDate"] = ("Date", Factory.TypeReference(Factory.Identifier("Date"))),
                ["System.String"] = ("string", Factory.StringType),
                ["System.Void"] = ("void", Factory.VoidType),
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
        public static bool TranslatesToNativeTypeScriptType(ITypeSymbol symbol)
        {
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            return s_nativeTypeMap.ContainsKey(fullTypeName);
        }

        /// <summary>
        /// Returns a value indicating whether the specified type name represents a native TypeScript
        /// type. Examples include void, boolean, string, number, any, Function, Object, or RegExp.
        /// </summary>
        /// <param name="scriptNameOfType">
        /// The script name of the type to inspect ("boolean", "number", etc.).
        /// </param>
        /// <returns>true if the type name is a native TypeScript type; otherwise, false.</returns>
        public static bool IsNativeTypeScriptTypeName(string scriptNameOfType)
        {
            switch (scriptNameOfType)
            {
                // special cases that aren't in the table
                case "Object":
                case "Error":
                    return true;

                default:
                    return s_nativeTypeMap.Values.Select(value => value.nativeTypeName)
                        .Any(nativeTypeName => nativeTypeName.Equals(scriptNameOfType, StringComparison.Ordinal));
            }
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

        /// <summary>
        /// Returns a value indicating if the specified symbol represents a <see cref="Nullable{T}"/> instance.
        /// </summary>
        /// <param name="symbol">The symbol to check.</param>
        /// <returns>True if the symbol is an instance of <see cref="Nullable{T}"/>; otherwise, false.</returns>
        public static bool IsNullableType(ITypeSymbol symbol)
        {
            var namedTypeSymbol = symbol as INamedTypeSymbol;
            return namedTypeSymbol?.OriginalDefinition?.ToHashDisplay() == "System.Nullable<T>";
        }

        /// <summary>
        /// Translates a type symbol into the associated TypeScript equivalent.
        /// </summary>
        /// <param name="context">The <see cref="TranslationContext"/> to use.</param>
        /// <param name="symbol">The type symbol to translate.</param>
        /// <param name="getLocationFunc">
        /// A function returning the location in the source code of the symbol. Used for error reporting.
        /// </param>
        /// <returns>The translated TypeScript <see cref="ITsType"/>.</returns>
        public static ITsType TranslateTypeSymbol(
            TranslationContext context,
            ITypeSymbol symbol,
            Func<Location> getLocationFunc)
        {
            var namedTypeSymbol = symbol as INamedTypeSymbol;

            // special case: Nullable<T> should be translated as 'T | null'
            if (namedTypeSymbol != null &&
                TryTranslateNullableT(context, namedTypeSymbol, getLocationFunc, out ITsType? unionType))
            {
                return unionType;
            }

            // special case: JsDictionary<TKey, TValue> should be translated as `{ [key: string]: TValue }`
            if (namedTypeSymbol != null &&
                TryTranslateJsDictionary(context, namedTypeSymbol, getLocationFunc, out ITsType? objectType))
            {
                return objectType;
            }

            // translate arrays
            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                ITsType elementType = TranslateTypeSymbol(context, arrayTypeSymbol.ElementType, getLocationFunc);
                return Factory.ArrayType(elementType);
            }

            // raw native types are easy to translate
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            if (s_nativeTypeMap.TryGetValue(fullTypeName, out (string nativeTypeName, ITsType? translatedType) value) &&
                value.translatedType != null)
            {
                return value.translatedType;
            }

            // Action<T1, ...> and Func<T1, ...> are special cases
            if (namedTypeSymbol != null && fullTypeName.IsOneOf("System.Action", "System.Func"))
            {
                return TranslateFunc(context, namedTypeSymbol, getLocationFunc);
            }

            // type parameters don't have a script name - just use their name
            if (symbol is ITypeParameterSymbol typeParameterSymbol)
            {
                return Factory.TypeReference(Factory.Identifier(typeParameterSymbol.Name));
            }

            string scriptName = context.ScriptSymbolTable.GetComputedScriptNameOrDefault(symbol, string.Empty);
            if (string.IsNullOrEmpty(scriptName))
            {
                Diagnostic error = DiagnosticFactory.UnknownTypeReference(symbol.ToHashDisplay(), getLocationFunc());

                context.Diagnostics.Add(error);
                scriptName = "UNKNOWN";
            }

            // check for a native type that requires special translation
            if (scriptName == "Array")
            {
                ITsType elementType = Factory.AnyType;
                if (namedTypeSymbol?.TypeArguments.FirstOrDefault() != null)
                {
                    elementType = TranslateTypeSymbol(context, namedTypeSymbol.TypeArguments.First(), getLocationFunc);
                }

                return Factory.ArrayType(elementType);
            }

            // this is a type that we'll need to import since it's not a native type
            context.TypesToImport.Add(symbol);

            // check for generic type arguments
            ITsType[]? translatedTypeMembers = null;
            if (namedTypeSymbol != null)
            {
                ImmutableArray<ITypeSymbol> typeMembers = namedTypeSymbol.TypeArguments;
                translatedTypeMembers = typeMembers
                    .Select(typeMember => TranslateTypeSymbol(context, typeMember, getLocationFunc))
                    .ToArray();
            }

            return Factory.TypeReference(Factory.Identifier(scriptName), translatedTypeMembers);
        }

        /// <summary>
        /// Translates the symbol if it is an instance of <see cref="Nullable{T}"/>.
        /// </summary>
        private static bool TryTranslateNullableT(
            TranslationContext context,
            INamedTypeSymbol namedTypeSymbol,
            Func<Location> getLocationFunc,
            [NotNullWhen(true)] out ITsType? unionType)
        {
            // special case: Nullable<T> should be translated as 'T | null'
            if (namedTypeSymbol?.OriginalDefinition?.ToHashDisplay() != "System.Nullable<T>")
            {
                unionType = null;
                return false;
            }

            ITsType translatedGenericArgument = TranslateTypeSymbol(
                context,
                namedTypeSymbol.TypeArguments[0],
                getLocationFunc);
            unionType = Factory.UnionType(translatedGenericArgument, Factory.NullType);
            return true;
        }

        /// <summary>
        /// Translates the symbol if it is an instance of <c>JsDictionary{TKey, TValue}</c>.
        /// </summary>
        private static bool TryTranslateJsDictionary(
            TranslationContext context,
            INamedTypeSymbol namedTypeSymbol,
            Func<Location> getLocationFunc,
            [NotNullWhen(true)] out ITsType? objectType)
        {
            // special case: JsDictionary<TKey, TValue> should be translated as `{ [key: string]: TValue }`
            if (!JsDictionaryTranslator.IsJsDictionary(namedTypeSymbol))
            {
                objectType = null;
                return false;
            }

            bool isParameterNumberType = false;
            ITsType translatedValueType = Factory.AnyType;

            // examine the type parameters for the key and value types
            if (!namedTypeSymbol.TypeArguments.IsEmpty)
            {
                // see if the key is a number type or a string type
                ITypeSymbol keySymbol = namedTypeSymbol.TypeArguments[0];
                isParameterNumberType = s_nativeTypeMap.TryGetValue(
                        keySymbol.ToDisplayString(s_displayFormat),
                        out (string nativeTypeName, ITsType? translatedType) value) &&
                    value.nativeTypeName == "number";

                // we also need to check enums
                if (keySymbol.TypeKind == TypeKind.Enum)
                {
                    // [NamedValues] are string keys
                    // [NumericValues] are number keys
                    isParameterNumberType = !keySymbol.GetFlagAttribute(SaltarelleAttributeName.NamedValues);
                }

                translatedValueType = TranslateTypeSymbol(context, namedTypeSymbol.TypeArguments[1], getLocationFunc);
            }

            objectType = Factory.ObjectType(
                forceSingleLine: true,
                typeMembers: Factory.IndexSignature(
                    Factory.Identifier("key"),
                    isParameterNumberType: isParameterNumberType,
                    returnType: translatedValueType));

            return true;
        }

        /// <summary>
        /// Translates a type of <c>Func{T1, T2, TResult}</c> to a TypeScript function type of the
        /// form <c>(t1: T1, t2: T2) =&gt; TResult</c>.
        /// </summary>
        private static ITsFunctionType TranslateFunc(
            TranslationContext context,
            INamedTypeSymbol symbol,
            Func<Location> getLocationFunc)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            bool isFunc = symbol.Name == "Func";
            var typeArgs = isFunc ? symbol.TypeArguments.Take(symbol.TypeArguments.Length - 1) : symbol.TypeArguments;

            foreach (ITypeSymbol typeArgument in typeArgs)
            {
                string parameterName = typeArgument.Name;
                parameterName = char.ToLowerInvariant(parameterName[0]) + parameterName.Substring(1);

                ITsType parameterType = TranslateTypeSymbol(context, typeArgument, getLocationFunc);
                ITsBoundRequiredParameter requiredParameter = Factory.BoundRequiredParameter(
                    Factory.Identifier(parameterName),
                    parameterType);
                requiredParameters.Add(requiredParameter);
            }

            ITsParameterList parameterList = Factory.ParameterList(requiredParameters: requiredParameters);

            ITsType returnType = isFunc
                ? TranslateTypeSymbol(context, symbol.TypeArguments.Last(), getLocationFunc)
                : Factory.VoidType;

            return Factory.FunctionType(parameterList, returnType);
        }
    }
}
