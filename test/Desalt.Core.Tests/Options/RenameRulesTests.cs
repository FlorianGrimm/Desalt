// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="RenameRulesTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.Tests.Options
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Desalt.Core.Options;
    using FluentAssertions;
    using NUnit.Framework;

    public class RenameRulesTests
    {
        [Test]
        public void RenameRules_should_populate_the_OperatorOverloadMethodNames_with_missing_defaults()
        {
            var operatorOverloadMethodNames = new Dictionary<UserDefinedOperatorKind, string>
            {
                [UserDefinedOperatorKind.Addition] = "WackyAddition"
            }.ToImmutableDictionary();

            var rules = new RenameRules(userDefinedOperatorMethodNames: operatorOverloadMethodNames);

            var builder = RenameRules.Default.UserDefinedOperatorMethodNames.ToBuilder();
            builder[UserDefinedOperatorKind.Addition] = "WackyAddition";
            var expected = builder.ToImmutable();

            rules.UserDefinedOperatorMethodNames.Should().BeEquivalentTo(expected);
        }
    }
}
