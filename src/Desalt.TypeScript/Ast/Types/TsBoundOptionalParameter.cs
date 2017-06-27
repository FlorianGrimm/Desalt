﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBoundOptionalParameter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

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
            ITsAssignmentExpression initializer,
            ITsType parameterType = null,
            TsAccessibilityModifier? modifier = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            ParameterType = parameterType;
            Modifier = modifier;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? Modifier { get; }
        public ITsBindingIdentifierOrPattern ParameterName { get; }
        public ITsType ParameterType { get; }
        public ITsAssignmentExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitBoundOptionalParameter(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitBoundOptionalParameter(this);

        public override string ToCodeDisplay()
        {
            string display = string.Empty;
            if (Modifier.HasValue)
            {
                display = $"{Modifier.Value.ToString().ToLowerInvariant()} ";
            }

            display += $"{ParameterName}${ParameterType.ToTypeAnnotationCodeDisplay()} = {Initializer}";

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

            writer.Write(" = ");
            Initializer.WriteFullCodeDisplay(writer);
        }
    }
}