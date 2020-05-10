// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="StatementTranslator.Visitor.VariableDeclarations.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.TypeScriptAst.Ast;
    using Desalt.TypeScriptAst.Ast.Declarations;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed partial class StatementVisitor
    {
        //// =======================================================================================================
        //// Variable Declaration Statements
        //// =======================================================================================================

        /// <summary>
        /// Called when the visitor visits a LocalDeclarationStatementSyntax node of the form `var x = y;`. A <see
        /// cref="LocalDeclarationStatementSyntax"/> contains a series of <see cref="VariableDeclaratorSyntax"/> for
        /// each variable declaration.
        /// </summary>
        /// <returns>An <see cref="ITsLexicalDeclaration"/>.</returns>
        public override IEnumerable<ITsStatementListItem> VisitLocalDeclarationStatement(
            LocalDeclarationStatementSyntax node)
        {
            // TODO: figure out if the variable ever changes to determine if it's const vs. let
            bool isConst = node.IsConst;

            // Get the type of all of the declarations.
            ITsType type = TypeTranslator.TranslateTypeSymbol(
                Context,
                Context.GetExpectedTypeSymbol(node.Declaration.Type),
                node.Declaration.Type.GetLocation);

            // Translate all of the VariableDeclaratorSyntax nodes.
            ITsLexicalDeclaration translated = TranslateDeclarationVariables(
                node.Declaration.Variables,
                isConst,
                type,
                out ImmutableArray<ITsStatementListItem> additionalStatements);

            return additionalStatements.Add(translated);
        }

        /// <summary>
        /// Called when the visitor visits a <see cref="VariableDeclarationSyntax"/> node, which is a variable
        /// declaration within a blocked statement like 'using' or 'for'. A <see cref="VariableDeclarationSyntax"/>
        /// contains a series of <see cref="VariableDeclaratorSyntax"/> for each variable declaration.
        /// </summary>
        /// <returns>An <see cref="ITsLexicalDeclaration"/>.</returns>
        private ITsLexicalDeclaration TranslateVariableDeclaration(
            VariableDeclarationSyntax node,
            out ImmutableArray<ITsStatementListItem> additionalStatements)
        {
            // TODO: Determine whether this should be a const or let declaration
            const bool isConst = false;

            // Iterate over all of the variables and translate them.
            ITsLexicalDeclaration translated = TranslateDeclarationVariables(
                node.Variables,
                isConst,
                type: null,
                out additionalStatements);

            return translated;
        }

        /// <summary>
        /// Called when the visitor visits a VariableDeclaratorSyntax node of the form `x [= y]`, which is an
        /// identifier followed by an optional initializer expression.
        /// </summary>
        /// <param name="node">The node to translate.</param>
        /// <param name="additionalStatements">
        /// Contains any temporary variable declarations that need to be prepended before the current statement.
        /// </param>
        /// <returns>An <see cref="ITsSimpleLexicalBinding"/>.</returns>
        private ITsSimpleLexicalBinding TranslateVariableDeclarator(
            VariableDeclaratorSyntax node,
            out ImmutableArray<ITsStatementListItem> additionalStatements)
        {
            ITsIdentifier variableName = TsAstFactory.Identifier(node.Identifier.Text);

            ITsExpression? initializer = null;
            if (node.Initializer != null)
            {
                var expressionTranslation = ExpressionTranslator.Translate(Context, node.Initializer);
                initializer = expressionTranslation.Expression;
                additionalStatements = expressionTranslation.AdditionalStatementsRequiredBeforeExpression;
            }

            // Note that we don't return the type here since in C# the type is declared first and
            // then it can have multiple variable declarators. The type will get bound in the parent
            // Visit callers.
            ITsSimpleLexicalBinding translated = TsAstFactory.SimpleLexicalBinding(
                variableName,
                initializer: initializer);
            return translated;
        }

        /// <summary>
        /// Translates a list of <see cref="VariableDeclaratorSyntax"/> nodes, taking into account user-defined
        /// operators. Used in both <see cref="TranslateVariableDeclaration"/> and <see
        /// cref="VisitLocalDeclarationStatement"/>, where the only difference is whether a type definition is added
        /// to the <see cref="ITsLexicalDeclaration"/>.
        /// </summary>
        /// <param name="variables">The list of <see cref="VariableDeclaratorSyntax"/> nodes to translate.</param>
        /// <param name="isConst">TODO: Determine whether this should be a const or let declaration</param>
        /// <param name="type">An optional type to add to the lexical declaration.</param>
        /// <param name="additionalStatements">
        /// Contains any temporary variable declarations that need to be prepended before the current statement.
        /// </param>
        /// <returns>A <see cref="ITsLexicalDeclaration"/> containing all of the translated initializers.</returns>
        private ITsLexicalDeclaration TranslateDeclarationVariables(
            SeparatedSyntaxList<VariableDeclaratorSyntax> variables,
            bool isConst,
            ITsType? type,
            out ImmutableArray<ITsStatementListItem> additionalStatements)
        {
            var preDeclarationStatements = new List<ITsStatementListItem>();
            var declarations = new List<ITsSimpleLexicalBinding>();

            ITsLexicalDeclaration CreateDeclaration()
            {
                ITsLexicalBinding[] bindings =
                    (type != null ? declarations.Select(x => x.WithVariableType(type)) : declarations)
                    .Cast<ITsLexicalBinding>()
                    .ToArray();

                return TsAstFactory.LexicalDeclaration(isConst, bindings);
            }

            foreach (var variableDeclaratorNode in variables)
            {
                ITsSimpleLexicalBinding translatedBinding = TranslateVariableDeclarator(
                    variableDeclaratorNode,
                    out ImmutableArray<ITsStatementListItem> additionalStatementsForVariable);

                if (!additionalStatementsForVariable.IsDefaultOrEmpty)
                {
                    // Add any already-translated bindings to a new lexical declaration that should appear before the
                    // additional statements (temporary variable declarations).
                    if (declarations.Any())
                    {
                        ITsLexicalDeclaration combinedDeclaration = CreateDeclaration();
                        preDeclarationStatements.Add(combinedDeclaration);

                        declarations.Clear();
                    }

                    // Add the additional statements (temporary variable declarations).
                    preDeclarationStatements.AddRange(additionalStatementsForVariable);
                }

                declarations.Add(translatedBinding);
            }

            ITsLexicalDeclaration declaration = CreateDeclaration();
            additionalStatements = preDeclarationStatements.ToImmutableArray();
            return declaration;
        }
    }
}
