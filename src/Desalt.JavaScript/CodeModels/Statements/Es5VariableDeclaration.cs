// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5VariableDeclaration.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.CodeModels.Statements
{
    using System;
    using Desalt.Core.Ast;
    using Desalt.Core.Utility;
    using Desalt.JavaScript.CodeModels;

    /// <summary>
    /// Represents a variable declaration of the form 'x' or 'x = y' (does not include the 'var'
    /// keyword - that's the <see cref="Es5VariableStatement"/>).
    /// </summary>
    public sealed class Es5VariableDeclaration : AstNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5VariableDeclaration(Es5Identifier identifier, IEs5Expression initializer = null)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Initializer = initializer;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5Identifier Identifier { get; }
        public IEs5Expression Initializer { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override string ToCodeDisplay()
        {
            return Initializer == null ? $"{Identifier}" : $"{Identifier} = {Initializer}";
        }

        public override void WriteFullCodeDisplay(IndentedTextWriter writer)
        {
            Identifier.WriteFullCodeDisplay(writer);

            if (Initializer == null)
            {
                return;
            }
            writer.Write(" = ");
            Initializer.WriteFullCodeDisplay(writer);
        }
    }
}
