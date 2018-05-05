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
        public IExtendedResult<bool> Validate(DocumentTranslationContextWithSymbolTables context)
        {
            SemanticModel semanticModel = context.SemanticModel;
            ScriptNameSymbolTable scriptNameTable = context.ScriptNameSymbolTable;

            IEnumerable<Diagnostic> errors =

                // get all of the classes
                from @class in context.RootSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>()

                    // and all of the field declarations in the class
                from field in @class.Members.OfType<FieldDeclarationSyntax>()
                from fieldVariable in field.Declaration.Variables

                    // and all of the property declarations in the class
                from property in @class.Members.OfType<PropertyDeclarationSyntax>()

                    // try to get all of the symbols for the field and property
                let fieldSymbol = semanticModel.GetDeclaredSymbol(fieldVariable)
                let propertySymbol = semanticModel.GetDeclaredSymbol(property)
                where fieldSymbol != null &&
                    propertySymbol != null &&
                    scriptNameTable.HasSymbol(fieldSymbol) &&
                    scriptNameTable.HasSymbol(propertySymbol)

                // lookup the compiled names
                let fieldScriptName = context.ScriptNameSymbolTable[fieldSymbol]
                let propertyScriptName = context.ScriptNameSymbolTable[propertySymbol]

                // and check for any duplicate names
                where string.Equals(fieldScriptName, propertyScriptName, StringComparison.Ordinal)
                select DiagnosticFactory.ClassWithDuplicateFieldAndPropertyName(
                    @class.Identifier.Text,
                    fieldScriptName,
                    fieldVariable.GetLocation());

            DiagnosticList diagnostics = DiagnosticList.From(context.Options, errors);
            return new SuccessOnNoErrorsResult(diagnostics.FilteredDiagnostics);
        }
    }
}
