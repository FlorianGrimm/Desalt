// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberDotExpression.cs" company="Justin Rockwood">
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
    /// Represents a member expression of the form 'expression.name'.
    /// </summary>
    internal class TsMemberDotExpression : AstNode, ITsMemberDotExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMemberDotExpression(ITsExpression leftSide, string dotName)
        {
            Param.VerifyString(dotName, nameof(dotName));

            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            DotName = dotName;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression LeftSide { get; }
        public string DotName { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitMemberDotExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitMemberDotExpression(this);

        public override string CodeDisplay => $"{LeftSide}.{DotName}";

        public override void Emit(IndentedTextWriter writer)
        {
            LeftSide.Emit(writer);
            writer.Write($".{DotName}");
        }
    }
}
