// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertySignature.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Types
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a property signature.
    /// </summary>
    internal class TsPropertySignature : TsAstNode, ITsPropertySignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertySignature(
            ITsPropertyName propertyName,
            ITsType? propertyType = null,
            bool isReadOnly = false,
            bool isOptional = false)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            IsReadOnly = isReadOnly;
            IsOptional = isOptional;
            PropertyType = propertyType;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public bool IsReadOnly { get; }
        public bool IsOptional { get; }
        public ITsType? PropertyType { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitPropertySignature(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            if (IsReadOnly)
            {
                emitter.Write("readonly ");
            }

            PropertyName.Emit(emitter);
            if (IsOptional)
            {
                emitter.Write("?");
            }

            PropertyType?.EmitOptionalTypeAnnotation(emitter);
        }
    }
}
