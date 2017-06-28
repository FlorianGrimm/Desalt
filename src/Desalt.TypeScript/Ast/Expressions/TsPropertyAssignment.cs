// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsPropertyAssignment.cs" company="Justin Rockwood">
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
    /// Represents a property assignment in the following form: 'propertyName: value'.
    /// </summary>
    internal class TsPropertyAssignment : AstNode, ITsPropertyAssignment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsPropertyAssignment(ITsPropertyName propertyName, ITsAssignmentExpression initializer)
        {
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsPropertyName PropertyName { get; }
        public ITsAssignmentExpression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void Accept(TsVisitor visitor) => visitor.VisitPropertyAssignment(this);

        public T Accept<T>(TsVisitor<T> visitor) => visitor.VisitPropertyAssignment(this);

        public override string CodeDisplay => $"{PropertyName.CodeDisplay}: {Initializer.CodeDisplay}";

        public override void Emit(IndentedTextWriter writer)
        {
            PropertyName.Emit(writer);
            writer.Write(": ");
            Initializer.Emit(writer);
        }
    }
}
