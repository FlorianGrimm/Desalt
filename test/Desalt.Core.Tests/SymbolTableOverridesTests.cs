// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="SymbolTableOverridesTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Desalt.Core.Utility;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SymbolTableOverridesTests
    {
        private const string JsonContents = @"{
  ""overrides"": {
    ""System.Script"": {
      ""scriptName"": ""ss""
    },
    ""System.String.ToString()"": {
      ""inlineCode"": ""console.log({this})""
    }
  }
}";

        private static readonly SymbolTableOverrides OverridesObject = new SymbolTableOverrides(
            new KeyValuePair<string, SymbolTableOverride>("System.Script", new SymbolTableOverride(scriptName: "ss")),
            new KeyValuePair<string, SymbolTableOverride>(
                "System.String.ToString()",
                new SymbolTableOverride(inlineCode: "console.log({this})")));

        [TestMethod]
        public void SymbolTableOverrides_should_serialize_to_a_json_stream()
        {
            using (var stream = new MemoryStream())
            {
                OverridesObject.Serialize(stream);
                stream.Flush();
                stream.Position = 0;

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    reader.ReadToEnd().Should().Be(JsonContents);
                }
            }
        }

        [TestMethod]
        public void SymbolTableOverrides_should_deserialize_from_a_json_stream()
        {
            using (var stream = new UnicodeStringStream(JsonContents))
            {
                SymbolTableOverrides deserialized = SymbolTableOverrides.Deserialize(stream);
                deserialized.Should().BeEquivalentTo(OverridesObject);
            }
        }
    }
}
