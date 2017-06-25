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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitPropertySignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitPropertySignature(this);

        public override string ToCodeDisplay() =>
            PropertyName + (IsOptional ? "?" : "") + PropertyType.ToTypeAnnotationCodeDisplay();

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            PropertyName.WriteFullCodeDisplay(writer);
            if (IsOptional)
            {
                writer.Write("?");
            }

            PropertyType.WriteTypeAnnotation(writer);
        }
    }
}
