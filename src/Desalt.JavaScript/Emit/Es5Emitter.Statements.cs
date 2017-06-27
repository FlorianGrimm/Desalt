﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5Emitter.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Emit
{
    using Desalt.JavaScript.Ast;
    using Desalt.JavaScript.Ast.Statements;

    public partial class Es5Emitter
    {
        public override void VisitBlockStatement(Es5BlockStatement node)
        {
            WriteBlock(node);
        }

        /// <summary>
        /// Visits a variable declaration statement of the form 'var x' or 'var x = y, z'.
        /// </summary>
        public override void VisitVariableStatement(Es5VariableStatement node)
        {
            WriteVariableDeclarations(node.Declarations);
            _emitter.Write(";");
        }

        /// <summary>
        /// Visits an empty statement.
        /// </summary>
        public override void VisitEmptyStatement(Es5EmptyStatement node)
        {
            _emitter.Write(";");
        }

        /// <summary>
        /// Visits an expression that is represented as a statement.
        /// </summary>
        public override void VisitExpressionStatement(Es5ExpressionStatement node)
        {
            Visit(node.Expression);
            _emitter.Write(";");
        }

        /// <summary>
        /// Visits an 'if (x) statement; else statement;' statement.
        /// </summary>
        public override void VisitIfStatement(Es5IfStatement node)
        {
            // write the 'if' expression
            _emitter.Write("if");
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? " (" : "(");
            Visit(node.IfExpression);
            _emitter.Write(_options.SpaceAfterClosingStatementParenthesis ? ") " : ")");

            // write the 'if' statement
            Visit(node.IfStatement);

            // write the 'else' statement if necessary
            if (node.ElseStatement == null)
            {
                return;
            }

            _emitter.Write(_options.SpaceBeforeCompoundStatementKeyword ? " else " : "else ");
            Visit(node.ElseStatement);
        }

        /// <summary>
        /// Visits a 'continue' or 'continue Identifier' statement.
        /// </summary>
        public override void VisitContinueStatement(Es5ContinueStatement node)
        {
            _emitter.Write("continue");
            if (node.Label != null)
            {
                _emitter.Write(" ");
                Visit(node.Label);
            }

            _emitter.Write(";");
        }

        /// <summary>
        /// Visits a 'break' or 'break Identifier' statement.
        /// </summary>
        public override void VisitBreakStatement(Es5BreakStatement node)
        {
            _emitter.Write("break");
            if (node.Label != null)
            {
                _emitter.Write(" ");
                Visit(node.Label);
            }

            _emitter.Write(";");
        }

        /// <summary>
        /// Visits a return statement of the form 'return expression;'.
        /// </summary>
        public override void VisitReturnStatement(Es5ReturnStatement node)
        {
            _emitter.Write("return");
            if (node.Expression != null)
            {
                _emitter.Write(" ");
                Visit(node.Expression);
            }

            _emitter.Write(";");
        }

        /// <summary>
        /// Visits a 'with (Expression) Statement' statement.
        /// </summary>
        public override void VisitWithStatement(Es5WithStatement node)
        {
            _emitter.Write("with");
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? " (" : "(");
            Visit(node.Expression);
            _emitter.Write(_options.SpaceAfterClosingStatementParenthesis ? ") " : ")");
            Visit(node.Statement);
        }

        /// <summary>
        /// Visits a labelled statement of the form 'Identifier: Statement'.
        /// </summary>
        public override void VisitLabelledStatement(Es5LabelledStatement node)
        {
            Visit(node.Identifier);
            _emitter.Write(_options.SpaceAfterColon ? ": " : ":");
            Visit(node.Statement);
        }

        /// <summary>
        /// Visits a 'switch' statement.
        /// </summary>
        public override void VisitSwitchStatement(Es5SwitchStatement node)
        {
            _emitter.Write("switch");
            _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? " (" : "(");
            Visit(node.Condition);
            _emitter.Write(_options.SpaceAfterClosingStatementParenthesis ? ") " : ")");

            _emitter.WriteBlock(() =>
            {
                for (int i = 0; i < node.CaseClauses.Length; i++)
                {
                    Es5CaseClause caseClause = node.CaseClauses[i];
                    caseClause.Accept(this);

                    // don't write a new line if this is the last clause and there aren't any default statements
                    bool shouldWriteNewline = _options.NewlineBetweenStatements &&
                        (i < node.CaseClauses.Length - 1 ||
                        node.DefaultClauseStatements.Length > 0);

                    if (shouldWriteNewline)
                    {
                        _emitter.WriteBlankLine();
                    }

                    // we still need to decrease the indentation level if we wrote a new line above
                    if (_options.NewlineBetweenStatements)
                    {
                        _emitter.IndentLevel--;
                    }
                }

                // do we need to write out any default clauses?
                if (node.DefaultClauseStatements.Length == 0)
                {
                    return;
                }

                _emitter.Write("default");
                _emitter.Write(_options.SpaceAfterColon && !_options.NewlineBetweenStatements ? ": " : ":");
                if (_options.NewlineBetweenStatements)
                {
                    _emitter.WriteLine();
                    _emitter.IndentLevel++;
                }

                for (int i = 0; i < node.DefaultClauseStatements.Length; i++)
                {
                    IEs5Statement statement = node.DefaultClauseStatements[i];

                    Visit(statement);

                    // don't write a new line if this is the last statement
                    if (_options.NewlineBetweenStatements && i < node.DefaultClauseStatements.Length - 1)
                    {
                        _emitter.WriteLine();
                    }
                }

                // don't write a new line, but do decrease the indentation
                if (_options.NewlineBetweenStatements)
                {
                    _emitter.IndentLevel--;
                }
            });
        }

        /// <summary>
        /// Visits a 'case' clause.
        /// </summary>
        public override void VisitCaseClause(Es5CaseClause node)
        {
            _emitter.Write("case ");
            Visit(node.Expression);
            _emitter.Write(_options.SpaceAfterColon && !_options.NewlineBetweenStatements ? ": " : ":");
            if (_options.NewlineBetweenStatements)
            {
                _emitter.WriteLine();
                _emitter.IndentLevel++;
            }

            foreach (IEs5Statement statement in node.Statements)
            {
                Visit(statement);
                if (_options.NewlineBetweenStatements)
                {
                    _emitter.WriteLine();
                }
            }
        }

        /// <summary>
        /// Visits a 'throw' statement.
        /// </summary>
        public override void VisitThrowStatement(Es5ThrowStatement node)
        {
            _emitter.Write("throw ");
            Visit(node.Expression);
            _emitter.Write(";");
        }

        /// <summary>
        /// Visits a 'try/catch/finally' statement.
        /// </summary>
        public override void VisitTryStatement(Es5TryStatement node)
        {
            // write the try block
            _emitter.Write("try");
            if (_options.SpaceBeforeOpeningBlockBrace)
            {
                _emitter.Write(" ");
            }

            WriteBlock(node.TryBlock);

            // write the catch block
            if (node.CatchBlock != null)
            {
                if (_options.SpaceAfterClosingBlockBrace)
                {
                    _emitter.Write(" ");
                }

                _emitter.Write("catch");
                _emitter.Write(_options.SpaceBeforeOpeningStatementParenthesis ? " (" : "(");
                Visit(node.CatchIdentifier);
                _emitter.Write(_options.SpaceAfterClosingStatementParenthesis || _options.SpaceBeforeOpeningBlockBrace
                    ? ") "
                    : ")");

                WriteBlock(node.CatchBlock);
            }

            // write the finally block
            if (node.FinallyBlock == null)
            {
                return;
            }

            if (_options.SpaceAfterClosingBlockBrace)
            {
                _emitter.Write(" ");
            }

            _emitter.Write("finally");
            if (_options.SpaceBeforeOpeningBlockBrace)
            {
                _emitter.Write(" ");
            }

            WriteBlock(node.FinallyBlock);
        }

        /// <summary>
        /// Visits a 'debugger' statement.
        /// </summary>
        public override void VisitDebuggerStatement(Es5DebuggerStatement node)
        {
            _emitter.Write("debugger;");
        }
    }
}