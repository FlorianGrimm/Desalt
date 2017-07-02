// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsForInStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Ast.Statements
{
    using System;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a for-in loop of the form 'for (const x: type in expression) statement'.
    /// </summary>
    internal class TsForInStatement : AstNode<TsVisitor>, ITsForInStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a for-in loop of the form, 'for (x in expression) statement'.
        /// </summary>
        public TsForInStatement(ITsExpression initializer, ITsExpression rightSide, ITsStatement statement)
        {
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        /// <summary>
        /// Creates a for-in loop of the form, 'for (const x in expression) statement'.
        /// </summary>
        public TsForInStatement(
            ForDeclarationKind declarationKind,
            ITsBindingIdentifierOrPattern declaration,
            ITsExpression rightSide, ITsStatement statement)
        {
            DeclarationKind = declarationKind;
            Declaration = declaration ?? throw new ArgumentNullException(nameof(declaration));
            RightSide = rightSide ?? throw new ArgumentNullException(nameof(rightSide));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Initializer { get; }
        public ForDeclarationKind? DeclarationKind { get; }
        public ITsBindingIdentifierOrPattern Declaration { get; }

        public ITsExpression RightSide { get; }
        public ITsStatement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitForInStatement(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("for (");

                if (Initializer != null)
                {
                    builder.Append(Initializer.CodeDisplay);
                }
                else
                {
                    builder.Append(DeclarationKind.ToString().ToLowerInvariant());
                    builder.Append(Declaration.CodeDisplay);
                }

                builder.Append(" in ");

                builder.Append(RightSide.CodeDisplay).Append(") ");
                builder.Append(Statement.CodeDisplay);

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("for (");

            if (Initializer != null)
            {
                Initializer.Emit(emitter);
            }
            else
            {
                emitter.Write(DeclarationKind.ToString().ToLowerInvariant() + " ");
                Declaration.Emit(emitter);
            }

            emitter.Write(" in ");
            RightSide.Emit(emitter);

            Statement.EmitIndentedOrInBlock(emitter);
        }
    }
}
