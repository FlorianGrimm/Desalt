// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSingleNameBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a single name binding pattern used in object and array bindings, of the form 'name = expression'.
    /// </summary>
    internal class TsSingleNameBinding : AstNode<TsVisitor>, ITsSingleNameBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSingleNameBinding(ITsIdentifier name, ITsExpression initializer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Name { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitSingleNameBinding(this);

        public override string CodeDisplay => $"{Name} = ${Initializer}";

        public override void Emit(Emitter emitter)
        {
            Name.Emit(emitter);
            Initializer.EmitAssignment(emitter);
        }
    }
}
