// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TempProject.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TestUtility
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
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

            string projectDir = Path.Combine("C:\\", projectName);

            // add a new project
            ProjectId projectId = ProjectId.CreateNewId(projectName);
            VersionStamp version = VersionStamp.Create();
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 1);
            var projectInfo = ProjectInfo.Create(
                    id: projectId,
                    filePath: Path.Combine(projectDir, $"{projectName}.csproj"),
                    version: version,
                    name: projectName,
                    assemblyName: projectName,
                    language: LanguageNames.CSharp,
                    compilationOptions: compilationOptions)
                .WithSaltarelleReferences();

            var project = workspace.AddProject(projectInfo);

            // add all of the files to the project
            foreach (TempProjectFile sourceFile in sourceFiles)
            {
                string filePath = Path.Combine(projectDir, sourceFile.FileName);

                var docId = DocumentId.CreateNewId(projectId, sourceFile.FileName);
                var loader = TextLoader.From(
                    TextAndVersion.Create(
                        text: SourceText.From(sourceFile.FileContents),
                        version: VersionStamp.Create(),
                        filePath: filePath));

                var documentInfo = DocumentInfo.Create(
                    id: docId,
                    name: sourceFile.FileName,
                    loader: loader,
                    filePath: filePath);

                workspace.AddDocument(documentInfo);
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
            Project project = _workspace.CurrentSolution.Projects.Single();

            // ReSharper disable once AssignNullToNotNullAttribute
            options = options ??
                new CompilerOptions(Path.Combine(Path.GetDirectoryName(project.FilePath), "outputPath"));

            Document document = project.Documents.Single(doc => doc.Name == fileName);

            IExtendedResult<DocumentTranslationContext> result =
                await DocumentTranslationContext.TryCreateAsync(document, options);
            result.Diagnostics.Should().BeEmpty();

            return result.Result;
        }

        public async Task<DocumentTranslationContextWithSymbolTables> CreateContextWithSymbolTablesForFileAsync(
            string fileName,
            CompilerOptions options = null,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes)
        {
            DocumentTranslationContext thisContext = null;

            // add all of the symbols from all of the documents in the project
            var contexts = new List<DocumentTranslationContext>();
            foreach (string docName in _workspace.CurrentSolution.Projects.Single().Documents.Select(doc => doc.Name))
            {
                DocumentTranslationContext context = await CreateContextForFileAsync(docName, options);
                if (docName == fileName)
                {
                    thisContext = context;
                }

                contexts.Add(context);
            }

            // create the import symbol table
            var importTable = ImportSymbolTable.Create(contexts, discoveryKind);

            // create the script name symbol table
            var scriptNameTable = ScriptNameSymbolTable.Create(contexts, discoveryKind);

            // create the inline code symbol table
            var inlineCodeTable = InlineCodeSymbolTable.Create(contexts, discoveryKind);

            thisContext.Should().NotBeNull();
            return new DocumentTranslationContextWithSymbolTables(
                thisContext,
                importTable,
                scriptNameTable,
                inlineCodeTable);
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
