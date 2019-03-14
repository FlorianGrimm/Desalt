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
    using System.Linq;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Pipeline;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Microsoft.CodeAnalysis;
    using TypeScriptAst.Ast;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

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
                importType => DiagnosticFactory.UnknownType(importType.ToHashDisplay()));

            var diagnostics = new List<Diagnostic>(undefinedTypeErrors);

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
                            relativePathOrModuleName = PathUtil.MakeRelativePath(
                                context.TypeScriptFilePath,
                                relativePathOrModuleName);

                            // remove the file extension
                            relativePathOrModuleName = PathUtil.ReplaceExtension(relativePathOrModuleName, "");

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
                            TypeName =
                                context.ScriptNameSymbolTable.GetComputedScriptNameOrDefault(symbol, symbol.Name),
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
                // special case: mscorlib - needs to be imported like this: `import 'mscorlib';`
                if (grouping.Key == "mscorlib")
                {
                    ITsImportDeclaration import = Factory.ImportDeclaration(Factory.String("mscorlib"));
                    importDeclarations.Add(import);
                }
                else
                {
                    // create the list inside of `import { list } from 'file'`
                    ITsImportSpecifier[] importNames = grouping.Select(
                            item => Factory.ImportSpecifier(Factory.Identifier(item.TypeName)))
                        .ToArray();
                    var importClause = Factory.ImportClause(importNames.First(), importNames.Skip(1).ToArray());

                    // create the from clause
                    ITsFromClause fromClause = Factory.FromClause(Factory.String(grouping.Key));

                    ITsImportDeclaration import = Factory.ImportDeclaration(importClause, fromClause);
                    importDeclarations.Add(import);
                }
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
            if (symbol is INamedTypeSymbol namedTypeSymbol &&
                TypeTranslator.TranslatesToNativeTypeScriptType(namedTypeSymbol))
            {
                return false;
            }

            // don't import types that get translated to a native TypeScript type - for example List<T> is really an array
            if (context.ScriptNameSymbolTable.TryGetValue(symbol, out IScriptSymbol scriptSymbol) &&
                TypeTranslator.IsNativeTypeScriptTypeName(scriptSymbol.ComputedScriptName))
            {
                return false;
            }

            // don't import types from the Web assembly, since those are already built-in to TypeScript
            if (symbol.ContainingAssembly?.Name == "Web")
            {
                return false;
            }

            return true;
        }
    }
}
