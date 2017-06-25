// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5DoStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a do-while statement.
    /// </summary>
    public sealed class Es5DoStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5DoStatement(IEs5Statement statement, IEs5Expression condition)
        {
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Statement Statement { get; }
        public IEs5Expression Condition { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitDoStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitDoStatement(this);
        }

        public override string ToCodeDisplay()
        {
            return $"do ({Condition}) {Statement}";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("do (");
            Condition.WriteFullCodeDisplay(writer);
            writer.Write(") ");
            Statement.WriteFullCodeDisplay(writer);
        }
    }
}
