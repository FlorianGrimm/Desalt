// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsIndexSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an index signature of the form '[parameterName: string|number]: type'.
    /// </summary>
    internal class TsIndexSignature : AstNode<TsVisitor>, ITsIndexSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsIndexSignature(ITsIdentifier parameterName, bool isParameterNumberType, ITsType returnType)
        {
            ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            IsParameterNumberType = isParameterNumberType;
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ParameterName { get; }
        public bool IsParameterNumberType { get; }
        public ITsType ReturnType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitIndexSignature(this);

        public override string CodeDisplay
        {
            get
            {
                string display = $"[{ParameterName.CodeDisplay}: ";
                display += IsParameterNumberType ? "number" : "string";
                display += $"]: {ReturnType}";
                return display;
            }
        }

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("[");
            ParameterName.Emit(emitter);
            emitter.Write(": ");
            emitter.Write(IsParameterNumberType ? "number" : "string");
            emitter.Write("]: ");
            ReturnType.Emit(emitter);
        }
    }
}
