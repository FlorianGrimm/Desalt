// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5VariableStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a variable declaration statement of the form 'var x' or 'var x = y, z'.
    /// </summary>
    public sealed class Es5VariableStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5VariableStatement(IEnumerable<Es5VariableDeclaration> declarations)
        {
            Declarations = declarations?.ToImmutableArray() ?? ImmutableArray<Es5VariableDeclaration>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<Es5VariableDeclaration> Declarations { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitVariableStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitVariableStatement(this);
        }

        public override string ToCodeDisplay() => $"var {Declarations.ToElidedList()};";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("var ");
            WriteItems(writer, Declarations, indent: false, itemDelimiter: ", ");
            writer.Write(";");
        }
    }
}
