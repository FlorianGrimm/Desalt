// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseLogAppender.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A base class for log appenders.
    /// </summary>
    public abstract class BaseLogAppender : ILogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly List<Func<Logger, LoggerLevel, bool>> filters;

        protected BaseLogAppender()
        {
            this.filters = new List<Func<Logger, LoggerLevel, bool>>();
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public void ClearFilters()
        {
            this.filters.Clear();
        }

        public void AddFilter(Func<Logger, LoggerLevel, bool> f)
        {
            this.filters.Add(f);
        }

        public void RemoveFilter(Func<Logger, LoggerLevel, bool> f)
        {
            this.filters.Remove(f);
        }

        public void Log(Logger source, LoggerLevel level, string message, object[] args)
        {
#if DEBUG // only enable logging in "debug" mode

            foreach (Func<Logger, LoggerLevel, bool> filter in this.filters)
            {
                if (!filter(source, level)) { continue; }
                this.LogInternal(source, level, message, args);
                return;
            }
#endif
        }

        /// <summary>
        /// Performs the actual logging.  The filters are checked before this method is called.
        /// </summary>
        /// <param name="source">The source Log</param>
        /// <param name="level">The message level</param>
        /// <param name="message">The message to write</param>
        /// <param name="args">Optional message arguments.</param>
        protected abstract void LogInternal(Logger source, LoggerLevel level, string message, object[] args);

        protected virtual string FormatMessage(string message, object[] args)
        {
            if (Script.IsNullOrUndefined(args) || args.Length == 0)
            {
                return message;
            }

            StringBuilder sb = new StringBuilder();
            int argNum = 0;

            bool prevPercent = false;
            for (int i = 0; i < message.Length; i++)
            {
                var currChar = message.CharAt(i);
                if (currChar == '%')
                {
                    if (prevPercent)
                    {
                        sb.Append("%");
                        prevPercent = false;
                    }
                    else
                    {
                        prevPercent = true;
                    }
                }
                else
                {
                    if (prevPercent)
                    {
                        switch (currChar)
                        {
                            case 'b':
                            case 's':
                            case 'd':
                            case 'n':
                            case 'o':
                                sb.Append(args.Length > argNum ? args[argNum] : "");
                                argNum++;
                                break;
                        }
                    }
                    else
                    {
                        sb.Append(currChar);
                    }

                    prevPercent = false;
                }
            }

            return sb.ToString();
        }
    }
}
