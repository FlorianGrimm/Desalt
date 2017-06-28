// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5LabelledStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Returns a labelled statement of the form 'Identifier: Statement'.
    /// </summary>
    public sealed class Es5LabelledStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5LabelledStatement(Es5Identifier identifier, IEs5Statement statement)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5Identifier Identifier { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitLabelledStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitLabelledStatement(this);
        }

        public override string CodeDisplay => $"{Identifier}: {Statement}";

        public override void Emit(IndentedTextWriter writer)
        {
            Identifier.Emit(writer);
            writer.Write(": ");
            Statement.Emit(writer);
        }
    }
}
