// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGetAccessorMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Declarations
{
    using System;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a 'get' member accessor declaration in a class.
    /// </summary>
    internal class TsGetAccessorMemberDeclaration : TsAstNode, ITsGetAccessorMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsGetAccessorMemberDeclaration(
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

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public bool IsAbstract { get; }
        public ITsGetAccessor GetAccessor { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitGetAccessorMemberDeclaration(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            emitter.Write(IsStatic ? "static " : "");
            emitter.Write(IsAbstract ? "abstract " : "");
            GetAccessor?.Emit(emitter);
            emitter.WriteLine();
        }
    }
}
