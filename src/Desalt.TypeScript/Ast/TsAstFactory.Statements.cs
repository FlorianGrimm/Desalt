// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstFactory.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    using Desalt.TypeScript.Ast.Statements;

    public static partial class TsAstFactory
    {
        public static ITsBlockStatement Block(params ITsStatementListItem[] statements) =>
            new TsBlockStatement(statements);

        public static ITsEmptyStatement EmptyStatement => TsEmptyStatement.Instance;

        public static ITsSimpleVariableDeclaration SimpleVariableDeclaration(
            ITsIdentifier variableName,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            return new TsSimpleVariableDeclaration(variableName, variableType, initializer);
        }

        public static ITsSingleNameBinding SingleNameBinding(ITsIdentifier name, ITsExpression initializer) =>
            new TsSingleNameBinding(name, initializer);

        public static ITsPatternBinding PatternBinding(
            ITsBindingPattern bindingPattern,
            ITsExpression initializer = null)
        {
            return new TsPatternBinding(bindingPattern, initializer);
        }

        public static ITsDebuggerStatement Debugger => TsDebuggerStatement.Instance;
    }
}
