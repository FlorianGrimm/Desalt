// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5ForInStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a 'for-in' loop.
    /// </summary>
    public sealed class Es5ForInStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal Es5ForInStatement(IEs5Expression leftSide, IEs5Expression rightSide, IEs5Statement statement)
        {
            LeftSide = leftSide ?? throw new ArgumentNullException(nameof(leftSide));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        internal Es5ForInStatement(
            Es5VariableDeclaration declaration,
            IEs5Expression rightSide,
            IEs5Statement statement)
        {
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public IEs5Expression LeftSide { get; }
        public Es5VariableDeclaration Declaration { get; }
        public IEs5Expression RightSide { get; }
        public IEs5Statement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitForInStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitForInStatement(this);
        }

        public override string CodeDisplay
        {
            get
            {
                string prefix = LeftSide?.ToString() ?? Declaration.ToString();
                return $"for ({prefix} in {RightSide}) {Statement}";
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("for (");
            if (LeftSide != null)
            {
                LeftSide.Emit(emitter);
            }
            else
            {
                Declaration.Emit(emitter);
            }

            emitter.Write(" in ");
            RightSide.Emit(emitter);
            emitter.Write(")");

            Statement.Emit(emitter);
        }
    }
}
