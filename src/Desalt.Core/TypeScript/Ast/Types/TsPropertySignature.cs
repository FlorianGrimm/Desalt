// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertySignature.cs" company="Justin Rockwood">
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
    /// Represents a property signature.
    /// </summary>
    internal class TsPropertySignature : AstNode, ITsPropertySignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertySignature(
            ITsLiteralPropertyName propertyName,
            bool isOptional = false,
            ITsType propertyType = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            IsOptional = isOptional;
            PropertyType = propertyType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsLiteralPropertyName PropertyName { get; }
        public bool IsOptional { get; }
        public ITsType PropertyType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitPropertySignature(this);

        public override string CodeDisplay => PropertyName + (IsOptional ? "?" : "") +
            PropertyType.OptionalTypeAnnotation();

        protected override void EmitInternal(Emitter emitter)
        {
            PropertyName.Emit(emitter);
            if (IsOptional)
            {
                emitter.Write("?");
            }

            PropertyType.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
