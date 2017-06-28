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

        public void Accept(TsVisitor visitor) => visitor.VisitIndexSignature(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitIndexSignature(this);

        public override string CodeDisplay
        {
            get
            {
                string display = $"[{ParameterName.CodeDisplay}: ";
                display += IsParameterNumberType ? "number" : "string";
                display += $"]: {ParameterType}";
                return display;
            }
        }

        public override void Emit(IndentedTextWriter emitter)
        {
            emitter.Write("[");
            ParameterName.Emit(emitter);
            emitter.Write(": ");
            emitter.Write(IsParameterNumberType ? "number" : "string");
            emitter.Write("]: ");
            ParameterType.Emit(emitter);
        }
    }
}
