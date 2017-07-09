﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExportedVariableStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an exported variable statement.
    /// </summary>
    internal class TsExportedVariableStatement : AstNode<TsVisitor>, ITsExportedVariableStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsExportedVariableStatement(ITsVariableStatement exportedStatement)
        {
            ExportedStatement = exportedStatement ?? throw new ArgumentNullException(nameof(exportedStatement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsVariableStatement ExportedStatement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitExportedVariableStatement(this);

        public override string CodeDisplay => $"export {ExportedStatement}";

        public override void Emit(Emitter emitter)
        {
            emitter.Write("export ");
            ExportedStatement.Emit(emitter);
        }
    }
}
