// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVisitor.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast
{
    public abstract partial class TsVisitor
    {
        /// <summary>
        /// Visits a debugger statement.
        /// </summary>
        public virtual void VisitDebuggerStatement(ITsDebuggerStatement node) => Visit(node);
    }
}
