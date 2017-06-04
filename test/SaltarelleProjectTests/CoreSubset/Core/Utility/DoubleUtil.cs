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
    }
}
