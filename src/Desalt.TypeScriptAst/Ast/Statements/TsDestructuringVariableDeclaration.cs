// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsDestructuringVariableDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Statements
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a destructuring variable declaration of the form '{x, y}: type = foo' or '[x, y]:type = foo'.
    /// </summary>
    internal class TsDestructuringVariableDeclaration : TsAstNode, ITsDestructuringVariableDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsDestructuringVariableDeclaration(
            ITsBindingPattern bindingPattern,
            ITsExpression initializer,
            ITsType variableType = null)
        {
            BindingPattern = bindingPattern ?? throw new ArgumentNullException(nameof(bindingPattern));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            VariableType = variableType;
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

        public override void Accept(TsVisitor visitor) => visitor.VisitDestructuringVariableDeclaration(this);

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
