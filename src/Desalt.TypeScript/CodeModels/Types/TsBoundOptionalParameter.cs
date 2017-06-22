// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBoundOptionalParameter.cs" company="Justin Rockwood">
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
    /// Represents a bound optional parameter in a function.
    /// </summary>
    internal class TsBoundOptionalParameter : CodeModel, ITsBoundOptionalParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsBoundOptionalParameter(
            ITsBindingIdentifierOrPattern parameterName,
            ITsAssignmentExpression initializer,
            ITsType typeAnnotation = null,
            TsAccessibilityModifier? modifier = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            TypeAnnotation = typeAnnotation;
            Modifier = modifier;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? Modifier { get; }
        public ITsBindingIdentifierOrPattern ParameterName { get; }
        public ITsType TypeAnnotation { get; }
        public ITsAssignmentExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitBoundOptionalParameter(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitBoundOptionalParameter(this);

        public override string ToCodeDisplay()
        {
            string display = string.Empty;
            if (Modifier.HasValue)
            {
                display = $"{Modifier.Value.ToString().ToLowerInvariant()} ";
            }

            display += $"{ParameterName}${TypeAnnotation.TypeAnnotationCodeDisplay()} = {Initializer}";

            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (Modifier.HasValue)
            {
                writer.Write($"{Modifier.Value.ToString().ToLowerInvariant()} ");
            }

            ParameterName.WriteFullCodeDisplay(writer);
            TypeAnnotation.WriteTypeAnnotation(writer);

            writer.Write(" = ");
            Initializer.WriteFullCodeDisplay(writer);
        }
    }
}
