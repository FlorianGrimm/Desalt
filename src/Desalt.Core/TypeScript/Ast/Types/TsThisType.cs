// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThisType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents the 'this' type.
    /// </summary>
    internal class TsThisType : AstNode<TsVisitor>, ITsThisType
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

        public override void Accept(TsVisitor visitor) => visitor.VisitThisType(this);

        public override string CodeDisplay => "this";

        public override void Emit(Emitter emitter) => emitter.Write("this");
    }
}
