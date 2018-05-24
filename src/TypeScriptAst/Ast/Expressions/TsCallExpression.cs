// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCallExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Expressions
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a call epression of the form 'expression(arguments)'.
    /// </summary>
    internal class TsCallExpression : AstNode, ITsCallExpression, ITsNewCallExpression, ITsSuperCallExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsCallExpression(ITsExpression leftSide, CallKind kind, ITsArgumentList arguments = null)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            Arguments = arguments ?? new TsArgumentList();
            Kind = kind;
        }

        //// ===========================================================================================================
        //// Enums
        //// ===========================================================================================================

        private enum CallKind
        {
            Call,
            New,
            Super,
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public ITsArgumentList Arguments { get; }

        private CallKind Kind { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsCallExpression Create(ITsExpression leftSide, ITsArgumentList arguments = null) =>
            new TsCallExpression(leftSide, CallKind.Call, arguments);

        public static TsCallExpression CreateNew(ITsExpression leftSide, ITsArgumentList arguments = null) =>
            new TsCallExpression(leftSide, CallKind.New, arguments);

        public static TsCallExpression CreateSuper(ITsArgumentList arguments = null) =>
            new TsCallExpression(TsIdentifier.Get("super"), CallKind.Super, arguments);

        public override void Accept(TsVisitor visitor)
        {
            switch (Kind)
            {
                case CallKind.Call:
                    visitor.VisitCallExpression(this);
                    break;

                case CallKind.New:
                    visitor.VisitNewCallExpression(this);
                    break;

                case CallKind.Super:
                    visitor.VisitSuperCallExpression(this);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string CodeDisplay =>
            (Kind == CallKind.New ? "new " : "") + $"{LeftSide}{Arguments}";

        protected override void EmitInternal(Emitter emitter)
        {
            if (Kind == CallKind.New)
            {
                emitter.Write("new ");
            }

            LeftSide.Emit(emitter);
            Arguments.Emit(emitter);
        }
    }
}
