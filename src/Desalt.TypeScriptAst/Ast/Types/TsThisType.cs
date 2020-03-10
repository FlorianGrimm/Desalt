// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThisType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents the 'this' type.
    /// </summary>
    internal class TsThisType : TsAstNode, ITsThisType
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsThisType Instance = new TsThisType();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsThisType()
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
            visitor.VisitThisType(this);
        }

        public override string CodeDisplay => "this";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("this");
        }
    }
}
