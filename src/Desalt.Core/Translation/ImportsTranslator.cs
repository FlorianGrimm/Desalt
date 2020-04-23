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
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates import declarations for a set of types.
    /// </summary>
    internal class ImportsTranslator
    {
        private readonly ScriptSymbolTable _scriptSymbolTable;

        public ImportsTranslator(ScriptSymbolTable scriptSymbolTable)
        {
            _scriptSymbolTable = scriptSymbolTable ?? throw new ArgumentNullException(nameof(scriptSymbolTable));
        }

        /// <summary>
        /// Creates import statements, grouping them by file and module for each of the types to import.
        /// </summary>
        public IExtendedResult<IEnumerable<ITsImportDeclaration>> TranslateImports(
            DocumentTranslationContext context,
            IEnumerable<ITypeSymbol> typesToImport,
            CancellationToken cancellationToken = default)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Get all of the types that we should import.
            var importTypes = typesToImport.Where(ShouldImport).ToImmutableArray();

            // Find all of the imports that aren't defined anywhere and create an error.
            ITypeSymbol[] undefinedTypes = importTypes.Where(symbol => !_scriptSymbolTable.HasSymbol(symbol)).ToArray();

            var undefinedTypeErrors = undefinedTypes.Select(
                importType => DiagnosticFactory.UnknownType(importType.ToHashDisplay()));

            var diagnostics = new List<Diagnostic>(undefinedTypeErrors);

            // Get rid of all of the undefined imports.
            var validImportTypes = importTypes.Except(undefinedTypes);

            // Group each valid import by file name (but don't include the types that are defined in this class).
            var groupedByFileName = validImportTypes.OrderBy(symbol => symbol.Name)
                .Select(symbol => _scriptSymbolTable.Get<IScriptSymbol>(symbol))
                .OfType<IScriptTypeSymbol>()
                .Select(
                    scriptSymbol =>
                    {
                        ImportSymbolInfo importInfo = scriptSymbol.ImportInfo;
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

                        ISymbol symbol = scriptSymbol.Symbol;
                        return new
                        {
                            TypeName = _scriptSymbolTable.GetComputedScriptNameOrDefault(symbol, symbol.Name),
                            RelativePathOrModuleName = relativePathOrModuleName
                        };
                    })
                .Where(item => item.RelativePathOrModuleName != "./")
                .Distinct()
                .GroupBy(item => item.RelativePathOrModuleName);

            // Add an import statement for each group
            var importDeclarations = new List<ITsImportDeclaration>();
            foreach (var grouping in groupedByFileName)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Special case: mscorlib - needs to be imported like this: `import 'mscorlib';`
                if (grouping.Key == "mscorlib")
                {
                    ITsImportDeclaration import = Factory.ImportDeclaration(Factory.String("mscorlib"));
                    importDeclarations.Add(import);
                }
                else
                {
                    // Create the list inside of `import { list } from 'file'`.
                    ITsImportSpecifier[] importNames = grouping.Select(
                            item => Factory.ImportSpecifier(Factory.Identifier(item.TypeName)))
                        .ToArray();
                    ITsImportClause importClause = Factory.ImportClause(importNames.First(), importNames.Skip(1).ToArray());

                    // Create the from clause.
                    ITsFromClause fromClause = Factory.FromClause(Factory.String(grouping.Key));

                    ITsImportDeclaration import = Factory.ImportDeclaration(importClause, fromClause);
                    importDeclarations.Add(import);
                }
            }

            return new ExtendedResult<IEnumerable<ITsImportDeclaration>>(importDeclarations, diagnostics);
        }

        private bool ShouldImport(ITypeSymbol symbol)
        {
            // Don't import array types.
            if (symbol is IArrayTypeSymbol)
            {
                return false;
            }

            // Don't import native TypeScript types.
            if (symbol is INamedTypeSymbol namedTypeSymbol &&
                TypeTranslator.TranslatesToNativeTypeScriptType(namedTypeSymbol))
            {
                return false;
            }

            // Don't import types that get translated to a native TypeScript type - for example List<T> is really an array.
            if (_scriptSymbolTable.TryGetValue(symbol, out IScriptSymbol? scriptSymbol) &&
                TypeTranslator.IsNativeTypeScriptTypeName(scriptSymbol.ComputedScriptName))
            {
                return false;
            }

            // Don't import types from the Web assembly, since those are already built-in to TypeScript.
            if (symbol.ContainingAssembly?.Name == "Web")
            {
                return false;
            }

            return true;
        }
    }
}
