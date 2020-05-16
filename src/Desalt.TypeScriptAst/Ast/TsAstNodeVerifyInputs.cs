// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstNodeVerifyInputs.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

// ReSharper disable UnusedParameterInPartialMethod
#pragma warning disable IDE0060 // Remove unused parameter

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Immutable;
    using Desalt.CompilerUtilities.Extensions;

    internal partial class TsNumericLiteral
    {
        /// <summary>
        /// The maximum safe integer in JavaScript (2 ^ 53 - 1).
        /// </summary>
        private static readonly long s_maxSafeInteger = (long)(Math.Pow(2, 53) - 1);

        partial void VerifyInputs(double value, TsNumericLiteralKind kind)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                throw new ArgumentException("Value cannot be infinity or NaN", nameof(value));
            }

            if (value < 0)
            {
                throw new ArgumentException("Value must be positive", nameof(value));
            }

            if (kind.IsOneOf(
                TsNumericLiteralKind.BinaryInteger, TsNumericLiteralKind.OctalInteger, TsNumericLiteralKind.HexInteger) &&
                value > s_maxSafeInteger)
            {
                throw new ArgumentException($"Integers must be less than {s_maxSafeInteger}", nameof(value));
            }
        }
    }

    internal partial class TsLexicalDeclaration
    {
        partial void VerifyInputs(bool isConst, ImmutableArray<ITsLexicalBinding> declarations)
        {
            if (declarations.IsEmpty)
            {
                throw new ArgumentException("There must be at least one declaration", nameof(declarations));
            }
        }
    }

    internal partial class TsVariableStatement
    {
        partial void VerifyInputs(ImmutableArray<ITsVariableDeclaration> declarations)
        {
            if (declarations.IsEmpty)
            {
                throw new ArgumentException("There must be at least one declaration", nameof(declarations));
            }
        }
    }
}
