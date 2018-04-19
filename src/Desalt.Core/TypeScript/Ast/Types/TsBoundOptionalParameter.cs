// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBoundOptionalParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a bound optional parameter in a function.
    /// </summary>
    internal class TsBoundOptionalParameter : AstNode, ITsBoundOptionalParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsBoundOptionalParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            ITsExpression initializer = null,
            TsAccessibilityModifier? modifier = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Initializer = initializer;
            ParameterType = parameterType;
            Modifier = modifier;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? Modifier { get; }
        public ITsBindingIdentifierOrPattern ParameterName { get; }
        public ITsType ParameterType { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitBoundOptionalParameter(this);

        public override string CodeDisplay =>
            $"{Modifier.OptionalCodeDisplay()}" +
            $"{ParameterName}${ParameterType.OptionalTypeAnnotation()} = {Initializer}";

        protected override void EmitInternal(Emitter emitter)
        {
            Modifier.EmitOptional(emitter);

            ParameterName.Emit(emitter);
            ParameterType.EmitOptionalTypeAnnotation(emitter);

            Initializer.EmitOptionalAssignment(emitter);
        }
    }
}
