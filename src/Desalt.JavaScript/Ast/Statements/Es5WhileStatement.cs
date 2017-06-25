// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5WhileStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'while' loop statement.
    /// </summary>
    public sealed class Es5WhileStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5WhileStatement(IEs5Expression condition, IEs5Statement statement)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Condition { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitWhileStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitWhileStatement(this);
        }

        public override string ToCodeDisplay() => $"while ({Condition}) {Statement}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("while (");
            Condition.WriteFullCodeDisplay(writer);
            writer.WriteLine(")");
            Statement.WriteFullCodeDisplay(writer);
        }
    }
}
