// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTemplatePart.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Expressions
{
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a part of a template literal.
    /// </summary>
    internal class TsTemplatePart : TsAstNode, ITsTemplatePart
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTemplatePart(string template, ITsExpression? expression = null)
        {
            Template = template;
            Expression = expression;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string Template { get; }
        public ITsExpression? Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitTemplatePart(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            if (Template != null)
            {
                emitter.Write(Template);
            }

            if (Expression != null)
            {
                emitter.Write("${");
                Expression.Emit(emitter);
                emitter.Write("}");
            }
        }
    }
}
