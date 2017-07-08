// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsMemberFunctionDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a member function declaration in a class.
    /// </summary>
    internal class TsMemberFunctionDeclaration : AstNode<TsVisitor>, ITsMemberFunctionDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsMemberFunctionDeclaration(
            ITsPropertyName propertyName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;

            if (functionBody != null)
            {
                FunctionBody = functionBody.ToImmutableArray();
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public ITsPropertyName PropertyName { get; }
        public ITsCallSignature CallSignature { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitMemberFunctionDeclaration(this);

        public override string CodeDisplay =>
            $"{AccessibilityModifier.OptionalAccessibility()}{IsStatic.OptionalStaticDeclaration()}" +
            $"{PropertyName}{CallSignature.CodeDisplay}" +
            (FunctionBody.IsDefault ? ";" : FunctionBody.ToElidedList());

        public override void Emit(Emitter emitter)
        {
            AccessibilityModifier.EmitOptionalAccessibility(emitter);
            IsStatic.EmitOptionalStaticDeclaration(emitter);
            PropertyName.Emit(emitter);
            CallSignature.Emit(emitter);

            if (FunctionBody.IsDefault)
            {
                emitter.WriteLine(";");
            }
            else
            {
                emitter.Write(" ");
                emitter.WriteBlock(FunctionBody, skipNewlines: true);
                emitter.WriteLine();
            }
        }
    }
}
