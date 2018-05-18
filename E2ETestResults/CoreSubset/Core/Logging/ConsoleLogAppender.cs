// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleLogAppender.cs" company="Tableau Software">
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
    using System.Diagnostics;
    using System.Html;
    using System.Runtime.CompilerServices;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// An appender that writes to console.log.
    /// </summary>
    public class ConsoleLogAppender : BaseLogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static ConsoleLogAppender globalAppender;

        private JsDictionary<string, object> levelMethods;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

#if DEBUG

        static ConsoleLogAppender()
        {
            // by default we're going to enable console.log for Info+ in debug
            EnableLogging(delegate(Logger l, LoggerLevel ll)
            {
                return ll >= LoggerLevel.Info;
            });
        }

#endif

        private ConsoleLogAppender()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        [AlternateSignature]
        public static extern void EnableLogging();

        /// <summary>
        /// Enables logging using this appender type.
        /// </summary>
        /// <param name="filter">The filter to apply to this appender or <c>null</c> to enable for all loggers</param>
        [Conditional("DEBUG")]
        public static void EnableLogging(Func<Logger, LoggerLevel, bool> filter)
        {
            if (Script.IsNullOrUndefined(globalAppender))
            {
                globalAppender = new ConsoleLogAppender();
                Logger.AddAppender(globalAppender);
            }

            globalAppender.AddFilter(ScriptEx.Value(filter, delegate { return true; }));
        }

        /// <summary>
        /// Disables logging using this appender type.
        /// </summary>
        [Conditional("DEBUG")]
        public static void DisableLogging()
        {
            if (Script.IsNullOrUndefined(globalAppender))
            {
                return;
            }

            Logger.RemoveAppender(globalAppender);
            globalAppender = null;
        }

        protected override void LogInternal(Logger source, LoggerLevel level, string message, object[] args)
        {
            if (Script.TypeOf(((dynamic)typeof(Window)).console) != "object")
            {
                return;
            }

            message = source.Name + ": " + message;

            JsArray<object> consoleArgs = new JsArray<object>();
            if (BrowserSupport.ConsoleLogFormating)
            {
                consoleArgs = consoleArgs.Concat(message).Concat(args);
            }
            else
            {
                // console.log doesn't do formatting so we need to
                consoleArgs = consoleArgs.Concat(this.FormatMessage(message, args));
            }

            try
            {
                ((dynamic)typeof(Function).Prototype).apply.call(this.GetConsoleMethod(level), ((dynamic)typeof(Window)).console, consoleArgs);
            }
            catch
            {
                // ignore
            }
        }

        private object GetConsoleMethod(LoggerLevel level)
        {
            dynamic console = TypeUtil.GetField<object>(Window.Self, "console");
            if (this.levelMethods == null)
            {
                this.levelMethods = new JsDictionary<string, object>();
                this.levelMethods[LoggerLevel.Debug.ToString()] = console.log;
                this.levelMethods[LoggerLevel.Error.ToString()] = console.error;
                this.levelMethods[LoggerLevel.Info.ToString()] = console.info;
                this.levelMethods[LoggerLevel.Warn.ToString()] = console.warn;
            }

            return this.levelMethods[level.ToString()] ?? console.log;
        }
    }
}
