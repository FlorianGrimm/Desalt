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
    using System.Threading;
    using System.Threading.Tasks;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
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

        public static async Task<TempProject> CreateAsync(string projectName, params TempProjectFile[] sourceFiles)
        {
            // create a new ad-hoc workspace
            var workspace = new AdhocWorkspace();

            // add a new project
            ProjectId projectId = ProjectId.CreateNewId(projectName);
            VersionStamp version = VersionStamp.Create();
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 1);
            var projectInfo = ProjectInfo.Create(
                    projectId,
                    version,
                    projectName,
                    projectName,
                    LanguageNames.CSharp,
                    compilationOptions: compilationOptions)
                .WithSaltarelleReferences();

            var project = workspace.AddProject(projectInfo);

            // add all of the files to the project
            foreach (TempProjectFile sourceFile in sourceFiles)
            {
                workspace.AddDocument(projectId, sourceFile.FileName, SourceText.From(sourceFile.FileContents));
            }

            // try to compile the project and report any diagnostics
            Compilation compilation = await project.GetCompilationAsync();
            compilation.GetDiagnostics().Should().BeEmpty();

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
            result.Diagnostics.Should().BeEmpty();

            return result.Result;
        }

        public async Task<DocumentTranslationContextWithSymbolTables> CreateContextWithSymbolTablesForFileAsync(
            string fileName,
            CompilerOptions options = null)
        {
            DocumentTranslationContext context = await CreateContextForFileAsync(fileName, options);

            // create the import symbol table
            var importTable = new ImportSymbolTable();
            importTable.AddDefinedTypesInDocument(context, CancellationToken.None);
            importTable.AddExternallyReferencedTypes(context, CancellationToken.None);

            // create the script name symbol table
            var scriptNameTable = new ScriptNameSymbolTable();
            scriptNameTable.AddDefinedTypesInDocument(context, CancellationToken.None);
            scriptNameTable.AddExternallyReferencedTypes(context, CancellationToken.None);

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
