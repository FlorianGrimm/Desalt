// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ContinueStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'continue' or 'continue Identifier' statement.
    /// </summary>
    public sealed class Es5ContinueStatement : AstNode<Es5Visitor>, IEs5Statement
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly Es5ContinueStatement UnlabelledContinueStatement = new Es5ContinueStatement();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ContinueStatement(Es5Identifier label = null)
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
            visitor.VisitContinueStatement(this);
        }

        public override string CodeDisplay => "continue" + (Label != null ? $" {Label}" : string.Empty) + ";";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("continue");

            if (Label != null)
            {
                emitter.Write(" ");
                Label.Emit(emitter);
            }

            emitter.Write(";");
        }
    }
}
