// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast.Statements;

    public static partial class TsAstFactory
    {
        public static ITsDebuggerStatement Debugger => TsDebuggerStatement.Instance;

        public static ITsBlockStatement Block(params ITsStatementListItem[] statements) =>
            new TsBlockStatement(statements);

        public static ITsEmptyStatement EmptyStatement => TsEmptyStatement.Instance;

        /// <summary>
        /// Creates a variable declaration statement of the form 'var x = y;'.
        /// </summary>
        public static ITsVariableStatement VariableStatement(
            ITsVariableDeclaration declaration,
            params ITsVariableDeclaration[] declarations)
        {
            if (declaration == null)
            {
                throw new ArgumentNullException(nameof(declaration));
            }

            return new TsVariableStatement(declaration.ToSafeArray().Concat(declarations));
        }

        /// <summary>
        /// Creates a simple variable declaration of the form 'x: type = y'.
        /// </summary>
        public static ITsSimpleVariableDeclaration SimpleVariableDeclaration(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
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
            return new TsDestructuringVariableDeclaration(bindingPattern, initializer);
        }

        /// <summary>
        /// Creates a destructuring variable declaration of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringVariableDeclaration DestructuringVariableDeclaration(
            ITsBindingPattern bindingPattern,
            ITsType variableType,
            ITsExpression initializer)
        {
            return new TsDestructuringVariableDeclaration(bindingPattern, initializer, variableType);
        }

        /// <summary>
        /// Creates an object binding pattern of the form '{propName = defaultValue, propName: otherPropName}'.
        /// </summary>
        public static ITsObjectBindingPattern ObjectBindingPattern(params ITsBindingProperty[] properties) =>
            new TsObjectBindingPattern(properties);

        /// <summary>
        /// Creates an array binding pattern of the form '[x = y, z, ...p]'.
        /// </summary>
        public static ITsArrayBindingPattern ArrayBindingPattern(
            IEnumerable<ITsBindingElement> elements,
            ITsIdentifier restElement = null)
        {
            return new TsArrayBindingPattern(elements, restElement);
        }

        /// <summary>
        /// Creates an array binding pattern of the form '[x = y, z]'.
        /// </summary>
        public static ITsArrayBindingPattern ArrayBindingPattern(params ITsBindingElement[] elements) =>
            new TsArrayBindingPattern(elements);

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
            ITsExpression defaultValue = null)
        {
            return new TsSingleNameBinding(name, defaultValue);
        }

        public static ITsPatternBinding PatternBinding(
            ITsBindingPattern bindingPattern,
            ITsExpression initializer = null)
        {
            return new TsPatternBinding(bindingPattern, initializer);
        }

        /// <summary>
        /// Creates an expression in statement form.
        /// </summary>
        public static ITsExpressionStatement ExpressionStatement(ITsExpression expression) =>
            new TsExpressionStatement(expression);

        /// <summary>
        /// Creates an 'if' statement of the form 'if (expression) statement else statement'.
        /// </summary>
        public static ITsIfStatement IfStatement(
            ITsExpression ifCondition,
            ITsStatement ifStatement,
            ITsStatement elseStatement = null)
        {
            return new TsIfStatement(ifCondition, ifStatement, elseStatement);
        }

        public static ITsTryStatement Try(ITsBlockStatement tryBlock) => TsTryStatement.CreateTry(tryBlock);

        public static ITsTryStatement TryCatch(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern catchParameter,
            ITsBlockStatement catchBlock)
        {
            return TsTryStatement.CreateTryCatch(tryBlock, catchParameter, catchBlock);
        }

        public static ITsTryStatement TryFinally(
            ITsBlockStatement tryBlock,
            ITsBlockStatement finallyBlock)
        {
            return TsTryStatement.CreateTryFinally(tryBlock, finallyBlock);
        }

        public static ITsTryStatement TryCatchFinally(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern catchParameter,
            ITsBlockStatement catchBlock,
            ITsBlockStatement finallyBlock)
        {
            return TsTryStatement.CreateTryCatchFinally(tryBlock, catchParameter, catchBlock, finallyBlock);
        }

        /// <summary>
        /// Creates a do/while statement.
        /// </summary>
        public static ITsDoWhileStatement DoWhile(ITsStatement doStatement, ITsExpression whileCondition) =>
            new TsDoWhileStatement(doStatement, whileCondition);

        /// <summary>
        /// Creates a while loop.
        /// </summary>
        public static ITsWhileStatement While(ITsExpression whileCondition, ITsStatement whileStatement) =>
            new TsWhileStatement(whileCondition, whileStatement);

        /// <summary>
        /// Creates a simple lexical binding of the form 'x: type = y'.
        /// </summary>
        public static ITsSimpleLexicalBinding SimpleLexicalBinding(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsSimpleLexicalBinding(variableName, variableType, initializer);
        }

        /// <summary>
        /// Creates a destructuring lexical binding of the form '{x, y}: type = foo' or '[x, y]: type = foo'.
        /// </summary>
        public static ITsDestructuringLexicalBinding DestructuringLexicalBinding(
            ITsBindingPattern bindingPattern,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsDestructuringLexicalBinding(bindingPattern, variableType, initializer);
        }

        /// <summary>
        /// Creates a lexical declaration of the form 'const|let x: type, y: type = z;'.
        /// </summary>
        public static ITsLexicalDeclaration LexicalDeclaration(
            bool isConst,
            params ITsLexicalBinding[] declarations)
        {
            return new TsLexicalDeclaration(isConst, declarations);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsExpression initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(initializer, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsVariableDeclaration initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(initializer.ToSafeArray(), condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            IEnumerable<ITsVariableDeclaration> initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(initializer, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (const i: number = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public static ITsForStatement For(
            ITsLexicalDeclaration initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            return new TsForStatement(initializer, condition, incrementor, statement);
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (x in expression) statement'.
        /// </summary>
        public static ITsForInStatement ForIn(
            ITsExpression initializer,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForInOrOfStatement(initializer, rightSide, statement, ofLoop: false);
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (const x in expression) statement'.
        /// </summary>
        public static ITsForInStatement ForIn(
            ForDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide, ITsStatement statement)
        {
            return new TsForInOrOfStatement(declarationKind, declaration, rightSide, statement, ofLoop: false);
        }

        /// <summary>
        /// Creates a for-of loop of the form, 'for (x of expression) statement'.
        /// </summary>
        public static ITsForOfStatement ForOf(
            ITsExpression initializer,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            return new TsForInOrOfStatement(initializer, rightSide, statement, ofLoop: true);
        }

        /// <summary>
        /// Creates a for-of loop of the form, 'for (const x of expression) statement'.
        /// </summary>
        public static ITsForOfStatement ForOf(
            ForDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide, ITsStatement statement)
        {
            return new TsForInOrOfStatement(declarationKind, declaration, rightSide, statement, ofLoop: true);
        }
    }
}
