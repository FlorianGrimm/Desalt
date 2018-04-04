// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsToken.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Ast.Parsing
{
    using System;
    /// <summary>
    /// Represents a token in TypeScript source code.
    /// </summary>
    internal class TsToken : IEquatable<TsToken>
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private TsToken(TsTokenCode tokenCode, string text)
        {
            TokenCode = tokenCode;
            Text = string.IsNullOrWhiteSpace(text) ? throw new ArgumentNullException(nameof(text)) : text;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public TsTokenCode TokenCode { get; }

        public string Text { get; }

        public virtual object Value
        {
            get
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (TokenCode)
                {
                    case TsTokenCode.True:
                        return true;

                    case TsTokenCode.False:
                        return false;

                    case TsTokenCode.Null:
                        return null;

                    default:
                        return Text;
                }
            }
        }

        //// ===========================================================================================================
        //// Operators
        //// ===========================================================================================================


        /// <summary>
        /// Returns a value that indicates whether the values of two <see
        /// cref="T:Desalt.Core.TypeScript.Ast.Parsing.TsToken"/> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the <paramref name="left"/> and <paramref name="right"/> parameters have the same
        /// value; otherwise, false.
        /// </returns>
        public static bool operator ==(TsToken left, TsToken right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see
        /// cref="T:Desalt.Core.TypeScript.Ast.Parsing.TsToken"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if <paramref name="left"/> and <paramref name="right"/> are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(TsToken left, TsToken right)
        {
            return !Equals(left, right);
        }


        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static TsToken Identifier(string identifierName) => new TsToken(TsTokenCode.Identifier, identifierName);

        public static TsToken WithValue<T>(TsTokenCode tokenCode, string text, T value) =>
            new TsTokenWithValue<T>(tokenCode, text, value);

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{TokenCode} {Text}";

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(TsToken other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return TokenCode == other.TokenCode && string.Equals(Text, other.Text);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as TsToken;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)TokenCode * 397) ^ Text.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Represents a token with a value (string literals, decimal literals, etc.)
    /// </summary>
    /// <typeparam name="T">The type of value.</typeparam>
    internal class TsTokenWithValue<T> : TsToken
    {
        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsTokenWithValue(TsTokenCode tokenCode, string text, T value)
            : base(tokenCode, text)
        {
            ValueField = value;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================


        public override object Value => ValueField;

        public T ValueField { get; }
    }
}
