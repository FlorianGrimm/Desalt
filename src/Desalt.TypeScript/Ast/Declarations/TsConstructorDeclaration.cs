// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsConstructorDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Declarations
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a constructor declaration in a class.
    /// </summary>
    internal class TsConstructorDeclaration : AstNode<TsVisitor>, ITsConstructorDeclaration
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsConstructorDeclaration(
            TsAccessibilityModifier? accessibilityModifier = null,
            ITsParameterList parameterList = null,
            IEnumerable<ITsStatementListItem> functionBody = null)
        {
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

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitConstructorDeclaration(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(AccessibilityModifier.OptionalAccessibilityCodeDisplay());
                builder.Append("constructor(").Append(ParameterList).Append(")");
                builder.Append(FunctionBody.IsDefault ? ";" : FunctionBody.ToElidedList());

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            AccessibilityModifier.EmitOptionalAccessibility(emitter);

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
