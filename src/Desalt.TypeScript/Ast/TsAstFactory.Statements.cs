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

        public static ITsDebuggerStatement Debugger => TsDebuggerStatement.Instance;
    }
}
