// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.IterationStatements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.Ast.Statements;

    public partial class Es5Emitter
    {
        public override void VisitDoStatement(Es5DoStatement node)
        {
            _emitter.Write("do");
            _emitter.WriteLine();
            _emitter.IndentLevel++;

            Visit(node.Statement);

            _emitter.WriteLine();
            _emitter.IndentLevel--;

            _emitter.Write("while (");
            Visit(node.Condition);
            _emitter.Write(");");
        }

        public override void VisitWhileStatement(Es5WhileStatement node)
        {
            _emitter.Write("while (");
            Visit(node.Condition);
            _emitter.Write(")");

            _emitter.WriteLine();
            _emitter.IndentLevel++;

            Visit(node.Statement);

            _emitter.IndentLevel--;
        }

        public override void VisitForStatement(Es5ForStatement node)
        {
            _emitter.Write("for (");

            // write declarations/initializers
            if (node.Declarations.Length > 0)
            {
                WriteVariableDeclarations(node.Declarations);
            }
            else
            {
                Visit(node.Initializer);
            }
            _emitter.Write("; ");

            // write condition
            Visit(node.Condition);
            _emitter.Write("; ");

            // write incrementor
            Visit(node.Incrementor);
            _emitter.Write(")");

            _emitter.WriteLine();
            _emitter.IndentLevel++;

            // write the statement
            Visit(node.Statement);

            _emitter.IndentLevel--;
        }

        public override void VisitForInStatement(Es5ForInStatement node)
        {
            _emitter.Write("for (");

            // write declarations/initializers
            if (node.Declaration != null)
            {
                WriteVariableDeclarations(node.Declaration.ToSafeArray());
            }
            else
            {
                Visit(node.LeftSide);
            }

            _emitter.Write(" in ");
            Visit(node.RightSide);
            _emitter.Write(")");

            _emitter.WriteLine();
            _emitter.IndentLevel++;

            // write the statement
            Visit(node.Statement);

            _emitter.IndentLevel--;
        }
    }
}
