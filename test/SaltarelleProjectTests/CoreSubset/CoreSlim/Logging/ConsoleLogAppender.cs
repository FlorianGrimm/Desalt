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
    using System.Html;

    /// <summary>
    /// An appender that writes to console.log.
    /// </summary>
    public class ConsoleLogAppender : BaseLogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly LogAppenderInstance<ConsoleLogAppender> GlobalAppender = new LogAppenderInstance<ConsoleLogAppender>(() => new ConsoleLogAppender());

        private JsDictionary<string, object> levelMethods;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

#if DEBUG

        static ConsoleLogAppender()
        {
            // by default we're going to enable console.log for Info+ in debug
            GlobalAppender.EnableLogging((_, loggerLevel) => loggerLevel >= LoggerLevel.Info);
        }

#endif

        private ConsoleLogAppender()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================
        protected override void LogInternal(Logger source, LoggerLevel level, string message, object[] args)
        {
            if (Script.TypeOf(((dynamic)typeof(Window)).console) != "object")
            {
                return;
            }

            message = source.Name + ": " + message;

            JsArray<object> consoleArgs = new JsArray<object>();
            consoleArgs = consoleArgs.Concat(message).Concat(args);

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
