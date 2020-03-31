// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Param.cs" company="Tableau Software">
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
    using System.Html;

    /// <summary>
    /// Contains utility methods for checking method parameters.
    /// </summary>
    public static class Param
    {
        //// ===========================================================================================================
        //// Fields
        //// ===========================================================================================================

        /// <summary>
        /// Indicates whether to suppress browser alerts when an excpetion is thrown.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields",
            Justification = "This is used in test code.")]
        public static bool SuppressAlerts = false;

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates a standard <see cref="Exception"/> for reporting null or undefined argument values. Useful for
        /// unit testing to verify that an exception gets thrown for an invalid parameter.
        /// </summary>
        /// <param name="paramName">The name of the argument that is null or undefined.</param>
        /// <returns>An <see cref="Exception"/> (Error in JavaScript) containing a helpful message and errorInfo of
        /// the following form:
        /// {
        ///   paramName: paramName
        /// }</returns>
        public static Exception CreateArgumentNullOrUndefinedException(string paramName)
        {
            Exception ex = new Exception(paramName + " is null or undefined.");
            ((dynamic)ex).paramName = paramName;
            return ex;
        }

        /// <summary>
        /// Verifies that the string parameter is not null, undefined, or whitespace. Throws an exception if the
        /// parameter is not valid.
        /// </summary>
        /// <param name="param">The parameter to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void VerifyString(string param, string paramName)
        {
            // Check the null/undefined cases first
            VerifyValue(param, paramName);
            if (param.Trim().Length == 0)
            {
                Exception ex = new Exception(paramName + " contains only white space");
                ((dynamic)ex).paramName = paramName;
                ShowParameterAlert(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Verifies that the parameter is not null, undefined, or whitespace. Throws an exception if the parameter is
        /// not valid.
        /// </summary>
        /// <param name="param">The parameter to check.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void VerifyValue(object param, string paramName)
        {
            if (Script.IsNullOrUndefined(param))
            {
                Exception ex = CreateArgumentNullOrUndefinedException(paramName);
                ShowParameterAlert(ex);
                throw ex;
            }
        }

        private static void ShowParameterAlert(Exception ex)
        {
            if (SuppressAlerts)
            {
                return;
            }

            // This try/catch block is so we can get the stack trace to display in the alert dialog.
            try
            {
                throw ex;
            }
            catch (Exception exceptionWithStack)
            {
                Window.Alert(FormatExceptionMessage(exceptionWithStack));
            }
        }

        /// <summary>
        /// Adds stack frame information to the error message if it's present.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>A formatted string useful for logging or displaying in an alert dialog.</returns>
        private static string FormatExceptionMessage(Exception ex)
        {
            string message;

            if (Script.IsValue(((dynamic)ex).stack))
            {
                message = ((dynamic)ex).stack;
            }
            else
            {
                message = ex.Message;
            }

            return message;
        }
    }
}
