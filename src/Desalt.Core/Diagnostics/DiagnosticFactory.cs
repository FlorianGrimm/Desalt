// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagnosticFactory.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Factory to create <see cref="Diagnostic"/> messages, based on the compiler options and source suppressions.
    /// </summary>
    internal static partial class DiagnosticFactory
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private const string IdPrefix = "DSC";
        private const string TranslationCategory = "Desalt.Translation";

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns the string identifier from the specified <see cref="DiagnosticId"/>
        /// </summary>
        public static string IdFromDiagnosticId(DiagnosticId diagnosticId)
        {
            DiagnosticInfoAttribute info = InfoFromDiagnosticKind(diagnosticId);
            return info.Id;
        }

        private static DiagnosticInfoAttribute InfoFromDiagnosticKind(DiagnosticId diagnosticId)
        {
            MemberInfo memberInfo = typeof(DiagnosticId).GetMember(diagnosticId.ToString()).First();
            var info = memberInfo.GetCustomAttribute<DiagnosticInfoAttribute>();
            return info;
        }

        /// <summary>
        /// Creates a new <see cref="Diagnostic"/>, taking the compiler options into account on how
        /// to report specific diagnostic messages. A null return value indicates that the diagnostic
        /// should not be reported.
        /// </summary>
        /// <param name="diagnosticId">The diagnostic to create.</param>
        /// <param name="location">The source code location of the diagnostic, if there is one.</param>
        /// <param name="messageArgs">The arguments to pass to <c>string.Format</c>.</param>
        /// <returns>
        /// A <see cref="Diagnostic"/> to report, or null if the diagnostic should not be reported.
        /// </returns>
        private static Diagnostic Create(DiagnosticId diagnosticId, Location location, params object[] messageArgs)
        {
            DiagnosticInfoAttribute info = InfoFromDiagnosticKind(diagnosticId);

            return Diagnostic.Create(
                id: info.Id,
                category: info.Category,
                message: string.Format(CultureInfo.InvariantCulture, info.MessageFormat, messageArgs),
                severity: info.DefaultSeverity,
                defaultSeverity: info.DefaultSeverity,
                isEnabledByDefault: info.IsEnabledByDefault,
                warningLevel: info.WarningLevel,
                title: info.Title,
                description: info.Description,
                helpLink: null,
                location: location,
                additionalLocations: null,
                customTags: info.CustomTags);
        }

        //// ===========================================================================================================
        //// Classes
        //// ===========================================================================================================

        private abstract class DiagnosticInfoAttribute : Attribute
        {
            protected DiagnosticInfoAttribute(
                int id,
                string title,
                string messageFormat,
                DiagnosticSeverity defaultSeverity,
                string category,
                string[] customTags,
                bool isEnabledByDefault = true,
                int warningLevel = 0,
                string description = null)
            {
                Id = $"{IdPrefix}{id.ToString("0000", CultureInfo.InvariantCulture)}";
                Title = title;
                MessageFormat = messageFormat;
                DefaultSeverity = defaultSeverity;
                Category = category;
                CustomTags = customTags;
                IsEnabledByDefault = isEnabledByDefault;
                WarningLevel = warningLevel;
                Description = description;
            }

            public string Id { get; }
            public string Category { get; }
            public string[] CustomTags { get; }
            public DiagnosticSeverity DefaultSeverity { get; }
            public string Description { get; }
            public bool IsEnabledByDefault { get; }
            public string MessageFormat { get; }
            public string Title { get; }
            public int WarningLevel { get; }
        }

        // ReSharper disable once UnusedMember.Local
        private sealed class ErrorAttribute : DiagnosticInfoAttribute
        {
            public ErrorAttribute(
                int id,
                string title,
                string messageFormat,
                string category = TranslationCategory,
                string[] customTags = null,
                string description = null) : base(
                id: id,
                title: title,
                messageFormat: messageFormat,
                defaultSeverity: DiagnosticSeverity.Error,
                category: category,
                customTags: customTags ?? new[] { WellKnownDiagnosticTags.Compiler },
                description: description)
            {
            }
        }

        // ReSharper disable once UnusedMember.Local
        private sealed class WarningAttribute : DiagnosticInfoAttribute
        {
            public WarningAttribute(
                int id,
                string title,
                string messageFormat = null,
                WarningLevel warningLevel = Core.WarningLevel.Severe,
                string category = TranslationCategory,
                string[] customTags = null, string description = null) : base(
                id: id,
                title: title,
                messageFormat: messageFormat ?? title,
                defaultSeverity: DiagnosticSeverity.Warning,
                category: category,
                customTags: customTags ?? new[] { WellKnownDiagnosticTags.Compiler },
                warningLevel: (int)warningLevel,
                description: description)
            {
            }
        }
    }
}
