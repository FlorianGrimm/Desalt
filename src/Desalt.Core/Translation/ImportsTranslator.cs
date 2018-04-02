// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportsTranslator.cs" company="Justin Rockwood">
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
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.TypeScript.Ast;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Translates import declarations for a set of types.
    /// </summary>
    internal static class ImportsTranslator
    {
        /// <summary>
        /// Adds all of the import statements to the top of the file.
        /// </summary>
        public static IExtendedResult<IEnumerable<ITsImportDeclaration>> TranslateImports(
            DocumentTranslationContextWithSymbolTables context,
            IEnumerable<ISymbol> typesToImport)
        {
            // get all of the types that we should import
            ISymbol[] importTypes = typesToImport.Where(symbol => ShouldImport(symbol, context)).ToArray();

            // find all of the imports that aren't defined anywhere and create an error
            ISymbol[] undefinedTypes =
                importTypes.Where(symbol => !context.ImportSymbolTable.HasSymbol(symbol)).ToArray();

            var undefinedTypeErrors = undefinedTypes.Select(
                importType => DiagnosticFactory.UnknownType(SymbolTable.KeyFromSymbol(importType)));

            DiagnosticList diagnostics = DiagnosticList.From(context.Options, undefinedTypeErrors);

            // get rid of all of the undefined imports
            var validImportTypes = importTypes.Except(undefinedTypes);

            // group each valid import by file name (but don't include the types that are defined in this class).
            var groupedByFileName = validImportTypes.OrderBy(symbol => symbol.Name)
                .Select(
                    symbol =>
                    {
                        ImportSymbolInfo importInfo = context.ImportSymbolTable[symbol];
                        string relativePathOrModuleName = importInfo.RelativeTypeScriptFilePathOrModuleName;
                        if (importInfo.IsInternalReference)
                        {
                            relativePathOrModuleName = MakeRelativePath(
                                context.TypeScriptFilePath,
                                relativePathOrModuleName);

                            // remove the file extension
                            relativePathOrModuleName = Path.GetFileNameWithoutExtension(relativePathOrModuleName) ??
                                throw new InvalidOperationException("Something went wrong with path parsing");

                            // TypeScript import paths can always use forward slashes
                            relativePathOrModuleName = relativePathOrModuleName.Replace("\\", "/");

                            // make sure the path start with ./ or ../
                            if (!relativePathOrModuleName.StartsWith(".", StringComparison.Ordinal))
                            {
                                relativePathOrModuleName = "./" + relativePathOrModuleName;
                            }
                        }
                        else
                        {
                            relativePathOrModuleName = importInfo.RelativeTypeScriptFilePathOrModuleName;
                        }

                        return new
                        {
                            TypeName = context.ScriptNameSymbolTable.GetValueOrDefault(symbol, symbol.Name),
                            RelativePathOrModuleName = relativePathOrModuleName
                        };
                    })
                .Where(item => item.RelativePathOrModuleName != "./")
                .Distinct()
                .GroupBy(item => item.RelativePathOrModuleName);

            // add an import statement for each group
            var importDeclarations = new List<ITsImportDeclaration>();
            foreach (var grouping in groupedByFileName)
            {
                // create the list inside of `import { list } from 'file'`
                ITsImportSpecifier[] importNames = grouping
                    .Select(item => TsAstFactory.ImportSpecifier(TsAstFactory.Identifier(item.TypeName)))
                    .ToArray();
                var importClause = TsAstFactory.ImportClause(importNames.First(), importNames.Skip(1).ToArray());

                // create the from clause
                ITsFromClause fromClause = TsAstFactory.FromClause(TsAstFactory.String(grouping.Key));

                ITsImportDeclaration import = TsAstFactory.ImportDeclaration(importClause, fromClause);
                importDeclarations.Add(import);
            }

            return new ExtendedResult<IEnumerable<ITsImportDeclaration>>(importDeclarations, diagnostics);
        }

        private static bool ShouldImport(ISymbol symbol, DocumentTranslationContextWithSymbolTables context)
        {
            // don't import array types
            if (symbol is IArrayTypeSymbol)
            {
                return false;
            }

            // don't import native TypeScript types
            if (symbol is INamedTypeSymbol namedTypeSymbol && TypeTranslator.TranslatesToNativeTypeScriptType(namedTypeSymbol))
            {
                return false;
            }

            // don't import types that get translated to a native TypeScript type - for example List<T> is really an array
            if (context.ScriptNameSymbolTable.TryGetValue(symbol, out string scriptName) &&
                TypeTranslator.IsNativeTypeScriptTypeName(scriptName))
            {
                return false;
            }

            // don't import types from the Saltarelle.Web assembly, since those are already built-in to TypeScript
            if (symbol.ContainingAssembly?.Name == "Saltarelle.Web")
            {
                return false;
            }

            return true;
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
