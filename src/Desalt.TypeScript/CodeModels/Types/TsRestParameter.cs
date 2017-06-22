// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsRestParameter.cs" company="Justin Rockwood">
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
    /// Represents a function parameter of the form '... parameterName: type'.
    /// </summary>
    internal class TsRestParameter : CodeModel, ITsRestParameter
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsRestParameter(ITsIdentifier parameterName, ITsType typeAnnotation = null)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            TypeAnnotation = typeAnnotation;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public ITsType TypeAnnotation { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitRestParameter(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitRestParameter(this);

        public override string ToCodeDisplay()
        {
            string display = $"... {ParameterName.ToCodeDisplay()}";
            if (TypeAnnotation != null)
            {
                display += $": {TypeAnnotation.ToCodeDisplay()}";
            }

            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("... ");
            ParameterName.WriteFullCodeDisplay(writer);

            if (TypeAnnotation != null)
            {
                writer.Write(": ");
                TypeAnnotation.WriteFullCodeDisplay(writer);
            }
        }
    }
}
