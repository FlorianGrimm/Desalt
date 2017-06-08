// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5LabelledStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using System;
    using Desalt.Core.Utility;
    using Desalt.JavaScript.CodeModels;

    /// <summary>
    /// Returns a labelled statement of the form 'Identifier: Statement'.
    /// </summary>
    public sealed class Es5LabelledStatement : Es5CodeModel, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5LabelledStatement(Es5Identifier identifier, IEs5Statement statement)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5Identifier Identifier { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitLabelledStatement(this);
        }

        public override string ToCodeDisplay() => $"{Identifier}: {Statement}";

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            Identifier.WriteFullCodeDisplay(writer);
            writer.Write(": ");
            Statement.WriteFullCodeDisplay(writer);
        }
    }
}
