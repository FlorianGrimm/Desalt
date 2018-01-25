// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTryStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;

    /// <summary>
    /// Represents a try/catch/finally statement.
    /// </summary>
    internal class TsTryStatement : AstNode<TsVisitor>, ITsTryStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsTryStatement(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern catchParameter = null,
            ITsBlockStatement catchBlock = null,
            ITsBlockStatement finallyBlock = null)
        {
            TryBlock = tryBlock ?? throw new ArgumentNullException(nameof(tryBlock));
            CatchParameter = catchParameter;
            CatchBlock = catchBlock;
            FinallyBlock = finallyBlock;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsBlockStatement TryBlock { get; }
        public ITsBindingIdentifierOrPattern CatchParameter { get; }
        public ITsBlockStatement CatchBlock { get; }
        public ITsBlockStatement FinallyBlock { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsTryStatement CreateTry(ITsBlockStatement tryBlock) => new TsTryStatement(tryBlock);

        public static TsTryStatement CreateTryCatch(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern catchParameter,
            ITsBlockStatement catchBlock)
        {
            return new TsTryStatement(
                tryBlock,
                catchParameter ?? throw new ArgumentNullException(nameof(catchParameter)),
                catchBlock ?? throw new ArgumentNullException(nameof(catchBlock)));
        }

        public static TsTryStatement CreateTryFinally(ITsBlockStatement tryBlock, ITsBlockStatement finallyBlock)
        {
            return new TsTryStatement(
                tryBlock,
                finallyBlock: finallyBlock ?? throw new ArgumentNullException(nameof(finallyBlock)));
        }

        public static TsTryStatement CreateTryCatchFinally(
            ITsBlockStatement tryBlock,
            ITsBindingIdentifierOrPattern catchParameter,
            ITsBlockStatement catchBlock,
            ITsBlockStatement finallyBlock)
        {
            return new TsTryStatement(
                tryBlock,
                catchParameter ?? throw new ArgumentNullException(nameof(catchParameter)),
                catchBlock ?? throw new ArgumentNullException(nameof(catchBlock)),
                finallyBlock ?? throw new ArgumentNullException(nameof(finallyBlock)));
        }

        public override void Accept(TsVisitor visitor) => visitor.VisitTryStatement(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("try ").Append(TryBlock.CodeDisplay);

                if (CatchParameter != null)
                {
                    builder.AppendLine();
                    builder.Append("catch (").Append(CatchParameter.CodeDisplay).Append(")");
                    builder.Append(CatchBlock);
                }

                if (FinallyBlock != null)
                {
                    builder.AppendLine();
                    builder.Append("finally ").Append(FinallyBlock.CodeDisplay);
                }

                return builder.ToString();
            }
        }

        public override void Emit(Emitter emitter)
        {
            emitter.Write("try ");
            TryBlock.Emit(emitter);

            if (CatchParameter != null)
            {
                emitter.Write(" catch (");
                CatchParameter.Emit(emitter);
                emitter.Write(") ");
                CatchBlock.Emit(emitter);
            }

            if (FinallyBlock != null)
            {
                emitter.Write(" finally ");
                FinallyBlock.Emit(emitter);
            }

            emitter.WriteLine();
        }
    }
}
