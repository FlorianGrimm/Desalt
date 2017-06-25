// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCoverInitializedName.cs" company="Justin Rockwood">
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
    /// Represents an element in an object initializer of the form 'identifer = expression'.
    /// </summary>
    internal class TsCoverInitializedName : AstNode, ITsCoverInitializedName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCoverInitializedName(ITsIdentifier identifier, ITsAssignmentExpression initializer)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Identifier { get; }
        public ITsAssignmentExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TypeScriptVisitor visitor) => visitor.VisitCoverInitializedName(this);

        public T Accept<T>(TypeScriptVisitor<T> visitor) => visitor.VisitCoverInitializedName(this);

        public override string ToCodeDisplay() => $"{Identifier.ToCodeDisplay()} = ${Initializer.ToCodeDisplay()}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            Identifier.WriteFullCodeDisplay(writer);
            writer.Write(" = ");
            Initializer.WriteFullCodeDisplay(writer);
        }
    }
}
