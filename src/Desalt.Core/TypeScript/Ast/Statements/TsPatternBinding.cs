// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsBindingElement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a recursive pattern binding in an object or array binding.
    /// </summary>
    internal class TsPatternBinding : AstNode, ITsPatternBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPatternBinding(ITsBindingPattern bindingPattern, ITsExpression initializer = null)
        {
            BindingPattern = bindingPattern ?? throw new ArgumentNullException(nameof(bindingPattern));
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsBindingPattern BindingPattern { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitPatternBinding(this);

        public override string CodeDisplay => $"{BindingPattern}{Initializer.OptionalAssignment()}";

        protected override void EmitInternal(Emitter emitter)
        {
            BindingPattern.Emit(emitter);
            Initializer.EmitOptionalAssignment(emitter);
        }
    }
}
