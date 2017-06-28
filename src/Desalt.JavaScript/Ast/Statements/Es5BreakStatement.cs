// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5BreakStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'break' or 'break Identifier' statement.
    /// </summary>
    public sealed class Es5BreakStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly Es5BreakStatement UnlabelledBreakStatement = new Es5BreakStatement();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5BreakStatement(Es5Identifier label = null)
        {
            Label = label;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5Identifier Label { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitBreakStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }

        public override string CodeDisplay
        {
            get { return "break" + (Label != null ? $" {Label}" : string.Empty) + ";"; }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("break");

            if (Label != null)
            {
                emitter.Write(" ");
                Label.Emit(emitter);
            }

            emitter.Write(";");
        }
    }
}
