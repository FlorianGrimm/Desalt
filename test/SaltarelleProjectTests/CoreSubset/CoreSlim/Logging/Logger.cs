// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The various levels of logging priority.
    /// <seealso cref="Logger.LoggerLevelNames"/>
    /// </summary>
    public enum LoggerLevel
    {
        All = 0, // when used in filtering allows all log levels.
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Off = 5 // when used in filtering denies all log levels.
    }

    /// <summary>
    /// Supports javascript logging.  See <a href="http://mytableau/display/DevServFront/JavaScript+Logging">the wiki</a>
    /// for details.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// A global logger.  To be used by code that doesn't want to create an local instance of
        /// a logger.
        /// </summary>
        public static readonly Logger Global = GetLoggerWithName("global");

        /// <summary>
        /// The translation of logging priority into string names, indexed by level value.
        /// <seealso cref="LoggerLevel"/>
        /// </summary>
        public static readonly JsArray<string> LoggerLevelNames = new JsArray<string>();

        private const string LogQueryParam = ":log";

        private static readonly JsArray<ILogAppender> Appenders = new JsArray<ILogAppender>();
        private static readonly JsArray<Func<Logger, LoggerLevel, bool>> Filters = new JsArray<Func<Logger, LoggerLevel, bool>>();
        private static readonly Logger NullLog = new Logger("");
        private readonly string name;

        static Logger()
        {
            // Include any FilterBy* statements below to enable logging at a granular level.
            // This can also be accomplished by using the :log URL parameter.  See the wiki for details.
            //
            // Note that since this class is in the common code it cannot reference any classes
            // from the web/mobile code directly.  To get granular control over those clasess
            // either user FilterByName or add static inits in that code directly.

            SetupUrlFilters();

            LoggerLevelNames[(int)LoggerLevel.All] = "all";
            LoggerLevelNames[(int)LoggerLevel.Debug] = "debug";
            LoggerLevelNames[(int)LoggerLevel.Info] = "info";
            LoggerLevelNames[(int)LoggerLevel.Warn] = "warn";
            LoggerLevelNames[(int)LoggerLevel.Error] = "error";
            LoggerLevelNames[(int)LoggerLevel.Off] = "off";
        }

        private Logger(string name)
        {
            this.name = name;
        }

        [Obsolete("Should use Global field instead")]
        public static Logger GlobalLog { get { return Global; } }

        /// <summary>
        /// Gets the name of this log.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Removes all existing filters.
        /// </summary>
        [Conditional("DEBUG")]
        public static void ClearFilters()
        {
            foreach (ILogAppender logAppender in Appenders)
            {
                logAppender.ClearFilters();
            }
            Filters.Splice(0, Filters.Length);
        }

        /// <summary>
        /// Adds a filter to allow logging from the given logger at the specified level.
        /// </summary>
        /// <param name="validLogger">The logger to accept</param>
        /// <param name="minLogLevel">The minimum level to accept</param>
        [Conditional("DEBUG")]
        public static void FilterByLogger(Logger validLogger, LoggerLevel minLogLevel)
        {
            minLogLevel = ScriptEx.Value(minLogLevel, LoggerLevel.All);

            AddFilter((Logger l, LoggerLevel ll) => l == validLogger && ll >= minLogLevel);
        }

        /// <summary>
        /// Adds a filter to allow logging from the given type at the specified level.  Assumes
        /// that the type contains a static logger generated using <see cref="GetLogger(System.Type, LoggerLevel?)"/>.
        /// </summary>
        /// <param name="t">The type used for creating the logger</param>
        /// <param name="minLogLevel">The minimum level to accept</param>
        [Conditional("DEBUG")]
        public static void FilterByType(Type t, LoggerLevel minLogLevel)
        {
            minLogLevel = ScriptEx.Value(minLogLevel, LoggerLevel.All);

            AddFilter((Logger l, LoggerLevel ll) => ll >= minLogLevel && l.Name == t.Name);
        }

        /// <summary>
        /// Adds a filter to allow logging from a logger that matches the given pattern at the specified
        /// level.
        /// </summary>
        /// <param name="namePattern">A regular expression to match against the logger name</param>
        /// <param name="minLogLevel">The minimum level to accept</param>
        [Conditional("DEBUG")]
        public static void FilterByName(string namePattern, LoggerLevel minLogLevel)
        {
            minLogLevel = ScriptEx.Value(minLogLevel, LoggerLevel.All);
            var regex = new Regex(namePattern, "i");

            AddFilter((Logger l, LoggerLevel ll) => ll >= minLogLevel && Script.IsValue(l.Name.Match(regex)));
        }

        /// <summary>
        /// Clears all appenders.
        /// </summary>
        public static void ClearAppenders()
        {
            Appenders.Splice(0, Filters.Length);
        }

        /// <summary>
        /// Is the given appender already added
        /// </summary>
        /// <param name="appender">The appender to check for</param>
        /// <returns>If the appender has been added</returns>
        public static bool HasAppender(ILogAppender appender)
        {
            return Appenders.IndexOf(appender) > -1;
        }

        /// <summary>
        /// Adds a logging appender.
        /// </summary>
        /// <param name="appender">The appender to be added</param>
        public static void AddAppender(ILogAppender appender)
        {
            foreach (Func<Logger, LoggerLevel, bool> filter in Filters)
            {
                appender.AddFilter(filter);
            }

            Appenders.Push(appender);
        }

        /// <summary>
        /// Removes a logging appender.
        /// </summary>
        /// <param name="appender">The appender to be removed</param>
        public static void RemoveAppender(ILogAppender appender)
        {
            int indexOfAppender = Appenders.IndexOf(appender);
            if (indexOfAppender > -1)
            {
                Appenders.Splice(indexOfAppender, 1);
            }
        }

        /// <summary>
        /// Convenience method for lazily getting a logger for a class.  A static instance of a logger
        /// will be created the first time this method is called.  All subsequent calls will use the
        /// existing value.
        /// </summary>
        /// <param name="t">The type to assign the logger to.</param>
        /// <returns>The type's logger</returns>
        public static Logger LazyGetLogger(Type t)
        {
            const string FieldName = "_logger";
            var logger = JsDictionary.GetDictionary(t)[FieldName].ReinterpretAs<Logger>();

            if (Script.IsNullOrUndefined(logger))
            {
                logger = GetLogger(t);
                JsDictionary.GetDictionary(t)[FieldName] = logger;
            }

            return logger;
        }

        /// <summary>
        /// Creates a new instance of a log for the given type and includes a filter for the
        /// created logger at the given level.  This method should be used sparingly when
        /// debugging.
        /// </summary>
        /// <see cref="FilterByType(System.Type, LoggerLevel)"/>
        /// <param name="t">The type to create a log for</param>
        /// <param name="ll">The min </param>
        /// <returns>A new Log instance</returns>
        public static Logger GetLogger(Type t, LoggerLevel? ll = null)
        {
            Logger l = GetLoggerWithName(t.Name);
            if (ll != null)
            {
                FilterByLogger(l, ll.Value);
            }
            return l;
        }

        /// <summary>
        /// Creates a new instance of a log with the given name.
        /// </summary>
        /// <param name="name">The log name</param>
        /// <returns>A new Log instance</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "name",
            Justification = "It's used in the debug version")]
        public static Logger GetLoggerWithName(string name)
        {
#if DEBUG
            return new Logger(name);
#else
            return NullLog;
#endif
        }

        /// <summary>
        /// Logs the given message with <see cref="LoggerLevel.Debug"/>. By default Debug level output
        /// is not shown in the Console. See <a href="http://mytableau/display/DevServFront/JavaScript+Logging">the wiki</a>
        /// for details on how to control the logging output.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">The format arguments.</param>
        [Conditional("DEBUG")]
        public void Debug(string message, params object[] args)
        {
            this.LogInternal(LoggerLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs the given message with <see cref="LoggerLevel.Info"/>.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">The format arguments.</param>
        [Conditional("DEBUG")]
        public void Info(string message, params object[] args)
        {
            this.LogInternal(LoggerLevel.Info, message, args);
        }

        /// <summary>
        /// Logs the given message with <see cref="LoggerLevel.Warn"/>.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">The format arguments.</param>
        [Conditional("DEBUG")]
        public void Warn(string message, params object[] args)
        {
            this.LogInternal(LoggerLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs the given message with <see cref="LoggerLevel.Error"/>.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="args">The format arguments.</param>
        [Conditional("DEBUG")]
        public void Error(string message, params object[] args)
        {
            this.LogInternal(LoggerLevel.Error, message, args);
        }

        /// <summary>
        /// Logs the given message.
        /// </summary>
        [Conditional("DEBUG")]
        public void Log(LoggerLevel level, string message, params object[] args)
        {
            this.LogInternal(level, message, args);
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "level"), Conditional("DEBUG")]
        private static void SetupUrlFilters()
        {
            JsDictionary<string, JsArray<string>> queryParams = UriExtensions.GetUriQueryParameters(Window.Self.Location.Search);
            if (!queryParams.ContainsKey(LogQueryParam)) { return; }

            ClearFilters();

            JsArray<string> logParams = queryParams[LogQueryParam];
            if (logParams.Length == 0)
            {
                // allow debug for alll
                FilterByName(".*", LoggerLevel.All);
            }
            foreach (string logParam in logParams)
            {
                string[] logVals = logParam.Split(':');
                LoggerLevel level = LoggerLevel.Debug;
                if (logVals.Length > 0 && Script.IsValue(logVals[1]))
                {
                    string key = logVals[1].ToLowerCase();
                    int index = LoggerLevelNames.IndexOf(key);
                    if (index >= 0)
                    {
                        level = (LoggerLevel)index;
                    }
                }

                FilterByName(logVals[0], level);
            }
        }

        private static void AddFilter(Func<Logger, LoggerLevel, bool> filterFunc)
        {
            Filters.Push(filterFunc);

            foreach (ILogAppender logAppender in Appenders)
            {
                logAppender.AddFilter(filterFunc);
            }
        }

        [Conditional("DEBUG")]
        private void LogInternal(LoggerLevel level, string message, params object[] args)
        {
            try
            {
                foreach (ILogAppender logAppender in Appenders)
                {
                    logAppender.Log(this, level, message, args);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    /// <summary>
    /// Utility class to make creating loggers a little less verbose.
    /// </summary>
    public sealed class Log
    {
        private Log() { }

        public static Logger Get(object o)
        {
            return Logger.LazyGetLogger(o.GetType());
        }

        public static Logger Get(Type t)
        {
            return Logger.LazyGetLogger(t);
        }
    }
}
