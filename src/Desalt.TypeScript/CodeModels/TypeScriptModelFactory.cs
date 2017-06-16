// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptModelFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels
{
    using System.Collections.Generic;
    using Desalt.TypeScript.CodeModels.Expressions;

    /// <summary>
    /// Provides factory methods for creating TypeScript code models.
    /// </summary>
    public static class TypeScriptModelFactory
    {
        //// ===========================================================================================================
        //// Singleton Properties
        //// ===========================================================================================================

        public static ITsThis This => TsThis.Instance;

        public static ITsLiteralExpression NullLiteral => TsLiteralExpression.Null;

        public static ITsLiteralExpression TrueLiteral => TsLiteralExpression.True;

        public static ITsLiteralExpression FalseLiteral => TsLiteralExpression.False;

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

        public static ITsLiteralExpression StringLiteral(string literal) =>
            TsLiteralExpression.CreateString(literal);

        public static ITsLiteralExpression DecimalLiteral(string literal) =>
            TsLiteralExpression.CreateDecimal(literal);

        public static ITsLiteralExpression BinaryInteger(string literal) =>
            TsLiteralExpression.CreateBinaryInteger(literal);

        public static ITsLiteralExpression OctalInteger(string literal) =>
            TsLiteralExpression.CreateOctalInteger(literal);

        public static ITsLiteralExpression HexIntegerLiteral(string literal) =>
            TsLiteralExpression.CreateHexInteger(literal);

        public static ITsLiteralExpression RegExpLiteral(string literal) =>
            TsLiteralExpression.CreateRegExp(literal);

        public static ITsArrayLiteralExpression ArrayLiteral(params ITsExpression[] elements)
        {
            return new TsArrayLiteralExpression(elements);
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
