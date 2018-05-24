// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGetSetAccessorMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents either a 'get' or a 'set' member accessor declaration in a class.
    /// </summary>
    internal class TsGetSetAccessorMemberDeclaration : AstNode,
        ITsGetAccessorMemberDeclaration, ITsSetAccessorMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsGetSetAccessorMemberDeclaration(
            ITsGetAccessor getAccessor,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isAbstract = false)
        {
            GetAccessor = getAccessor ?? throw new ArgumentNullException(nameof(getAccessor));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
        }

        public TsGetSetAccessorMemberDeclaration(
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
        public ITsGetAccessor GetAccessor { get; }
        public ITsSetAccessor SetAccessor { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            if (GetAccessor != null)
            {
                visitor.VisitGetAccessorMemberDeclaration(this);
            }
            else
            {
                visitor.VisitSetAccessorMemberDeclaration(this);
            }
        }

        public override string CodeDisplay =>
            AccessibilityModifier.OptionalCodeDisplay() +
            (IsStatic ? "static " : "") +
            (IsAbstract ? "abstract " : "") +
            (GetAccessor != null ? GetAccessor.CodeDisplay : SetAccessor.CodeDisplay);

        protected override void EmitInternal(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            emitter.Write(IsStatic ? "static " : "");
            emitter.Write(IsAbstract ? "abstract " : "");
            GetAccessor?.Emit(emitter);
            SetAccessor?.Emit(emitter);
            emitter.WriteLine();
        }
    }
}
