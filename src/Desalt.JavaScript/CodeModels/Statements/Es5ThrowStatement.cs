// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ThrowStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using System;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'throw' statement.
    /// </summary>
    public sealed class Es5ThrowStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ThrowStatement(IEs5Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression Expression { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitThrowStatement(this);
        }

        public override string ToCodeDisplay()
        {
            return $"throw {Expression};";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write("throw ");
            Expression.WriteFullCodeDisplay(writer);
            writer.Write(";");
        }
    }
}
