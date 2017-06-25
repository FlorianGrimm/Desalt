// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a function declaration acting as an expression.
    /// </summary>
    internal class TsFunctionExpression : AstNode, ITsFunctionExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsFunctionExpression(
            ITsCallSignature callSignature,
            ITsIdentifier functionName = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            FunctionName = functionName;
            FunctionBody = functionBody?.ToImmutableArray() ?? ImmutableArray<ITsStatementListItem>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier FunctionName { get; }
        public ITsCallSignature CallSignature { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitFunctionExpression(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitFunctionExpression(this);

        public override string ToCodeDisplay() =>
            $"function {FunctionName?.ToCodeDisplay()}{CallSignature} {{ {FunctionBody.ToElidedList()} }}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("function ");
            FunctionName?.WriteFullCodeDisplay(writer);
            CallSignature.WriteFullCodeDisplay(writer);
            WriteBlock(writer, FunctionBody);
        }
    }
}
