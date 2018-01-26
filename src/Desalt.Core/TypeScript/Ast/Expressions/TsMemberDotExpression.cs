// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberDotExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a member expression of the form 'expression.name'.
    /// </summary>
    internal class TsMemberDotExpression : AstNode, ITsMemberDotExpression, ITsSuperDotExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsMemberDotExpression(ITsExpression leftSide, string dotName, bool isSuper)
        {
            Param.VerifyString(dotName, nameof(dotName));

            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            DotName = dotName;
            IsSuper = isSuper;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public string DotName { get; }
        public bool IsSuper { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsMemberDotExpression Create(ITsExpression leftSide, string dotName) =>
            new TsMemberDotExpression(leftSide, dotName, isSuper: false);

        public static TsMemberDotExpression CreateSuper(string dotName) =>
            new TsMemberDotExpression(TsIdentifier.Get("super"), dotName, isSuper: true);

        public override void Accept(TsVisitor visitor)
        {
            if (IsSuper)
            {
                visitor.VisitSuperDotExpression(this);
            }
            else
            {
                visitor.VisitMemberDotExpression(this);
            }
        }

        public override string CodeDisplay => $"{LeftSide}.{DotName}";

        protected override void EmitInternal(Emitter emitter)
        {
            LeftSide.Emit(emitter);
            emitter.Write($".{DotName}");
        }
    }
}
