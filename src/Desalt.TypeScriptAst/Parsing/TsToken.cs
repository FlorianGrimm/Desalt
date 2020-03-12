// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsToken.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Parsing
{
    using System;
    using Desalt.CompilerUtilities;

    /// <summary>
    /// Represents a token in TypeScript source code.
    /// </summary>
    public class TsToken : IEquatable<TsToken>
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly TsToken ErrorPlaceholder = new TsToken(
            TsTokenCode.ErrorPlaceholder,
            "ErrorPlaceholder",
            TextReaderLocation.Empty);

        public static readonly TsToken EndOfTokens = new TsToken(
            TsTokenCode.EndOfTokens,
            "EndOfTokens",
            TextReaderLocation.Empty);

        public static readonly object NullValue = new object();

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public TsToken(TsTokenCode tokenCode, string text, TextReaderLocation location)
        {
            TokenCode = tokenCode;
            Text = string.IsNullOrWhiteSpace(text) ? throw new ArgumentNullException(nameof(text)) : text;
            Location = location;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the location in the source code where this token was present.
        /// </summary>
        public TextReaderLocation Location { get; }

        public TsTokenCode TokenCode { get; }

        public string Text { get; }

        /// <summary>
        /// Gets the value of the token. For example, if the token represents an integer literal,
        /// then this property would return the actual integer.
        /// </summary>
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
                        return NullValue;

                    default:
                        return Text;
                }
            }
        }

        //// ===========================================================================================================
        //// Operators
        //// ===========================================================================================================

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="TsToken"/> objects
        /// are equal.
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
        /// Returns a value that indicates whether two <see cref="TsToken"/> objects have different values.
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

        public static TsToken Identifier(string text, string identifier, TextReaderLocation location)
        {
            return WithValue(TsTokenCode.Identifier, text, identifier, location);
        }

        public static TsToken StringLiteral(string text, string value, TextReaderLocation location)
        {
            return WithValue(TsTokenCode.StringLiteral, text, value, location);
        }

        public static TsToken NumericLiteral(
            TsTokenCode tokenCode,
            string text,
            double value,
            TextReaderLocation location)
        {
            return WithValue(tokenCode, text, value, location);
        }

        public static TsToken WithValue<T>(TsTokenCode tokenCode, string text, T value, TextReaderLocation location)
        {
            return new TsTokenWithValue<T>(tokenCode, text, value, location);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"TokenCode={TokenCode}, Text={Text}, Value={Value}";
        }

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

            return Location.Equals(other.Location) &&
                TokenCode == other.TokenCode &&
                string.Equals(Text, other.Text) &&
                (Value?.Equals(other.Value) ?? other.Value == null);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <see langword="true"/> if the specified object is equal to the current object; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is TsToken other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Location.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)TokenCode;
                hashCode = (hashCode * 397) ^ (Text != null ? Text.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                return hashCode;
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

        public TsTokenWithValue(TsTokenCode tokenCode, string text, T value, TextReaderLocation location)
            : base(tokenCode, text, location)
        {
            Value = value;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public new T Value { get; }
    }
}
