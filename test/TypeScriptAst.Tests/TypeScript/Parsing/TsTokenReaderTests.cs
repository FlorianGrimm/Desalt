// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsTokenReaderTests.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Tests.TypeScript.Parsing
{
    using System.Linq;
    using CompilerUtilities;
    using FluentAssertions;
    using TypeScriptAst.TypeScript.Parsing;
    using Xunit;

    public class TsTokenReaderTests
    {
        private static readonly TsToken s_token1 = new TsToken(TsTokenCode.Plus, "+", new TextReaderLocation(1, 1));
        private static readonly TsToken s_token2 = new TsToken(TsTokenCode.As, "as", new TextReaderLocation(1, 2));
        private static readonly TsToken s_token3 = new TsToken(TsTokenCode.Caret, "^", new TextReaderLocation(1, 4));
        private static readonly TsToken[] s_tokens = { s_token1, s_token2, s_token3 };

        [Fact]
        public void TsTokenReader_Peek_should_return_EndOfTokens_if_there_are_no_tokens_left()
        {
            var reader = new TsTokenReader(Enumerable.Empty<TsToken>());
            reader.Peek().Should().Be(TsToken.EndOfTokens);
        }

        [Fact]
        public void TsTokenReader_Peek_should_return_the_next_token_without_advancing()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.Peek().Should().Be(s_token1);
            reader.Peek().Should().Be(s_token1);
        }

        [Fact]
        public void TsTokenReader_Read_should_return_EndOfTokens_if_there_are_no_tokens_left()
        {
            var reader = new TsTokenReader(Enumerable.Empty<TsToken>());
            reader.Read().Should().Be(TsToken.EndOfTokens);
        }

        [Fact]
        public void TsTokenReader_Read_should_return_the_next_token_and_advance()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.Read().Should().Be(s_token1);
            reader.Read().Should().Be(s_token2);
            reader.Read().Should().Be(s_token3);
            reader.Read().Should().Be(TsToken.EndOfTokens);
        }

        [Fact]
        public void TsTokenReader_IsAtEnd_should_return_true_on_an_empty_reader()
        {
            var reader = new TsTokenReader(Enumerable.Empty<TsToken>());
            reader.IsAtEnd.Should().BeTrue();
        }

        [Fact]
        public void TsTokenReader_IsAtEnd_should_return_false_on_a_non_empty_reader()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.IsAtEnd.Should().BeFalse();
        }

        [Fact]
        public void TsTokenReader_ReadIf_should_return_false_and_not_advance_if_the_next_token_does_not_match()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.ReadIf(TsTokenCode.Abstract, out TsToken token).Should().BeFalse();
            token.Should().BeNull();
            reader.Peek().Should().Be(s_token1);
        }

        [Fact]
        public void TsTokenReader_ReadIf_should_return_true_and_advance_if_the_next_token_matches()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.ReadIf(s_token1.TokenCode, out TsToken token).Should().BeTrue();
            token.Should().Be(s_token1);
            reader.Read().Should().Be(s_token2);
        }

        [Fact]
        public void TsTokenReader_ReadIf_should_return_false_if_at_the_end_of_the_tokens()
        {
            var reader = new TsTokenReader(Enumerable.Empty<TsToken>());
            reader.ReadIf(TsTokenCode.Abstract, out _).Should().BeFalse();
        }

        [Fact]
        public void TsTokenReader_IsNext_should_return_false_if_the_token_codes_to_not_match()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.IsNext(s_token1.TokenCode, s_token2.TokenCode, TsTokenCode.Any).Should().BeFalse();
        }

        [Fact]
        public void TsTokenReader_IsNext_should_return_true_if_all_the_token_codes_match()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.IsNext(s_token1.TokenCode, s_token2.TokenCode, s_token3.TokenCode).Should().BeTrue();
        }

        [Fact]
        public void TsTokenReader_IsNext_should_return_false_if_there_are_not_enough_tokens_left()
        {
            var reader = new TsTokenReader(new[] { s_token1 });
            reader.IsNext(s_token1.TokenCode, TsTokenCode.Async, TsTokenCode.Any).Should().BeFalse();
        }

        [Fact]
        public void TsTokenReader_IsNext_should_return_true_if_there_are_more_tokens_left_but_there_is_only_one_token_to_find()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.IsNext(s_token1.TokenCode).Should().BeTrue();
        }

        [Fact]
        public void TsTokenReader_Skip_should_skip_the_next_token()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.Skip();
            reader.Read().Should().Be(s_token2);
        }

        [Fact]
        public void TsTokenReader_SkipIf_should_return_false_and_not_advance_if_the_next_token_does_not_match()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.SkipIf(TsTokenCode.Abstract).Should().BeFalse();
            reader.Peek().Should().Be(s_token1);
        }

        [Fact]
        public void TsTokenReader_SkipIf_should_return_true_and_advance_if_the_next_token_matches()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.SkipIf(s_token1.TokenCode).Should().BeTrue();
            reader.Read().Should().Be(s_token2);
        }

        [Fact]
        public void TsTokenReader_SkipIf_should_return_false_if_at_the_end_of_the_tokens()
        {
            var reader = new TsTokenReader(Enumerable.Empty<TsToken>());
            reader.SkipIf(TsTokenCode.Abstract).Should().BeFalse();
        }

        [Fact]
        public void
            TsTokenReader_ReadWithSavedState_should_preserve_the_state_before_running_the_function_and_restore_it()
        {
            var reader = new TsTokenReader(s_tokens);
            reader.Skip();
            reader.ReadWithSavedState(
                () =>
                {
                    reader.Skip(2);
                    reader.IsAtEnd.Should().BeTrue();
                });
            reader.Read().Should().Be(s_token2);
        }
    }
}
