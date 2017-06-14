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

        public static ITypeScriptThisExpression ThisExpression => TypeScriptThisExpression.Instance;

        public static ITypeScriptLiteralExpression NullLiteral => TypeScriptLiteralExpression.Null;

        public static ITypeScriptLiteralExpression TrueLiteral => TypeScriptLiteralExpression.True;

        public static ITypeScriptLiteralExpression FalseLiteral => TypeScriptLiteralExpression.False;

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITypeScriptIdentifier Identifier(string name) => TypeScriptIdentifier.Get(name);

        public static ITypeScriptLiteralExpression StringLiteral(string literal) =>
            TypeScriptLiteralExpression.CreateString(literal);

        public static ITypeScriptLiteralExpression DecimalLiteral(string literal) =>
            TypeScriptLiteralExpression.CreateDecimal(literal);

        public static ITypeScriptLiteralExpression BinaryInteger(string literal) =>
            TypeScriptLiteralExpression.CreateBinaryInteger(literal);

        public static ITypeScriptLiteralExpression OctalInteger(string literal) =>
            TypeScriptLiteralExpression.CreateOctalInteger(literal);

        public static ITypeScriptLiteralExpression HexIntegerLiteral(string literal) =>
            TypeScriptLiteralExpression.CreateHexInteger(literal);

        public static ITypeScriptLiteralExpression RegExpLiteral(string literal) =>
            TypeScriptLiteralExpression.CreateRegExp(literal);

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
