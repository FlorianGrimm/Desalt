// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsArrayType.cs" company="Justin Rockwood">
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
    /// Represents a TypeScript array type.
    /// </summary>
    internal class TsArrayType : AstNode, ITsArrayType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsArrayType(ITsPrimaryType type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPrimaryType Type { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitArrayType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitArrayType(this);

        public override string ToCodeDisplay() => $"{Type.ToCodeDisplay()}[]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            Type.WriteFullCodeDisplay(writer);
            writer.Write("[]");
        }
    }
}
