// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsConstructorDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a constructor declaration in a class.
    /// </summary>
    internal class TsConstructorDeclaration : AstNode,
        ITsConstructorDeclaration, ITsAmbientConstructorDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsConstructorDeclaration(
            bool isAmbient,
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList parameterList = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            IsAmbient = isAmbient;
            AccessibilityModifier = accessibilityModifier;
            ParameterList = parameterList;
            if (functionBody != null)
            {
                FunctionBody = functionBody.ToImmutableArray();
            }
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsAccessibilityModifier? AccessibilityModifier { get; }
        public ITsParameterList ParameterList { get; }
        public ImmutableArray<ITsStatementListItem> FunctionBody { get; }

        private bool IsAmbient { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static ITsConstructorDeclaration Create(
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList parameterList = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
            return new TsConstructorDeclaration(false, accessibilityModifier, parameterList, functionBody);
        }

        public static ITsAmbientConstructorDeclaration CreateAmbient(ITsParameterList parameterList = null) =>
            new TsConstructorDeclaration(true, parameterList: parameterList);

        public override void Accept(TsVisitor visitor)
        {
            if (IsAmbient)
            {
                visitor.VisitAmbientConstructorDeclaration(this);
            }
            else
            {
                visitor.VisitConstructorDeclaration(this);
            }
        }

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(AccessibilityModifier.OptionalCodeDisplay());
                builder.Append("constructor(").Append(ParameterList).Append(")");
                builder.Append(FunctionBody.IsDefault ? ";" : FunctionBody.ToElidedList());

                return builder.ToString();
            }
        }

        protected override void EmitInternal(Emitter emitter)
        {
            AccessibilityModifier.EmitOptional(emitter);

            emitter.Write("constructor(");
            ParameterList?.Emit(emitter);
            emitter.Write(")");

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
