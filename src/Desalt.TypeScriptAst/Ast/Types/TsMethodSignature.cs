// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMethodSignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a method signature, which is a shorthand for declaring a property of a function type.
    /// </summary>
    internal class TsMethodSignature : TsAstNode, ITsMethodSignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMethodSignature(ITsPropertyName propertyName, bool isOptional, ITsCallSignature callSignature)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            IsOptional = isOptional;
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public bool IsOptional { get; }
        public ITsCallSignature CallSignature { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitMethodSignature(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            PropertyName.Emit(emitter);
            if (IsOptional)
            {
                emitter.Write("?");
            }

            CallSignature.Emit(emitter);
        }
    }
}
