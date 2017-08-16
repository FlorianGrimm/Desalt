// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionMemberDeclaration.cs" company="Justin Rockwood">
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
    internal class TsFunctionMemberDeclaration : AstNode<TsVisitor>,
        ITsFunctionMemberDeclaration, ITsAmbientFunctionMemberDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsFunctionMemberDeclaration(
            bool isAmbient,
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            IsAmbient = isAmbient;
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            FunctionBody = functionBody?.ToImmutableArray();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public ITsPropertyName FunctionName { get; }
        public ITsCallSignature CallSignature { get; }
        public ImmutableArray<ITsStatementListItem>? FunctionBody { get; }

        private bool IsAmbient { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsFunctionMemberDeclaration Create(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsFunctionMemberDeclaration(
                false, functionName, callSignature, accessibilityModifier, isStatic, functionBody);
        }

        public static ITsAmbientFunctionMemberDeclaration CreateAmbient(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false)
        {
            return new TsFunctionMemberDeclaration(true, functionName, callSignature, accessibilityModifier, isStatic);
        }

        public override void Accept(TsVisitor visitor)
        {
            if (IsAmbient)
            {
                visitor.VisitAmbientFunctionMemberDeclaration(this);
            }
            else
            {
                visitor.VisitFunctionMemberDeclaration(this);
            }
        }

        public override string CodeDisplay =>
            $"{AccessibilityModifier.OptionalCodeDisplay()}{IsStatic.OptionalStaticDeclaration()}" +
            $"{FunctionName}{CallSignature.CodeDisplay}" +
            (FunctionBody?.ToElidedList() ?? ";");

        public override void Emit(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            IsStatic.EmitOptionalStaticDeclaration(emitter);
            FunctionName.Emit(emitter);
            CallSignature.Emit(emitter);

            if (FunctionBody == null)
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
