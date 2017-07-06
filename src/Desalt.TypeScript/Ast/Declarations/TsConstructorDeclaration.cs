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
                if (AccessibilityModifier.HasValue)
                {
                    builder.Append(AccessibilityModifier.Value.ToString().ToLowerInvariant()).Append(" ");
                }

                builder.Append("constructor(").Append(ParameterList).Append(")");
                builder.Append(FunctionBody.IsDefault ? ";" : FunctionBody.ToElidedList());

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            if (AccessibilityModifier.HasValue)
            {
                emitter.Write(AccessibilityModifier.Value.ToString().ToLowerInvariant());
                emitter.Write(" ");
            }

            emitter.Write("constructor(");
            ParameterList?.Emit(emitter);
            emitter.Write(")");

            if (FunctionBody.IsDefault)
            {
                emitter.WriteLine(";");
            }
            else
            {
                emitter.WriteBlock(FunctionBody, skipNewlines: true);
                emitter.WriteLine();
            }
        }
    }
}
