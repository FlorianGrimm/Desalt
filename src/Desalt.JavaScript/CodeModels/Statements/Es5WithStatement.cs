// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5WithStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'with (Expression) Statement' statement.
    /// </summary>
    public sealed class Es5WithStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5WithStatement(IEs5Expression expression, IEs5Statement statement)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Expression { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitWithStatement(this);
        }

        public override string ToCodeDisplay() => $"with ({Expression}) {Statement}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("with (");
            Expression.WriteFullCodeDisplay(writer);
            writer.WriteLine(")");
            Statement.WriteFullCodeDisplay(writer);
        }
    }
}
