// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a function declaration acting as an expression.
    /// </summary>
    internal class TsFunctionExpression : AstNode<TsVisitor>, ITsFunctionExpression
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

        public override void Accept(TsVisitor visitor) => visitor.VisitFunctionExpression(this);

        public override string CodeDisplay =>
            $"function {FunctionName.CodeDisplay}{CallSignature} " +
            $"{{ {FunctionBody.ToElidedList(Environment.NewLine)} }}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("function ");
            FunctionName?.Emit(emitter);
            CallSignature.Emit(emitter);
            WriteBlock(emitter, FunctionBody);
        }
    }
}
