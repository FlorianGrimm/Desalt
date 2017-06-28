// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5SwitchStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'switch' statement.
    /// </summary>
    public sealed class Es5SwitchStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5SwitchStatement(
            IEs5Expression condition,
            IEnumerable<Es5CaseClause> caseClauses = null,
            IEnumerable<IEs5Statement> defaultClauseStatements = null)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            CaseClauses = caseClauses?.ToImmutableArray() ?? ImmutableArray<Es5CaseClause>.Empty;
            DefaultClauseStatements =
                defaultClauseStatements?.ToImmutableArray() ?? ImmutableArray<IEs5Statement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Condition { get; }
        public ImmutableArray<Es5CaseClause> CaseClauses { get; }
        public ImmutableArray<IEs5Statement> DefaultClauseStatements { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitSwitchStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitSwitchStatement(this);
        }

        public override string CodeDisplay => $"switch ({Condition}) {{...}}";

        public override void Emit(IndentedTextWriter writer)
        {
            writer.Write("switch (");
            Condition.Emit(writer);
            writer.Write(") ");

            WriteItems(writer, CaseClauses, indent: true, prefix: "{", itemDelimiter: Environment.NewLine);

            if (DefaultClauseStatements.Length > 0)
            {
                writer.WriteLine("default:");
                WriteItems(writer, DefaultClauseStatements, indent: true, itemDelimiter: Environment.NewLine);
            }

            writer.Write("}");
        }
    }
}
