// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNewTargetExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents an expression of the form 'new.target'.
    /// </summary>
    internal class TsNewTargetExpression : TsAstNode, ITsNewTargetExpression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsNewTargetExpression Instance = new TsNewTargetExpression();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsNewTargetExpression()
        {
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitNewTargetExpression(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("new.target");
        }
    }
}
