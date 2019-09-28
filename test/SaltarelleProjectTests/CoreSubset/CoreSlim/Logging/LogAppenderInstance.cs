// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAppenderInstance.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Used to hold an instance of ILogAppender, ensures that when enabled/disabled the appender is
    /// removed/added to the global Logger instance.
    /// This has the IncludeGenericArguments(false) attribute because it is part of CoreSlim which
    /// does not include the full mscorlib so does not have the generic class initializer.
    /// This attribute means that this class is treated like a normal class (generic is checked only
    /// at compile time)
    /// </summary>
    /// <typeparam name="T">The concrete implementation of ILogAppender</typeparam>
    [IncludeGenericArguments(false)]
    public class LogAppenderInstance<T> where T : class, ILogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly Func<T> appenderFactoryFunc;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================
        public LogAppenderInstance(Func<T> appenderFactoryFunc)
        {
            this.appenderFactoryFunc = appenderFactoryFunc;
        }

        //// ===========================================================================================================
        //// Events
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================
        public T Instance { get; private set; }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Enables logging using this appender type.
        /// </summary>
        /// <param name="filter">The filter to apply to this appender or <c>null</c> to enable for all loggers</param>
        [Conditional("DEBUG")]
        public void EnableLogging(Func<Logger, LoggerLevel, bool> filter = null)
        {
            if (this.Instance == null)
            {
                this.Instance = this.appenderFactoryFunc();
                Logger.AddAppender(this.Instance);
            }
            else if (!Logger.HasAppender(this.Instance))
            {
                Logger.AddAppender(this.Instance);
            }

            this.Instance.AddFilter(Script.Coalesce(filter, (_, __) => true));
        }

        /// <summary>
        /// Disables logging using this appender type.
        /// </summary>
        [Conditional("DEBUG")]
        public void DisableLogging()
        {
            if (this.Instance == null)
            {
                return;
            }

            Logger.RemoveAppender(this.Instance);
            this.Instance = null;
        }
    }
}
