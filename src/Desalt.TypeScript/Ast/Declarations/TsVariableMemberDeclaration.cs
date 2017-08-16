// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsVariableMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a member variable declaration in a class.
    /// </summary>
    internal class TsVariableMemberDeclaration : AstNode<TsVisitor>,
        ITsVariableMemberDeclaration, ITsAmbientVariableMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsVariableMemberDeclaration(
            bool isAmbient,
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            IsAmbient = isAmbient;
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            TypeAnnotation = typeAnnotation;
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
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
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            return new TsVariableMemberDeclaration(
                false, variableName, accessibilityModifier, isStatic, typeAnnotation, initializer);
        }

        public static ITsAmbientVariableMemberDeclaration CreateAmbient(
            ITsPropertyName variableName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            ITsType typeAnnotation = null)
        {
            return new TsVariableMemberDeclaration(
                true, variableName, accessibilityModifier, isStatic, typeAnnotation);
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
            $"{AccessibilityModifier.OptionalCodeDisplay()}{IsStatic.OptionalStaticDeclaration()}" +
            $"{VariableName}{TypeAnnotation.OptionalTypeAnnotation()}{Initializer.OptionalAssignment()};";

        public override void Emit(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            IsStatic.EmitOptionalStaticDeclaration(emitter);
            VariableName.Emit(emitter);
            TypeAnnotation.EmitOptionalTypeAnnotation(emitter);
            Initializer.EmitOptionalAssignment(emitter);
            emitter.WriteLine(";");
        }
    }
}
