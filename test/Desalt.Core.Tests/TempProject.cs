// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TempProject.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Desalt.Core.Translation;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.MSBuild;

    /// <summary>
    /// Represents a temporary C# project that can be compiled using the Roslyn object model.
    /// </summary>
    internal sealed class TempProject : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private Lazy<MSBuildWorkspace> _workspace;
        private Lazy<Task<Project>> _project;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TempProject(string projectDirectory, string projectFilePath)
        {
            ProjectDirectory = projectDirectory;
            ProjectFilePath = projectFilePath;

            _workspace = new Lazy<MSBuildWorkspace>(MSBuildWorkspace.Create, isThreadSafe: true);
            _project = new Lazy<Task<Project>>(
                () => _workspace.Value.OpenProjectAsync(projectFilePath),
                isThreadSafe: true);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public string ProjectDirectory { get; }
        public string ProjectFilePath { get; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TempProject Create(string projectName, params TempProjectFile[] sourceFiles)
        {
            // create the project directory
            string projectDirectory = Path.Combine(Path.GetTempPath(), projectName);
            Directory.CreateDirectory(projectDirectory);

            string projectFilePath = Path.Combine(projectDirectory, projectName + ".csproj");

            // create the .csproj file
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine
            };

            using (var writer = XmlWriter.Create(projectFilePath, settings))
            {
                writer.WriteProcessingInstruction("xml", @"version=""1.0"" encoding=""utf-8""");

                // <Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                writer.WriteStartElement("Project", "http://schemas.microsoft.com/developer/msbuild/2003");
                writer.WriteAttributeString("ToolsVersion", "15.0");

                // <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"
                //   Condition ="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"/>
                writer.WriteStartElement("Import");
                writer.WriteAttributeString(
                    "Project",
                    @"$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props");
                writer.WriteAttributeString(
                    "Condition",
                    @"Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')");
                writer.WriteEndElement();

                WritePropertyGroup(writer, projectName);
                WriteReferences(writer);
                WriteCompileItems(writer, sourceFiles.Select(x => x.FileName + ".cs"));

                // <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
                writer.WriteStartElement("Import");
                writer.WriteAttributeString("Project", @"$(MSBuildToolsPath)\Microsoft.CSharp.targets");
                writer.WriteEndElement();
            }

            // create all of the project source files
            foreach (TempProjectFile sourceFile in sourceFiles)
            {
                string filePath = Path.Combine(projectDirectory, sourceFile.FileName + ".cs");
                File.WriteAllText(filePath, sourceFile.FileContents);
            }

            return new TempProject(projectDirectory, projectFilePath);
        }

        public async Task<DocumentTranslationContext> CreateContextForFileAsync(
            string fileName,
            CompilerOptions options = null)
        {
            options = options ?? new CompilerOptions("outputPath");

            Project project = await _project.Value;
            Document document =
                project.Documents.Single(doc => doc.FilePath == Path.Combine(ProjectDirectory, fileName + ".cs"));

            IExtendedResult<DocumentTranslationContext> result =
                await DocumentTranslationContext.TryCreateAsync(document, options);

            return result.Result;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Lazy<MSBuildWorkspace> ws = Interlocked.Exchange(ref _workspace, null);
            if (ws?.IsValueCreated == true)
            {
                ws.Value.Dispose();
            }

            Interlocked.Exchange(ref _project, null);

            try
            {
                Directory.Delete(ProjectDirectory, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        private static void WritePropertyGroup(XmlWriter writer, string projectName)
        {
            // <PropertyGroup>
            writer.WriteStartElement("PropertyGroup");

            //   <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
            writer.WriteStartElement("Configuration");
            writer.WriteAttributeString("Condition", @" '$(Configuration)' == '' ");
            writer.WriteValue("Debug");
            writer.WriteEndElement();

            //   <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
            writer.WriteStartElement("Platform");
            writer.WriteAttributeString("Condition", @" '$(Platform)' == '' ");
            writer.WriteValue("AnyCPU");
            writer.WriteEndElement();

            //   <ProjectGuid>{987796C8-EE76-4FFD-B2EB-1B3A0767920B}</ProjectGuid>
            writer.WriteElementString(
                "ProjectGuid",
                Guid.NewGuid().ToString("B", CultureInfo.InvariantCulture).ToUpperInvariant());

            //   <OutputType>Library</OutputType>
            writer.WriteElementString("OutputType", "Library");

            //   <RootNamespace>Desalt.Core</RootNamespace>
            writer.WriteElementString("RootNamespace", projectName);

            //   <AssemblyName>Desalt.Core</AssemblyName>
            writer.WriteElementString("AssemblyName", projectName);

            //   <TargetFrameworkVersion>v4.6</TargetFrameworkVersion>
            writer.WriteElementString("TargetFrameworkVersion", "v4.6");

            // </PropertyGroup>
            writer.WriteEndElement();
        }

        private static void WriteReferences(XmlWriter writer)
        {
            // <ItemGroup>
            writer.WriteStartElement("ItemGroup");

            WriteReference(writer, "System");
            WriteReference(writer, "System.Core");
            WriteReference(writer, "Microsoft.CSharp");

            // </ItemGroup>
            writer.WriteEndElement();
        }

        private static void WriteReference(XmlWriter writer, string assemblyName)
        {
            // <Reference Include="System" />
            writer.WriteStartElement("Reference");
            writer.WriteAttributeString("Include", assemblyName);
            writer.WriteEndElement();
        }

        private static void WriteCompileItems(XmlWriter writer, IEnumerable<string> projectFileNames)
        {
            // <ItemGroup>
            writer.WriteStartElement("ItemGroup");

            foreach (string projectFileName in projectFileNames)
            {
                WriteCompileItem(writer, projectFileName);
            }

            // </ItemGroup>
            writer.WriteEndElement();
        }

        private static void WriteCompileItem(XmlWriter writer, string fileName)
        {
            // <Compile Include="fileName" />
            writer.WriteStartElement("Compile");
            writer.WriteAttributeString("Include", fileName);
            writer.WriteEndElement();
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
