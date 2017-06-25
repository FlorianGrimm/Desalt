// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBoundRequiredParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a bound required parameter in a parameter list for a function.
    /// </summary>
    internal class TsBoundRequiredParameter : CodeModel, ITsBoundRequiredParameter
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitBoundRequiredParameter(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitBoundRequiredParameter(this);

        public override string ToCodeDisplay()
        {
            string display = string.Empty;
            if (Modifier.HasValue)
            {
                display = $"{Modifier.Value.ToString().ToLowerInvariant()} ";
            }

            display += $"{ParameterName}{ParameterType.ToTypeAnnotationCodeDisplay()}";
            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (Modifier.HasValue)
            {
                writer.Write($"{Modifier.Value.ToString().ToLowerInvariant()} ");
            }

            ParameterName.WriteFullCodeDisplay(writer);
            ParameterType.WriteTypeAnnotation(writer);
        }
    }
}
