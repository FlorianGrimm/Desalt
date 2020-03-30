// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Tableau.JavaScript.Vql.Core")]
[assembly: AssemblyDescription("Saltarelle project for the Web and Mobile JavaScript.")]
[assembly: AssemblyProduct("Tableau.JavaScript.Vql")]
[assembly: ScriptAssembly("vqlcore")]
[assembly: ScriptNamespace("tab")]

// for perf reasons
#if !DEBUG
[assembly: ScriptSharpCompatibility(OmitDowncasts = true, OmitNullableChecks = true)]
#endif
