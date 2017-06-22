// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIndexSignature.cs" company="Justin Rockwood">
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
    /// Represents an index signature of the form '[parameterName: string|number]: type'.
    /// </summary>
    internal class TsIndexSignature : CodeModel, ITsIndexSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsIndexSignature(ITsIdentifier parameterName, bool isParameterNumberType, ITsType typeAnnotation)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            IsParameterNumberType = isParameterNumberType;
            TypeAnnotation = typeAnnotation ?? throw new ArgumentNullException(nameof(typeAnnotation));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public bool IsParameterNumberType { get; }
        public ITsType TypeAnnotation { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitIndexSignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitIndexSignature(this);

        public override string ToCodeDisplay()
        {
            string display = $"[{ParameterName.ToCodeDisplay()}: ";
            display += IsParameterNumberType ? "number" : "string";
            display += $"]: {TypeAnnotation.ToCodeDisplay()}";
            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("[");
            ParameterName.WriteFullCodeDisplay(writer);
            writer.Write(": ");
            writer.Write(IsParameterNumberType ? "number" : "string");
            writer.Write("]: ");
            TypeAnnotation.WriteFullCodeDisplay(writer);
        }
    }
}
