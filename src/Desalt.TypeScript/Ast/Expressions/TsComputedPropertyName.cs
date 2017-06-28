// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsComputedPropertyName.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property name inside of an object of the form '[ expression ]'.
    /// </summary>
    internal class TsComputedPropertyName : AstNode, ITsComputedPropertyName
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsComputedPropertyName(ITsAssignmentExpression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsAssignmentExpression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitComputedPropertyName(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitComputedPropertyName(this);

        public override string CodeDisplay => $"[{Expression.CodeDisplay}]";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("[");
            Expression.WriteFullCodeDisplay(writer);
            writer.Write("]");
        }
    }
}
