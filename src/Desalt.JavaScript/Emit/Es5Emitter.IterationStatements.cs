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
            _emitter.Write("do");
            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            Visit(model.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel--;
            }
            else if (_options.SpaceBeforeCompoundStatementKeyword)
            {
                _emitter.Write(" ");
            }

            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while");
            Visit(model.Condition);
            _emitter.Write(");");
        }

        public override void VisitWhileStatement(Es5WhileStatement model)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "while (" : "while(");
            Visit(model.Condition);
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

            Visit(model.Statement);

            if (_options.NewlineBetweenStatements)
            {
                _emitter.IndentLevel--;
            }
        }

        public override void VisitForStatement(Es5ForStatement model)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

            // write declarations/initializers
            if (model.Declarations.Length > 0)
            {
                WriteVariableDeclarations(model.Declarations);
            }
            else
            {
                Visit(model.Initializer);
            }
            _emitter.Write(_options.SpaceAfterSemicolonInForLoop ? "; " : ";");

            // write condition
            Visit(model.Condition);
            _emitter.Write(_options.SpaceAfterSemicolonInForLoop ? "; " : ";");

            // write incrementor
            Visit(model.Incrementor);
            _emitter.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            // write the statement
            Visit(model.Statement);
        }

        public override void VisitForInStatement(Es5ForInStatement model)
        {
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? "for (" : "for(");

            // write declarations/initializers
            if (model.Declaration != null)
            {
                WriteVariableDeclarations(model.Declaration.ToSafeArray());
            }
            else
            {
                Visit(model.LeftSide);
            }

            _emitter.Write(" in ");
            Visit(model.RightSide);
            _emitter.Write(")");

            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            // write the statement
            Visit(model.Statement);
        }
    }
}
