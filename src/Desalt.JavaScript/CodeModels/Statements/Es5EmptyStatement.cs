// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5EmptyStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents an empty statement.
    /// </summary>
    public sealed class Es5EmptyStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal static readonly Es5EmptyStatement Instance = new Es5EmptyStatement();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5EmptyStatement()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitEmptyStatement(this);
        }

        public override string ToCodeDisplay() => ";";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            writer.Write(";");
        }
    }
}
