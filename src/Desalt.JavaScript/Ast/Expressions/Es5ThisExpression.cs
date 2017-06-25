// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ThisExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents the 'this' expression.
    /// </summary>
    public class Es5ThisExpression : Es5CodeModel, IEs5Expression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly Es5ThisExpression Instance = new Es5ThisExpression();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5ThisExpression()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitThisExpresssion(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitThisExpresssion(this);
        }

        public override string ToCodeDisplay() => "this";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write(ToCodeDisplay());
        }
    }
}
