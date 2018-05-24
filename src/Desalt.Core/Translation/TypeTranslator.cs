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
    using System.Linq;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Microsoft.CodeAnalysis;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates known types in C# to known TypeScript types.
    /// </summary>
    internal class TypeTranslator
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
            ["dynamic"] = ("any", Factory.AnyType),
            ["System.Array"] = ("Array", null),
            ["System.Action"] = ("Function", null),
            ["System.Func"] = ("Function", null),
            ["System.Text.RegularExpressions.Regex"] = ("RegExp", null),
        }.ToImmutableDictionary();

        private static readonly SymbolDisplayFormat s_displayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        private readonly ScriptNameSymbolTable _scriptNameSymbolTable;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeTranslator"/> class.
        /// </summary>
        /// <param name="scriptNameSymbolTable">
        /// The script name symbol table. For example, List{T} gets translated to a native array, so
        /// it has a [ScriptName("Array")] attribute.
        /// </param>
        public TypeTranslator(ScriptNameSymbolTable scriptNameSymbolTable)
        {
            _scriptNameSymbolTable =
                scriptNameSymbolTable ?? throw new ArgumentNullException(nameof(scriptNameSymbolTable));
        }

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
            // special cases that aren't in the table
            switch (scriptNameOfType)
            {
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
        /// Translates a type symbol into the associated TypeScript equivalent.
        /// </summary>
        /// <param name="symbol">The type symbol to translate.</param>
        /// <param name="typesToImport">
        /// A set of symbols that will need to be imported. This method will add to the set if necessary.
        /// </param>
        /// <param name="diagnostics">A collection to add errors to.</param>
        /// <param name="getLocationFunc">
        /// A function returning the location in the source code of the symbol. Used for error reporting.
        /// </param>
        /// <returns>The translated TypeScript <see cref="ITsType"/>.</returns>
        public ITsType TranslateSymbol(
            ITypeSymbol symbol,
            ISet<ISymbol> typesToImport,
            ICollection<Diagnostic> diagnostics,
            Func<Location> getLocationFunc)
        {
            typesToImport = typesToImport ?? new HashSet<ISymbol>();
            diagnostics = diagnostics ?? new List<Diagnostic>();

            if (symbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                ITsType elementType = TranslateSymbol(
                    arrayTypeSymbol.ElementType,
                    typesToImport,
                    diagnostics,
                    getLocationFunc);
                return Factory.ArrayType(elementType);
            }

            // raw native types are easy to translate
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            if (s_nativeTypeMap.TryGetValue(fullTypeName, out (string nativeTypeName, ITsType translatedType) value) &&
                value.translatedType != null)
            {
                return value.translatedType;
            }

            // Action<T1, ...> and Func<T1, ...> are special cases
            if (fullTypeName.IsOneOf("System.Action", "System.Func"))
            {
                return TranslateFunc((INamedTypeSymbol)symbol, typesToImport, diagnostics, getLocationFunc);
            }

            // type parameters don't have a script name - just use their name
            if (symbol is ITypeParameterSymbol typeParameterSymbol)
            {
                return Factory.TypeReference(Factory.Identifier(typeParameterSymbol.Name));
            }

            INamedTypeSymbol namedTypeSymbol = symbol as INamedTypeSymbol;

            string scriptName = _scriptNameSymbolTable.GetValueOrDefault(symbol, null);
            if (scriptName == null)
            {
                Diagnostic error = DiagnosticFactory.UnknownTypeReference(
                    SymbolTableUtils.KeyFromSymbol(symbol),
                    getLocationFunc());

                diagnostics.Add(error);
                scriptName = "UNKNOWN";
            }

            // check for a native type that requires special translation
            if (scriptName == "Array")
            {
                ITsType elementType = Factory.AnyType;
                if (namedTypeSymbol?.TypeArguments.FirstOrDefault() != null)
                {
                    elementType = TranslateSymbol(
                        namedTypeSymbol.TypeArguments.First(),
                        typesToImport,
                        diagnostics,
                        getLocationFunc);
                }

                return Factory.ArrayType(elementType);
            }

            // this is a type that we'll need to import since it's not a native type
            typesToImport.Add(symbol);

            // check for generic type arguments
            ITsType[] translatedTypeMembers = null;
            if (namedTypeSymbol != null)
            {
                ImmutableArray<ITypeSymbol> typeMembers = namedTypeSymbol.TypeArguments;
                translatedTypeMembers = typeMembers
                    .Select(typeMember => TranslateSymbol(typeMember, typesToImport, diagnostics, getLocationFunc))
                    .ToArray();
            }

            return Factory.TypeReference(Factory.Identifier(scriptName), translatedTypeMembers);
        }

        /// <summary>
        /// Translates a type of <c>Func{T1, T2, TRsult}</c> to a TypeScript function type of the
        /// form <c>(t1: T1, t2: T2) =&gt; TResult</c>.
        /// </summary>
        private ITsFunctionType TranslateFunc(
            INamedTypeSymbol symbol,
            ISet<ISymbol> typesToImport,
            ICollection<Diagnostic> diagnostics,
            Func<Location> getLocationFunc)
        {
            var requiredParameters = new List<ITsRequiredParameter>();
            bool isFunc = symbol.Name == "Func";
            var typeArgs = isFunc ? symbol.TypeArguments.Take(symbol.TypeArguments.Length - 1) : symbol.TypeArguments;

            foreach (ITypeSymbol typeArgument in typeArgs)
            {
                string parameterName = typeArgument.Name;
                parameterName = char.ToLowerInvariant(parameterName[0]) + parameterName.Substring(1);

                ITsType parameterType = TranslateSymbol(typeArgument, typesToImport, diagnostics, getLocationFunc);
                ITsBoundRequiredParameter requiredParameter = Factory.BoundRequiredParameter(
                    Factory.Identifier(parameterName),
                    parameterType);
                requiredParameters.Add(requiredParameter);
            }

            ITsParameterList parameterList = Factory.ParameterList(requiredParameters: requiredParameters);

            ITsType returnType = isFunc
                ? TranslateSymbol(symbol.TypeArguments.Last(), typesToImport, diagnostics, getLocationFunc)
                : Factory.VoidType;

            return Factory.FunctionType(parameterList, returnType);
        }
    }
}
