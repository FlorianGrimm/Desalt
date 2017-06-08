// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.IterationStatements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using Desalt.Core.Extensions;
    using Desalt.JavaScript.CodeModels.Statements;

    public partial class Es5Emitter
    {
        public override void VisitDoStatement(Es5DoStatement model)
        {
            _writer.Write("do");
            if (_options.NewlineBetweenStatements)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }

            Visit(model.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _writer.WriteLine();
                _writer.IndentLevel--;
            }
            else if (_options.SpaceBeforeCompoundStatementKeyword)
            {
                _writer.Write(" ");
            }

            _writer.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while");
            Visit(model.Condition);
            _writer.Write(");");
        }

        public override void VisitWhileStatement(Es5WhileStatement model)
        {
            _writer.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while(");
            Visit(model.Condition);
            _writer.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }
            else if (_options.SpaceAfterClosingStatementParenthesis)
            {
                _writer.WriteLine(" ");
            }

            Visit(model.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _writer.IndentLevel--;
            }
        }

        public override void VisitForStatement(Es5ForStatement model)
        {
            _writer.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

            // write declarations/initializers
            if (model.Declarations.Length > 0)
            {
                WriteVariableDeclarations(model.Declarations);
            }
            else
            {
                Visit(model.Initializer);
            }
            _writer.Write(_options.SpaceAfterSemicolonInForLoop ? "; " : ";");

            // write condition
            Visit(model.Condition);
            _writer.Write(_options.SpaceAfterSemicolonInForLoop ? "; " : ";");

            // write incrementor
            Visit(model.Incrementor);
            _writer.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }

            // write the statement
            Visit(model.Statement);
        }

        public override void VisitForInStatement(Es5ForInStatement model)
        {
            _writer.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

            // write declarations/initializers
            if (model.Declaration != null)
            {
                WriteVariableDeclarations(model.Declaration.ToSafeArray());
            }
            else
            {
                Visit(model.LeftSide);
            }

            _writer.Write(" in ");
            Visit(model.RightSide);
            _writer.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _writer.WriteLine();
                _writer.IndentLevel++;
            }

            // write the statement
            Visit(model.Statement);
        }
    }
}
