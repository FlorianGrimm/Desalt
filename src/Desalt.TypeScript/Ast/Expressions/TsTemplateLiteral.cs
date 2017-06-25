// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTemplateLiteral.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a template literal of the form `string${Expression}`.
    /// </summary>
    internal class TsTemplateLiteral : AstNode, ITsTemplateLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTemplateLiteral(IEnumerable<TsTemplatePart> parts)
        {
            Parts = parts?.ToImmutableArray() ?? ImmutableArray<TsTemplatePart>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<TsTemplatePart> Parts { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitTemplateLiteral(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitTemplateLiteral(this);

        public override string ToCodeDisplay()
        {
            var builder = new StringBuilder("`");
            foreach (TsTemplatePart part in Parts)
            {
                if (part.Template != null)
                {
                    builder.Append(part.Template);
                }

                if (part.Expression != null)
                {
                    builder.Append("${").Append(part.Expression.ToCodeDisplay()).Append("}");
                }
            }

            builder.Append("`");
            return builder.ToString();
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("`");
            foreach (TsTemplatePart part in Parts)
            {
                if (part.Template != null)
                {
                    writer.Write(part.Template);
                }

                if (part.Expression != null)
                {
                    writer.Write("${");
                    part.Expression.WriteFullCodeDisplay(writer);
                    writer.Write("}");
                }
            }

            writer.Write("`");
        }
    }
}
