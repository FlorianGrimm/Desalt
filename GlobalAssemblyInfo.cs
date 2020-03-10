// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalAssemblyInfo.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

#if NETFRAMEWORK
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("")]
#endif

// Product Information
[assembly: AssemblyCompany("Justin Rockwood")]
[assembly: AssemblyCopyright("Copyright © Justin Rockwood. All rights reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyProduct("Desalt Toolset")]

// Version Information
[assembly: AssemblyVersion("0.1.0")]
[assembly: AssemblyFileVersion("0.1.0")]
[assembly: AssemblyInformationalVersion("0.1 Pre-Release")]
#endif

// Globalization and Localization
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]

// COM and CLS Compliance
[assembly: CLSCompliant(false)]
[assembly: ComVisible(false)]
