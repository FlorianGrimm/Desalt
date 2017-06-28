// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5PropertyValueAssignment.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Expressions
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a property assignment in the following form: 'propertyName: value'.
    /// </summary>
    public class Es5PropertyValueAssignment : Es5AstNode, IEs5PropertyAssignment
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5PropertyValueAssignment(string propertyName, IEs5Expression value)
        {
            Param.VerifyString(propertyName, nameof(propertyName));
            PropertyName = propertyName;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string PropertyName { get; }
        public IEs5Expression Value { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitPropertyValueAssignment(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitPropertyValueAssignment(this);
        }

        public override string CodeDisplay => $"{PropertyName}: {Value}";

        public override void Emit(IndentedTextWriter writer)
        {
            writer.Write($"{PropertyName}: ");
            Value.Emit(writer);
        }
    }
}
