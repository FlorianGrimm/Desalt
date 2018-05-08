// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="InlineCodeSymbolTableTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Translation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Desalt.Core.Extensions;
    using Desalt.Core.Tests.TestUtility;
    using Desalt.Core.Translation;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InlineCodeSymbolTableTests
    {
        private async Task AssertInlineCodeEntries(
            string classContents,
            SymbolTableDiscoveryKind discoveryKind,
            params KeyValuePair<string, string>[] expectedEntries)
        {
            string code = $@"
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

class C
{{
    {classContents}
}}
";

            using (var tempProject = await TempProject.CreateAsync(code))
            {
                DocumentTranslationContext context = await tempProject.CreateContextForFileAsync("File.cs");
                var contexts = context.ToSingleEnumerable().ToImmutableArray();

                var directlyReferencedExternalTypeSymbols =
                    SymbolTableUtils.DiscoverDirectlyReferencedExternalTypes(contexts, discoveryKind);

                var symbolTable = InlineCodeSymbolTable.Create(
                    contexts,
                    directlyReferencedExternalTypeSymbols,
                    indirectlyReferencedExternalTypeSymbols: SymbolTableUtils.DiscoverTypesInReferencedAssemblies(
                        directlyReferencedExternalTypeSymbols,
                        context.SemanticModel.Compilation));

                switch (discoveryKind)
                {
                    case SymbolTableDiscoveryKind.OnlyDocumentTypes:
                        symbolTable.DocumentSymbols
                            .Select(
                                pair => new KeyValuePair<string, string>(
                                    SymbolTableUtils.KeyFromSymbol(pair.Key),
                                    pair.Value))
                            .Should()
                            .BeEquivalentTo(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndReferencedTypes:
                        symbolTable.DirectlyReferencedExternalSymbols.Select(
                                pair => new KeyValuePair<string, string>(
                                    SymbolTableUtils.KeyFromSymbol(pair.Key),
                                    pair.Value))
                            .Should()
                            .Contain(expectedEntries);
                        break;

                    case SymbolTableDiscoveryKind.DocumentAndAllAssemblyTypes:
                        var expectedKeys = expectedEntries.Select(pair => pair.Key).ToImmutableArray();
                        symbolTable.IndirectlyReferencedExternalSymbols
                            .Where(pair => SymbolTableUtils.KeyFromSymbol(pair.Key).IsOneOf(expectedKeys))
                            .Select(
                                pair => new KeyValuePair<string, string>(
                                    SymbolTableUtils.KeyFromSymbol(pair.Key),
                                    pair.Value.Value))
                            .Should()
                            .BeEquivalentTo(expectedEntries);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(discoveryKind), discoveryKind, null);
                }
            }
        }

        [TestMethod]
        public async Task InlineCodeSymbolTable_should_store_inline_code_attributes_for_members_in_the_document()
        {
            const string code = @"
[InlineCode(""[]"")]
C() {}

[InlineCode(""$.doThis()"")]
void Method() {}

int Property
{
    [InlineCode(""getter"")]
    get { return 10; }

    [InlineCode(""setter"")]
    set { }
}
";

            await AssertInlineCodeEntries(
                code,
                SymbolTableDiscoveryKind.OnlyDocumentTypes,
                new KeyValuePair<string, string>("C.C()", "[]"),
                new KeyValuePair<string, string>("C.Method()", "$.doThis()"),
                new KeyValuePair<string, string>("C.Property.get", "getter"),
                new KeyValuePair<string, string>("C.Property.set", "setter"));
        }

        [TestMethod]
        public async Task InlineCodeSymbolTable_should_store_inline_code_attributes_for_external_references()
        {
            const string prefix = "System.Collections.Generic.List<C>";

            // this isn't all of them, just a sampling
            await AssertInlineCodeEntries(
                "List<C> list;",
                SymbolTableDiscoveryKind.DocumentAndReferencedTypes,
                new KeyValuePair<string, string>($"{prefix}.List()", "[]"),
                new KeyValuePair<string, string>($"{prefix}.List(C first, params C[] rest)", "[ {first}, {*rest} ]"),
                new KeyValuePair<string, string>($"{prefix}.Clone()", "{$System.Script}.arrayClone({this})"));
        }
    }
}
