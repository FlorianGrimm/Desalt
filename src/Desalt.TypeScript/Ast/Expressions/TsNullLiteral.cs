// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNullLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a null literal.
    /// </summary>
    internal class TsNullLiteral : AstNode, ITsNullLiteral
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

        public void Accept(TsVisitor visitor) => visitor.VisitNullLiteral(this);

        public override string CodeDisplay => "null";

        public override void Emit(Emitter emitter) => emitter.Write(CodeDisplay);
    }
}
