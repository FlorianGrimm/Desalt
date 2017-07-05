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
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_type_parameter_with_no_constraint()
        {
            VerifyOutput(Factory.TypeParameter(s_T), "T");
        }

        [TestMethod]
        public void Emit_type_parameter_with_constraint()
        {
            VerifyOutput(Factory.TypeParameter(s_T, Factory.TypeReference(s_MyType)), "T extends MyType");
        }

        [TestMethod]
        public void Emit_parenthesized_type()
        {
            VerifyOutput(Factory.SymbolType.WithParentheses(), "(symbol)");
        }

        [TestMethod]
        public void Emit_simple_type_reference_with_no_type_arguments()
        {
            VerifyOutput(Factory.TypeReference(s_T), "T");
        }

        [TestMethod]
        public void Emit_qualified_name_type_reference_with_no_type_arguments()
        {
            VerifyOutput(Factory.TypeReference(Factory.QualifiedName("Ns.Class")), "Ns.Class");
        }

        [TestMethod]
        public void Emit_type_reference_with_type_arguments()
        {
            VerifyOutput(
                Factory.TypeReference(
                    Factory.Identifier("Sub"),
                    Factory.TypeReference(
                        Factory.Identifier("T1"), Factory.TypeReference(Factory.Identifier("T2")))),
                "Sub<T1<T2>>");
        }
    }
}
