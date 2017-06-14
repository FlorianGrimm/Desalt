// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptThisExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents the 'this' expression.
    /// </summary>
    internal class TypeScriptThisExpression : CodeModel, ITypeScriptThisExpression
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly TypeScriptThisExpression Instance = new TypeScriptThisExpression();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TypeScriptThisExpression()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor)
        {
            visitor.VisitThisExpresssion(this);
        }

        public T Accept<T>(TypeScriptVisitor<T> visitor)
        {
            return visitor.VisitThisExpresssion(this);
        }

        public override string ToCodeDisplay() => "this";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
