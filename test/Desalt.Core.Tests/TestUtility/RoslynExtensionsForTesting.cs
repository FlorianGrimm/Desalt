// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RoslynExtensionsForTesting.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.TestUtility
{
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
        /// <summary>
        /// Adds all of the Saltarelle assemblies to the compilation.
        /// </summary>
        public static CSharpCompilation AddSaltarelleReferences(this CSharpCompilation compilation)
        {
            string[] standardAssemblies = new[]
            {
                "mscorlib.dll",
                "NativeJsTypeDefs.dll",
                "Saltarelle.jQuery.dll",
                "Saltarelle.Web.dll",
                "TypeDefs.dll",
                "Underscore.dll"
            };

            IEnumerable<PortableExecutableReference> references = standardAssemblies.Select(
                assemblyName => MetadataReference.CreateFromStream(GetSaltarelleAssemblyStream(assemblyName)));

            return compilation.AddReferences(references);
        }

        /// <summary>
        /// Gets an assembly stream from the embedded resource in this test assembly.
        /// </summary>
        /// <param name="assemblyName">The assembly name to get, for example "mscorlib.dll".</param>
        /// <returns>The loaded assembly stream.</returns>
        public static Stream GetSaltarelleAssemblyStream(string assemblyName)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            return thisAssembly.GetManifestResourceStream(typeof(RoslynExtensionsForTesting), assemblyName);
        }
    }
}
