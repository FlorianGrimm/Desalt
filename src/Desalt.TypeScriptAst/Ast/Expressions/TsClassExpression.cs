// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a class declaration acting as an expression.
    /// </summary>
    internal class TsClassExpression : TsAstNode, ITsClassExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassExpression(
            ITsIdentifier? className = null,
            ITsClassHeritage? heritage = null,
            IEnumerable<ITsClassElement>? classBody = null)
        {
            ClassName = className;
            Heritage = heritage;
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier? ClassName { get; }
        public ITsClassHeritage? Heritage { get; }
        public ImmutableArray<ITsClassElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitClassExpression(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("class ");
            ClassName?.Emit(emitter);
            Heritage?.Emit(emitter);

            emitter.WriteBlock(ClassBody);
        }
    }
}
