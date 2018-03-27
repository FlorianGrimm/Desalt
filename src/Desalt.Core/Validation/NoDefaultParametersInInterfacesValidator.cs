// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoDefaultParametersInInterfacesValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System.Collections.Generic;
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
        public IExtendedResult<bool> Validate(DocumentTranslationContextWithSymbolTables context)
        {
            IEnumerable<Diagnostic> query =
                from iface in context.RootSyntax.DescendantNodes().OfType<InterfaceDeclarationSyntax>()
                from method in iface.Members.OfType<MethodDeclarationSyntax>()
                from parameter in method.ParameterList.Parameters
                where parameter.Default != null
                select DiagnosticFactory.InterfaceWithDefaultParam(
                    iface.Identifier.Text,
                    method.Identifier.Text,
                    parameter);

            DiagnosticList diagnostics = DiagnosticList.From(context.Options, query);
            return new SuccessResult(diagnostics.FilteredDiagnostics);
        }
    }
}
