// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.CodeModels.Expressions
{
    using System;
    using Desalt.Core.CodeModels;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an element within a class.
    /// </summary>
    internal sealed class TsClassElement : CodeModel, ITsClassElement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassElement(ITsMethodDefinition methodDefinition, bool isStatic)
        {
            MethodDefinition = methodDefinition ?? throw new ArgumentNullException(nameof(methodDefinition));
            IsStatic = isStatic;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public bool IsStatic { get; }
        public ITsMethodDefinition MethodDefinition { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitClassElement(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitClassElement(this);

        public override string ToCodeDisplay()
        {
            string display = IsStatic ? "static " : "";
            display += MethodDefinition.ToCodeDisplay();
            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            if (IsStatic)
            {
                writer.Write("static ");
            }

            MethodDefinition.WriteFullCodeDisplay(writer);
        }
    }
}
