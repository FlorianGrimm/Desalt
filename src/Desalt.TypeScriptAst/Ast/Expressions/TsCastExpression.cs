// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCastExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a unary cast expression of the form, '&lt;Type&gt;.
    /// </summary>
    internal class TsCastExpression : TsAstNode, ITsCastExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCastExpression(ITsType castType, ITsExpression expression)
        {
            CastType = castType ?? throw new ArgumentNullException(nameof(castType));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        ITsExpression ITsUnaryExpression.Operand => Expression;
        TsUnaryOperator ITsUnaryExpression.Operator => TsUnaryOperator.Cast;

        public ITsType CastType { get; }
        public ITsExpression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitCastExpression(this);

        public override string CodeDisplay => $"<{CastType}>{Expression}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("<");
            CastType.Emit(emitter);
            emitter.Write(">");
            Expression.Emit(emitter);
        }
    }
}
