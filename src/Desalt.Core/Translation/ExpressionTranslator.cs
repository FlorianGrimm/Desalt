// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Immutable;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static partial class ExpressionTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Translates the C# expression into an equivalent TypeScript expression.
        /// </summary>
        /// <param name="context">The <see cref="TranslationContext"/> to use.</param>
        /// <param name="node">The C# syntax node to translate.</param>
        public static IExpressionTranslation Translate(TranslationContext context, ExpressionSyntax node)
        {
            var visitor = new ExpressionVisitor(context);
            var translatedExpression = visitor.StartVisit(node);
            return new Result(translatedExpression, visitor);
        }

        public static IExpressionTranslation Translate(TranslationContext context, EqualsValueClauseSyntax node)
        {
            var visitor = new ExpressionVisitor(context);
            var translatedExpression = visitor.VisitSubExpression(node.Value)
                .PrependTrailingCommentsFrom(node.EqualsToken, toLeadingTrivia: true);

            return new Result(translatedExpression, visitor);
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class Result : IExpressionTranslation
        {
            public Result(ITsExpression translatedExpression, ExpressionVisitor visitor)
            {
                Expression = translatedExpression;
                AdditionalStatementsRequiredBeforeExpression = visitor.AdditionalStatements.ToImmutableArray();
            }

            public ITsExpression Expression { get; }
            public ImmutableArray<ITsStatementListItem> AdditionalStatementsRequiredBeforeExpression { get; }
        }
    }

    public interface IExpressionTranslation
    {
        public ITsExpression Expression { get; }
        public ImmutableArray<ITsStatementListItem> AdditionalStatementsRequiredBeforeExpression { get; }
    }
}
