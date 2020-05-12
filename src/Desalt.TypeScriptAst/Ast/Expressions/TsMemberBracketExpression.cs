// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberBracketExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a member expression of the form 'expression[expression]'.
    /// </summary>
    internal class TsMemberBracketExpression : TsAstNode, ITsMemberBracketExpression, ITsSuperBracketExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsMemberBracketExpression(ITsExpression leftSide, ITsExpression bracketContents, bool isSuper)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            BracketContents = bracketContents ?? throw new ArgumentNullException(nameof(bracketContents));
            IsSuper = isSuper;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public ITsExpression BracketContents { get; }
        public bool IsSuper { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsMemberBracketExpression Create(ITsExpression leftSide, ITsExpression bracketContents)
        {
            return new TsMemberBracketExpression(leftSide, bracketContents, isSuper: false);
        }

        public static TsMemberBracketExpression CreateSuper(ITsExpression bracketContents)
        {
            return new TsMemberBracketExpression(TsIdentifier.Get("super"), bracketContents, isSuper: true);
        }

        public override void Accept(TsVisitor visitor)
        {
            if (IsSuper)
            {
                visitor.VisitSuperBracketExpression(this);
            }
            else
            {
                visitor.VisitMemberBracketExpression(this);
            }
        }

        public override string CodeDisplay => $"{LeftSide}[{BracketContents}]";

        protected override void EmitContent(Emitter emitter)
        {
            LeftSide.Emit(emitter);
            emitter.Write("[");
            BracketContents.Emit(emitter);
            emitter.Write("]");
        }
    }
}
