// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoDefaultParametersInInterfacesValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Validates that none of the interfaces contain default parameters, which TypeScript does not support.
    /// </summary>
    internal class NoDefaultParametersInInterfacesValidator : IValidator
    {
        public IExtendedResult<bool> Validate(ImmutableArray<DocumentTranslationContextWithSymbolTables> contexts)
        {
            IEnumerable<Diagnostic> query =
                from context in contexts.AsParallel()
                from iface in context.RootSyntax.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                from method in iface.Members.OfType<MethodDeclarationSyntax>()
                from parameter in method.ParameterList.Parameters
                where parameter.Default != null
                select DiagnosticFactory.InterfaceWithDefaultParam(
                    iface.Identifier.Text,
                    method.Identifier.Text,
                    parameter);

            if (contexts.Length == 0)
            {
                return new ExtendedResult<bool>(true);
            }

            DiagnosticList diagnostics = DiagnosticList.From(contexts[0].Options, query);
            return new SuccessOnNoErrorsResult(diagnostics.FilteredDiagnostics);
        }
    }
}
