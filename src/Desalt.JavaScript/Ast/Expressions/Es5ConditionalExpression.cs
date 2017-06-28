// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ConditionalExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a conditional expression of the form 'x ? y : z'.
    /// </summary>
    public sealed class Es5ConditionalExpression : Es5AstNode, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ConditionalExpression(
            IEs5Expression condition,
            IEs5Expression whenTrue,
            IEs5Expression whenFalse)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            WhenTrue = whenTrue ?? throw new ArgumentNullException(nameof(whenTrue));
            WhenFalse = whenFalse ?? throw new ArgumentNullException(nameof(whenFalse));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Condition { get; }
        public IEs5Expression WhenTrue { get; }
        public IEs5Expression WhenFalse { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitConditionalExpression(this);
        }

        public override string CodeDisplay => $"{Condition} ? {WhenTrue} : {WhenFalse}";

        public override void Emit(Emitter emitter)
        {
            Condition.Emit(emitter);
            emitter.Write(" ? ");
            WhenTrue.Emit(emitter);
            emitter.Write(" : ");
            WhenFalse.Emit(emitter);
        }
    }
}
