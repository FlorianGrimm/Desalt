// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System;
    using System.Collections.Generic;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    public partial class TsParser
    {
        /// <summary>
        /// Parses a series of statement list items.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// StatementList:
        ///     StatementListItem
        ///     StatementList StatementListItem
        ///
        /// StatementListItem:
        ///     Statement
        ///     Declaration
        /// ]]></code></remarks>
        private ITsStatementListItem[] ParseStatementList()
        {
            var items = new List<ITsStatementListItem>();

            while (true)
            {
                if (IsStartOfStatement())
                {
                    ITsStatement statement = ParseStatement();
                    items.Add(statement);
                }
                else if (IsStartOfDeclaration())
                {
                    ITsDeclaration declaration = ParseDeclaration();
                    items.Add(declaration);
                }
                else
                {
                    break;
                }
            }

            return items.ToArray();
        }

        /// <summary>
        /// Returns a value indicating whether the specified token code can start a Statement production.
        /// </summary>
        private bool IsStartOfStatement()
        {
            TsTokenCode tokenCode = _reader.Peek().TokenCode;

            if (tokenCode.IsOneOf(
                TsTokenCode.LeftBrace,
                TsTokenCode.Var,
                TsTokenCode.Semicolon,
                TsTokenCode.If,
                TsTokenCode.Do,
                TsTokenCode.Switch,
                TsTokenCode.Continue,
                TsTokenCode.Break,
                TsTokenCode.Return,
                TsTokenCode.With,
                TsTokenCode.Throw,
                TsTokenCode.Try,
                TsTokenCode.Debugger))
            {
                return true;
            }

            // LabeledStatement
            if (IsStartOfIdentifier(tokenCode) && _reader.Peek(2)[1].TokenCode == TsTokenCode.Colon)
            {
                return true;
            }

            return IsStartOfExpressionStatement();
        }

        /// <summary>
        /// Returns a value indicating whether the specified token code can start an
        /// ExpressionStatement production.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ExpressionStatement:
        ///   [lookahead not { {, function, class, let [ }] Expression ;
        /// ]]></code></remarks>
        private bool IsStartOfExpressionStatement()
        {
            return !_reader.Peek().TokenCode.IsOneOf(TsTokenCode.LeftBrace, TsTokenCode.Function, TsTokenCode.Class) &&
                !_reader.IsNext(TsTokenCode.Let, TsTokenCode.LeftBracket) &&
                IsStartOfExpression();
        }

        /// <summary>
        /// Parses a statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Statement:
        ///     BlockStatement          (starts with '{')
        ///     VariableStatement       (starts with 'var')
        ///     EmptyStatement          (starts with ';')
        ///     ExpressionStatement
        ///     IfStatement             (starts with 'if')
        ///     BreakableStatement      (starts with 'do' or 'switch')
        ///     ContinueStatement       (starts with 'continue')
        ///     BreakStatement          (starts with 'break')
        ///     ReturnStatement         (starts with 'return')
        ///     WithStatement           (starts with 'with')
        ///     LabeledStatement       (starts with Identifier ':')
        ///     ThrowStatement          (starts with 'throw')
        ///     TryStatement            (starts with 'try')
        ///     DebuggerStatement       (starts with 'debugger')
        ///
        /// BreakableStatement:
        ///   IterationStatement
        ///   SwitchStatement
        /// ]]></code></remarks>
        private ITsStatement ParseStatement()
        {
            switch (_reader.Peek().TokenCode)
            {
                // BlockStatement
                case TsTokenCode.LeftBrace:
                    return ParseBlockStatement();

                // VariableStatement
                case TsTokenCode.Var:
                    return ParseVariableStatement();

                // EmptyStatement: ;
                case TsTokenCode.Semicolon:
                    _reader.Skip();
                    return Factory.EmptyStatement;

                // IfStatement
                case TsTokenCode.If:
                    return ParseIfStatement();

                // BreakableStatement -> IterationStatement (which is a 'do/while' loop)
                case TsTokenCode.Do:
                    return ParseIterationStatement();

                // BreakableStatement -> SwitchStatement
                case TsTokenCode.Switch:
                    return ParseSwitchStatement();

                // ContinueStatement
                case TsTokenCode.Continue:
                    return ParseContinueStatement();

                // BreakStatement
                case TsTokenCode.Break:
                    return ParseBreakStatement();

                // ReturnStatement
                case TsTokenCode.Return:
                    return ParseReturnStatement();

                // WithStatement
                case TsTokenCode.With:
                    return ParseWithStatement();

                // LabeledStatement
                // ReSharper disable once PatternAlwaysMatches
                case TsTokenCode tc when IsStartOfIdentifier(tc) && _reader.Peek(2)[1].TokenCode == TsTokenCode.Colon:
                    return ParseLabeledStatement();

                // ThrowStatement
                case TsTokenCode.Throw:
                    return ParseThrowStatement();

                // TryStatement
                case TsTokenCode.Try:
                    return ParseTryStatement();

                // DebuggerStatement: debugger ;
                case TsTokenCode.Debugger:
                    _reader.Skip();
                    Read(TsTokenCode.Semicolon);
                    return Factory.Debugger;
            }

            // ExpressionStatement
            if (IsStartOfExpressionStatement())
            {
                ITsExpression expression = ParseExpression();
                return Factory.ExpressionStatement(expression);
            }

            throw new TsParserException($"Unknown statement: {_reader.Peek()}", _reader.Peek().Location);
        }

        /// <summary>
        /// Tries to parse a block statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BlockStatement:
        ///     Block
        ///
        /// Block:
        ///     { StatementListOpt }
        /// ]]></code></remarks>
        private ITsBlockStatement ParseBlockStatement()
        {
            Read(TsTokenCode.LeftBrace);

            if (_reader.ReadIf(TsTokenCode.RightBrace))
            {
                return Factory.Block();
            }

            ITsStatementListItem[] statements = ParseStatementList();
            Read(TsTokenCode.RightBrace);

            return Factory.Block(statements);
        }

        /// <summary>
        /// Tries to parse a variable statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// VariableStatement:
        ///     var VariableDeclarationList ;
        /// ]]></code></remarks>
        private ITsVariableStatement ParseVariableStatement()
        {
            Read(TsTokenCode.Var);
            ITsVariableDeclaration[] declarations = ParseVariableDeclarationList();
            Read(TsTokenCode.Semicolon);

            return Factory.VariableStatement(declarations);
        }

        /// <summary>
        /// Parses a variable declaration list.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// VariableDeclarationList:
        ///     VariableDeclaration
        ///     VariableDeclarationList , VariableDeclaration
        /// ]]></code></remarks>
        private ITsVariableDeclaration[] ParseVariableDeclarationList()
        {
            var declarations = new List<ITsVariableDeclaration>();

            do
            {
                ITsVariableDeclaration declaration = ParseVariableDeclaration();
                declarations.Add(declaration);
            }
            while (_reader.ReadIf(TsTokenCode.Comma));

            return declarations.ToArray();
        }

        /// <summary>
        /// Parses a variable declaration.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// VariableDeclaration: ( Modified )
        ///     SimpleVariableDeclaration
        ///     DestructuringVariableDeclaration
        ///
        /// SimpleVariableDeclaration:
        ///     BindingIdentifier TypeAnnotationOpt InitializerOpt
        ///
        /// DestructuringVariableDeclaration:
        ///     BindingPattern TypeAnnotationOpt Initializer
        /// ]]></code></remarks>
        private ITsVariableDeclaration ParseVariableDeclaration()
        {
            ITsVariableDeclaration declaration;

            // SimpleVariableDeclaration
            if (TryParseIdentifier(out ITsIdentifier? variableName))
            {
                ITsType? variableType = ParseOptionalTypeAnnotation();
                ITsExpression? initializer = ParseOptionalInitializer();

                declaration = Factory.SimpleVariableDeclaration(variableName, variableType, initializer);
            }

            // DestructuringVariableDeclaration
            else
            {
                ITsBindingPattern bindingPattern = ParseBindingPattern();
                ITsType? variableType = ParseOptionalTypeAnnotation();
                ITsExpression initializer = ParseInitializer();

                declaration = Factory.DestructuringVariableDeclaration(bindingPattern, variableType, initializer);
            }

            return declaration;
        }

        /// <summary>
        /// Parses an 'if' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IfStatement:
        ///   if ( Expression ) Statement else Statement
        ///   if ( Expression ) Statement
        /// ]]></code></remarks>
        private ITsIfStatement ParseIfStatement()
        {
            Read(TsTokenCode.If);

            Read(TsTokenCode.LeftParen);
            ITsExpression ifCondition = ParseExpression();
            Read(TsTokenCode.RightParen);

            ITsStatement ifStatement = ParseStatement();

            ITsStatement? elseStatement = null;
            if (_reader.ReadIf(TsTokenCode.Else))
            {
                elseStatement = ParseStatement();
            }

            return Factory.IfStatement(ifCondition, ifStatement, elseStatement);
        }

        /// <summary>
        /// Parses an iteration statement, which is a 'do/while' loop.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// IterationStatement:
        ///   do Statement while ( Expression ) ;
        /// ]]></code></remarks>
        private ITsDoWhileStatement ParseIterationStatement()
        {
            Read(TsTokenCode.Do);
            ITsStatement doStatement = ParseStatement();
            Read(TsTokenCode.While);

            Read(TsTokenCode.LeftParen);
            ITsExpression whileCondition = ParseExpression();
            Read(TsTokenCode.RightParen);
            Read(TsTokenCode.Semicolon);

            return Factory.DoWhile(doStatement, whileCondition);
        }

        /// <summary>
        /// Parses a switch statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// SwitchStatement:
        ///   switch ( Expression ) CaseBlock
        ///
        /// CaseBlock:
        ///   { CaseClausesOpt }
        ///   { CaseClausesOpt DefaultClause CaseClausesOpt }
        ///
        /// CaseClauses:
        ///   CaseClause
        ///   CaseClauses CaseClause
        ///
        /// CaseClause:
        ///   case Expression : StatementListOpt
        ///
        /// DefaultClause:
        ///   default : StatementListOpt
        /// ]]></code></remarks>
        private ITsSwitchStatement ParseSwitchStatement()
        {
            Read(TsTokenCode.Switch);

            Read(TsTokenCode.LeftParen);
            ITsExpression condition = ParseExpression();
            Read(TsTokenCode.RightParen);

            bool HasStatements() =>
                !_reader.Peek().TokenCode.IsOneOf(TsTokenCode.Case, TsTokenCode.Default, TsTokenCode.RightBrace);

            var clauses = new List<ITsCaseOrDefaultClause>();
            Read(TsTokenCode.LeftBrace);
            while (!_reader.IsAtEnd && !_reader.IsNext(TsTokenCode.RightBrace))
            {
                if (_reader.ReadIf(TsTokenCode.Default))
                {
                    Read(TsTokenCode.Colon);
                    ITsStatementListItem[] statements =
                        HasStatements() ? ParseStatementList() : Array.Empty<ITsStatementListItem>();

                    ITsDefaultClause defaultClause = Factory.DefaultClause(statements);
                    clauses.Add(defaultClause);
                }
                else
                {
                    Read(TsTokenCode.Case);

                    ITsExpression expression = ParseExpression();
                    Read(TsTokenCode.Colon);
                    ITsStatementListItem[] statements =
                        HasStatements() ? ParseStatementList() : Array.Empty<ITsStatementListItem>();

                    ITsCaseClause caseClause = Factory.CaseClause(expression, statements);
                    clauses.Add(caseClause);
                }
            }

            Read(TsTokenCode.RightBrace);

            return Factory.Switch(condition, clauses.ToArray());
        }

        /// <summary>
        /// Parses a 'continue' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ContinueStatement:
        ///   continue ;
        ///   continue [no LineTerminator here] LabelIdentifier ;
        /// ]]></code></remarks>
        private ITsContinueStatement ParseContinueStatement()
        {
            Read(TsTokenCode.Continue);
            TryParseIdentifier(out ITsIdentifier? label);
            Read(TsTokenCode.Semicolon);

            return Factory.Continue(label);
        }

        /// <summary>
        /// Parses a 'break' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BreakStatement:
        ///   break ;
        ///   break [no LineTerminator here] LabelIdentifier ;
        /// ]]></code></remarks>
        private ITsBreakStatement ParseBreakStatement()
        {
            Read(TsTokenCode.Break);
            TryParseIdentifier(out ITsIdentifier? label);
            Read(TsTokenCode.Semicolon);

            return Factory.Break(label);
        }

        /// <summary>
        /// Parses a 'return' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ReturnStatement:
        ///   return ;
        ///   return [no LineTerminator here] Expression ;
        /// ]]></code></remarks>
        private ITsReturnStatement ParseReturnStatement()
        {
            Read(TsTokenCode.Return);

            ITsExpression? expression = null;
            if (!_reader.IsNext(TsTokenCode.Semicolon))
            {
                expression = ParseExpression();
            }

            Read(TsTokenCode.Semicolon);

            return Factory.Return(expression);
        }

        /// <summary>
        /// Parses a 'with' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// WithStatement:
        ///   with ( Expression ) Statement
        /// ]]></code></remarks>
        private ITsWithStatement ParseWithStatement()
        {
            Read(TsTokenCode.With);

            Read(TsTokenCode.LeftParen);
            ITsExpression expression = ParseExpression();
            Read(TsTokenCode.RightParen);

            ITsStatement statement = ParseStatement();

            return Factory.With(expression, statement);
        }

        /// <summary>
        /// Parses a 'throw' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ThrowStatement:
        ///   throw [no LineTerminator here] Expression ;
        /// ]]></code></remarks>
        private ITsThrowStatement ParseThrowStatement()
        {
            Read(TsTokenCode.Throw);
            ITsExpression expression = ParseExpression();
            return Factory.Throw(expression);
        }

        /// <summary>
        /// Parses a 'try' statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// TryStatement:
        ///   try Block Catch
        ///   try Block Finally
        ///   try Block Catch Finally
        ///
        /// Catch:
        ///   catch Block
        ///   catch ( CatchParameter ) Block
        ///
        /// Finally:
        ///   finally Block
        ///
        /// CatchParameter:
        ///   BindingIdentifier
        ///   BindingPattern
        /// ]]></code></remarks>
        private ITsTryStatement ParseTryStatement()
        {
            Read(TsTokenCode.Try);
            ITsBlockStatement tryBlock = ParseBlockStatement();

            // try/catch
            if (_reader.ReadIf(TsTokenCode.Catch))
            {
                ITsBindingIdentifierOrPattern? catchParameter = null;
                if (_reader.ReadIf(TsTokenCode.LeftParen))
                {
                    catchParameter = ParseBindingIdentifierOrPattern();
                    Read(TsTokenCode.RightParen);
                }

                ITsBlockStatement catchBlock = ParseBlockStatement();

                // try/catch/finally
                if (_reader.ReadIf(TsTokenCode.Finally))
                {
                    ITsBlockStatement finallyBlock = ParseBlockStatement();
                    return Factory.TryCatchFinally(tryBlock, catchParameter, catchBlock, finallyBlock);
                }

                return Factory.TryCatch(tryBlock, catchParameter, catchBlock);
            }

            // try/finally
            if (_reader.ReadIf(TsTokenCode.Finally))
            {
                ITsBlockStatement finallyBlock = ParseBlockStatement();
                return Factory.TryFinally(tryBlock, finallyBlock);
            }

            return Factory.Try(tryBlock);
        }

        /// <summary>
        /// Parses a labeled statement.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// LabeledStatement:
        ///   LabelIdentifier : LabelledItem
        ///
        /// LabelledItem:
        ///   Statement
        ///   FunctionDeclaration       (starts with 'function')
        /// ]]></code></remarks>
        private ITsLabeledStatement ParseLabeledStatement()
        {
            ITsIdentifier label = ParseIdentifier();
            Read(TsTokenCode.Colon);

            if (_reader.IsNext(TsTokenCode.Function))
            {
                ITsFunctionDeclaration functionDeclaration = ParseFunctionDeclaration();
                return Factory.LabeledStatement(label, functionDeclaration);
            }

            ITsStatement statement = ParseStatement();
            return Factory.LabeledStatement(label, statement);
        }
    }
}
