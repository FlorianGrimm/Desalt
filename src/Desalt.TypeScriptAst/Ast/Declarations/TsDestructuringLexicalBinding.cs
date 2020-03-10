// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsDestructuringLexicalBinding.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a destructuring lexical binding of the form '{x, y}: type = foo' or '[x, y]:type = foo'.
    /// </summary>
    internal class TsDestructuringLexicalBinding : TsAstNode, ITsDestructuringLexicalBinding
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsDestructuringLexicalBinding(
            ITsBindingPattern bindingPattern,
            ITsType variableType = null,
            ITsExpression initializer = null)
        {
            BindingPattern = bindingPattern ?? throw new ArgumentNullException(nameof(bindingPattern));
            VariableType = variableType;
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsBindingPattern BindingPattern { get; }
        public ITsType VariableType { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitDestructuringLexicalBinding(this);

        public override string CodeDisplay =>
            $"{BindingPattern}{VariableType.OptionalTypeAnnotation()}{Initializer.OptionalAssignment()}";

        protected override void EmitInternal(Emitter emitter)
        {
            BindingPattern.Emit(emitter);
            VariableType.EmitOptionalTypeAnnotation(emitter);
            Initializer.EmitOptionalAssignment(emitter);
        }
    }
}
