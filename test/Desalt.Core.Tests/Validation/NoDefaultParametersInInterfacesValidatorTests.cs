// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoDefaultParametersInInterfacesValidatorTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Validation
{
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Diagnostics;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using Desalt.Core.Validation;
    using FluentAssertions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NoDefaultParametersInInterfacesValidatorTests
    {
        [TestMethod]
        public async Task An_error_should_be_logged_for_a_default_parameter_in_an_interface()
        {
            const string code = @"
public interface TestInterface
{
    void InvalidMethod(int defaultValue = 0);
}";

            using (var tempProject = await TempProject.CreateAsync(
                "TestProject",
                new TempProjectFile("MyInterface.cs", code)))
            {
                DocumentTranslationContextWithSymbolTables context =
                    await tempProject.CreateContextWithSymbolTablesForFileAsync("MyInterface.cs");
                var validator = new NoDefaultParametersInInterfacesValidator();
                IExtendedResult<bool> result = validator.Validate(context);

                ParameterSyntax parameterSyntax =
                    context.RootSyntax.DescendantNodes().OfType<ParameterSyntax>().Single();

                result.Diagnostics.Select(d => d.ToString())
                    .Should()
                    .HaveCount(1)
                    .And.BeEquivalentTo(
                        DiagnosticFactory.InterfaceWithDefaultParam("TestInterface", "InvalidMethod", parameterSyntax)
                            .ToString());
            }
        }
    }
}
