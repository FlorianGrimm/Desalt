// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsForStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Text;
    using Desalt.Core.Ast;
    using Desalt.Core.Emit;
    using Desalt.Core.Extensions;

    /// <summary>
    /// Represents a 'for' loop.
    /// </summary>
    internal class TsForStatement : AstNode, ITsForStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a for loop of the form, 'for (i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            ITsExpression initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            Initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
            InitializerWithVariableDeclarations = ImmutableArray<ITsVariableDeclaration>.Empty;
            InitializerWithLexicalDeclaration = null;

            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Incrementor = incrementor ?? throw new ArgumentNullException(nameof(incrementor));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            IEnumerable<ITsVariableDeclaration> initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            Initializer = null;
            InitializerWithLexicalDeclaration = null;
            InitializerWithVariableDeclarations = initializer?.ToImmutableArray() ??
                throw new ArgumentNullException(nameof(initializer));
            if (InitializerWithVariableDeclarations.IsEmpty)
            {
                throw new ArgumentException(
                    "There must be at least one variable declaration in the initializer.", nameof(initializer));
            }

            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Incrementor = incrementor ?? throw new ArgumentNullException(nameof(incrementor));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (const i: number = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            ITsLexicalDeclaration initializer,
            ITsExpression condition,
            ITsExpression incrementor,
            ITsStatement statement)
        {
            Initializer = null;
            InitializerWithVariableDeclarations = ImmutableArray<ITsVariableDeclaration>.Empty;
            InitializerWithLexicalDeclaration = initializer ?? throw new ArgumentNullException(nameof(initializer));

            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Incrementor = incrementor ?? throw new ArgumentNullException(nameof(incrementor));
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression Initializer { get; }
        public ImmutableArray<ITsVariableDeclaration> InitializerWithVariableDeclarations { get; }
        public ITsLexicalDeclaration InitializerWithLexicalDeclaration { get; }

        public ITsExpression Condition { get; }
        public ITsExpression Incrementor { get; }
        public ITsStatement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor) => visitor.VisitForStatement(this);

        public override string CodeDisplay
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append("for (");

                if (Initializer != null)
                {
                    builder.Append(Initializer.CodeDisplay);
                    builder.Append("; ");
                }
                else if (InitializerWithVariableDeclarations.Length > 0)
                {
                    builder.Append("var ");
                    builder.Append(InitializerWithVariableDeclarations.ToElidedList());
                    builder.Append("; ");
                }
                else
                {
                    builder.Append(InitializerWithLexicalDeclaration.CodeDisplay);
                    builder.Append(" ");
                }

                builder.Append(Condition.CodeDisplay).Append("; ");
                builder.Append(Incrementor.CodeDisplay).Append(") ");
                builder.Append(Statement.CodeDisplay);

                return builder.ToString();
            }
        }

        protected override void EmitInternal(Emitter emitter)
        {
            emitter.Write("for (");

            if (Initializer != null)
            {
                Initializer.Emit(emitter);
                emitter.Write("; ");
            }
            else if (InitializerWithVariableDeclarations.Length > 0)
            {
                emitter.Write("var ");
                emitter.WriteList(InitializerWithVariableDeclarations, indent: false, itemDelimiter: ", ");
                emitter.Write("; ");
            }
            else
            {
                // Normally a lexical declaration ends in a newline, but we don't want that in our
                // for loop. This is kind of kludgy, but we'll create a temporary emitter for it
                // to use with spaces instead of newlines.
                using (var memoryStream = new MemoryStream())
                using (var tempEmitter = new Emitter(memoryStream, emitter.Encoding, emitter.Options.WithNewline(" ")))
                {
                    InitializerWithLexicalDeclaration.Emit(tempEmitter);
                    emitter.Write(memoryStream.ReadAllText(emitter.Encoding));
                }
            }

            Condition.Emit(emitter);
            emitter.Write("; ");

            Incrementor.Emit(emitter);

            Statement.EmitIndentedOrInBlock(emitter);
        }
    }
}
