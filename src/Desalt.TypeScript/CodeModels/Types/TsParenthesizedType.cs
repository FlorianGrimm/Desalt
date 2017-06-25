// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParenthesizedType.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Types
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a parenthesized type, of the form '(Type)'.
    /// </summary>
    internal class TsParenthesizedType : AstNode, ITsParenthesizedType
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsParenthesizedType(ITsType type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsType Type { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitParenthesizedType(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitParenthesizedType(this);

        public override string ToCodeDisplay() => $"({Type.ToCodeDisplay()})";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("(");
            Type.WriteFullCodeDisplay(writer);
            writer.Write(")");
        }
    }
}
