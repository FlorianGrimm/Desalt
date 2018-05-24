// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVariableMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Ast.Declarations
{
    using System;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a member variable declaration in a class.
    /// </summary>
    internal class TsVariableMemberDeclaration : TsAstNode,
        ITsVariableMemberDeclaration,
        ITsAmbientVariableMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsVariableMemberDeclaration(
            bool isAmbient,
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            IsAmbient = isAmbient;
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            IsReadOnly = isReadOnly;
            TypeAnnotation = typeAnnotation;
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public bool IsReadOnly { get; }
        public ITsPropertyName VariableName { get; }
        public ITsType TypeAnnotation { get; }
        public ITsExpression Initializer { get; }

        private bool IsAmbient { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsVariableMemberDeclaration Create(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            return new TsVariableMemberDeclaration(
                false,
                variableName,
                accessibilityModifier,
                isStatic,
                isReadOnly,
                typeAnnotation,
                initializer);
        }

        public static ITsAmbientVariableMemberDeclaration CreateAmbient(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            bool isReadOnly = false,
            ITsType typeAnnotation = null)
        {
            return new TsVariableMemberDeclaration(
                true,
                variableName,
                accessibilityModifier,
                isStatic,
                isReadOnly,
                typeAnnotation);
        }

        public override void Accept(TsVisitor visitor)
        {
            if (IsAmbient)
            {
                visitor.VisitAmbientVariableMemberDeclaration(this);
            }
            else
            {
                visitor.VisitVariableMemberDeclaration(this);
            }
        }

        public override string CodeDisplay =>
            AccessibilityModifier.OptionalCodeDisplay() +
            (IsStatic ? "static " : "") +
            (IsReadOnly ? "readonly " : "") +
            VariableName +
            TypeAnnotation.OptionalTypeAnnotation() +
            Initializer.OptionalAssignment() +
            ";";

        protected override void EmitInternal(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            emitter.Write(IsStatic ? "static " : "");
            emitter.Write(IsReadOnly ? "readonly " : "");
            VariableName.Emit(emitter);
            TypeAnnotation.EmitOptionalTypeAnnotation(emitter);
            Initializer.EmitOptionalAssignment(emitter);
            emitter.WriteLine(";");
        }
    }
}
