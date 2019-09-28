// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorTrace.cs" company="Tableau Software">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// Method used to collect the stack trace.
    /// </summary>
    [Imported]
    [NamedValues]
    public enum StackTraceMode
    {
        Stack,
        OnError,
        Failed
    }

    /// <summary>
    /// Saltarelle adaptation/minimization of TraceKit - Cross browser stack traces - http://github.com/occ/TraceKit
    /// We have modified it to not synchronously handle subscriptions from an arbitrary number of clients.
    /// We want, instead, to enqueue them for retrieval, so that we can process exceptions very early in
    /// initialization, without depending on higher level code.
    /// Original license MIT Public License
    ///
    /// Additional parts are ports from https://github.com/stacktracejs/error-stack-parser
    /// Original license "The Unlicense" http://unlicense.org/
    /// </summary>
    public static class ErrorTrace
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        // Configuration:

        private const bool ShouldReThrow = false;
        private const bool CollectWindowErrors = true;
        private static bool getStack;

        // Operational:
        private static JsArray<StackTrace> queuedTraces = new JsArray<StackTrace>();

        private static bool onErrorHandlerInstalled;
        private static ErrorEventHandler oldOnErrorHandler;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Install all handlers for unhandled exceptions.
        /// </summary>
        public static void Install()
        {
            string enabled = TsConfig.ClientErrorReportingLevel;
            if (!string.IsNullOrEmpty(enabled))
            {
                if (enabled == "debug")
                {
                    getStack = true;
                }
            }

            ExtendToAsynchronousCallback("setTimeout");
            ExtendToAsynchronousCallback("setInterval");
            // TFS 754591: RCA We need more robust global error handling in the vizclient.
            // For now, the CommandController will override the bootstrap error handler and call ErrorTrace directly.
            // InstallGlobalHandler();
        }

        /// <summary>
        /// Wrap any function in a reporter.
        /// Example: <code>func = ErrorTrace.Wrap(func);</code>
        /// </summary>
        /// <param name="func">The function to be wrapped.</param>
        /// <returns>The wrapped function.</returns>
        public static Func<object> Wrap(Action func)
        {
            return () =>
            {
                try
                {
                    return func.ReinterpretAs<Function>().Apply(Script.This, (object[])Arguments.ToArray());
                }
                catch (Exception e)
                {
                    Report(e);
                    throw;
                }
            };
        }

        /// <summary>
        /// Extends global error handling support for asynchronous browser operations.
        /// Derives from Closure's errorhandler.js.
        /// Helper function for protecting setTimeout/setInterval.
        /// functionName is the name of the function we're protecting. Must be setTimeout or setInterval.
        /// </summary>
        /// <param name="functionName"></param>
        private static void ExtendToAsynchronousCallback(string functionName)
        {
            var originalFunction = JsDictionary.GetDictionary(typeof(Window))[functionName]
                .ReinterpretAs<Func<object, object, object>>();
            Func<object> callback = delegate
            {
                // make a copy of the arguments
                object[] args = ((object[])Arguments.ToArray()).Clone();
                object originalCallback = args[0];

                // In the case of strings, we lose the wrapping.
                // Need to figure out a way to do a global eval similar to the goog.globalEval in closure
#if REMOVE_WHEN_SUPPORTED_IN_DESALT
                if (originalCallback is Function)
                {
                    args[0] = ErrorTrace.Wrap(originalCallback.ReinterpretAs<Action>());
                }
#endif

                return Script.Reinterpret<Function>(originalFunction).Apply(Script.This, args);
            };

            JsDictionary.GetDictionary(typeof(Window))[functionName] = callback;
        }

        /// <summary>
        /// Ensures all global unhandled exceptions are recorded.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="url">URL of the script that generated the exception</param>
        /// <param name="lineNo">The line number where the error occurred</param>
        /// <param name="column">The column number where the error occurred</param>
        /// <param name="error">Error from the javascript engine</param>
        public static bool WindowOnError(
            TypeOption<Event, string> message,
            string url,
            int lineNo,
            int column,
            object error)
        {
            return WindowOnError(message, url, lineNo, column, error, errorDialogShown: false);
        }

        /// <summary>
        /// Overload: Ensures all global unhandled exceptions are recorded.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="url">URL of the script that generated the exception</param>
        /// <param name="lineNo">The line number where the error occurred</param>
        /// <param name="column">The column number where the error occurred</param>
        /// <param name="error">Error from the javascript engine</param>
        /// <param name="errorDialogShown">Flag indicating if the error resulted in an error dialog being shown</param>
        public static bool WindowOnError(
            TypeOption<Event, string> message,
            string url,
            int lineNo,
            int column,
            object error,
            bool errorDialogShown)
        {
            JsArray<StackLocation> locations = StackTraceParser.ParseJsErrorForStackLines((Error)error);
            var stack = new StackTrace(StackTraceMode.OnError, (string)message, "window.onError", locations, errorDialogShown);
            queuedTraces.Push(stack);

            if (Script.IsValue(oldOnErrorHandler))
            {
                oldOnErrorHandler(message, url, lineNo, column, error);
            }

            return false;
        }

        /// <summary>
        /// Returns a StackTrace instance created from an exception.
        /// </summary>
        /// <param name="e">The exception to use for creating the StackTrace.</param>
        /// <param name="errorDialogShown">Flag indicating if the exception resulted in an error dialog being shown.</param>
        public static StackTrace GetStackTraceFor(Exception e, bool errorDialogShown = false)
        {
            var defaultTrace = new StackTrace(
                StackTraceMode.Stack,
                StackTraceParser.GetExceptionMessage(e),
                (string)((dynamic)e).name,
                null,
                errorDialogShown);

            if (getStack)
            {
                StackTrace stack = null;
                try
                {
                    stack = StackTraceParser.ComputeStackTraceFromStackTraceProp(e, errorDialogShown);
                }
                catch (Exception)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (ShouldReThrow)
#pragma warning disable 162
                    {
                        throw;
                    }
#pragma warning restore 162
                }

                if (Script.IsValue(stack))
                {
                    return stack;
                }
            }
            else
            {
                return defaultTrace;
            }

            defaultTrace.TraceMode = StackTraceMode.Failed;
            return defaultTrace;
        }

        /// <summary>
        /// Returns whether any stack traces have been reported and enqueued.
        /// </summary>
        public static bool HasTraces()
        {
            return queuedTraces.Length > 0;
        }

        /// <summary>
        /// Retrieves and clears all enqueued stack trace objects in one step.
        /// This assumes that exactly one listener is handling the policy for these unhandled exceptions.
        /// </summary>
        /// <returns>All pending stack trace objects.</returns>
        public static JsArray<StackTrace> DequeueTraces()
        {
            JsArray<StackTrace> traces = queuedTraces;
            queuedTraces = new JsArray<StackTrace>();
            return traces;
        }

        /// <summary>
        /// Replaces and wraps the existing <code>window.onerror</code> handler.
        /// This will avoid wrapping itself, so it can be called multiple times.
        /// </summary>
        public static void InstallGlobalHandler()
        {
            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (onErrorHandlerInstalled || !CollectWindowErrors)
            {
                return;
            }

            oldOnErrorHandler = Window.OnError;
            Window.OnError = ErrorTrace.WindowOnError;
            onErrorHandlerInstalled = true;
        }

        /// <summary>
        /// Cross-browser processing of unhandled exceptions.
        /// Enqueues them for later retrieval by a higher-level processor (like FailureHandler).
        /// Example: <code>try { } catch (Exception e) { ErrorTrace.Report(e); }</code>
        /// Retrieval: <code>JsArray&gt;StackTrace&lt; traces = ErrorTrace.DequeueTraces();</code>
        /// </summary>
        /// <param name="e">The exception to report.</param>
        /// <param name="errorDialogShown">Flag indicating if the exception resulted in an error dialog being shown.</param>
        public static void Report(Exception e, bool errorDialogShown = false)
        {
            StackTrace stack = GetStackTraceFor(e, errorDialogShown);
            queuedTraces.Push(stack);
        }
    }

    public sealed class StackLocation
    {
        public readonly URLStr Url;

        /// <summary>
        /// 1-indexed line number. This is what humans think of as line numbers
        /// </summary>
        public readonly int LineNo;

        public readonly int ColumnNo;
        public readonly string FunctionName;
        public readonly JsArray<string> Context;

        public StackLocation(
            string url,
            int lineNo,
            int colNo,
            string defaultContextLine = "",
            string functionName = null)
        {
            this.Url = url;
            this.LineNo = lineNo;
            this.ColumnNo = colNo;
            int zeroIndexedLineNumber = lineNo - 1;
            this.Context = SourceCacheForErrorStacks.GatherContext(this.Url, zeroIndexedLineNumber, colNo) ??
                new[] { defaultContextLine };
            this.FunctionName =
                functionName ?? SourceCacheForErrorStacks.GuessFunctionName(this.Url, zeroIndexedLineNumber);
        }
    }

    public sealed class StackTrace
    {
        public readonly string UserAgent = Window.Navigator.UserAgent;
        public readonly DisplayTextStr Message;
        public readonly URLStr Url;
        public StackTraceMode TraceMode;
        public readonly JsArray<StackLocation> Locations;
        public bool IsIncomplete = false;
        public readonly string Name;
        public readonly bool ErrorDialogShown;

        public StackTrace(
            StackTraceMode traceMode,
            DisplayTextStr message,
            string name,
            JsArray<StackLocation> locations = null,
            bool errorDialogShown = false)
        {
            this.TraceMode = traceMode;
            this.Message = message;
            this.Url = Document.URL;
            this.Name = name;
            this.Locations = locations ?? new JsArray<StackLocation>();
            this.ErrorDialogShown = errorDialogShown;
        }
    }

    /// <summary>
    /// A logger that creates a stack trace to aggregate and send to the server.
    /// </summary>
    public class StackTraceAppender : BaseLogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly LogAppenderInstance<StackTraceAppender> GlobalAppender =
            new LogAppenderInstance<StackTraceAppender>(() => new StackTraceAppender());

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

#if DEBUG

        static StackTraceAppender()
        {
            // by default we're going to enable server-side reporting for Warn+ in debug
            GlobalAppender.EnableLogging((_, loggerLevel) => loggerLevel > LoggerLevel.Info);
        }

#endif

        private StackTraceAppender()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================
        protected override void LogInternal(Logger source, LoggerLevel level, string message, object[] args)
        {
            message = this.FormatMessage(message.Replace(@"\n", "<br />"), args);
            if (level > LoggerLevel.Info)
            {
                try
                {
                    throw new Exception(
                        "Logged(" + Logger.LoggerLevelNames[(int)level] + ", from " + source.Name + "): " + message);
                }
                catch (Exception e)
                {
                    ErrorTrace.Report(e);
                }
            }
        }
    }

    public static class SourceCacheForErrorStacks
    {
        private const string UnknownFunctionName = "?";

        private static readonly JsDictionary<URLStr, JsArray<string>> SourceCache =
            new JsDictionary<URLStr, JsArray<string>>();

        private const bool RemoteFetching = true;

        /// <summary>
        /// Retrieves source code from the source code cache.
        /// </summary>
        /// <param name="url">URL of source code.</param>
        /// <returns>Source contents.</returns>
        internal static JsArray<string> GetSource(URLStr url)
        {
            if (Script.IsNullOrUndefined(url))
            {
                return new JsArray<string>();
            }

            if (!SourceCache.ContainsKey(url))
            {
                string source = "";
                if (((string)url).IndexOf(Document.Domain) > -1)
                {
                    source = LoadSource(url);
                }

                SourceCache[url] = string.IsNullOrEmpty(source)
                    ? new JsArray<string>()
                    : (JsArray<string>)source.Split("\n");
            }

            return SourceCache[url];
        }

        internal static string GuessFunctionName(URLStr url, int lineNo)
        {
            var functionArgNames = new Regex("function ([^(]*)\\(([^)]*)\\)");
            var guessFunction = new Regex("['\"]?([0-9A-Za-z$_]+)['\"]?\\s*[:=]\\s*(function|eval|new Function)");
            string line = "";
            const int MaxLines = 10;
            JsArray<string> source = GetSource(url);

            if (source.Length == 0)
            {
                return UnknownFunctionName;
            }

            for (int i = 0; i < MaxLines; i++)
            {
                line = source[lineNo] + line;

                if (!string.IsNullOrEmpty(line))
                {
                    string[] matches = guessFunction.Exec(line);
                    if (Script.IsValue(matches) && matches.Length > 0)
                    {
                        return matches[1];
                    }

                    matches = functionArgNames.Exec(line);
                    if (Script.IsValue(matches) && matches.Length > 0)
                    {
                        return matches[1];
                    }
                }
            }

            return UnknownFunctionName;
        }

        [SuppressMessage(
            "Microsoft.Usage",
            "CA1801:ReviewUnusedParameters",
            MessageId = "colPos",
            Justification = "the column position is used in release code, but not debug")]
        internal static JsArray<string> GatherContext(URLStr url, int lineNo, int colPos)
        {
            JsArray<string> source = GetSource(url);

            if (Script.IsNullOrUndefined(source) || source.Length == 0)
            {
                return null;
            }

            var context = new JsArray<string>();

#if DEBUG
            // in debug "context" is lines above and below
            const uint LinesOfContext = 3;

            int linesBefore = Math.Floor(LinesOfContext / 2.0);
            long linesAfter = linesBefore + (LinesOfContext % 2);
            int start = Math.Max(0, lineNo - linesBefore);
            int end = (int)Math.Min(source.Length, lineNo + linesAfter);

            for (int i = start; i < end; i++)
            {
                if (!string.IsNullOrEmpty(source[i]))
                {
                    context.Push(source[i]);
                }
            }
#else
            // in release "context" is text left and right, since the minifier put all the code on one line.
            const int TextColumnsContext = 60;

            string sourceLine = source[lineNo];
            int start = Math.Max(0, colPos - TextColumnsContext);
            int end = Math.Min(colPos + TextColumnsContext, sourceLine.Length);
            context.Push(sourceLine.Substring(start, end - start));
#endif

            return context;
        }

        /// <summary>
        /// Attempts to retrieve source code via XmlHttpRequest, which is used to look up anonymous function names.
        /// </summary>
        /// <param name="url">URL of source code.</param>
        /// <returns>Source contents.</returns>
        private static string LoadSource(URLStr url)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!RemoteFetching)
#pragma warning disable 162
            {
                return "";
            }
#pragma warning restore 162

            try
            {
                var srcRequest = new XmlHttpRequest();
                srcRequest.Open("GET", url, false);
                srcRequest.Send("");
                return srcRequest.ResponseText;
            }
            catch
            {
                return "";
            }
        }
    }

    internal static class StackTraceParser
    {
        private static readonly Regex SafariNativeCodeRegexp = new Regex(@"^(eval@)?(\[native code\])?$");
        private static readonly Regex ChromeIEStackRegexp = new Regex(@"^\s*at .*(\S+\:\d+|\(native\))", "m");
        private static readonly Regex ThrowAwayEvalRegexp = new Regex(@"(\(eval at [^\()]*)|(\)\,.*$)");
        private static readonly Regex ExtractLocationRegexp = new Regex(@"(.+?)(?:\:(\d+))?(?:\:(\d+))?$");
        private static readonly Regex ExtractLocationUrlLikeRegexp = new Regex(@"[\(\)]", "g");

        internal static StackTrace ComputeStackTraceFromStackTraceProp(Exception e, bool errorDialogShown = false)
        {
            JsArray<StackLocation> locations = ParseException(e);
            var stack = new StackTrace(
                StackTraceMode.Stack,
                GetExceptionMessage(e),
                (string)((dynamic)e).name,
                locations,
                errorDialogShown);
            return stack;
        }

        internal static string GetExceptionMessage(Exception e)
        {
            string errorMessage = e.Message;
            if (e.InnerException != null)
            {
                errorMessage += " inner: " + e.InnerException.Message;
            }

            return errorMessage;
        }

        /// <summary>
        /// Get the js error from the exception, delegate it to a browser dependent helper, and
        /// parse the error into our stack location data structure.
        /// Most of this comes as a simplified port from https://github.com/stacktracejs/error-stack-parser/blob/master/dist/error-stack-parser.js
        /// </summary>
        private static JsArray<StackLocation> ParseException(Exception e)
        {
            Error err;
            // 2016/12/28 - The interesting stuff (i.e. the stack for the real error, not the wrapping error) is on
            // e.InnerException.error on many browsers I tried (win chrome 54, IE 11, win FF 49, Safari 10).
            if (e.InnerException != null)
            {
                err = ((dynamic)e.InnerException).error;
            }
            else
            {
                err = ((dynamic)e)._error;
            }

            return ParseJsErrorForStackLines(err);
        }

        internal static JsArray<StackLocation> ParseJsErrorForStackLines(Error err)
        {
            if (err == null || err.Stack == null)
            {
                return null;
            }

            if (err.Stack.Match(ChromeIEStackRegexp) != null)
            {
                return ParseChromeOrIE(err);
            }

            return ParseFirefoxOrSafari(err);
        }

        private static JsArray<StackLocation> ParseChromeOrIE(Error error)
        {
            string[] filtered =
                error.Stack.Split('\n').Filter((string line) => line.Match(ChromeIEStackRegexp) != null);

            return filtered.Map(
                (string line) =>
                {
                    if (line.IndexOf("(eval ") > -1)
                    {
                        // Throw away eval information until we implement stacktrace.js/stackframe#8
                        line = line.Replace("eval code", "eval").Replace(ThrowAwayEvalRegexp, "");
                    }

                    JsArray<string> tokens = line.Replace(new Regex(@"^\s+"), "")
                        .Replace(new Regex(@"\(eval code"), "(").Split(new Regex(@"\s+")).Slice(1);
                    string[] locationParts = ExtractLocation(tokens.Pop());
                    string functionName = tokens.Join(" ") ?? "undefined";
                    string fileName = new[] { "eval", "<anonymous>" }.IndexOf(locationParts[0]) > -1
                        ? "undefined"
                        : locationParts[0];

                    int lineNum = Script.ParseInt(locationParts[1]).ReinterpretAs<int>();
                    int colNum = Script.ParseInt(locationParts[2]).ReinterpretAs<int>();

                    return new StackLocation(fileName, lineNum, colNum, line, functionName);
                });
        }

        private static JsArray<StackLocation> ParseFirefoxOrSafari(Error error)
        {
            string[] filtered = error.Stack.Split('\n')
                .Filter((string line) => line.Match(SafariNativeCodeRegexp) == null);

            return filtered.Map(
                (string line) =>
                {
                    // Throw away eval information until we implement stacktrace.js/stackframe#8
                    if (line.IndexOf(" > eval") > -1)
                    {
                        line = line.Replace(
                            new Regex(@" line (\d+)(?: > eval line \d+)* > eval\:\d+\:\d+", "g"),
                            ":$1");
                    }

                    if (line.IndexOf("@") == -1 && line.IndexOf(":") == -1)
                    {
                        // Safari eval frames only have function names and nothing else
                        return new StackLocation(line, 0, 0);
                    }
                    else
                    {
                        JsArray<string> tokens = line.Split("@");
                        string[] locationParts = ExtractLocation(tokens.Pop());
                        string functionName = tokens.Join("@") ?? "undefined";

                        string fileName = locationParts[0];
                        int lineNum = Script.ParseInt(locationParts[1]).ReinterpretAs<int>();
                        int colNum = Script.ParseInt(locationParts[2]).ReinterpretAs<int>();
                        var stackFrame = new StackLocation(fileName, lineNum, colNum, line, functionName);

                        return stackFrame;
                    }
                });
        }

        private static string[] ExtractLocation(string urlLike)
        {
            // Fail-fast but return locations like "(native)"
            if (urlLike.IndexOf(':') == -1)
            {
                return new[] { urlLike };
            }

            var parts = ExtractLocationRegexp.Exec(urlLike.Replace(ExtractLocationUrlLikeRegexp, ""));
            return new[] { parts[1], parts[2] ?? "undefined", parts[3] ?? "undefined" };
        }
    }
}
