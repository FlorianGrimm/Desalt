// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsForStatement.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Represents a 'for' loop.
    /// </summary>
    internal class TsForStatement : TsAstNode, ITsForStatement
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a for loop of the form, 'for (i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            ITsExpression? initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            Initializer = initializer;
            InitializerWithVariableDeclarations = ImmutableArray<ITsVariableDeclaration>.Empty;
            InitializerWithLexicalDeclaration = null;

            Condition = condition;
            Incrementor = incrementor;
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (var i = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            IEnumerable<ITsVariableDeclaration> initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            Initializer = null;
            InitializerWithLexicalDeclaration = null;
            InitializerWithVariableDeclarations = initializer?.ToImmutableArray() ??
                throw new ArgumentNullException(nameof(initializer));
            if (InitializerWithVariableDeclarations?.IsEmpty == true)
            {
                throw new ArgumentException(
                    "There must be at least one variable declaration in the initializer.",
                    nameof(initializer));
            }

            Condition = condition;
            Incrementor = incrementor;
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        /// <summary>
        /// Creates a for loop of the form, 'for (const i: number = 0; i &lt; 10; i++) statement'.
        /// </summary>
        public TsForStatement(
            ITsLexicalDeclaration initializer,
            ITsExpression? condition,
            ITsExpression? incrementor,
            ITsStatement statement)
        {
            Initializer = null;
            InitializerWithVariableDeclarations = ImmutableArray<ITsVariableDeclaration>.Empty;
            InitializerWithLexicalDeclaration = initializer ?? throw new ArgumentNullException(nameof(initializer));

            Condition = condition;
            Incrementor = incrementor;
            Statement = statement ?? throw new ArgumentNullException(nameof(statement));
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public ITsExpression? Initializer { get; }
        public ImmutableArray<ITsVariableDeclaration>? InitializerWithVariableDeclarations { get; }
        public ITsLexicalDeclaration? InitializerWithLexicalDeclaration { get; }

        public ITsExpression? Condition { get; }
        public ITsExpression? Incrementor { get; }
        public ITsStatement Statement { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public override void Accept(TsVisitor visitor)
        {
            visitor.VisitForStatement(this);
        }

        protected override void EmitContent(Emitter emitter)
        {
            emitter.Write("for (");

            if (Initializer != null)
            {
                Initializer.Emit(emitter);
                emitter.Write("; ");
            }
            else if (InitializerWithVariableDeclarations?.Length > 0)
            {
                emitter.Write("var ");
                emitter.WriteList(InitializerWithVariableDeclarations, indent: false, itemDelimiter: ", ");
                emitter.Write("; ");
            }
            else
            {
                // Normally a lexical declaration ends in a newline, but we don't want that in our
                // for loop. This is kind of clunky, but we'll create a temporary emitter for it
                // to use with spaces instead of newlines.
                using var memoryStream = new MemoryStream();
                using var tempEmitter = new Emitter(memoryStream, emitter.Encoding, emitter.Options.WithNewline(" "));
                InitializerWithLexicalDeclaration?.Emit(tempEmitter);
                emitter.Write(memoryStream.ReadAllText(emitter.Encoding));
            }

            Condition?.Emit(emitter);
            emitter.Write("; ");

            Incrementor?.Emit(emitter);

            Statement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }
    }
}
