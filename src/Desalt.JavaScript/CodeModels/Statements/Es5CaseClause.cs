// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5CaseClause.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'case' clause within a 'switch' statement.
    /// </summary>
    public sealed class Es5CaseClause : Es5CodeModel
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5CaseClause(IEs5Expression expression, IEnumerable<IEs5Statement> statements = null)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Statements = statements?.ToImmutableArray() ?? ImmutableArray<IEs5Statement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Expression { get; }
        public ImmutableArray<IEs5Statement> Statements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitCaseClause(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitCaseClause(this);
        }

        public override string ToCodeDisplay()
        {
            return $"case {Expression}: {Statements.ToElidedList()}";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.WriteLine("case ");
            Expression.WriteFullCodeDisplay(writer);
            writer.Write(": ");
            WriteItems(writer, Statements, indent: true, itemDelimiter: Environment.NewLine);
        }
    }
}
