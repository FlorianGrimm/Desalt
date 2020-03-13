// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticsTestFactories.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Diagnostics
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;

    internal static class DiagnosticsTestFactories
    {
        public static Diagnostic CreateDiagnostic(
            string id = "id",
            string category = "category",
            string message = "message",
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Error,
            DiagnosticSeverity? severity = null,
            bool isEnabledByDefault = true,
            int warningLevel = 0,
            IEnumerable<string>? customTags = null)
        {
            return Diagnostic.Create(
                id,
                category,
                message,
                severity.GetValueOrDefault(defaultSeverity),
                defaultSeverity,
                isEnabledByDefault,
                warningLevel,
                customTags: customTags);
        }

        public static Diagnostic CreateWarning(
            string id = "id",
            string category = "category",
            string message = "message",
            bool isEnabledByDefault = true,
            int warningLevel = 1,
            IEnumerable<string>? customTags = null)
        {
            return CreateDiagnostic(
                id,
                category,
                message,
                defaultSeverity: DiagnosticSeverity.Warning,
                severity: DiagnosticSeverity.Warning,
                isEnabledByDefault: isEnabledByDefault,
                warningLevel: warningLevel,
                customTags: customTags);
        }
    }
}
