// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThisType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents the 'this' type.
    /// </summary>
    internal class TsThisType : AstNode, ITsThisType
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitThisType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitThisType(this);

        public override string ToCodeDisplay() => "this";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write("this");
    }
}
