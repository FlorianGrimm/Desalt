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
    using Microsoft.CodeAnalysis;
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
                implementationModule = AddImports(translationContext, context.TypeScriptFilePath, implementationModule);
            }

            return new ExtendedResult<ITsImplementationModule>(implementationModule, diagnostics);
        }

        private static ITsImplementationModule TranslateCompilationUnit(
            TranslationContext translationContext,
            CompilationUnitSyntax node)
        {
            var visitor = new Visitor(translationContext);
            var elements = node.Members.SelectMany(visitor.Visit).ToArray();
            ITsImplementationModule implementationScript = Factory.ImplementationModule(elements);
            return implementationScript;
        }

        private static ITsImplementationModule AddImports(
            TranslationContext translationContext,
            string typeScriptFilePath,
            ITsImplementationModule implementationModule)
        {
            ImmutableArray<ITsImportDeclaration> importDeclarations = ImportsTranslator.GatherImportDeclarations(
                translationContext,
                typeScriptFilePath);

            // Insert the imports at the top of the translated file.
            ImmutableArray<ITsImplementationModuleElement> newElements =
                implementationModule.Elements.InsertRange(0, importDeclarations);
            ITsImplementationModule moduleWithImports = Factory.ImplementationModule(newElements);
            return moduleWithImports;
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private sealed class Visitor : BaseTranslationVisitor<SyntaxNode, IEnumerable<ITsImplementationModuleElement>>
        {
            public Visitor(TranslationContext context)
                : base(context)
            {
            }

            public override IEnumerable<ITsImplementationModuleElement> VisitNamespaceDeclaration(
                NamespaceDeclarationSyntax node)
            {
                // TODO #77 - Support namespaces. For now, we'll just return the elements in the namespace.
                var members = node.Members.SelectMany(Visit);
                return members;
            }

            public override IEnumerable<ITsImplementationModuleElement> VisitInterfaceDeclaration(
                InterfaceDeclarationSyntax node)
            {
                ITsInterfaceDeclaration translated = InterfaceTranslator.TranslateInterfaceDeclaration(Context, node);
                ITsImplementationModuleElement exported = ExportIfNeeded(translated, node);
                yield return exported;
            }

            public override IEnumerable<ITsImplementationModuleElement> VisitEnumDeclaration(EnumDeclarationSyntax node)
            {
                ITsEnumDeclaration translated = EnumTranslator.TranslateEnumDeclaration(Context, node);
                ITsImplementationModuleElement exported = ExportIfNeeded(translated, node);
                yield return exported;
            }

            public override IEnumerable<ITsImplementationModuleElement> VisitClassDeclaration(
                ClassDeclarationSyntax node)
            {
                IClassTranslation translated = ClassTranslator.TranslateClassDeclaration(Context, node);
                ITsImplementationModuleElement exported = ExportIfNeeded(translated.ClassDeclaration, node);
                yield return exported;

                if (translated.StaticCtorInvocationStatement != null)
                {
                    yield return translated.StaticCtorInvocationStatement;
                }
            }

            /// <summary>
            /// Converts the translated declaration to an exported declaration if the C# declaration is public.
            /// </summary>
            /// <param name="translatedDeclaration">The TypeScript declaration to conditionally export.</param>
            /// <param name="node">The C# syntax node to inspect.</param>
            /// <returns>
            /// If the type does not need to be exported, <paramref name="translatedDeclaration"/> is
            /// returned; otherwise a wrapped exported <see cref="ITsExportImplementationElement"/> is returned.
            /// </returns>
            private ITsImplementationModuleElement ExportIfNeeded(
                ITsImplementationElement translatedDeclaration,
                BaseTypeDeclarationSyntax node)
            {
                // Determine if this declaration should be exported
                INamedTypeSymbol symbol = Context.GetExpectedDeclaredSymbol<INamedTypeSymbol>(node);
                if (symbol.DeclaredAccessibility != Accessibility.Public)
                {
                    return translatedDeclaration;
                }

                ITsExportImplementationElement exportedInterfaceDeclaration =
                    Factory.ExportImplementationElement(translatedDeclaration);
                return exportedInterfaceDeclaration;
            }
        }
    }
}
