// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsCoverInitializedName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents an element in an object initializer of the form 'identifer = expression'.
    /// </summary>
    internal class TsCoverInitializedName : AstNode<TsVisitor>, ITsCoverInitializedName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsCoverInitializedName(ITsIdentifier identifier, ITsExpression initializer)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Identifier { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitCoverInitializedName(this);

        public override string CodeDisplay => $"{Identifier} = ${Initializer}";

        public override void Emit(Emitter emitter)
        {
            Identifier.Emit(emitter);
            Initializer.EmitOptionalAssignment(emitter);
        }
    }
}
