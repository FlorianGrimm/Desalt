// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoPartialClassesValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Validates that there are no partial classes, which TypeScript doesn't support.
    /// </summary>
    internal class NoPartialClassesValidator : IValidator
    {
        public IExtendedResult<bool> Validate(ImmutableArray<DocumentTranslationContextWithSymbolTables> contexts)
        {
            if (contexts.Length == 0)
            {
                return new ExtendedResult<bool>(true);
            }

            var errors = from context in contexts.AsParallel()
                         from classNode in context.RootSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>()
                         where classNode.Modifiers.Any(token => token.IsKind(SyntaxKind.PartialKeyword))
                         select DiagnosticFactory.PartialClassesNotSupported(classNode);

            DiagnosticList diagnostics = DiagnosticList.From(contexts[0].Options, errors);
            return new SuccessOnNoErrorsResult(diagnostics.FilteredDiagnostics);
        }
    }
}
