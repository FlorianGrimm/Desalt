// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseTranslationVisitor.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Threading;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Abstract base class that contains useful methods when translating from a C# AST into a TypeScript AST.
    /// </summary>
    internal abstract class BaseTranslationVisitor<TInput, TOutput> : CSharpSyntaxVisitor<TOutput>
        where TInput : SyntaxNode
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        protected BaseTranslationVisitor(TranslationContext context)
        {
            Context = context;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        protected TranslationContext Context { get; }

        protected CancellationToken CancellationToken => Context.CancellationToken;
        protected DiagnosticList Diagnostics => Context.Diagnostics;
        protected ScriptSymbolTable ScriptSymbolTable => Context.ScriptSymbolTable;
        protected SemanticModel SemanticModel => Context.SemanticModel;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public TOutput StartVisit(TInput node)
        {
            return Visit(node);
        }

        public sealed override TOutput Visit(SyntaxNode node)
        {
            CancellationToken.ThrowIfCancellationRequested();

            if (!(node is TInput))
            {
                Context.ReportInternalError(
                    $"This visitor should not be visiting nodes that aren't of type '{typeof(TInput).Name}'. " +
                    $"NodeType={node.GetType().Name}, Node='{node}'",
                    node);
            }

            return base.Visit(node);
        }

        public override TOutput DefaultVisit(SyntaxNode node)
        {
            Diagnostic diagnostic = DiagnosticFactory.TranslationNotSupported(node);
            Diagnostics.Add(diagnostic);
            throw new InvalidOperationException(diagnostic.ToString());
        }
    }
}
