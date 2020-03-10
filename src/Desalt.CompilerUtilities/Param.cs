// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Param.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.CompilerUtilities
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Contains utility methods for verifying parameters to methods.
    /// </summary>
    public static class Param
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal const string ParameterNotValid = "Parameter not valid";
        internal const string ParameterOutOfRange = "Parameter out of range.";
        internal const string StringParameterCannotBeNullOrEmpty = "String parameter cannot be null or empty.";
        internal const string InvalidEnumArgument = "The value of argument '{0}' ({1}) is invalid for Enum type '{2}'.";

        /// <summary>
        /// Used as a marker method to annotate that a parameter does not need
        /// to be validated. Only defined for Debug builds.
        /// </summary>
        /// <param name="parameterValue">The parameter to test.</param>
        [Conditional("DEBUG")]
        public static void Ignore(params object[] parameterValue)
        {
        }

        /// <summary>
        /// Verifies that the specified parameter is not null, throwing an <see cref="ArgumentNullException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void VerifyNotNull(object parameterValue, string parameterName)
        {
            Ignore(parameterValue, parameterName);

            if (parameterValue == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Verifies that the specified string parameter is not null or an empty string, throwing an
        /// <see cref="ArgumentException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void VerifyString(string parameterValue, string parameterName)
        {
            Ignore(parameterName);
            VerifyNotNull(parameterValue, parameterName);

            if (string.IsNullOrEmpty(parameterValue.Trim()))
            {
                throw new ArgumentException(StringParameterCannotBeNullOrEmpty, parameterName);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is a valid enum value using reflection, throwing an
        /// <see cref="ArgumentException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <param name="parameterValue">The enum value to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="enumType">The <see cref="Type"/> of the enum.</param>
        public static void VerifyEnum(Enum parameterValue, string parameterName, Type enumType)
        {
            Ignore(parameterValue, parameterName, enumType);

            if (!Enum.IsDefined(enumType, parameterValue))
            {
                int valueInt = Convert.ToInt32(parameterValue, CultureInfo.InvariantCulture);
                string message = string.Format(
                    CultureInfo.CurrentCulture,
                    InvalidEnumArgument,
                    parameterName,
                    valueInt,
                    enumType.Name);
                throw new ArgumentException(message, parameterName);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is less than the specified maximum value, throwing an
        /// <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="maxValue">The maximum acceptable value of the parameter
        /// (exclusive).</param>
        public static void VerifyLessThan<T>(T parameterValue, string parameterName, T maxValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, maxValue);
            if (parameterValue.CompareTo(maxValue) >= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is less than or equal to the specified maximum value, throwing an
        /// <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="maxValue">The maximum acceptable value of the parameter
        /// (inclusive).</param>
        public static void VerifyLessThanOrEqualTo<T>(T parameterValue, string parameterName, T maxValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, maxValue);
            if (parameterValue.CompareTo(maxValue) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is greater than the specified minimum value, throwing an
        /// <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="minValue">The minimum acceptable value of the parameter
        /// (exclusive).</param>
        public static void VerifyGreaterThan<T>(T parameterValue, string parameterName, T minValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, minValue);
            if (parameterValue.CompareTo(minValue) <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is greater than or equal to the specified minimum value, throwing an
        /// <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="minValue">The minimum acceptable value of the parameter (inclusive).</param>
        public static void VerifyGreaterThanOrEqualTo<T>(T parameterValue, string parameterName, T minValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, minValue);
            if (parameterValue.CompareTo(minValue) < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is between the specified minimum and maximum values, throwing an
        /// <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="minValue">The minimum acceptable value of the parameter (exclusive).</param>
        /// <param name="maxValue">The maximum acceptable value of the parameter (exclusive).</param>
        public static void VerifyBetween<T>(T parameterValue, string parameterName, T minValue, T maxValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, minValue, maxValue);
            if (parameterValue.CompareTo(minValue) <= 0 || parameterValue.CompareTo(maxValue) >= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the specified parameter is between or equal to the specified minimum and maximum values,
        /// throwing an <see cref="ArgumentOutOfRangeException"/> if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <typeparam name="T">The type of the parameter to test.</typeparam>
        /// <param name="parameterValue">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="minValue">The minimum acceptable value of the parameter (inclusive).</param>
        /// <param name="maxValue">The maximum acceptable value of the parameter (inclusive).</param>
        public static void VerifyBetweenOrEqualTo<T>(T parameterValue, string parameterName, T minValue, T maxValue)
            where T : IComparable<T>
        {
            Ignore(parameterValue, parameterName, minValue, maxValue);
            if (parameterValue.CompareTo(minValue) < 0 || parameterValue.CompareTo(maxValue) > 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, ParameterOutOfRange);
            }
        }

        /// <summary>
        /// Verifies that the condition is true for the specified parameter, throwing an <see cref="ArgumentException"/>
        /// if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void VerifyValid(bool condition, string parameterName)
        {
            VerifyValid(condition, parameterName, null);
        }

        /// <summary>
        /// Verifies that the condition is true for the specified parameter, throwing an <see cref="ArgumentException"/>
        /// if it is not.
        /// Meant to be used in public methods, since it is defined in both Debug and Release builds.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="exceptionMessageGetter">The function to call to get the exception message. Can be null for a
        /// default message.</param>
        public static void VerifyValid(bool condition, string parameterName, Func<string> exceptionMessageGetter)
        {
            Ignore(condition, parameterName, exceptionMessageGetter);

            if (!condition)
            {
                string message = exceptionMessageGetter?.Invoke();
                message = string.IsNullOrEmpty(message) ? ParameterNotValid : message;
                throw new ArgumentException(message, parameterName);
            }
        }
    }
}
