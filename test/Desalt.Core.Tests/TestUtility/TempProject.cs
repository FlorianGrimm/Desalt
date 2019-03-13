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
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
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

        private readonly AdhocWorkspace _workspace;

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
            Path.Combine(Path.GetDirectoryName(_workspace.CurrentSolution.Projects.Single().FilePath)!, "outputPath");

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

            var files = new List<TempProjectFile>(sourceFileContents.Length);
            for (int i = 0; i < sourceFileContents.Length; i++)
            {
                var file = new TempProjectFile($"File{i}.cs", sourceFileContents[i]);
                files.Add(file);
            }

            return CreateAsync(files.ToArray());
        }

        public static async Task<TempProject> CreateAsync(params TempProjectFile[] sourceFiles)
        {
            try
            {
                // create a new ad-hoc workspace
                var workspace = new AdhocWorkspace();

                // add a new project
                var projectId = ProjectId.CreateNewId(ProjectName);
                var version = VersionStamp.Create();
                var compilationOptions = new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    warningLevel: 1);
                ProjectInfo projectInfo = ProjectInfo.Create(
                        id: projectId,
                        filePath: Path.Combine(ProjectDir, $"{ProjectName}.csproj"),
                        version: version,
                        name: ProjectName,
                        assemblyName: ProjectName,
                        language: LanguageNames.CSharp,
                        compilationOptions: compilationOptions)
                    .WithSaltarelleReferences();

                Project project = workspace.AddProject(projectInfo);

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
                Compilation? compilation = await project.GetCompilationAsync();
                (compilation?.GetDiagnostics()).Should().BeEmpty();

                return new TempProject(workspace);
            }
            catch (ReflectionTypeLoadException e)
            {
                string subMessages = e.LoaderExceptions.Aggregate(
                    new StringBuilder().AppendLine().AppendLine("LoaderExceptions:"),
                    (builder, ex) => builder.AppendLine(ex?.Message),
                    builder => builder.ToString());
                throw new Exception($"{e.Message}{subMessages}");
            }
        }

        public async Task<DocumentTranslationContext> CreateContextForFileAsync(
            string fileName = "File.cs",
            CompilerOptions? options = null)
        {
            Project project = _workspace.CurrentSolution.Projects.Single();
            options ??= Options;
            Document document = project.Documents.Single(doc => doc.Name == fileName);

            IExtendedResult<DocumentTranslationContext?> result =
                await DocumentTranslationContext.TryCreateAsync(document, options);
            result.Diagnostics.Should().BeEmpty();
            result.Result.Should().NotBeNull();

            return result.Result!;
        }

        public async Task<ImmutableArray<DocumentTranslationContextWithSymbolTables>>
            CreateContextsWithSymbolTablesAsync(
                CompilerOptions? options = null,
                SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.DocumentAndAllAssemblyTypes)
        {
            options ??= Options;

            // add all of the symbols from all of the documents in the project
            var contexts = (await Task.WhenAll(
                _workspace.CurrentSolution.Projects.Single()
                    .Documents.Select(doc => CreateContextForFileAsync(doc.Name, options)))).ToImmutableArray();

            // create the script symbol table
            var scriptNamer = new ScriptNamer(
                SymbolDiscoverer.GetMscorlibAssemblySymbol(contexts.First().SemanticModel.Compilation),
                options.RenameRules);

            var scriptSymbolTable = ScriptSymbolTable.Create(contexts, scriptNamer, discoveryKind);

            // create the alternate signature symbol table
            IExtendedResult<AlternateSignatureSymbolTable> result = AlternateSignatureSymbolTable.Create(contexts);
            result.Diagnostics.Should().BeEmpty();
            AlternateSignatureSymbolTable alternateSignatureTable = result.Result;

            return contexts.Select(
                    context => new DocumentTranslationContextWithSymbolTables(
                        context,
                        scriptSymbolTable,
                        alternateSignatureTable))
                .ToImmutableArray();
        }

        public async Task<DocumentTranslationContextWithSymbolTables> CreateContextWithSymbolTablesForFileAsync(
            string fileName = "File.cs",
            CompilerOptions? options = null,
            SymbolDiscoveryKind discoveryKind = SymbolDiscoveryKind.DocumentAndReferencedTypes)
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
