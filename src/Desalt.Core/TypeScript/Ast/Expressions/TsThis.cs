// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThis.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents the 'this' expression.
    /// </summary>
    internal class TsThis : AstNode, ITsThis
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly TsThis Instance = new TsThis();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsThis()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitThis(this);

        public override string CodeDisplay => "this";

        protected override void EmitInternal(Emitter emitter) => emitter.Write("this");
    }
}
