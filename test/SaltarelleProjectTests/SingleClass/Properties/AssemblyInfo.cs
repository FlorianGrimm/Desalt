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

[assembly: AssemblyTitle("SingleClass")]
[assembly: AssemblyDescription("Saltarelle project for a simple class.")]
[assembly: AssemblyProduct("SingleClass")]
[assembly: ScriptAssembly("SingleClass")]
[assembly: ScriptNamespace("myNs")]

// for perf reasons
#if !DEBUG
[assembly: ScriptSharpCompatibility(OmitDowncasts = true, OmitNullableChecks = true)]
#endif
