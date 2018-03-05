// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="CSharpToTypeScriptTranslator.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.TypeScript.Ast;
    using Factory = Desalt.Core.TypeScript.Ast.TsAstFactory;

    /// <summary>
    /// Converts a CSharp syntax tree into a TypeScript syntax tree.
    /// </summary>
    internal class CSharpToTypeScriptTranslator
    {
        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public IExtendedResult<ITsImplementationModule> TranslateDocument(
            DocumentTranslationContextWithSymbolTable context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var walker = new TranslationVisitor(context.Options, context.SemanticModel);
            var implementationModule = (ITsImplementationModule)walker.Visit(context.RootSyntax).Single();

            IExtendedResult<ITsImplementationModule> addImportsResult = AddImports(
                context,
                walker,
                implementationModule);

            return new ExtendedResult<ITsImplementationModule>(
                addImportsResult.Result,
                walker.Diagnostics.Concat(addImportsResult.Diagnostics));
        }

        /// <summary>
        /// Adds all of the import statements to the top of the file.
        /// </summary>
        private static IExtendedResult<ITsImplementationModule> AddImports(
            DocumentTranslationContextWithSymbolTable context,
            TranslationVisitor walker,
            ITsImplementationModule translatedModule)
        {
            string[] importTypes = walker.TypesToImport.ToArray();

            // find all of the imports that aren't defined anywhere and create an error
            string[] undefinedTypes = importTypes.Where(import => !context.SymbolTable.HasSymbol(import)).ToArray();
            var undefinedTypeErrors = undefinedTypes.Select(importType => DiagnosticFactory.UnknownType(importType));
            DiagnosticList diagnostics = DiagnosticList.From(context.Options, undefinedTypeErrors);

            // get rid of all of the undefined imports
            var validImportTypes = importTypes.Except(undefinedTypes);

            // group each valid import by file name
            var groupedByFileName = validImportTypes.OrderBy(_ => _)
                .GroupBy(
                    import =>
                    {
                        string tsPath = context.SymbolTable[import];
                        string relativePath = MakeRelativePath(context.TypeScriptFilePath, tsPath);

                        // remove the file extension
                        relativePath = Path.GetFileNameWithoutExtension(relativePath) ??
                            throw new InvalidOperationException("Something went wrong with path parsing");

                        // TypeScript import paths can always use forward slashes
                        relativePath = relativePath.Replace("\\", "/");

                        // make sure the path start with ./ or ../
                        if (!relativePath.StartsWith(".", StringComparison.Ordinal))
                        {
                            relativePath = "./" + relativePath;
                        }

                        return relativePath;
                    });

            // add an import statement for each group
            var importDeclarations = new List<ITsImportDeclaration>();
            foreach (IGrouping<string, string> grouping in groupedByFileName)
            {
                // create the list inside of `import { list } from 'file'`
                ITsImportSpecifier[] importNames = grouping
                    .Select(importName => Factory.ImportSpecifier(Factory.Identifier(importName)))
                    .ToArray();
                var importClause = Factory.ImportClause(importNames.First(), importNames.Skip(1).ToArray());

                // create the from clause
                ITsFromClause fromClause = Factory.FromClause(Factory.String(grouping.Key));

                ITsImportDeclaration import = Factory.ImportDeclaration(importClause, fromClause);
                importDeclarations.Add(import);
            }

            ITsImplementationModuleElement[] newElements =
                translatedModule.Elements.InsertRange(0, importDeclarations).ToArray();
            ITsImplementationModule moduleWithImports = Factory.ImplementationModule(newElements);

            return new ExtendedResult<ITsImplementationModule>(moduleWithImports, diagnostics);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">
        /// Contains the directory that defines the start of the relative path.
        /// </param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>
        /// The relative path from the start directory to the end path or <c>toPath</c> if the paths
        /// are not related.
        /// </returns>
        /// <remarks>Taken from Stack Overflow at <see href="https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path"/></remarks>
        private static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException(nameof(fromPath));
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException(nameof(toPath));
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // path can't be made relative
                return toPath;
            }

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }
    }
}
