// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoPartialClassesValidatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Validation
{
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using CompilerUtilities.Extensions;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Validation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoPartialClassesValidatorTests
    {
        [TestMethod]
        public async Task Having_a_partial_class_should_log_a_diagnostic()
        {
            const string code = "public partial class C {}";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                var context = await tempProject.CreateContextWithSymbolTablesForFileAsync(
                    "File.cs",
                    discoveryKind: SymbolTableDiscoveryKind.OnlyDocumentTypes);

                var classDeclarationSyntax =
                    context.RootSyntax.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

                var validator = new NoPartialClassesValidator();
                IExtendedResult<bool> result = validator.Validate(context.ToSingleEnumerable().ToImmutableArray());
                result.Success.Should().BeFalse();

                result.Diagnostics.Select(d => d.ToString())
                    .Should()
                    .HaveCount(1)
                    .And.BeEquivalentTo(
                        DiagnosticFactory.PartialClassesNotSupported(classDeclarationSyntax).ToString());
            }
        }
    }
}
