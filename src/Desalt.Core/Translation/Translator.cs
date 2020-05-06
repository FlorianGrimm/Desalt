// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Translator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.Diagnostics;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Converts a CSharp syntax tree into a TypeScript syntax tree.
    /// </summary>
    internal static class Translator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Translates a C# document into a TypeScript document.
        /// </summary>
        /// <param name="context">The <see cref="DocumentTranslationContextWithSymbolTables"/> to use.</param>
        /// <param name="skipImports">
        /// Determines whether an imports section is emitted at the top of the TypeScript file. Should only be used for
        /// unit tests.
        /// </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to control canceling a translation.</param>
        /// <returns>A translated TypeScript module document.</returns>
        public static IExtendedResult<ITsImplementationModule> TranslateDocument(
            DocumentTranslationContextWithSymbolTables context,
            bool skipImports = false,
            CancellationToken cancellationToken = default)
        {
            var diagnostics = new DiagnosticList(context.Options.DiagnosticOptions);
            var translationContext = new TranslationContext(context, diagnostics, cancellationToken: cancellationToken);

            ITsImplementationModule implementationModule =
                TranslateCompilationUnit(translationContext, context.RootSyntax);

            if (!skipImports)
            {
                implementationModule = AddImports(translationContext, context, implementationModule);
            }

            return new ExtendedResult<ITsImplementationModule>(implementationModule, diagnostics);
        }

        private static ITsImplementationModule TranslateCompilationUnit(
            TranslationContext translationContext,
            CompilationUnitSyntax node)
        {
            var visitor = new TranslationVisitor(translationContext);
            var elements = node.Members.SelectMany(visitor.Visit).Cast<ITsImplementationModuleElement>().ToArray();
            ITsImplementationModule implementationScript = Factory.ImplementationModule(elements);
            return implementationScript;
        }

        private static ITsImplementationModule AddImports(
            TranslationContext translationContext,
            DocumentTranslationContextWithSymbolTables documentContext,
            ITsImplementationModule implementationModule)
        {
            var importsTranslator = new ImportsTranslator(translationContext.ScriptSymbolTable);
            IExtendedResult<IEnumerable<ITsImportDeclaration>> importDeclarations = importsTranslator.GatherImportDeclarations(
                documentContext,
                translationContext.TypesToImport,
                translationContext.CancellationToken);

            // Insert the imports at the top of the translated file.
            ImmutableArray<ITsImplementationModuleElement> newElements =
                implementationModule.Elements.InsertRange(0, importDeclarations.Result);
            ITsImplementationModule moduleWithImports = Factory.ImplementationModule(newElements);
            return moduleWithImports;
        }
    }
}
