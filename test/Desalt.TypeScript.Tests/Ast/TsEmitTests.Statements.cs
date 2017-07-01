// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Statements.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Ast
{
    using Desalt.TypeScript.Ast;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_debugger_statement()
        {
            VerifyOutput(TsAstFactory.Debugger, "debugger;\n");
        }
    }
}
