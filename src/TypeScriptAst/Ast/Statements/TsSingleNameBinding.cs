﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSingleNameBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a single name binding pattern used in object and array bindings, of the form 'name = expression'.
    /// </summary>
    internal class TsSingleNameBinding : TsAstNode, ITsSingleNameBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSingleNameBinding(ITsIdentifier name, ITsExpression defaultValue = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            DefaultValue = defaultValue;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier Name { get; }
        public ITsExpression DefaultValue { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitSingleNameBinding(this);

        public override string CodeDisplay => $"{Name}{DefaultValue.OptionalAssignment()}";

        protected override void EmitInternal(Emitter emitter)
        {
            Name.Emit(emitter);
            DefaultValue.EmitOptionalAssignment(emitter);
        }
    }
}