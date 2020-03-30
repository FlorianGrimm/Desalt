// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DoubleUtil.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains utility methods for floating point values.
    /// </summary>
    public static class DoubleUtil
    {
        // Useful references for understanding floating-point representation and arithmetic:
        // http://docs.oracle.com/cd/E19957-01/806-3568/ncg_goldberg.html
        // https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
        // http://babbage.cs.qc.cuny.edu/IEEE-754/

        // https://en.wikipedia.org/wiki/Machine_epsilon for double precision, approx. 2e-16.
        // Crucially, this is finer than the precision of Tableau's display format (15 significant digits).
        public static readonly double Epsilon = Math.Pow(2, -52);

        private static readonly double OnePlusEpsilon = 1.0 + Epsilon;

        private static readonly double UpperBound = OnePlusEpsilon;

        private static readonly double LowerBound = 1.0 / OnePlusEpsilon;

        // Approximate 64-bit integer limits based on the first 15 digits of 2^63.
        // http://stackoverflow.com/questions/9297434/parseint-rounds-incorrectly
        private const double LongMaxValue = 9223372036854770000.0;

        private const double LongMinValue = -LongMaxValue;

        private static Logger Log
        {
            get
            {
                return Logger.LazyGetLogger(typeof(DoubleUtil));
            }
        }

        /// <summary>
        /// Returns whether d1 equals, or is within rounding error of equal, to d2
        /// (For why this is needed, see https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/ )
        /// </summary>
        public static bool IsApproximatelyEqual(double d1, double d2)
        {
            // special case for numbers very close to zero; see "Infernal zero" in the link above
            if (Math.Abs(d1 - d2) < Epsilon)
            {
                return true;
            }

            // Avoid divide-by-zero
            if (d1 == 0.0)
            {
                return false;
            }

            double intermediate = d2 / d1;
            if (LowerBound <= intermediate && intermediate <= UpperBound)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether d is approximately equal to 0
        /// </summary>
        public static bool IsApproximatelyZero(double d)
        {
            return IsApproximatelyEqual(0.0, d);
        }

        /// <summary>
        /// Returns whether d1 is less than d2, but not (approximately) equal
        /// (i.e., the inverse of IsGreaterThanOrApproximatelyEqual).
        /// </summary>
        public static bool IsLessThanAndNotApproximatelyEqual(double d1, double d2)
        {
            return d1 < d2 && !IsApproximatelyEqual(d1, d2);
        }

        /// <summary>
        /// Returns whether d1 is less than or approximately equal to d2
        /// (i.e., the inverse of IsGreaterThanAndNotApproximatelyEqual).
        /// </summary>
        public static bool IsLessThanOrApproximatelyEqual(double d1, double d2)
        {
            return d1 < d2 || IsApproximatelyEqual(d1, d2);
        }

        /// <summary>
        /// Returns whether d1 is greater than d2, but not (approximately) equal
        /// (i.e., the inverse of IsLessThanOrApproximatelyEqual).
        /// </summary>
        public static bool IsGreaterThanAndNotApproximatelyEqual(double d1, double d2)
        {
            return d1 > d2 && !IsApproximatelyEqual(d1, d2);
        }

        /// <summary>
        /// Returns whether d1 is greater than or approximately equal to d2
        /// (i.e., the inverse of IsLessThanAndNotApproximatelyEqual).
        /// </summary>
        public static bool IsGreaterThanOrApproximatelyEqual(double d1, double d2)
        {
            return d1 > d2 || IsApproximatelyEqual(d1, d2);
        }

        /// <summary>
        /// Returns n rounded to the number of significant figures indicated by numSigFigs
        /// </summary>
        public static double SigFigs(double n, uint numSigFigs)
        {
            if (n == 0 || numSigFigs == 0)
            {
                Log.Warn("Neither the input nor the number of significant figures can be 0");
                return n;
            }
            double mult = Math.Pow(10, (int)numSigFigs - Math.Floor(Math.Log(Math.Abs(n)) / Math.LN10) - 1);
            return Math.Round(n * mult) / mult;
        }

        /// <summary>
        /// Extension method on double to convert to a rounded int.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1720", Justification = "Int is used to signify type, so it is in the title.")]
        public static int RoundToInt(this double value)
        {
            return Math.Round(value);
        }

        /// <summary>
        /// Returns double.Parse(s) if it yields a finite value, null otherwise
        /// </summary>
        public static double? ParseDouble(string s)
        {
            double val = double.Parse(s);
            return double.IsFinite(val) ? val : (double?)null;
        }

        /// <summary>
        /// Returns double.Parse(s) if it yields a value, or the default otherwise
        /// </summary>
        public static double TryParseDouble(string s, double defaultValue)
        {
            double val = double.Parse(s);

            if (double.IsNaN(val) || !double.IsFinite(val))
            {
                return defaultValue;
            }

            return val;
        }

        /// <summary>
        /// Returns true if string can be parsed into a double
        /// </summary>
        public static bool IsValidDouble(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return jQueryExtensions.IsNumeric(s);
        }

        /// <summary>
        /// Uses parseInt in javascript to parse string
        /// This means that accuracy is not guaranteed to 64-bits
        /// See:http://stackoverflow.com/questions/9297434/parseint-rounds-incorrectly
        /// </summary>
        public static long? Parse64BitInteger(string s)
        {
            long parsed;
            if (!long.TryParse(s, out parsed))
            {
                return (long?)null;
            }

            double? validDouble = ParseValidDouble(s);
            if (validDouble > LongMaxValue || validDouble < LongMinValue)
            {
                return (long?)null;
            }

            return parsed;
        }

        /// <summary>
        /// Returns true if string can be parsed into a 32-bit integer
        /// Returns false if string can be parsed, but overflows or underflows
        /// </summary>
        public static bool IsValid32BitInteger(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            long? parsed = Parse64BitInteger(s);
            if (parsed == null)
            {
                return false;
            }

            return (parsed <= int.MaxValue) && (parsed >= int.MinValue);
        }

        /// <summary>
        /// Returns true if string can be parsed into a 64-bit integer
        /// </summary>
        public static bool IsValid64BitInteger(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            long? parsed = Parse64BitInteger(s);
            if (parsed == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a double if the string represents a double, NaN otherwise
        /// Protects against double.Parse parsing of "1a" into "1"
        /// </summary>
        public static double ParseValidDouble(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return double.NaN;
            }

            // Protect against double.Parse parsing of "1a" into "1"
            if (!IsValidDouble(s))
            {
                return double.NaN;
            }

            return double.Parse(s);
        }

        public static double MultiplyBy100(double num)
        {
            return (num * 1000) / 10; // Workaround for when JS computes .58 * 100 = 57.9999
        }

        public static double TruncateTwoDecimalPlaces(double num)
        {
            return (int)(MultiplyBy100(num)) / 100f;
        }

        public static double RoundTwoDecimalPlaces(double num)
        {
            return Math.Round(MultiplyBy100(num)) / 100f; // Used for when JS uses .98 as .97999999, causing truncating problems
        }
    }
}
