// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5AstFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.JavaScript.Ast.Expressions;
    using Desalt.JavaScript.Ast.Statements;

    /// <summary>
    /// Provides factory methods for creating ES5 abstract syntax tree nodes.
    /// </summary>
    public static class Es5AstFactory
    {
        //// ===========================================================================================================
        //// Singleton Properties
        //// ===========================================================================================================

        public static Es5ThisExpression ThisExpression => Es5ThisExpression.Instance;

        public static Es5LiteralExpression NullLiteral => Es5LiteralExpression.Null;

        public static Es5LiteralExpression TrueLiteral => Es5LiteralExpression.True;

        public static Es5LiteralExpression FalseLiteral => Es5LiteralExpression.False;

        public static Es5DebuggerStatement DebuggerStatement => Es5DebuggerStatement.Instance;

        public static Es5EmptyStatement EmptyStatement => Es5EmptyStatement.Instance;

        public static Es5ContinueStatement ContinueStatement => Es5ContinueStatement.UnlabelledContinueStatement;

        public static Es5BreakStatement BreakStatement => Es5BreakStatement.UnlabelledBreakStatement;

        public static readonly Es5ObjectLiteralExpression EmptyObjectLiteral = new Es5ObjectLiteralExpression(null);

        //// ===========================================================================================================
        //// Literal Expressions
        //// ===========================================================================================================

        public static Es5Identifier Identifier(string name) => Es5Identifier.Get(name);

        public static Es5LiteralExpression StringLiteral(string literal) =>
            Es5LiteralExpression.CreateString(literal);

        public static Es5LiteralExpression DecimalLiteral(string literal) =>
            Es5LiteralExpression.CreateDecimal(literal);

        public static Es5LiteralExpression HexIntegerLiteral(string literal) =>
            Es5LiteralExpression.CreateHexInteger(literal);

        public static Es5LiteralExpression RegExpLiteral(string literal) =>
            Es5LiteralExpression.CreateRegExp(literal);

        public static Es5AssignmentExpression AssignmentExpression(
            IEs5Expression leftSide,
            Es5AssignmentOperator @operator,
            IEs5Expression rightSide)
        {
            return new Es5AssignmentExpression(leftSide, @operator, rightSide);
        }

        public static Es5ArrayLiteralExpression ArrayLiteral(params IEs5Expression[] elements)
        {
            return new Es5ArrayLiteralExpression(elements);
        }

        //// ===========================================================================================================
        //// Object Literal Expressions
        //// ===========================================================================================================

        public static Es5ObjectLiteralExpression ObjectLiteral(IEnumerable<IEs5PropertyAssignment> properties)
        {
            return new Es5ObjectLiteralExpression(properties);
        }

        public static Es5ObjectLiteralExpression ObjectLiteral(params IEs5PropertyAssignment[] properties)
        {
            return new Es5ObjectLiteralExpression(properties);
        }

        public static Es5PropertyValueAssignment PropertyValueAssignment(string propertyName, IEs5Expression value)
        {
            return new Es5PropertyValueAssignment(propertyName, value);
        }

        public static Es5PropertyGetAssignment PropertyGet(string propertyName, params IEs5SourceElement[] functionBody)
        {
            return new Es5PropertyGetAssignment(propertyName, functionBody);
        }

        public static Es5PropertySetAssignment PropertySet(
            string propertyName,
            Es5Identifier setParameter,
            params IEs5SourceElement[] functionBody)
        {
            return new Es5PropertySetAssignment(propertyName, setParameter, functionBody);
        }

        //// ===========================================================================================================
        //// Function Expressions
        //// ===========================================================================================================

        public static Es5FunctionExpression Function(IEnumerable<IEs5SourceElement> functionBody)
        {
            return new Es5FunctionExpression(functionName: null, parameters: null, functionBody: functionBody);
        }

        public static Es5FunctionExpression Function(params IEs5SourceElement[] functionBody)
        {
            return new Es5FunctionExpression(functionName: null, parameters: null, functionBody: functionBody);
        }

        public static Es5FunctionExpression Function(
            string functionName,
            IEnumerable<Es5Identifier> parameters,
            params IEs5SourceElement[] functionBody)
        {
            return new Es5FunctionExpression(functionName, parameters, functionBody);
        }

        public static Es5FunctionExpression Function(
            string functionName,
            IEnumerable<Es5Identifier> parameters,
            IEnumerable<IEs5SourceElement> functionBody)
        {
            return new Es5FunctionExpression(functionName, parameters, functionBody);
        }

        public static IList<Es5Identifier> ParamList(params string[] parameterNames)
        {
            IEnumerable<Es5Identifier> identifiers = parameterNames.Select(Identifier);
            return new List<Es5Identifier>(identifiers);
        }

        //// ===========================================================================================================
        //// Member Expressions
        //// ===========================================================================================================

        public static Es5MemberExpression MemberBracket(
            IEs5Expression memberExpression,
            IEs5Expression bracketExpression)
        {
            return Es5MemberExpression.CreateBracketNotation(memberExpression, bracketExpression);
        }

        public static Es5MemberExpression MemberDot(IEs5Expression memberExpression, Es5Identifier dotName)
        {
            return Es5MemberExpression.CreateDotNotation(memberExpression, dotName);
        }

        /// <summary>
        /// Creates a nested qualified name member expression from the specified identifiers.
        /// </summary>
        public static Es5MemberExpression MemberDot(IEnumerable<Es5Identifier> identifiers)
        {
            if (identifiers == null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }

            Es5Identifier[] array = identifiers.ToArray();
            if (array.Length < 2)
            {
                throw new ArgumentException("There must be at least two identifiers for a member expression");
            }

            Es5MemberExpression nestedDot = MemberDot(array[0], array[1]);
            if (array.Length > 2)
            {
                nestedDot = array.Skip(2).Aggregate(
                    seed: nestedDot,
                    func: MemberDot,
                    resultSelector: dotExpression => dotExpression);
            }

            return nestedDot;
        }

        public static Es5CallExpression NewCall(
            IEs5Expression memberExpression,
            IEnumerable<IEs5Expression> arguments)
        {
            return new Es5CallExpression(memberExpression, arguments, isNewCall: true);
        }

        public static Es5CallExpression NewCall(
            IEs5Expression memberExpression,
            params IEs5Expression[] arguments)
        {
            return new Es5CallExpression(memberExpression, arguments, isNewCall: true);
        }

        public static Es5CallExpression Call(
            IEs5Expression memberExpression,
            IEnumerable<IEs5Expression> arguments)
        {
            return new Es5CallExpression(memberExpression, arguments, isNewCall: false);
        }

        public static Es5CallExpression Call(
            IEs5Expression memberExpression,
            params IEs5Expression[] arguments)
        {
            return new Es5CallExpression(memberExpression, arguments, isNewCall: false);
        }

        //// ===========================================================================================================
        //// Unary and Binary Expressions
        //// ===========================================================================================================

        public static Es5UnaryExpression UnaryExpression(IEs5Expression operand, Es5UnaryOperator @operator)
        {
            return new Es5UnaryExpression(operand, @operator);
        }

        public static Es5BinaryExpression BinaryExpression(
            IEs5Expression leftSide,
            Es5BinaryOperator @operator,
            IEs5Expression rightSide)
        {
            return new Es5BinaryExpression(leftSide, @operator, rightSide);
        }

        public static Es5ConditionalExpression ConditionalExpression(
            IEs5Expression condition,
            IEs5Expression whenTrue,
            IEs5Expression whenFalse)
        {
            return new Es5ConditionalExpression(condition, whenTrue, whenFalse);
        }

        //// ===========================================================================================================
        //// Statements
        //// ===========================================================================================================

        public static Es5BlockStatement BlockStatement(IEnumerable<IEs5Statement> statements)
        {
            return new Es5BlockStatement(statements);
        }

        public static Es5BlockStatement BlockStatement(params IEs5Statement[] statements)
        {
            return new Es5BlockStatement(statements);
        }

        public static Es5VariableStatement VariableStatement(params Es5VariableDeclaration[] declarations)
        {
            return new Es5VariableStatement(declarations);
        }

        public static Es5VariableDeclaration VariableDeclaration(
            Es5Identifier identifier,
            IEs5Expression initializer = null)
        {
            return new Es5VariableDeclaration(identifier, initializer);
        }

        public static Es5IfStatement IfStatement(
            IEs5Expression ifExpression,
            IEs5Statement ifStatement,
            IEs5Statement elseStatement)
        {
            return new Es5IfStatement(ifExpression, ifStatement, elseStatement);
        }

        public static Es5ContinueStatement ContinueLabelStatement(Es5Identifier identifier = null)
        {
            return new Es5ContinueStatement(identifier);
        }

        public static Es5BreakStatement BreakLabelStatement(Es5Identifier identifier = null)
        {
            return new Es5BreakStatement(identifier);
        }

        public static Es5ReturnStatement ReturnStatement(IEs5Expression expression)
        {
            return new Es5ReturnStatement(expression);
        }

        public static Es5WithStatement WithStatement(IEs5Expression expression, IEs5Statement statement)
        {
            return new Es5WithStatement(expression, statement);
        }

        public static Es5LabelledStatement LabelledStatement(Es5Identifier identifier, IEs5Statement statement)
        {
            return new Es5LabelledStatement(identifier, statement);
        }

        public static Es5SwitchStatement SwitchStatement(
            IEs5Expression condition,
            IEnumerable<Es5CaseClause> caseClauses,
            IEnumerable<IEs5Statement> defaultClauseStatements = null)
        {
            return new Es5SwitchStatement(condition, caseClauses, defaultClauseStatements);
        }

        public static IList<Es5CaseClause> CaseClauses(params Es5CaseClause[] clauses)
        {
            return new List<Es5CaseClause>(clauses);
        }

        public static IList<IEs5Statement> StatementList(params IEs5Statement[] statements)
        {
            return new List<IEs5Statement>(statements);
        }

        public static Es5CaseClause CaseClause(IEs5Expression expression, params IEs5Statement[] statements)
        {
            return new Es5CaseClause(expression, statements);
        }

        public static Es5ThrowStatement ThrowStatement(IEs5Expression expression)
        {
            return new Es5ThrowStatement(expression);
        }

        public static Es5TryStatement TryStatement(params IEs5Statement[] tryStatements)
        {
            return Es5TryStatement.CreateTry(new Es5BlockStatement(tryStatements));
        }

        //// ===========================================================================================================
        //// Iteration Statements
        //// ===========================================================================================================

        public static Es5DoStatement DoStatement(IEs5Statement statement, IEs5Expression condition)
        {
            return new Es5DoStatement(statement, condition);
        }

        public static Es5WhileStatement WhileStatement(IEs5Expression condition, IEs5Statement statement)
        {
            return new Es5WhileStatement(condition, statement);
        }

        public static Es5ForStatement ForStatement(
            IEs5Statement statement,
            IEnumerable<Es5VariableDeclaration> declarations = null,
            IEs5Expression condition = null,
            IEs5Expression incrementor = null)
        {
            return new Es5ForStatement(statement, declarations, condition, incrementor);
        }

        public static Es5ForStatement ForStatement(
            IEs5Statement statement,
            IEs5Expression initializer = null,
            IEs5Expression condition = null,
            IEs5Expression incrementor = null)
        {
            return new Es5ForStatement(statement, initializer, condition, incrementor);
        }

        public static Es5ForInStatement ForInStatement(
            IEs5Expression leftHandSide,
            IEs5Expression rightSide,
            IEs5Statement statement)
        {
            return new Es5ForInStatement(leftHandSide, rightSide, statement);
        }

        public static Es5ForInStatement ForInStatement(
            Es5VariableDeclaration declaration,
            IEs5Expression rightSide,
            IEs5Statement statement)
        {
            return new Es5ForInStatement(declaration, rightSide, statement);
        }

        //// ===========================================================================================================
        //// Program
        //// ===========================================================================================================

        public static Es5FunctionDeclaration FunctionDeclaration(
            string functionName,
            IEnumerable<Es5Identifier> parameters,
            params IEs5SourceElement[] functionBody)
        {
            return new Es5FunctionDeclaration(functionName, parameters, functionBody);
        }

        public static Es5Program Program(IEnumerable<IEs5SourceElement> sourceElements)
        {
            return new Es5Program(sourceElements);
        }

        public static Es5Program Program(params IEs5SourceElement[] sourceElements)
        {
            return new Es5Program(sourceElements);
        }
    }
}
