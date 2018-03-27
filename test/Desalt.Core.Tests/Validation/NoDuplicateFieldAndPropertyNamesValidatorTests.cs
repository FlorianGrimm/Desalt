// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NoDuplicateFieldAndPropertyNamesValidatorTests.cs" company="Justin Rockwood">
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
    public class NoDuplicateFieldAndPropertyNamesValidatorTests
    {
        [TestMethod]
        public async Task Having_a_duplicate_field_and_property_name_in_a_class_should_log_a_diagnostic()
        {
            const string code = @"
public class C
{
    private string name;

    public string Name
    {
        get { return this.name; }
        set { this.name = value; }
    }
}
";

            using (var tempProject = TempProject.Create("TestProject", new TempProjectFile("File.cs", code)))
            {
                DocumentTranslationContextWithSymbolTables context =
                    await tempProject.CreateContextWithSymbolTablesForFileAsync("File.cs");

                var validator = new NoDuplicateFieldAndPropertyNamesValidator();
                IExtendedResult<bool> result = validator.Validate(context);

                VariableDeclaratorSyntax fieldDeclaration = context.RootSyntax.DescendantNodes()
                    .OfType<FieldDeclarationSyntax>()
                    .Single()
                    .Declaration.Variables.First();

                result.Diagnostics.Select(d => d.ToString())
                    .Should()
                    .HaveCount(1)
                    .And.BeEquivalentTo(
                        DiagnosticFactory.ClassWithDuplicateFieldAndPropertyName(
                                "C",
                                "name",
                                fieldDeclaration.GetLocation())
                            .ToString());
            }
        }
    }
}
