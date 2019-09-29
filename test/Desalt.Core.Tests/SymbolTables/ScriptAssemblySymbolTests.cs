// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptAssemblySymbolTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.SymbolTables
{
    using System.Threading.Tasks;
    using Desalt.Core.SymbolTables;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScriptAssemblySymbolTests
    {
        [TestMethod]
        public async Task ScriptAssemblySymbol_should_use_the_right_defaults_when_there_are_no_attributes()
        {
            const string code = @"class C { public void Method() {} }";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync();
                var assemblySymbol = context.SemanticModel.Compilation.Assembly;

                var assemblyScriptSymbol = new ScriptAssemblySymbol(assemblySymbol, "AssemblyName");
                ScriptSymbolTests.AssertScriptSymbolDefaultValues(assemblySymbol, assemblyScriptSymbol);

                assemblyScriptSymbol.AssemblySymbol.Should().Be(assemblySymbol);
                assemblyScriptSymbol.DefaultReflectability.Should().Be(MemberReflectability.All);
                assemblyScriptSymbol.IncludeGenericArgumentsMethodDefault.Should()
                    .Be(GenericArgumentsBehaviorType.Ignore);
                assemblyScriptSymbol.IncludeGenericArgumentsTypeDefault.Should()
                    .Be(GenericArgumentsBehaviorType.Ignore);
                assemblyScriptSymbol.MinimizePublicNames.Should().BeFalse();
                assemblyScriptSymbol.ModuleName.Should().BeNull();
                assemblyScriptSymbol.OmitDowncasts.Should().BeFalse();
                assemblyScriptSymbol.OmitNullableChecks.Should().BeFalse();
                assemblyScriptSymbol.ScriptAssemblyName.Should().BeNull();
                assemblyScriptSymbol.ScriptNamespace.Should().BeNull();
                assemblyScriptSymbol.ScriptQualifier.Should().BeNull();
            }
        }
    }
}
