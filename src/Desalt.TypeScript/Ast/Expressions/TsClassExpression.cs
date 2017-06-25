// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsClassExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a class declaration acting as an expression.
    /// </summary>
    internal class TsClassExpression : AstNode, ITsClassExpression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsClassExpression(
            ITsIdentifier className = null,
            ITsLeftHandSideExpression heritage = null,
            IEnumerable<ITsClassElement> classBody = null)
        {
            ClassName = className;
            Heritage = heritage;
            ClassBody = classBody?.ToImmutableArray() ?? ImmutableArray<ITsClassElement>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsIdentifier ClassName { get; }
        public ITsLeftHandSideExpression Heritage { get; }
        public ImmutableArray<ITsClassElement> ClassBody { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitClassExpression(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitClassExpression(this);

        public override string ToCodeDisplay()
        {
            return $"class {ClassName?.ToCodeDisplay()}" +
                (Heritage != null ? $" extends {Heritage}" : "") +
                $"{{ {ClassBody.ToElidedList()} }}";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("class ");
            ClassName?.WriteFullCodeDisplay(writer);

            if (Heritage != null)
            {
                if (ClassName != null)
                {
                    writer.Write(" ");
                }

                writer.Write("extends ");
                Heritage.WriteFullCodeDisplay(writer);
            }

            WriteBlock(writer, ClassBody);
        }
    }
}
