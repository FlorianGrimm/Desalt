// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsNullLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitNullLiteral(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitNullLiteral(this);

        public override string ToCodeDisplay() => "null";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer) => writer.Write(ToCodeDisplay());
    }
}
