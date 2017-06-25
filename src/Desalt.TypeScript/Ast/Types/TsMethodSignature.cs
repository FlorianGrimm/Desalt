// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMethodSignature.cs" company="Justin Rockwood">
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
    /// Represents a method signature, which is a shorthand for declaring a property of a function type.
    /// </summary>
    internal class TsMethodSignature : AstNode, ITsMethodSignature
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

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitMethodSignature(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitMethodSignature(this);

        public override string ToCodeDisplay()
        {
            string display = PropertyName.ToCodeDisplay();
            if (IsOptional)
            {
                display += "?";
            }

            display += CallSignature.ToCodeDisplay();
            return display;
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            PropertyName.WriteFullCodeDisplay(writer);
            if (IsOptional)
            {
                writer.Write("?");
            }

            CallSignature.WriteFullCodeDisplay(writer);
        }
    }
}
