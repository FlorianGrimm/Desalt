// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    internal partial class TsParser
    {
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
        ///     BreakableStatement
        ///     ContinueStatement       (starts with 'continue')
        ///     BreakStatement          (starts with 'break')
        ///     ReturnStatement         (starts with 'return')
        ///     WithStatement
        ///     LabelledStatement       (starts with
        ///     ThrowStatement          (starts with 'throw')
        ///     TryStatement            (starts with 'try')
        ///     DebuggerStatement       (starts with 'debugger')
        /// ]]></code></remarks>
        private ITsStatement ParseStatement()
        {
            // BlockStatement
            if (_reader.IsNext(TsTokenCode.LeftBrace))
            {
                ITsBlockStatement blockStatement = ParseBlockStatement();
                return blockStatement;
            }

            // VariableStatement
            if (_reader.IsNext(TsTokenCode.Var))
            {
                ITsVariableStatement variableStatement = ParseVariableStatement();
                return variableStatement;
            }

            // EmptyStatement: ;
            if (_reader.ReadIf(TsTokenCode.Semicolon))
            {
                return Factory.EmptyStatement;
            }

            // DebuggerStatement: debugger ;
            if (_reader.ReadIf(TsTokenCode.Debugger))
            {
                Read(TsTokenCode.Semicolon);
                return Factory.Debugger;
            }

            throw NewParseException("Statement not yet supported");
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
            var declarations = ParseVariableDeclarationList();
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
        ///
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
        private ITsVariableDeclaration[] ParseVariableDeclarationList()
        {
            var declarations = new List<ITsVariableDeclaration>();

            do
            {
                ITsVariableDeclaration declaration;

                // SimpleVariableDeclaration
                if (TryParseIdentifier(out ITsIdentifier variableName))
                {
                    ITsType variableType = ParseOptionalTypeAnnotation();
                    ITsExpression initializer = ParseOptionalInitializer();

                    declaration = Factory.SimpleVariableDeclaration(variableName, variableType, initializer);
                }
                // DestructuringVariableDeclaration
                else
                {
                    ITsBindingPattern bindingPattern = ParseBindingPattern();
                    ITsType variableType = ParseOptionalTypeAnnotation();
                    ITsExpression initializer = ParseInitializer();

                    declaration = Factory.DestructuringVariableDeclaration(bindingPattern, variableType, initializer);
                }

                declarations.Add(declaration);
            }
            while (_reader.ReadIf(TsTokenCode.Comma));

            return declarations.ToArray();
        }

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
            throw NotYetImplementedException("Statement lists are not yet supported");
        }
    }
}
