// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ObjectLiteralExpression.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents on object literal of the form '{ propertyAssignment... }'.
    /// </summary>
    public sealed class Es5ObjectLiteralExpression : Es5AstNode, IEs5Expression
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ObjectLiteralExpression(IEnumerable<IEs5PropertyAssignment> properties)
        {
            PropertyAssignments = properties?.ToImmutableArray() ?? ImmutableArray<IEs5PropertyAssignment>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<IEs5PropertyAssignment> PropertyAssignments { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitObjectLiteralExpression(this);
        }

        public override string CodeDisplay => $"Object Literal, PropertyCount = {PropertyAssignments.Length}";

        public override void Emit(Emitter emitter)
        {
            WriteCommaNewlineSeparatedBlock(emitter, PropertyAssignments);
        }
    }
}
