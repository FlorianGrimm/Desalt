// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="JsDictionaryTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Contains methods to aid in translating <c>JsDictionary</c> types.
    /// </summary>
    internal static class JsDictionaryTranslator
    {
        /// <summary>
        /// Returns a value indicating whether the specified type symbol is an instance of
        /// <c>JsDictionary</c> or <c>JsDictionary{TKey, TValue}</c>.
        /// </summary>
        /// <param name="typeSymbol">The <see cref="ITypeSymbol"/> to examine.</param>
        /// <returns>
        /// True if the specified type symbol is an instance of <c>JsDictionary</c> or
        /// <c>JsDictionary{TKey, TValue}</c>; otherwise, false.
        /// </returns>
        public static bool IsJsDictionary(ITypeSymbol? typeSymbol)
        {
            var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
            return namedTypeSymbol?.ToHashDisplay() == "System.Collections.JsDictionary" ||
                namedTypeSymbol?.OriginalDefinition?.ToHashDisplay() ==
                "System.Collections.Generic.JsDictionary<TKey, TValue>";
        }

        /// <summary>
        /// Attempts to translate an <see cref="ObjectCreationExpressionSyntax"/> of the form:
        /// <code>
        /// new JsDictionary("key", "value", expr, "value2");
        /// </code>
        /// to
        /// <code>
        /// { 'key': 'value', [expr]: 'value2' }
        /// </code>
        /// </summary>
        /// <param name="context">The <see cref="TranslationContext"/> to use.</param>
        /// <param name="node">The <see cref="ObjectCreationExpressionSyntax"/> node to translate.</param>
        /// <param name="translatedArgumentList">An already-translated argument list.</param>
        /// <param name="translation">
        /// The translated <see cref="ITsObjectLiteral"/>, or null if it was not translated.
        /// </param>
        /// <returns>True if the node was translated; otherwise, false.</returns>
        public static bool TryTranslateObjectCreation(
            TranslationContext context,
            ObjectCreationExpressionSyntax node,
            ITsArgumentList translatedArgumentList,
            [NotNullWhen(true)] out ITsObjectLiteral? translation)
        {
            // Nothing to translate if we don't have a JsDictionary.
            if (!IsJsDictionary(context.SemanticModel.GetTypeInfo(node).Type))
            {
                translation = null;
                return false;
            }

            // Arguments must be in pairs, so if there are less than 2 this initializer is just going
            // to be a normal ctor call, which is handled elsewhere.
            int argCount = node.ArgumentList?.Arguments.Count ?? 0;
            if (argCount < 2)
            {
                translation = null;
                return false;
            }

            // Check some preconditions that the Saltarelle compiler should have enforced.
            if (argCount % 2 != 0)
            {
                context.ReportInternalError(
                        "Saltarelle should have ensured that there are an even number of parameters to a JsDictionary ctor",
                        node.ArgumentList?.GetLocation() ?? node.GetLocation());
                translation = null;
                return false;
            }

            // The translated argument list may be an array. In which case, let's expand it so the rest of the code
            // below simply works on a list of arguments.
            ITsExpression[] translatedArguments =
                translatedArgumentList.Arguments.Length == 1 &&
                translatedArgumentList.Arguments[0].Expression is ITsArrayLiteral array
                    ? array.Elements.Select(x => x?.Expression).WhereNotNull().ToArray()
                    : translatedArgumentList.Arguments.Select(x => x.Expression).ToArray();

            if (argCount != translatedArguments.Length)
            {
                context.ReportInternalError(
                    "The translated argument list count should have been equal to the node's argument list count",
                    node.ArgumentList?.GetLocation() ?? node.GetLocation());
                translation = null;
                return false;
            }

            // Create the TypeScript object creation node.
            var propertyDefinitions = new List<ITsPropertyDefinition>();
            for (int i = 0; i < argCount; i += 2)
            {
                ITsExpression key = translatedArguments[i];
                ITsExpression value = translatedArguments[i + 1];

                // We may have an expression as a property name, which would have been translated as an expression
                // instead of a literal string or number.
                if (!(key is ITsPropertyName propertyName))
                {
                    propertyName = Factory.ComputedPropertyName(key);
                }

                ITsPropertyDefinition propertyDefinition = Factory.PropertyAssignment(propertyName, value);
                propertyDefinitions.Add(propertyDefinition);
            }

            translation = Factory.Object(propertyDefinitions);
            return true;
        }
    }
}
