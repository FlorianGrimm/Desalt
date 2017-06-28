// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsConditionalExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a conditional expression of the form 'x ? y : z'.
    /// </summary>
    internal class TsConditionalExpression : AstNode, ITsConditionalExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsConditionalExpression(
            ITsExpression condition,
            ITsExpression whenTrue,
            ITsExpression whenFalse)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            WhenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
            WhenFalse = whenFalse ?? throw new ArgumentNullException(nameof(whenFalse));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Condition { get; }
        public ITsExpression WhenTrue { get; }
        public ITsExpression WhenFalse { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitConditionalExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitConditionalExpression(this);

        public override string CodeDisplay => $"{Condition} ? {WhenTrue} : {WhenFalse}";

        public override void Emit(IndentedTextWriter writer)
        {
            Condition.Emit(writer);
            writer.Write(" ? ");
            WhenTrue.Emit(writer);
            writer.Write(" : ");
            WhenFalse.Emit(writer);
        }
    }
}
