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

        public static ITsSimpleVariableDeclaration SimpleVariableDeclaration(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsSimpleVariableDeclaration(variableName, variableType, initializer);
        }

        /// <summary>
        /// Creates a destructuring variable declaration of the form '{x, y} = foo' or '[x, y] = foo'.
        /// </summary>
        public static ITsDestructuringVariableDeclaration DestructuringVariableDeclaration(
            ITsBindingPattern bindingPattern,
            ITsExpression initializer)
        {
            return new TsDestructuringVariableDeclaration(bindingPattern, initializer);
        }

        /// <summary>
        /// Creates a destructuring variable declaration of the form '{x, y} = foo' or '[x, y] = foo'.
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

        public static ITsDebuggerStatement Debugger => TsDebuggerStatement.Instance;
    }
}
