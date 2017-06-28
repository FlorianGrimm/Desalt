// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Es5TryStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.JavaScript.Ast.Statements
{
    using System;
    using System.Text;
    using Desalt.Core.Utility;

    /// <summary>
    /// Represents a 'try/catch/finally' statement.
    /// </summary>
    public class Es5TryStatement : Es5AstNode, IEs5Statement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private Es5TryStatement(
            Es5BlockStatement tryBlock,
            Es5Identifier catchIdentifier = null,
            Es5BlockStatement catchBlock = null,
            Es5BlockStatement finallyBlock = null)
        {
            if (catchIdentifier == null && catchBlock != null)
            {
                throw new ArgumentNullException(nameof(catchIdentifier));
            }

            if (catchIdentifier != null && catchBlock == null)
            {
                throw new ArgumentNullException(nameof(catchBlock));
            }

            TryBlock = tryBlock ?? throw new ArgumentNullException(nameof(tryBlock));
            CatchIdentifier = catchIdentifier;
            CatchBlock = catchBlock;
            FinallyBlock = finallyBlock;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public Es5BlockStatement TryBlock { get; }
        public Es5Identifier CatchIdentifier { get; }
        public Es5BlockStatement CatchBlock { get; }
        public Es5BlockStatement FinallyBlock { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(Es5Visitor visitor)
        {
            visitor.VisitTryStatement(this);
        }

        public override T Accept<T>(Es5Visitor<T> visitor)
        {
            return visitor.VisitTryStatement(this);
        }

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder("try {...}");

                if (CatchBlock != null)
                {
                    builder.Append(" catch ");
                    if (CatchIdentifier != null)
                    {
                        builder.Append("(").Append(CatchIdentifier).Append(") ");
                    }
                    builder.Append("{...}");
                }

                if (FinallyBlock != null)
                {
                    builder.Append(" finally {...}");
                }

                return builder.ToString();
            }
        }

        public override void Emit(IndentedTextWriter emitter)
        {
            WriteItems(
                emitter, TryBlock.Statements, indent: true, prefix: "try {", suffix: "}",
                itemDelimiter: Environment.NewLine);

            if (CatchBlock != null)
            {
                emitter.Write(" catch ");
                if (CatchIdentifier != null)
                {
                    emitter.Write($"({CatchIdentifier}) ");
                }
                WriteBlock(emitter, CatchBlock.Statements);
            }

            if (FinallyBlock == null)
            {
                return;
            }

            emitter.Write(" finally ");
            WriteBlock(emitter, FinallyBlock.Statements);
        }

        public Es5TryStatement WithCatch(Es5Identifier catchIdentifier, Es5BlockStatement catchBlock)
        {
            return new Es5TryStatement(TryBlock, catchIdentifier, catchBlock, FinallyBlock);
        }

        public Es5TryStatement WithCatch(Es5Identifier catchIdentifier, params IEs5Statement[] catchStatements)
        {
            return new Es5TryStatement(TryBlock, catchIdentifier, new Es5BlockStatement(catchStatements), FinallyBlock);
        }

        public Es5TryStatement WithFinally(Es5BlockStatement finallyBlock)
        {
            return new Es5TryStatement(TryBlock, CatchIdentifier, CatchBlock, finallyBlock);
        }

        public Es5TryStatement WithFinally(params IEs5Statement[] finallyStatements)
        {
            return new Es5TryStatement(TryBlock, CatchIdentifier, CatchBlock, new Es5BlockStatement(finallyStatements));
        }

        internal static Es5TryStatement CreateTry(Es5BlockStatement tryBlock)
        {
            return new Es5TryStatement(tryBlock);
        }

        internal static Es5TryStatement CreateTry(params IEs5Statement[] tryStatements)
        {
            return new Es5TryStatement(new Es5BlockStatement(tryStatements));
        }

        internal static Es5TryStatement CreateTryCatch(
            Es5BlockStatement tryBlock,
            Es5Identifier catchIdentifier,
            Es5BlockStatement catchBlock)
        {
            return new Es5TryStatement(tryBlock, catchIdentifier, catchBlock);
        }

        internal static Es5TryStatement CreateTryCatchFinally(
            Es5BlockStatement tryBlock,
            Es5Identifier catchIdentifier,
            Es5BlockStatement catchBlock,
            Es5BlockStatement finallyBlock)
        {
            return new Es5TryStatement(tryBlock, catchIdentifier, catchBlock, finallyBlock);
        }
    }
}
