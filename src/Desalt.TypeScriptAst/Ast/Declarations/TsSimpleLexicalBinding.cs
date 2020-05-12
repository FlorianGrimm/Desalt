// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSimpleLexicalBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a simple lexical binding of the form 'x: type = y'.
    /// </summary>
    internal class TsSimpleLexicalBinding : TsAstNode, ITsSimpleLexicalBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSimpleLexicalBinding(
            ITsIdentifier variableName,
            ITsType? variableType = null,
            ITsExpression? initializer = null)
        {
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            VariableType = variableType;
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier VariableName { get; }
        public ITsType? VariableType { get; }
        public ITsExpression? Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitSimpleLexicalBinding(this);
        }

        public override string CodeDisplay
        {
            get
            {
                string display = VariableName.CodeDisplay;
                display += VariableType?.OptionalTypeAnnotation();
                display += Initializer?.OptionalAssignment();
                return display;
            }
        }

        protected override void EmitContent(Emitter emitter)
        {
            VariableName.Emit(emitter);
            VariableType?.EmitOptionalTypeAnnotation(emitter);
            Initializer?.EmitOptionalAssignment(emitter);
        }
    }

    public static class SimpleLexicalBindingExtensions
    {
        public static ITsSimpleLexicalBinding WithVariableType(this ITsSimpleLexicalBinding source, ITsType value)
        {
            return (source.VariableType == null && value == null) || source.VariableType?.Equals(value) == true
                ? source
                : new TsSimpleLexicalBinding(source.VariableName, value, source.Initializer);
        }
    }
}
