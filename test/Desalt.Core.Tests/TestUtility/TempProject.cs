// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TempProject.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TestUtility
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Represents a temporary in-memory C# project that can be compiled using the Roslyn object model.
    /// </summary>
    internal sealed class TempProject : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private AdhocWorkspace _workspace;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TempProject(AdhocWorkspace workspace)
        {
            _workspace = workspace;
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TempProject Create(string projectName, params TempProjectFile[] sourceFiles)
        {
            // create a new ad-hoc workspace
            var workspace = new AdhocWorkspace();

            // add a new project
            var project = workspace.AddProject(projectName, "C#");

            // add all of the files to the project
            foreach (TempProjectFile sourceFile in sourceFiles)
            {
                workspace.AddDocument(project.Id, sourceFile.FileName, SourceText.From(sourceFile.FileContents));
            }

            return new TempProject(workspace);
        }

        public async Task<DocumentTranslationContext> CreateContextForFileAsync(
            string fileName,
            CompilerOptions options = null)
        {
            options = options ?? new CompilerOptions("outputPath");
            Project project = _workspace.CurrentSolution.Projects.Single();
            Document document = project.Documents.Single(doc => doc.Name == fileName);

            IExtendedResult<DocumentTranslationContext> result =
                await DocumentTranslationContext.TryCreateAsync(document, options);

            return result.Result;
        }

        public async Task<DocumentTranslationContextWithSymbolTables> CreateContextWithSymbolTablesForFileAsync(
            string fileName,
            CompilerOptions options = null)
        {
            DocumentTranslationContext context = await CreateContextForFileAsync(fileName, options);

            // create the import symbol table
            var importTable = new ImportSymbolTable();
            importTable.AddDefinedTypesInDocument(context);

            // create the script name symbol table
            var scriptNameTable = new ScriptNameSymbolTable();
            scriptNameTable.AddDefinedTypesInDocument(context);

            return new DocumentTranslationContextWithSymbolTables(context, importTable, scriptNameTable);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _workspace?.Dispose();
            _workspace = null;
        }
    }

    internal sealed class TempProjectFile
    {
        public TempProjectFile(string fileName, string fileContents)
        {
            FileName = fileName;
            FileContents = fileContents;
        }

        public string FileName { get; }
        public string FileContents { get; }
    }
}
