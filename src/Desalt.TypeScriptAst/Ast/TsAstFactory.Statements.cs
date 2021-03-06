// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    public static partial class TsAstFactory
    {
        public static readonly ITsDebuggerStatement Debugger = new TsDebuggerStatement();

        public static ITsBlockStatement Block(params ITsStatementListItem[] statements)
        {
            return new TsBlockStatement(statements.ToImmutableArray());
        }

        public static readonly ITsEmptyStatement EmptyStatement = new TsEmptyStatement();

        /// <summary>
        /// Creates a variable declaration statement of the form 'var x = y;'.
        /// </summary>
        public static ITsVariableStatement VariableStatement(params ITsVariableDeclaration[] declarations)
        {
            return new TsVariableStatement(declarations.ToImmutableArray());
        }

        /// <summary>
        /// Creates a simple variable declaration of the form 'x: type = y'.
        /// </summary>
        public static ITsSimpleVariableDeclaration SimpleVariableDeclaration(
            ITsIdentifier variableName,
            ITsType? variableType = null,
            ITsExpression? initializer = null)
        {
            return new TsSimpleVariableDeclaration(variableName, variableType, initializer);
        }

        /// <summary>
        /// Creates a destructuring variable declaration of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringVariableDeclaration DestructuringVariableDeclaration(
            ITsBindingPattern bindingPattern,
            ITsExpression initializer)
        {
            return new TsDestructuringVariableDeclaration(bindingPattern, variableType: null, initializer);
        }

        /// <summary>
        /// Creates a destructuring variable declaration of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringVariableDeclaration DestructuringVariableDeclaration(
            ITsBindingPattern bindingPattern,
            ITsType? variableType,
            ITsExpression initializer)
        {
            return new TsDestructuringVariableDeclaration(bindingPattern, variableType, initializer);
        }

        /// <summary>
        /// Creates an object binding pattern of the form '{propName = defaultValue, propName: otherPropName}'.
        /// </summary>
        public static ITsObjectBindingPattern ObjectBindingPattern(params ITsBindingProperty[] properties)
        {
            return new TsObjectBindingPattern(properties.ToImmutableArray());
        }

        /// <summary>
        /// Creates an array binding pattern of the form '[x = y, z, ...p]'.
        /// </summary>
        public static ITsArrayBindingPattern ArrayBindingPattern(
            IEnumerable<ITsBindingElement?> elements,
            ITsIdentifier? restElement = null)
        {
            return new TsArrayBindingPattern(elements.ToImmutableArray(), restElement);
        }

        /// <summary>
        /// Creates an array binding pattern of the form '[x = y, z]'.
        /// </summary>
        public static ITsArrayBindingPattern ArrayBindingPattern(params ITsBindingElement?[] elements)
        {
            return new TsArrayBindingPattern(elements.ToImmutableArray(), null);
        }

        /// <summary>
        /// Creates a single name binding within an object or array pattern binding, of the form
        /// 'name = defaultValue'.
        /// </summary>
        /// <param name="name">
        /// The name of the variable in an array pattern binding, or the name of the property in an
        /// object pattern binding.
        /// </param>
        /// <param name="defaultValue">The default value assigned to the variable or property.</param>
        /// <returns>An <see cref="ITsSingleNameBinding"/> instance.</returns>
        public static ITsSingleNameBinding SingleNameBinding(
            ITsIdentifier name,
            ITsExpression? defaultValue = null)
        {
            return new TsSingleNameBinding(name, defaultValue);
        }

        /// <summary>
        /// Creates a property name binding pattern used in object and array bindings, of the form
        /// 'propertyName = expression'.
        /// </summary>
        public static ITsPropertyNameBinding PropertyNameBinding(
            ITsPropertyName propertyName,
            ITsBindingElement bindingElement)
        {
            return new TsPropertyNameBinding(propertyName, bindingElement);
        }

        public static ITsPatternBinding PatternBinding(
            ITsBindingPattern bindingPattern,
            ITsExpression? initializer = null)
        {
            return new TsPatternBinding(bindingPattern, initializer);
        }

        /// <summary>
        /// Creates an expression in statement form.
        /// </summary>
        public static ITsExpressionStatement ExpressionStatement(ITsExpression expression)
        {
            return new TsExpressionStatement(expression);
        }

        /// <summary>
        /// Creates an 'if' statement of the form 'if (expression) statement else statement'.
        /// </summary>
        public static ITsIfStatement IfStatement(
            ITsExpression ifCondition,
            ITsStatement ifStatement,
            ITsStatement? elseStatement = null)
        {
            return new TsIfStatement(ifCondition, ifStatement, elseStatement);
        }

        public static ITsTryStatement Try(ITsBlockStatement tryBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter: null, catchBlock: null, finallyBlock: null);
        }

        public static ITsTryStatement TryCatch(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern? catchParameter,
            ITsBlockStatement catchBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter, catchBlock, finallyBlock: null);
        }

        public static ITsTryStatement TryCatch(
            ITsBlockStatement tryBlock,
            ITsBlockStatement catchBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter: null, catchBlock, finallyBlock: null);
        }

        public static ITsTryStatement TryFinally(
            ITsBlockStatement tryBlock,
            ITsBlockStatement finallyBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter: null, catchBlock: null, finallyBlock);
        }

        public static ITsTryStatement TryCatchFinally(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern? catchParameter,
            ITsBlockStatement catchBlock,
            ITsBlockStatement finallyBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter, catchBlock, finallyBlock);
        }

        public static ITsTryStatement TryCatchFinally(
            ITsBlockStatement tryBlock,
            ITsBlockStatement catchBlock,
            ITsBlockStatement finallyBlock)
        {
            return new TsTryStatement(tryBlock, catchParameter: null, catchBlock, finallyBlock);
        }

        /// <summary>
        /// Creates a do/while statement.
        /// </summary>
        public static ITsDoWhileStatement DoWhile(ITsStatement doStatement, ITsExpression whileCondition)
        {
            return new TsDoWhileStatement(doStatement, whileCondition);
        }

        /// <summary>
        /// Creates a while loop.
        /// </summary>
        public static ITsWhileStatement While(ITsExpression whileCondition, ITsStatement whileStatement)
        {
            return new TsWhileStatement(whileCondition, whileStatement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsExpression? initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(initializer, null, null, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsVariableDeclaration initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(
                null,
                ImmutableArray.Create(initializer),
                null,
                condition,
                incrementor,
                statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            IEnumerable<ITsVariableDeclaration> initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(null, initializer.ToImmutableArray(), null, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (const i: number = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsLexicalDeclaration initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(null, null, initializer, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (x in expression) statement'.
        /// </summary>
        public static ITsForInStatement ForIn(
            ITsExpression initializer,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForInStatement(initializer, declarationKind: null, declaration: null, rightSide, statement);
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (const x in expression) statement'.
        /// </summary>
        public static ITsForInStatement ForIn(
            VariableDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForInStatement(initializer: null, declarationKind, declaration, rightSide, statement);
        }

        /// <summary>
        /// Creates a for-of loop of the form, 'for (x of expression) statement'.
        /// </summary>
        public static ITsForOfStatement ForOf(
            ITsExpression initializer,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForOfStatement(initializer, declarationKind: null, declaration: null, rightSide, statement);
        }

        /// <summary>
        /// Creates a for-of loop of the form, 'for (const x of expression) statement'.
        /// </summary>
        public static ITsForOfStatement ForOf(
            VariableDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForOfStatement(initializer: null, declarationKind, declaration, rightSide, statement);
        }

        /// <summary>
        /// Creates a case clause in a switch statement.
        /// </summary>
        public static ITsCaseClause CaseClause(ITsExpression expression, params ITsStatementListItem[] statements)
        {
            return new TsCaseClause(expression, statements.ToImmutableArray());
        }

        /// <summary>
        /// Creates a default clause in a switch statement of the form 'default: statements'.
        /// </summary>
        public static ITsDefaultClause DefaultClause(params ITsStatementListItem[] statements)
        {
            return new TsDefaultClause(statements.ToImmutableArray());
        }

        /// <summary>
        /// Creates a switch statement of the form 'switch (condition) { case x: statement; default: statement; }'.
        /// </summary>
        public static ITsSwitchStatement Switch(ITsExpression condition, params ITsCaseOrDefaultClause[] clauses)
        {
            return new TsSwitchStatement(condition, clauses.ToImmutableArray());
        }

        /// <summary>
        /// Creates a continue statement with an optional label to continue to.
        /// </summary>
        public static ITsContinueStatement Continue(ITsIdentifier? label = null)
        {
            return new TsContinueStatement(label: label);
        }

        /// <summary>
        /// Creates a break statement with an optional label to break to.
        /// </summary>
        public static ITsBreakStatement Break(ITsIdentifier? label = null)
        {
            return new TsBreakStatement(label: label);
        }

        /// <summary>
        /// Create a 'return' statement.
        /// </summary>
        public static ITsReturnStatement Return(ITsExpression? expression = null)
        {
            return new TsReturnStatement(expression);
        }

        /// <summary>
        /// Creates a 'with' statement of the form, 'with (expression) statement'.
        /// </summary>
        public static ITsWithStatement With(ITsExpression expression, ITsStatement statement)
        {
            return new TsWithStatement(expression, statement);
        }

        /// <summary>
        /// Creates a new labeled statement.
        /// </summary>
        public static ITsLabeledStatement LabeledStatement(ITsIdentifier label, ITsStatement statement)
        {
            return new TsLabeledStatement(label, statement, functionDeclaration: null);
        }

        /// <summary>
        /// Creates a new labeled statement.
        /// </summary>
        public static ITsLabeledStatement LabeledStatement(
            ITsIdentifier label,
            ITsFunctionDeclaration functionDeclaration)
        {
            return new TsLabeledStatement(label, statement: null, functionDeclaration);
        }

        /// <summary>
        /// Creates a new 'throw' statement.
        /// </summary>
        public static ITsThrowStatement Throw(ITsExpression expression)
        {
            return new TsThrowStatement(expression);
        }
    }
}
