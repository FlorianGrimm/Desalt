// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAmbientBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an ambient variable binding of the form 'name: type'.
    /// </summary>
    internal class TsAmbientBinding : AstNode<TsVisitor>, ITsAmbientBinding
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

        public override void Emit(Emitter emitter)
        {
            VariableName.Emit(emitter);
            VariableType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
