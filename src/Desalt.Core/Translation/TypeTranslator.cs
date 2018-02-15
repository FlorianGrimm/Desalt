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

        private static readonly ImmutableDictionary<string, ITsType> s_typeMap = new Dictionary<string, ITsType>
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
        }.ToImmutableDictionary();

        private static readonly SymbolDisplayFormat s_displayFormat = new SymbolDisplayFormat(
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsType TranslateSymbol(INamedTypeSymbol symbol)
        {
            string fullTypeName = symbol.ToDisplayString(s_displayFormat);
            if (s_typeMap.ContainsKey(fullTypeName))
            {
                return s_typeMap[fullTypeName];
            }

            return Factory.TypeReference(Factory.Identifier(symbol.Name));
        }
    }
}
