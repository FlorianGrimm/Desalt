// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsDebuggerStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a 'debugger' statement.
    /// </summary>
    internal class TsDebuggerStatement : TsAstNode, ITsDebuggerStatement
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsDebuggerStatement Instance = new TsDebuggerStatement();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsDebuggerStatement()
        {
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitDebuggerStatement(this);
        }

        public override string CodeDisplay => "debugger;";

        protected override void EmitContent(Emitter emitter)
        {
            emitter.WriteLine("debugger;");
        }
    }
}
