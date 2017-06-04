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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
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
        public static readonly List<string> LoggerLevelNames = new List<string>();

        private const string LogQueryParam = ":log";

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
        /// Gets the list of static appenders.  You might ask yourself, why do I need to do some crazy initialization like this,
        /// can't I just use a static field? Sadly the answer is no.  This is because of ordering dependencies in the way
        /// Script# initializes static variables.  Because we want to be able to register appenders inside of other static
        /// initializers we need to make sure that Appenders don't depend on the order of static init.
        /// </summary>
        private static List<ILogAppender> Appenders
        {
            get
            {
                return (List<ILogAppender>)MiscUtil.LazyInitStaticField(
                    typeof(Logger), "appenders", delegate { return new List<ILogAppender>(); });
            }
        }

        /// <summary>
        /// Gets the list of static filters.  You might ask yourself, why do I need to do some crazy initialization like this,
        /// can't I just use a static field? Sadly the answer is no.  This is because of ordering dependencies in the way
        /// Script# initializes static variables.
        /// </summary>
        private static List<Func<Logger, LoggerLevel, bool>> Filters
        {
            get
            {
                return (List<Func<Logger, LoggerLevel, bool>>)MiscUtil.LazyInitStaticField(
                    typeof(Logger), "filters", delegate { return new List<Func<Logger, LoggerLevel, bool>>(); });
            }
        }

        /// <summary>
        /// Gets the Null logger.  Again, have to do crazy static lazy init here to avoid issues with Script#
        /// compilation/static init.
        /// </summary>
        private static Logger NullLog
        {
            get
            {
                return (Logger)MiscUtil.LazyInitStaticField(
                    typeof(Logger), "nullLog", delegate { return new Logger(""); });
            }
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
            Filters.Clear();
        }

        /// <summary>
        /// Adds a filter to allow logging from the given logger at any level.
        /// </summary>
        /// <param name="l">The logger to accept</param>
        [AlternateSignature]
        [Conditional("DEBUG")]
        public static extern void FilterByLogger(Logger l);

        /// <summary>
        /// Adds a filter to allow logging from the given logger at the specified level.
        /// </summary>
        /// <param name="validLogger">The logger to accept</param>
        /// <param name="minLogLevel">The minimum level to accept</param>
        [Conditional("DEBUG")]
        public static void FilterByLogger(Logger validLogger, LoggerLevel minLogLevel)
        {
            minLogLevel = ScriptEx.Value(minLogLevel, LoggerLevel.All);

            AddFilter(delegate(Logger l, LoggerLevel ll)
            {
                return l == validLogger && ll >= minLogLevel;
            });
        }

        /// <summary>
        /// Adds a filter to allow logging from the given type at any level.  Assumes
        /// that the type contains a static logger generated using <see cref="GetLogger(System.Type)"/>.
        /// </summary>
        /// <param name="t">The type used for creating the logger</param>
        [AlternateSignature]
        [Conditional("DEBUG")]
        public static extern void FilterByType(Type t);

        /// <summary>
        /// Adds a filter to allow logging from the given type at the specified level.  Assumes
        /// that the type contains a static logger generated using <see cref="GetLogger(System.Type)"/>.
        /// </summary>
        /// <param name="t">The type used for creating the logger</param>
        /// <param name="minLogLevel">The minimum level to accept</param>
        [Conditional("DEBUG")]
        public static void FilterByType(Type t, LoggerLevel minLogLevel)
        {
            minLogLevel = ScriptEx.Value(minLogLevel, LoggerLevel.All);

            AddFilter(delegate(Logger l, LoggerLevel ll)
            {
                return ll >= minLogLevel && l.Name == t.Name;
            });
        }

        /// <summary>
        /// Adds a filter to allow logging from a logger that matches the given pattern at any level.
        /// </summary>
        /// <param name="namePattern">A regular expression to match against the logger name</param>
        [AlternateSignature]
        [Conditional("DEBUG")]
        public static extern void FilterByName(string namePattern);

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
            Regex regex = new Regex(namePattern, "i");

            AddFilter(delegate(Logger l, LoggerLevel ll)
            {
                return ll >= minLogLevel && Script.IsValue(l.Name.Match(regex));
            });
        }

        /// <summary>
        /// Clears all appenders.
        /// </summary>
        public static void ClearAppenders()
        {
            Appenders.Clear();
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

            Appenders.Add(appender);
        }

        /// <summary>
        /// Removes a logging appender.
        /// </summary>
        /// <param name="appender">The appender to be removed</param>
        public static void RemoveAppender(ILogAppender appender)
        {
            Appenders.Remove(appender);
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
            return Script.Reinterpret<Logger>(MiscUtil.LazyInitStaticField(t, "_logger", delegate { return GetLogger(t); }));
        }

        /// <summary>
        /// Creates a new instance of a log for the given type.
        /// </summary>
        /// <param name="t">The type to create a log for</param>
        /// <returns>A new Log instance</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters"), AlternateSignature]
        public static extern Logger GetLogger(Type t);

        /// <summary>
        /// Creates a new instance of a log for the given type and includes a filter for the
        /// created logger at the given level.  This method should be used sparingly when
        /// debugging.
        /// </summary>
        /// <see cref="FilterByType(System.Type, LoggerLevel)"/>
        /// <param name="t">The type to create a log for</param>
        /// <param name="ll">The min </param>
        /// <returns>A new Log instance</returns>
        public static Logger GetLogger(Type t, LoggerLevel ll)
        {
            Logger l = GetLoggerWithName(t.Name);
            if (Script.IsValue(ll))
            {
                FilterByLogger(l, ll);
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
            JsDictionary<string, List<string>> queryParams = MiscUtil.GetUriQueryParameters(Window.Self.Location.Search);
            if (!queryParams.ContainsKey(LogQueryParam)) { return; }

            ClearFilters();

            List<string> logParams = queryParams[LogQueryParam];
            if (logParams.Count == 0)
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
            Filters.Add(filterFunc);

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
    }
}
