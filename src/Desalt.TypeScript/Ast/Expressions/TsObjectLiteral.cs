// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsObjectLiteral.cs" company="Justin Rockwood">
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
    /// Represents on object literal of the form '{ PropertyDefinition... }'.
    /// </summary>
    internal class TsObjectLiteral : AstNode, ITsObjectLiteral
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsObjectLiteral(IEnumerable<ITsPropertyDefinition> propertyDefinitions)
        {
            PropertyDefinitions = propertyDefinitions?.ToImmutableArray() ??
                ImmutableArray<ITsPropertyDefinition>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ImmutableArray<ITsPropertyDefinition> PropertyDefinitions { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitObjectLiteral(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitObjectLiteral(this);

        public override string CodeDisplay => $"Object Literal, PropertyCount = {PropertyDefinitions.Length}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            WriteCommaNewlineSeparatedBlock(writer, PropertyDefinitions);
        }
    }
}
