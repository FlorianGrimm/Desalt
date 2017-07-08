// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberVariableDeclaration.cs" company="Justin Rockwood">
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
    internal class TsMemberVariableDeclaration : AstNode<TsVisitor>, ITsMemberVariableDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMemberVariableDeclaration(
            ITsPropertyName propertyName,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            ITsType typeAnnotation = null,
            ITsExpression initializer = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
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
        public ITsPropertyName PropertyName { get; }
        public ITsType TypeAnnotation { get; }
        public ITsExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitMemberVariableDeclaration(this);

        public override string CodeDisplay =>
            $"{AccessibilityModifier.OptionalAccessibility()}{IsStatic.OptionalStaticDeclaration()}" +
            $"{PropertyName}{TypeAnnotation.OptionalTypeAnnotation()}{Initializer.OptionalAssignment()};";

        public override void Emit(Emitter emitter)
        {
            AccessibilityModifier.EmitOptionalAccessibility(emitter);
            IsStatic.EmitOptionalStaticDeclaration(emitter);
            PropertyName.Emit(emitter);
            TypeAnnotation.EmitOptionalTypeAnnotation(emitter);
            Initializer.EmitOptionalAssignment(emitter);
            emitter.WriteLine(";");
        }
    }
}
