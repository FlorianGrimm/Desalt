// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIndexSignature.cs" company="Justin Rockwood">
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
    /// Represents an index signature of the form '[parameterName: string|number]: type'.
    /// </summary>
    internal class TsIndexSignature : AstNode, ITsIndexSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsIndexSignature(ITsIdentifier parameterName, bool isParameterNumberType, ITsType parameterType)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            IsParameterNumberType = isParameterNumberType;
            ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public bool IsParameterNumberType { get; }
        public ITsType ParameterType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitIndexSignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitIndexSignature(this);

        public override string ToCodeDisplay()
        {
            string display = $"[{ParameterName.ToCodeDisplay()}: ";
            display += IsParameterNumberType ? "number" : "string";
            display += $"]: {ParameterType}";
            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("[");
            ParameterName.WriteFullCodeDisplay(writer);
            writer.Write(": ");
            writer.Write(IsParameterNumberType ? "number" : "string");
            writer.Write("]: ");
            ParameterType.WriteFullCodeDisplay(writer);
        }
    }
}
