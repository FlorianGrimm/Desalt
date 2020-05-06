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
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Utility;
    using Desalt.TypeScriptAst.Ast;
    using Microsoft.CodeAnalysis;
    using Factory = TypeScriptAst.Ast.TsAstFactory;

    /// <summary>
    /// Translates import declarations for a set of types.
    /// </summary>
    internal static class ImportsTranslator
    {
        /// <summary>
        /// Creates import statements, grouping them by file and module for each of the types to import.
        /// </summary>
        public static ImmutableArray<ITsImportDeclaration> GatherImportDeclarations(
            TranslationContext context,
            string typeScriptFilePath)
        {
            // Get all of the types that we should import.
            ITypeSymbol[] importTypes =
                context.TypesToImport.Where(typeSymbol => ShouldImport(context, typeSymbol)).ToArray();

            // Find all of the imports that aren't defined anywhere and create an error.
            ITypeSymbol[] undefinedTypes =
                importTypes.Where(symbol => !context.ScriptSymbolTable.HasSymbol(symbol)).ToArray();

            Diagnostic[] undefinedTypeErrors = undefinedTypes.Select(
                importType => DiagnosticFactory.UnknownType(importType.ToHashDisplay())).ToArray();

            context.Diagnostics.AddRange(undefinedTypeErrors);

            // Get rid of all of the undefined imports.
            IEnumerable<ITypeSymbol> validImportTypes = importTypes.Except(undefinedTypes);

            // Group each valid import by file name (but don't include the types that are defined in this class).
            var groupedByFileName = validImportTypes.OrderBy(symbol => symbol.Name)
                .Select(symbol => context.ScriptSymbolTable.Get<IScriptSymbol>(symbol))
                .OfType<IScriptTypeSymbol>()
                .Select(
                    scriptSymbol =>
                    {
                        ImportSymbolInfo importInfo = scriptSymbol.ImportInfo;
                        string relativePathOrModuleName = importInfo.RelativeTypeScriptFilePathOrModuleName;
                        if (importInfo.IsInternalReference)
                        {
                            relativePathOrModuleName = PathUtil.MakeRelativePath(
                                typeScriptFilePath,
                                relativePathOrModuleName);

                            // Remove the file extension.
                            relativePathOrModuleName = PathUtil.ReplaceExtension(relativePathOrModuleName, "");

                            // TypeScript import paths can always use forward slashes.
                            relativePathOrModuleName = relativePathOrModuleName.Replace("\\", "/");

                            // Make sure the path starts with './' or '../'.
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
                            TypeName = context.ScriptSymbolTable.GetComputedScriptNameOrDefault(symbol, symbol.Name),
                            RelativePathOrModuleName = relativePathOrModuleName
                        };
                    })
                .Where(item => item.RelativePathOrModuleName != "./")
                .Distinct()
                .GroupBy(item => item.RelativePathOrModuleName);

            // Add an import statement for each group.
            var importDeclarations = new List<ITsImportDeclaration>();
            foreach (var grouping in groupedByFileName)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

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
                    ITsImportClause importClause = Factory.ImportClause(
                        importNames.First(),
                        importNames.Skip(1).ToArray());

                    // Create the from clause.
                    ITsFromClause fromClause = Factory.FromClause(Factory.String(grouping.Key));

                    ITsImportDeclaration import = Factory.ImportDeclaration(importClause, fromClause);
                    importDeclarations.Add(import);
                }
            }

            return importDeclarations.ToImmutableArray();
        }

        private static bool ShouldImport(TranslationContext context, ITypeSymbol symbol)
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
            if (context.ScriptSymbolTable.TryGetValue(symbol, out IScriptSymbol? scriptSymbol) &&
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
