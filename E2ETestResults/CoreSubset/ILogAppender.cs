// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogAppender.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;

    /// <summary>
    /// An interface for all Log appenders.
    /// </summary>
    public interface ILogAppender
    {
        /// <summary>
        /// Adds a function used to filter calls to <see cref="Log"/>.  Should return <c>true</c>
        /// to include a log message or <see langword="false" /> to exclude.
        /// </summary>
        void AddFilter(Func<Logger, LoggerLevel, bool> f);

        void RemoveFilter(Func<Logger, LoggerLevel, bool> f);

        void ClearFilters();

        /// <summary>
        /// Logs the specified information.
        /// </summary>
        /// <param name="source">The source Log</param>
        /// <param name="level">The level of the log message</param>
        /// <param name="message">The message to write</param>
        /// <param name="args">Optional message arguments.</param>
        void Log(Logger source, LoggerLevel level, string message, object[] args);
    }
}
