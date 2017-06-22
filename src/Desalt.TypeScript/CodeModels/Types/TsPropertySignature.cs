// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertySignature.cs" company="Justin Rockwood">
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
    /// Represents a property signature.
    /// </summary>
    internal class TsPropertySignature : CodeModel, ITsPropertySignature
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertySignature(
            ITsLiteralPropertyName propertyName,
            bool isNullable = false,
            ITsType typeAnnotation = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            IsNullable = isNullable;
            TypeAnnotation = typeAnnotation;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsLiteralPropertyName PropertyName { get; }
        public bool IsNullable { get; }
        public ITsType TypeAnnotation { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitPropertySignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitPropertySignature(this);

        public override string ToCodeDisplay()
        {
            return PropertyName.ToCodeDisplay() +
                (IsNullable ? "?" : "") +
                (TypeAnnotation != null ? $": {TypeAnnotation.ToCodeDisplay()}" : "");
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            PropertyName.WriteFullCodeDisplay(writer);
            if (IsNullable)
            {
                writer.Write("?");
            }

            TypeAnnotation.WriteTypeAnnotation(writer);
        }
    }
}
