// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsSetAccessorMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a 'set' member accessor declaration in a class.
    /// </summary>
    internal class TsSetAccessorMemberDeclaration : TsAstNode, ITsSetAccessorMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsSetAccessorMemberDeclaration(
            ITsSetAccessor setAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            SetAccessor = setAccessor ?? throw new ArgumentNullException(nameof(setAccessor));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public bool IsAbstract { get; }
        public ITsSetAccessor SetAccessor { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitSetAccessorMemberDeclaration(this);
        }

        public override string CodeDisplay =>
            AccessibilityModifier.OptionalCodeDisplay() +
            (IsStatic ? "static " : "") +
            (IsAbstract ? "abstract " : "") +
            SetAccessor.CodeDisplay;

        protected override void EmitContent(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            emitter.Write(IsStatic ? "static " : "");
            emitter.Write(IsAbstract ? "abstract " : "");
            SetAccessor?.Emit(emitter);
            emitter.WriteLine();
        }
    }
}
