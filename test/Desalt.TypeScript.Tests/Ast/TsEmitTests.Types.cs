// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEmitTests.Types.cs" company="Justin Rockwood">
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
        public void Emit_type_parameter_with_no_constraint()
        {
            VerifyOutput(TsAstFactory.TypeParameter(s_T), "T");
        }

        [TestMethod]
        public void Emit_type_parameter_with_constraint()
        {
            VerifyOutput(TsAstFactory.TypeParameter(s_T, TsAstFactory.TypeReference(s_MyType)), "T extends MyType");
        }
    }
}
