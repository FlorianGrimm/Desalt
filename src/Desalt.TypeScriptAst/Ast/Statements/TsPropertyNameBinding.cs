// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertyNameBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a property name binding pattern used in object and array bindings, of the form
    /// 'propertyName = expression'.
    /// </summary>
    internal class TsPropertyNameBinding : TsAstNode, ITsPropertyNameBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertyNameBinding(ITsPropertyName propertyName, ITsBindingElement bindingElement)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            BindingElement = bindingElement ?? throw new ArgumentNullException(nameof(bindingElement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsBindingElement BindingElement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitPropertyNameBinding(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            PropertyName.Emit(emitter);
            emitter.Write(": ");
            BindingElement.Emit(emitter);
        }
    }
}
