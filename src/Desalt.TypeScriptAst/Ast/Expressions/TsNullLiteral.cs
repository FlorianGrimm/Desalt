// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNullLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a null literal.
    /// </summary>
    internal class TsNullLiteral : TsAstNode, ITsNullLiteral
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsNullLiteral Instance = new TsNullLiteral();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsNullLiteral()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitNullLiteral(this);
        }

        public override string CodeDisplay => "null";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("null");
        }
    }
}
