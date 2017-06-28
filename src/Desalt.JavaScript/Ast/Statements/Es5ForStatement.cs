// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ForStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'for' loop statement.
    /// </summary>
    public sealed class Es5ForStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ForStatement(
            IEs5Statement statement,
            IEnumerable<Es5VariableDeclaration> declarations = null,
            IEs5Expression condition = null,
            IEs5Expression incrementer = null)
            : this(statement, declarations, null, condition, incrementer)
        {
        }

        internal Es5ForStatement(
            IEs5Statement statement,
            IEs5Expression initializer = null,
            IEs5Expression condition = null,
            IEs5Expression incrementer = null)
            : this(statement, null, initializer, condition, incrementer)
        {
        }

        private Es5ForStatement(
            IEs5Statement statement,
            IEnumerable<Es5VariableDeclaration> declarations = null,
            IEs5Expression initializer = null,
            IEs5Expression condition = null,
            IEs5Expression incrementor = null)
        {
            if (declarations != null && initializer != null)
            {
                throw new ArgumentException("Only declarations or initializer can be defined");
            }

            Declarations = declarations?.ToImmutableArray() ?? ImmutableArray<Es5VariableDeclaration>.Empty;
            Initializer = initializer;
            Condition = condition;
            Incrementor = incrementor;
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<Es5VariableDeclaration> Declarations { get; }

        public IEs5Expression Initializer { get; }

        public IEs5Expression Condition { get; }

        public IEs5Expression Incrementor { get; }

        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitForStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitForStatement(this);
        }

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder("for (");
                if (Initializer != null)
                {
                    builder.Append(Initializer);
                }
                builder.Append("; ");

                if (Condition != null)
                {
                    builder.Append(Condition);
                }
                builder.Append("; ");

                if (Incrementor != null)
                {
                    builder.Append(Incrementor);
                }
                builder.AppendLine(")");

                builder.Append(Statement);

                return builder.ToString();
            }
        }

        public override void Emit(IndentedTextWriter writer)
        {
            writer.Write("for (");
            Initializer?.Emit(writer);
            writer.Write("; ");

            Condition?.Emit(writer);
            writer.Write("; ");

            Incrementor?.Emit(writer);
            writer.WriteLine(")");

            Statement.Emit(writer);
        }
    }
}
