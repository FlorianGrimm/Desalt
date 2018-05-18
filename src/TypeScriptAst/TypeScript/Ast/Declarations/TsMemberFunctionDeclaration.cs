// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsFunctionMemberDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.TypeScript.Ast.Declarations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using TypeScriptAst.Emit;

    /// <summary>
    /// Represents a member function declaration in a class.
    /// </summary>
    internal class TsFunctionMemberDeclaration : AstNode,
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
            bool isAbstract = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            IsAmbient = isAmbient;
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            CallSignature = callSignature ?? throw new ArgumentNullException(nameof(callSignature));
            AccessibilityModifier = accessibilityModifier;
            IsStatic = isStatic;
            IsAbstract = isAbstract;
            FunctionBody = functionBody?.ToImmutableArray();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public bool IsStatic { get; }
        public bool IsAbstract { get; }
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
            bool isAbstract = false,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsFunctionMemberDeclaration(
                isAmbient: false,
                functionName: functionName,
                callSignature: callSignature,
                accessibilityModifier: accessibilityModifier,
                isStatic: isStatic,
                isAbstract: isAbstract,
                functionBody: functionBody);
        }

        public static ITsAmbientFunctionMemberDeclaration CreateAmbient(
            ITsPropertyName functionName,
            ITsCallSignature callSignature,
            TsAccessibilityModifier? accessibilityModifier = null,
            bool isStatic = false)
        {
            return new TsFunctionMemberDeclaration(
                isAmbient: true,
                functionName: functionName,
                callSignature: callSignature,
                accessibilityModifier: accessibilityModifier,
                isStatic: isStatic);
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
            AccessibilityModifier.OptionalCodeDisplay() +
            (IsStatic ? "static " : "") +
            (IsAbstract ? "abstract " : "") +
            $"{FunctionName}{CallSignature.CodeDisplay}" +
            (FunctionBody?.ToElidedList() ?? ";");

        protected override void EmitInternal(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);
            emitter.Write(IsStatic ? "static " : "");
            emitter.Write(IsAbstract ? "abstract " : "");
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
