// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynExtensionsForTesting.cs" company="Justin Rockwood">
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
    using System.Reflection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Contains extension methods used for testing code.
    /// </summary>
    internal static class RoslynExtensionsForTesting
    {
        private static readonly string[] s_standardAssemblies =
        {
            "jQuery.dll",
            "mscorlib.dll",
            "NativeJsTypeDefs.dll",
            "TypeDefs.dll",
            "Underscore.dll",
            "Web.dll",
        };

        /// <summary>
        /// Adds all of the Saltarelle assemblies to the compilation.
        /// </summary>
        public static CSharpCompilation AddSaltarelleReferences(this CSharpCompilation compilation)
        {
            return compilation.AddReferences(GetSaltarelleMetadataReferences());
        }

        /// <summary>
        /// Replaces all of the existing metadata references in the project with the Saltarelle assemblies.
        /// </summary>
        public static ProjectInfo WithSaltarelleReferences(this ProjectInfo projectInfo)
        {
            return projectInfo.WithMetadataReferences(GetSaltarelleMetadataReferences());
        }

        /// <summary>
        /// Gets all of the Saltarelle assembly references from the embedded resources in this test assembly.
        /// </summary>
        private static IEnumerable<MetadataReference> GetSaltarelleMetadataReferences()
        {
            IEnumerable<PortableExecutableReference> references = s_standardAssemblies.Select(
                assemblyName => MetadataReference.CreateFromStream(GetSaltarelleAssemblyStream(assemblyName)));
            return references;
        }

        /// <summary>
        /// Gets an assembly stream from the embedded resource in this test assembly.
        /// </summary>
        /// <param name="assemblyName">The assembly name to get, for example "mscorlib.dll".</param>
        /// <returns>The loaded assembly stream.</returns>
        private static Stream GetSaltarelleAssemblyStream(string assemblyName)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            return thisAssembly.GetManifestResourceStream(typeof(RoslynExtensionsForTesting), assemblyName) ??
                throw new InvalidOperationException(
                    "Could not get the Saltarelle assembly stream from this assembly's resources.");
        }
    }
}
