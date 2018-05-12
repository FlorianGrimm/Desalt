﻿// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TempProject.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TestUtility
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
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

        private const string ProjectName = "TempProject";

        private AdhocWorkspace _workspace;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TempProject(AdhocWorkspace workspace)
        {
            _workspace = workspace;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public static string ProjectDir => Path.Combine("C:\\", ProjectName);

        public string OutputPath =>
            // ReSharper disable once AssignNullToNotNullAttribute
            Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.Projects.Single().FilePath), "outputPath");

        public CompilerOptions Options => new CompilerOptions(OutputPath);

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static Task<TempProject> CreateAsync(params string[] sourceFileContents)
        {
            if (sourceFileContents.Length == 1)
            {
                return CreateAsync(new TempProjectFile("File.cs", sourceFileContents[0]));
            }

            List<TempProjectFile> files = new List<TempProjectFile>(sourceFileContents.Length);
            for (int i = 0; i < sourceFileContents.Length; i++)
            {
                var file = new TempProjectFile($"File{i}.cs", sourceFileContents[i]);
                files.Add(file);
            }

            return CreateAsync(files.ToArray());
        }

        public static async Task<TempProject> CreateAsync(params TempProjectFile[] sourceFiles)
        {
            // create a new ad-hoc workspace
            var workspace = new AdhocWorkspace();

            // add a new project
            ProjectId projectId = ProjectId.CreateNewId(ProjectName);
            VersionStamp version = VersionStamp.Create();
            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, warningLevel: 1);
            var projectInfo = ProjectInfo.Create(
                    id: projectId,
                    filePath: Path.Combine(ProjectDir, $"{ProjectName}.csproj"),
                    version: version,
                    name: ProjectName,
                    assemblyName: ProjectName,
                    language: LanguageNames.CSharp,
                    compilationOptions: compilationOptions)
                .WithSaltarelleReferences();

            var project = workspace.AddProject(projectInfo);

            // add all of the files to the project
            foreach (TempProjectFile sourceFile in sourceFiles)
            {
                string filePath = Path.Combine(ProjectDir, sourceFile.FileName);

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
            string fileName = "File.cs",
            CompilerOptions options = null)
        {
            Project project = _workspace.CurrentSolution.Projects.Single();
            options = options ?? Options;
            Document document = project.Documents.Single(doc => doc.Name == fileName);

            IExtendedResult<DocumentTranslationContext> result =
                await DocumentTranslationContext.TryCreateAsync(document, options);
            result.Diagnostics.Should().BeEmpty();

            return result.Result;
        }

        public async Task<ImmutableArray<DocumentTranslationContextWithSymbolTables>>
            CreateContextsWithSymbolTablesAsync(
                CompilerOptions options = null,
                SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes)
        {
            // add all of the symbols from all of the documents in the project
            var contexts = (await Task.WhenAll(
                _workspace.CurrentSolution.Projects.Single()
                    .Documents.Select(doc => CreateContextForFileAsync(doc.Name, options)))).ToImmutableArray();

            ImmutableArray<ITypeSymbol> directlyReferencedExternalTypeSymbols =
                SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(contexts, discoveryKind);

            ImmutableArray<INamedTypeSymbol> indirectlyReferencedExternalTypeSymbols =
                SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                    directlyReferencedExternalTypeSymbols,
                    contexts.FirstOrDefault()?.SemanticModel.Compilation,
                    discoveryKind: discoveryKind);

            // create the import symbol table
            var importTable = ImportSymbolTable.Create(contexts, directlyReferencedExternalTypeSymbols);

            // create the script name symbol table
            var scriptNameTable = ScriptNameSymbolTable.Create(
                contexts,
                directlyReferencedExternalTypeSymbols,
                indirectlyReferencedExternalTypeSymbols);

            // create the inline code symbol table
            var inlineCodeTable = InlineCodeSymbolTable.Create(
                contexts,
                directlyReferencedExternalTypeSymbols,
                indirectlyReferencedExternalTypeSymbols);

            // create the alternate signature symbol table
            var result = AlternateSignatureSymbolTable.Create(contexts);
            result.Diagnostics.Should().BeEmpty();
            var alternateSignatureTable = result.Result;

            return contexts.Select(
                    context => new DocumentTranslationContextWithSymbolTables(
                        context,
                        importTable,
                        scriptNameTable,
                        inlineCodeTable,
                        alternateSignatureTable))
                .ToImmutableArray();
        }

        public async Task<DocumentTranslationContextWithSymbolTables> CreateContextWithSymbolTablesForFileAsync(
            string fileName = "File.cs",
            CompilerOptions options = null,
            SymbolTableDiscoveryKind discoveryKind = SymbolTableDiscoveryKind.DocumentAndReferencedTypes)
        {
            var allContexts = await CreateContextsWithSymbolTablesAsync(options, discoveryKind);
            DocumentTranslationContextWithSymbolTables thisContext =
                allContexts.First(context => context.Document.Name == fileName);

            return thisContext;
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
