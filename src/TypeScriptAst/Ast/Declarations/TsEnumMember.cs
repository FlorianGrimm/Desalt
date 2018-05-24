// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsEnumMember.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents an enum member of the form, 'name = value'.
    /// </summary>
    internal class TsEnumMember : AstNode, ITsEnumMember
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsEnumMember(ITsPropertyName name, ITsExpression value = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName Name { get; }
        public ITsExpression Value { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitEnumMember(this);

        public override string CodeDisplay => $"{Name}{Value.OptionalAssignment()}";

        protected override void EmitInternal(Emitter emitter)
        {
            Name.Emit(emitter);
            Value.EmitOptionalAssignment(emitter);
        }
    }
}
