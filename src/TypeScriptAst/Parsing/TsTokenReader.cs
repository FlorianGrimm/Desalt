// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsExpressionParser.TokenReader.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace TypeScriptAst.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class TsTokenReader
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly ImmutableArray<TsToken> _tokens;
        private int _index;
        private int _withinSavedStateBlockCount;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTokenReader(IEnumerable<TsToken> tokens)
        {
            _tokens = tokens?.ToImmutableArray() ?? ImmutableArray<TsToken>.Empty;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a value indicating if the reader is at the end and there are no tokens left.
        /// </summary>
        public bool IsAtEnd => _index >= _tokens.Length;

        /// <summary>
        /// Gets a value indicating whether the reader is currently within a saved state block.
        /// </summary>
        public bool WithinSavedStateBlock => _withinSavedStateBlockCount > 0;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value indicating whether the next tokens match the specified token codes.
        /// </summary>
        /// <param name="tokenCodes">The token codes to match.</param>
        /// <returns>true if the next tokens match the specified token codes in order; otherwise, false.</returns>
        public bool IsNext(params TsTokenCode[] tokenCodes)
        {
            // quick check to see if there are enough tokens left
            if (_index + tokenCodes.Length > _tokens.Length)
            {
                return false;
            }

            // check each token in turn
            for (int i = 0; i < tokenCodes.Length; i++)
            {
                if (_tokens[_index + i].TokenCode != tokenCodes[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Peeks ahead one token without advancing the reader.
        /// </summary>
        /// <returns>
        /// The next token or <see cref="TsToken.EndOfTokens"/> if there are no more tokens.
        /// </returns>
        public TsToken Peek() => _index < _tokens.Length ? _tokens[_index] : TsToken.EndOfTokens;

        /// <summary>
        /// Peeks ahead <paramref name="count"/> tokens and returns those tokens without advancing the reader.
        /// </summary>
        /// <param name="count">The number of tokens to peek ahead.</param>
        /// <returns>The next tokens.</returns>
        public ImmutableArray<TsToken> Peek(int count)
        {
            var tokens = _tokens.Skip(_index).Take(count).ToList();
            if (tokens.Count < count)
            {
                tokens.Add(TsToken.EndOfTokens);
            }

            return tokens.ToImmutableArray();
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        /// <returns>The next token or null if there are no more tokens.</returns>
        public TsToken Read()
        {
            TsToken token = Peek();
            _index = Math.Min(_index + 1, _tokens.Length);
            return token;
        }

        /// <summary>
        /// Reads the next <paramref name="count"/> tokens.
        /// </summary>
        /// <param name="count">The number of tokens to read.</param>
        /// <returns>The next tokens.</returns>
        public ImmutableArray<TsToken> Read(int count)
        {
            ImmutableArray<TsToken> tokens = Peek(count);
            _index = Math.Min(_index + count, _tokens.Length);
            return tokens;
        }

        /// <summary>
        /// Skips the next token.
        /// </summary>
        public void Skip() => Read();

        /// <summary>
        /// Skips the next <paramref name="count"/> tokens.
        /// </summary>
        public void Skip(int count) => Read(count);

        /// <summary>
        /// Reads the next token, but only if the token code matches.
        /// </summary>
        /// <param name="tokenCode">The token code to match.</param>
        /// <returns>true if the next token was read because it matched the token code; otherwise, false.</returns>
        public bool ReadIf(TsTokenCode tokenCode) => ReadIf(tokenCode, out _);

        /// <summary>
        /// Reads the next token, but only if the token code matches.
        /// </summary>
        /// <param name="tokenCode">The token code to match.</param>
        /// <param name="token">The read token, or null if the next token does not match the token code.</param>
        /// <returns>true if the next token was read because it matched the token code; otherwise, false.</returns>
        public bool ReadIf(TsTokenCode tokenCode, out TsToken token)
        {
            if (Peek().TokenCode == tokenCode)
            {
                token = Read();
                return true;
            }

            token = null;
            return false;
        }

        /// <summary>
        /// Reads the next token, but only if the token function returns true.
        /// </summary>
        /// <param name="expectedTokenFunc">The function to match a token code.</param>
        /// <param name="token">The read token, or null if the next token does not match the token code.</param>
        /// <returns>true if the next token was read because it matched the token code; otherwise, false.</returns>
        public bool ReadIf(Func<TsTokenCode, bool> expectedTokenFunc, out TsToken token)
        {
            if (expectedTokenFunc(Peek().TokenCode))
            {
                token = Read();
                return true;
            }

            token = null;
            return false;
        }

        /// <summary>
        /// Reads the next token, but only if the token function returns true.
        /// </summary>
        /// <param name="expectedTokenFunc">The function to match a token code.</param>
        /// <returns>true if the next token was read because it matched the token code; otherwise, false.</returns>
        public bool ReadIf(Func<TsTokenCode, bool> expectedTokenFunc) => ReadIf(expectedTokenFunc, out _);

        /// <summary>
        /// Skips the next token, but only if the token code matches.
        /// </summary>
        /// <param name="tokenCode">The token code to match.</param>
        /// <returns>true if the next token was read because it matched the token code; otherwise, false.</returns>
        public bool SkipIf(TsTokenCode tokenCode) => ReadIf(tokenCode, out _);

        /// <summary>
        /// Preserves the state of the reader while <paramref name="action"/> is called and restores
        /// the state after it returns. This is useful for parsing potential productions, but rolling
        /// back state if a production does not match.
        /// </summary>
        /// <param name="action">The function to run inside of saved state.</param>
        public void ReadWithSavedState(Action action)
        {
            bool ReturnTrue()
            {
                action();
                return true;
            }

            ReadWithSavedState(ReturnTrue);
        }

        /// <summary>
        /// Preserves the state of the reader while <paramref name="func"/> is called and restores
        /// the state after it returns. This is useful for parsing potential productions, but rolling
        /// back state if a production does not match.
        /// </summary>
        /// <typeparam name="T">The return type of the function to run.</typeparam>
        /// <param name="func">The function to run inside of saved state.</param>
        /// <param name="shouldCommitReadFunc">
        /// An optional function that will be called with the result of <paramref name="func"/>,
        /// which returns a value indicating whether to commit the read or to rollback to the saved
        /// state. This is really useful in scenarios where a parse needs to be tried, but if it
        /// succeeds we don't need to parse again.
        /// </param>
        /// <returns>Whatever <paramref name="func"/> returns.</returns>
        public T ReadWithSavedState<T>(Func<T> func, Func<T, bool> shouldCommitReadFunc = null)
        {
            int savedIndex = _index;
            _withinSavedStateBlockCount++;

            var returnValue = default(T);
            try
            {
                returnValue = func();
            }
            finally
            {
                if (shouldCommitReadFunc?.Invoke(returnValue) != true)
                {
                    _index = savedIndex;
                    _withinSavedStateBlockCount--;
                    Debug.Assert(_withinSavedStateBlockCount >= 0);
                }
            }

            return returnValue;
        }
    }
}
