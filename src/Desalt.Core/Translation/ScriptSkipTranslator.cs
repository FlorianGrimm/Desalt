// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptSkipTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using TypeScriptAst.Ast;

    /// <summary>
    /// Translates method and constructor declarations with [ScriptSkip] and invocations against methods and
    /// constructors marked with [ScriptSkip].
    /// </summary>
    internal class ScriptSkipTranslator
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly SemanticModel _semanticModel;
        private readonly ScriptSymbolTable _scriptSymbolTable;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        /// <summary>
        /// Creates a new instance of a <see cref="ScriptSkipTranslator"/> from the specified
        /// semantic model and symbol tables.
        /// </summary>
        /// <param name="semanticModel">The semantic model to use.</param>
        /// <param name="scriptSymbolTable"> A symbol table containing script names given a symbol.</param>
        public ScriptSkipTranslator(SemanticModel semanticModel, ScriptSymbolTable scriptSymbolTable)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));

            _scriptSymbolTable =
                scriptSymbolTable ?? throw new ArgumentNullException(nameof(scriptSymbolTable));
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Tries to translate an invocation expression that may contain a method marked with [ScriptSkip].
        /// </summary>
        /// <param name="node">The <see cref="InvocationExpressionSyntax"/> node to translate.</param>
        /// <param name="methodSymbol">The method symbol of the invocation.</param>
        /// <param name="translatedLeftSide">The already-translated left side of the invocation expression.</param>
        /// <param name="translatedArgumentList">The already-translated argument list for the method invocation.</param>
        /// <param name="translatedExpression">The translated expression if this method returns true; otherwise, null.</param>
        /// <param name="diagnostics">Contains any potential errors while attempting to translate.</param>
        /// <returns>
        /// True if the node was translated (it contained a [ScriptSkip] attribute); false if the node was not
        /// translated or if there was an error.
        /// </returns>
        public bool TryTranslateInvocationExpression(
            InvocationExpressionSyntax node,
            IMethodSymbol methodSymbol,
            ITsExpression translatedLeftSide,
            ITsArgumentList translatedArgumentList,
            ICollection<Diagnostic> diagnostics,
            [NotNullWhen(true)] out ITsExpression? translatedExpression)
        {
            // See if the method should be translated (if the [ScriptSkip] attribute is present).
            if (methodSymbol == null ||
                !_scriptSymbolTable.TryGetValue(methodSymbol, out IScriptMethodSymbol? scriptMethodSymbol) ||
                !scriptMethodSymbol.ScriptSkip)
            {
                translatedExpression = null;
                return false;
            }

            // Taken from the [ScriptSkip] documentation: This attribute causes a method to not be invoked. The method
            // must either be a static method with one argument (in case Foo.M(x) will become x), or an instance method
            // with no arguments (in which x.M() will become x). Can also be applied to a constructor, in which case the
            // constructor will not be called if used as an initializer (": base()" or ": this()").

            // [ScriptSkip] on a static method with a single argument: `Foo.M(x)` becomes `x`.
            if (methodSymbol.IsStatic)
            {
                if (translatedArgumentList.Arguments.Length != 1)
                {
                    translatedExpression = null;
                    diagnostics.Add(
                        DiagnosticFactory.IncorrectScriptSkipUsage(methodSymbol.Name, node.GetLocation()));
                    return false;
                }

                translatedExpression = translatedArgumentList.Arguments[0].Expression;
                return true;
            }

            // [ScriptSkip] on an instance method with no arguments
            if (translatedArgumentList.Arguments.Length != 0)
            {
                translatedExpression = null;
                diagnostics.Add(
                    DiagnosticFactory.IncorrectScriptSkipUsage(methodSymbol.Name, node.GetLocation()));
                return false;
            }

            translatedExpression = translatedLeftSide is ITsMemberDotExpression memberDotExpression
                ? memberDotExpression.LeftSide
                : translatedLeftSide;
            return true;
        }
    }
}
