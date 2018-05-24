// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an ambient variable binding of the form 'name: type'.
    /// </summary>
    internal class TsAmbientBinding : TsAstNode, ITsAmbientBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsAmbientBinding(ITsIdentifier variableName, ITsType variableType = null)
        {
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            VariableType = variableType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier VariableName { get; }
        public ITsType VariableType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitAmbientBinding(this);

        public override string CodeDisplay => $"{VariableName}{VariableType.OptionalTypeAnnotation()}";

        protected override void EmitInternal(Emitter emitter)
        {
            VariableName.Emit(emitter);
            VariableType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
