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
    using Desalt.TypeScript.CodeModels.Types;

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

        public static readonly ITsObjectLiteral EmptyObjectLiteral = new TsObjectLiteral(null);

        public static readonly ITsPredefinedType Any = TsPredefinedType.Any;
        public static readonly ITsPredefinedType Number = TsPredefinedType.Number;
        public static readonly ITsPredefinedType Boolean = TsPredefinedType.Boolean;
        public static readonly ITsPredefinedType String = TsPredefinedType.String;
        public static readonly ITsPredefinedType Symbol = TsPredefinedType.Symbol;
        public static readonly ITsPredefinedType Void = TsPredefinedType.Void;

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static ITsIdentifier Identifier(string name) => TsIdentifier.Get(name);

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

        //// ===========================================================================================================
        //// Types
        //// ===========================================================================================================

        public static ITsTypeParameter TypeParameter(ITsIdentifier typeName, ITsType constraint = null) =>
            new TsTypeParameter(typeName, constraint);

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
