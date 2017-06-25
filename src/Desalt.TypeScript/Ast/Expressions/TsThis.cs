// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsThis.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitThis(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitThis(this);

        public override string ToCodeDisplay() => "this";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
