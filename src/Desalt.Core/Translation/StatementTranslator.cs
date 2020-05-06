// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StatementTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Translates statements from C# to TypeScript.
    /// </summary>
    internal static partial class StatementTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Translates the C# expression into an equivalent TypeScript expression.
        /// </summary>
        /// <param name="context">The <see cref="TranslationContext"/> to use.</param>
        /// <param name="node">The C# syntax node to translate.</param>
        public static ImmutableArray<ITsStatementListItem> Translate(
            TranslationContext context,
            StatementSyntax node)
        {
            var visitor = new StatementVisitor(context);
            IEnumerable<ITsStatementListItem> statements = visitor.StartVisit(node);
            return statements.ToImmutableArray();
        }

        public static ITsBlockStatement TranslateBlockStatement(TranslationContext context, BlockSyntax node)
        {
            var visitor = new StatementVisitor(context);
            var blockStatement = visitor.TranslateBlock(node);
            return blockStatement;
        }
    }
}
