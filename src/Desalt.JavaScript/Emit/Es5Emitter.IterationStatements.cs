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
            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            Visit(node.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel--;
            }
            else
            {
                _emitter.Write(" ");
            }

            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while");
            Visit(node.Condition);
            _emitter.Write(");");
        }

        public override void VisitWhileStatement(Es5WhileStatement node)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while(");
            Visit(node.Condition);
            _emitter.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }
            else if (_options.SpaceAfterClosingStatementParenthesis)
            {
                _emitter.WriteLine(" ");
            }

            Visit(node.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _emitter.IndentLevel--;
            }
        }

        public override void VisitForStatement(Es5ForStatement node)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

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

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            // write the statement
            Visit(node.Statement);
        }

        public override void VisitForInStatement(Es5ForInStatement node)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

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

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            // write the statement
            Visit(node.Statement);
        }
    }
}
