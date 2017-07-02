// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBoundRequiredParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a bound required parameter in a parameter list for a function.
    /// </summary>
    internal class TsBoundRequiredParameter : AstNode<TsVisitor>, ITsBoundRequiredParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsBoundRequiredParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsType parameterType = null,
            TsAccessibilityModifier? modifier = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            ParameterType = parameterType;
            Modifier = modifier;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? Modifier { get; }
        public ITsBindingIdentifierOrPattern ParameterName { get; }
        public ITsType ParameterType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitBoundRequiredParameter(this);

        public override string CodeDisplay
        {
            get
            {
                string display = string.Empty;
                if (Modifier.HasValue)
                {
                    display = $"{Modifier.Value.ToString().ToLowerInvariant()} ";
                }

                display += $"{ParameterName}{ParameterType.ToTypeAnnotationCodeDisplay()}";
                return display;
            }
        }

        public override void Emit(Emitter emitter)
        {
            if (Modifier.HasValue)
            {
                emitter.Write($"{Modifier.Value.ToString().ToLowerInvariant()} ");
            }

            ParameterName.Emit(emitter);
            ParameterType.EmitTypeAnnotation(emitter);
        }
    }
}
