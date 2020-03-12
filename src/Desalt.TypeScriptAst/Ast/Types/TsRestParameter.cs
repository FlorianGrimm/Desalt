// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsRestParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a function parameter of the form '... parameterName: type'.
    /// </summary>
    internal class TsRestParameter : TsAstNode, ITsRestParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsRestParameter(ITsIdentifier parameterName, ITsType? parameterType = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            ParameterType = parameterType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public ITsType? ParameterType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitRestParameter(this);
        }

        public override string CodeDisplay => $"... {ParameterName}{ParameterType?.OptionalTypeAnnotation()}";

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("... ");
            ParameterName.Emit(emitter);
            ParameterType?.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
