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

        public static ITsNullLiteral NullLiteral => TsNullLiteral.Instance;

        public static ITsBooleanLiteral TrueLiteral => TsBooleanLiteral.True;

        public static ITsBooleanLiteral FalseLiteral => TsBooleanLiteral.False;

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

        public static ITsNumericLiteral DecimalLiteral(double value) =>
            new TsNumericLiteral(TsNumericLiteralKind.Decimal, value);

        public static ITsNumericLiteral BinaryIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.BinaryInteger, value);

        public static ITsNumericLiteral OctalIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.OctalInteger, value);

        public static ITsNumericLiteral HexIntegerLiteral(long value) =>
            new TsNumericLiteral(TsNumericLiteralKind.HexInteger, value);

        public static ITsArrayLiteral ArrayLiteral(params ITsArrayElement[] elements) => new TsArrayLiteral(elements);

        public static ITsArrayElement ArrayElement(ITsAssignmentExpression element, bool isSpreadElement = false) =>
            new TsArrayElement(element, isSpreadElement);

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
