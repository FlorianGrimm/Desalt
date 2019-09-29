// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoDuplicateFieldAndPropertyNamesValidator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// Validates that there are no duplicate field and property names in a class. Saltarelle renames
    /// properties as <c>get_</c> and <c>set_</c>, so it's perfectly fine to have duplicate field and
    /// property names. However, in our TypeScript translation, we preserve the field names and
    /// property names, so we need to check that there aren't any duplicates.
    /// </summary>
    internal class NoDuplicateFieldAndPropertyNamesValidator : IValidator
    {
        public IExtendedResult<bool> Validate(ImmutableArray<DocumentTranslationContextWithSymbolTables> contexts)
        {
            if (contexts.Length == 0)
            {
                return new ExtendedResult<bool>(true);
            }

            SemanticModel semanticModel = contexts[0].SemanticModel;
            var context = contexts[0];

            IEnumerable<Diagnostic> errors =

                // get all of the classes in all of the documents
                from @class in context.RootSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>()

                    // and all of the field declarations in the class
                from field in @class.Members.OfType<FieldDeclarationSyntax>()
                from fieldVariable in field.Declaration.Variables

                    // and all of the property declarations in the class
                from property in @class.Members.OfType<PropertyDeclarationSyntax>()

                    // try to get all of the symbols for the field and property
                let fieldSymbol = semanticModel.GetDeclaredSymbol(fieldVariable)
                let propertySymbol = semanticModel.GetDeclaredSymbol(property)
                where fieldSymbol != null && propertySymbol != null

                // lookup the compiled names
                let fieldScriptName = context.ScriptSymbolTable[fieldSymbol].ComputedScriptName
                let propertyScriptName = context.ScriptSymbolTable[propertySymbol].ComputedScriptName

                // and check for any duplicate names
                where string.Equals(fieldScriptName, propertyScriptName, StringComparison.Ordinal)
                select DiagnosticFactory.ClassWithDuplicateFieldAndPropertyName(
                    @class.Identifier.Text,
                    fieldScriptName,
                    fieldVariable.GetLocation());

            return new SuccessOnNoErrorsResult(errors);
        }
    }
}
