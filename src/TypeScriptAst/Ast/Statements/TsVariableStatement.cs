// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVariableStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a variable declaration statement of the form 'var x = y;'.
    /// </summary>
    internal class TsVariableStatement : AstNode, ITsVariableStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsVariableStatement(IEnumerable<ITsVariableDeclaration> declarations)
        {
            Declarations = declarations?.ToImmutableArray() ?? throw new ArgumentNullException(nameof(declarations));
            if (Declarations.IsEmpty)
            {
                throw new ArgumentException("There must be at least one declaration", nameof(declarations));
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsVariableDeclaration> Declarations { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitVariableStatement(this);

        public override string CodeDisplay => $"var {Declarations.ToElidedList()};";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("var ");
            emitter.WriteList(Declarations, indent: false, itemDelimiter: ", ");
            emitter.WriteLine(";");
        }
    }
}
