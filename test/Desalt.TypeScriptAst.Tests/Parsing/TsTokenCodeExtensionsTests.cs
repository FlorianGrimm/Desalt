// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTokenCodeExtensionsTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Tests.Parsing
{
    using Desalt.TypeScriptAst.Parsing;
    using FluentAssertions;
    using NUnit.Framework;

    public class TsTokenCodeExtensionsTests
    {
        [Test]
        public void IsKeywordAllowedAsIdentifier_should_always_return_false_for_the_reserved_keywords()
        {
            foreach (TsTokenCode keyword in TsTokenCodeExtensions.s_reservedKeywords)
            {
                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: false)
                    .Should()
                    .BeFalse($"because '{keyword}' is a reserved keyword");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: false)
                    .Should()
                    .BeFalse($"because '{keyword}' is a reserved keyword");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: true)
                    .Should()
                    .BeFalse($"because '{keyword}' is a reserved keyword");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: true)
                    .Should()
                    .BeFalse($"because '{keyword}' is a reserved keyword");
            }
        }

        [Test]
        public void IsKeywordAllowedAsIdentifier_should_allow_some_keywords_in_non_strict_mode()
        {
            foreach (TsTokenCode keyword in TsTokenCodeExtensions.s_reservedInStrictModeKeywords)
            {
                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: false)
                    .Should()
                    .BeTrue($"because '{keyword}' is allowed in non-strict mode");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: false)
                    .Should()
                    .BeTrue($"because '{keyword}' is allowed in non-strict mode");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: true)
                    .Should()
                    .BeFalse($"because '{keyword}' is not allowed in strict mode");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: true)
                    .Should()
                    .BeFalse($"because '{keyword}' is not allowed in strict mode");
            }
        }

        [Test]
        public void IsKeywordAllowedAsIdentifier_should_allow_some_keywords_except_when_used_as_a_user_defined_type()
        {
            foreach (TsTokenCode keyword in TsTokenCodeExtensions.s_restrictedInUserDefinedTypeNamesKeywords)
            {
                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: false)
                    .Should()
                    .BeTrue($"because '{keyword}' is allowed as an identifier in non user-defined types");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: true)
                    .Should()
                    .BeTrue($"because '{keyword}' is allowed as an identifier in non user-defined types");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: false)
                    .Should()
                    .BeFalse($"because '{keyword}' is not allowed as an identifier in user-defined types");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: true)
                    .Should()
                    .BeFalse($"because '{keyword}' is not allowed as an identifier in user-defined types");
            }
        }

        [Test]
        public void
            IsKeywordAllowedAsIdentifier_should_always_be_true_for_some_reserved_words_that_are_always_valid_as_identifiers()
        {
            foreach (TsTokenCode keyword in TsTokenCodeExtensions.s_validIdentifierKeywords)
            {
                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: false)
                    .Should()
                    .BeTrue($"because '{keyword}' is always allowed as an identifier");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: false)
                    .Should()
                    .BeTrue($"because '{keyword}' is always allowed as an identifier");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: false, isStrictMode: true)
                    .Should()
                    .BeTrue($"because '{keyword}' is always allowed as an identifier");

                keyword.IsKeywordAllowedAsIdentifier(isTypeDeclaration: true, isStrictMode: true)
                    .Should()
                    .BeTrue($"because '{keyword}' is always allowed as an identifier");
            }
        }
    }
}
