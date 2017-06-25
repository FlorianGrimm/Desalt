// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ContinueStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'continue' or 'continue Identifier' statement.
    /// </summary>
    public sealed class Es5ContinueStatement : Es5CodeModel, IEs5Statement
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

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitContinueStatement(this);
        }

        public override string ToCodeDisplay()
        {
            return "continue" + (Label != null ? $" {Label}" : string.Empty) + ";";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("continue");

            if (Label != null)
            {
                writer.Write(" ");
                Label.WriteFullCodeDisplay(writer);
            }

            writer.Write(";");
        }
    }
}
