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
    using System.Diagnostics;
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
        StackTrace,
        MultiLine,
        Callers,
        OnError,
        Failed
    }

    /// <summary>
    /// Saltarelle adaptation/minimization of TraceKit - Cross browser stack traces - http://github.com/occ/TraceKit
    /// We have modified it to not synchronously handle subscriptions from an arbitrary number of clients.
    /// We want, instead, to enqueue them for retrieval, so that we can process exceptions very early in
    /// initialization, without depending on higher level code.
    /// Original license MIT Public License
    /// </summary>
    public sealed class ErrorTrace
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal const string UnknownFunctionName = "?";

        // Configuration:

        internal static bool ShouldReThrow = false;
        internal static bool RemoteFetching = true;
        internal static bool CollectWindowErrors = true;
        internal static uint LinesOfContext = 3;
        internal static bool GetStack = false;

        // Operational:

        internal static StackTrace LastExceptionStack;
        internal static Exception LastException;
        private static readonly JsDictionary<URLStr, JsArray<string>> SourceCache = new JsDictionary<URLStr, JsArray<string>>();
        private static JsArray<StackTrace> queuedTraces = new JsArray<StackTrace>();
        private static bool onErrorHandlerInstalled = false;
        private static ErrorEventHandler oldOnErrorHandler = null;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private ErrorTrace()
        {
        }

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
                    GetStack = true;
                }
            }

            ExtendToAsynchronousCallback("setTimeout");
            ExtendToAsynchronousCallback("setInterval");
            InstallGlobalHandler();
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
                    return func.As<Function>().Apply(Script.This, Arguments.ToArray());
                }
                catch (Exception e)
                {
                    Report(e, false);
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
                .As<Func<object, object, object>>();
            Func<object> callback = delegate
            {
                // make a copy of the arguments
                object[] args = ((object[])Arguments.ToArray()).Clone();
                object originalCallback = args[0];

                // In the case of strings, we lose the wrapping.
                // Need to figure out a way to do a global eval similar to the goog.globalEval in closure
                if (originalCallback.GetType() == typeof(Function))
                {
                    args[0] = ErrorTrace.Wrap((Action)originalCallback);
                }
                // IE < 9 doesn't support .call/.apply on setInterval/setTimeout, but it also only
                // supports 2 arguments and doesn't care what "this" is, so we can just call the
                // original function directly.
                if (Script.In(originalFunction, "apply"))
                {
                    return Script.Reinterpret<Function>(originalFunction).Apply(Script.This, args);
                }
                else
                {
                    return originalFunction(args[0], args[1]);
                }
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
        /// <param name="error">Not used currently.</param>
        public static bool WindowOnError(TypeOption<Event, string> message, string url, int lineNo, int column, object error)
        {
            StackTrace stack;
            if (Script.IsValue(LastExceptionStack))
            {
                AugmentStackTraceWithInitialElement(LastExceptionStack, url, lineNo, (string)message);
                stack = LastExceptionStack;
                LastExceptionStack = null;
                LastException = null;
            }
            else
            {
                StackLocation location = new StackLocation(url, lineNo);
                location.FunctionName = GuessFunctionName(location);
                location.Context = GatherContext(location);

                stack = new StackTrace(StackTraceMode.OnError, (string)message);
                stack.Name = "window.onError";
                stack.Locations = new JsArray<StackLocation>(location);
            }

            queuedTraces.Push(stack);

            if (Script.IsValue(oldOnErrorHandler))
            {
                Script.Reinterpret<Function>(oldOnErrorHandler).Apply(Script.This, (object[])Arguments.ToArray());
            }

            return false;
        }

        #region ComputeStackTrace

        /// <summary>
        /// Adds information about the first frame to incomplete stack traces.
        /// Safari and IE require this to get complete data on the first frame
        /// </summary>
        /// <returns>Whether the stack trace was successfully augmented.</returns>
        internal static bool AugmentStackTraceWithInitialElement(StackTrace stack, URLStr url, int lineNo, string message)
        {
            StackLocation initial = new StackLocation(url, lineNo);

            if (Script.IsValue(initial.Url) && Script.IsValue(initial.LineNo))
            {
                stack.IsIncomplete = false;

                if (Script.IsNullOrUndefined(initial.FunctionName))
                {
                    initial.FunctionName = GuessFunctionName(initial);
                }

                if (Script.IsNullOrUndefined(initial.Context))
                {
                    initial.Context = GatherContext(initial);
                }

                string[] reference = message.Match(new Regex(" '([^']+)' "));
                if (Script.IsValue(reference) && reference.Length > 1)
                {
                    initial.ColumnNo = FindSourceInLine(reference[1], initial);
                }

                if (Script.IsValue(stack.Locations) && stack.Locations.Length > 0)
                {
                    StackLocation top = stack.Locations[0];
                    if (top.Url == initial.Url)
                    {
                        if (top.LineNo == initial.LineNo)
                        {
                            return false; // already in stack trace
                        }
                        else if (Script.IsNullOrUndefined(top.LineNo) && top.FunctionName == initial.FunctionName)
                        {
                            top.LineNo = initial.LineNo;
                            top.Context = initial.Context;
                            return false;
                        }
                    }
                }

                stack.Locations.Unshift(initial);
                stack.IsPartial = true;
                return true;
            }
            else
            {
                stack.IsIncomplete = true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to retrieve source code via XmlHttpRequest, which is used to look up anonymous function names.
        /// </summary>
        /// <param name="url">URL of source code.</param>
        /// <returns>Source contents.</returns>
        private static string LoadSource(URLStr url)
        {
            if (!RemoteFetching)
            {
                return "";
            }
            try
            {
                XmlHttpRequest srcRequest = new XmlHttpRequest();
                srcRequest.Open("GET", url, false);
                srcRequest.Send("");
                return srcRequest.ResponseText;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Retrieves source code from the source code cache.
        /// </summary>
        /// <param name="url">URL of source code.</param>
        /// <returns>Source contents.</returns>
        private static JsArray<string> GetSource(URLStr url)
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
                SourceCache[url] = string.IsNullOrEmpty(source) ? new JsArray<string>() : (JsArray<string>)source.Split("\n");
            }
            return SourceCache[url];
        }

        /// <summary>
        /// Determines at which column a code fragment occurs in a line of source code.
        /// </summary>
        private static int FindSourceInLine(string fragment, StackLocation location)
        {
            JsArray<string> source = GetSource(location.Url);
            Regex re = new Regex("\\b" + EscapeRegexp(fragment) + "\\b");

            if (Script.IsValue(source) && source.Length > location.LineNo)
            {
                RegexMatch matches = re.Exec(source[location.LineNo]);
                if (Script.IsValue(matches))
                {
                    return matches.Index;
                }
            }

            return -1;
        }

        internal static string GuessFunctionName(StackLocation location)
        {
            Regex functionArgNames = new Regex("function ([^(]*)\\(([^)]*)\\)");
            Regex guessFunction = new Regex("['\"]?([0-9A-Za-z$_]+)['\"]?\\s*[:=]\\s*(function|eval|new Function)");
            string line = "";
            int maxLines = 10;
            JsArray<string> source = GetSource(location.Url);

            if (source.Length == 0)
            {
                return UnknownFunctionName;
            }

            for (int i = 0; i < maxLines; i++)
            {
                line = source[location.LineNo - 1] + line;

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

        internal static JsArray<string> GatherContext(StackLocation location)
        {
            JsArray<string> source = GetSource(location.Url);

            if (Script.IsNullOrUndefined(source) || source.Length == 0)
            {
                return null;
            }

            JsArray<string> context = new JsArray<string>();
            int linesBefore = Math.Floor(LinesOfContext / 2.0);
            long linesAfter = linesBefore + (LinesOfContext % 2);
            int start = Math.Max(0, location.LineNo - linesBefore - 1);
            int end = (int)Math.Min(source.Length, location.LineNo + linesAfter - 1);

            location.LineNo -= 1; // convert to 0-based index

            for (int i = start; i < end; i++)
            {
                if (!string.IsNullOrEmpty(source[i]))
                {
                    context.Push(source[i]);
                }
            }

            return context;
        }

        internal static string EscapeRegexp(string input)
        {
            return input.Replace(new Regex("[\\-\\[\\]{}()*+?.,\\\\\\^$|#]", "g"), "\\\\$&");
        }

        internal static string EscapeCodeAsRegexpForMatchingInsideHTML(string body)
        {
            return EscapeRegexp(body)
                .Replace("<", "(?:<|&lt;)")
                .Replace(">", "(?:>|&gt;)")
                .Replace("&", "(?:&|&amp;)")
                .Replace("\"", "(?:\"|&quot;)")
                .Replace(new Regex("\\\\s+", "g"), "\\\\s+");
        }

        internal static StackLocation FindSourceInUrls(Regex re, JsArray<URLStr> urls)
        {
            foreach (URLStr url in urls)
            {
                JsArray<string> source = GetSource(url);
                if (Script.IsValue(source) && source.Length > 0)
                {
                    for (int lineNo = 0; lineNo < source.Length; lineNo++)
                    {
                        RegexMatch matches = re.Exec(source[lineNo]);
                        if (Script.IsValue(matches) && matches.Length > 0)
                        {
                            StackLocation location = new StackLocation(url, lineNo);
                            location.ColumnNo = matches.Index;
                            return location;
                        }
                    }
                }
            }

            return null;
        }

        internal static StackTrace GetStackTraceFor(Exception e)
        {
            StackTrace defaultTrace = new StackTrace(StackTraceMode.Stack, e.Message);
            defaultTrace.Name = (string)((dynamic)e).name;

            if (GetStack)
            {
                JsArray<Func<Exception, StackTrace>> stackTraceComputers = new JsArray<Func<Exception, StackTrace>>();

                stackTraceComputers.Push(ComputeStackTraceFromStackTraceProp);
                //stackTraceComputers.Push(ComputeStackTraceFromOperaMultiLineMessage);
                stackTraceComputers.Push(ComputeStackTraceByWalkingCallerChain);

                foreach (Func<Exception, StackTrace> stackTraceComputer in stackTraceComputers)
                {
                    StackTrace stack = null;
                    try
                    {
                        stack = stackTraceComputer(e);
                    }
                    catch (Exception inner)
                    {
                        if (ShouldReThrow)
                        {
                            throw inner;
                        }
                    }
                    if (Script.IsValue(stack))
                    {
                        return stack;
                    }
                }
            }
            else
            {
                return defaultTrace;
            }

            defaultTrace.TraceMode = StackTraceMode.Failed;
            return defaultTrace;
        }

        [ScriptName("computeStackTraceByWalkingCallerChain")]
        internal static StackTrace ComputeStackTraceByWalkingCallerChain(Exception e)
        {
            Error err = ((dynamic)e)._error;

            Regex functionName = new Regex("function\\s+([_$a-zA-Z\x00a0-\xFFFF][_$a-zA-Z0-9\x00a0-\xFFFF]*)?\\s*\\(", "i");
            JsArray<StackLocation> locations = new JsArray<StackLocation>();
            JsDictionary<string, bool> funcs = new JsDictionary<string, bool>();
            bool recursion = false;
            dynamic curr = null;

            for (curr = ((dynamic)typeof(ErrorTrace)).computeStackTraceByWalkingCallerChain.caller;
                 Script.IsValue(curr) && !recursion;
                 curr = curr.caller)
            {
                if (curr == typeof(ErrorTrace))
                {
                    // Skipping internal function
                    continue;
                }
                string functionText = curr.toString();
                StackLocation item = new StackLocation(null, 0);

                if (Script.IsValue(curr.name))
                {
                    item.FunctionName = (string)curr.name;
                }
                else
                {
                    string[] parts = functionName.Exec(functionText);
                    if (Script.IsValue(parts) && parts.Length > 1)
                    {
                        item.FunctionName = parts[1];
                    }
                }

                StackLocation source = FindSourceByFunctionBody(curr);
                if (Script.IsValue(source))
                {
                    item.Url = source.Url;
                    item.LineNo = source.LineNo;

                    if (item.FunctionName == UnknownFunctionName)
                    {
                        item.FunctionName = GuessFunctionName(item);
                    }

                    RegexMatch reference = new Regex(" '([^']+)' ")
                        .Exec(ScriptEx.Value(e.Message, TypeUtil.GetField<string>(e, "description")));
                    if (Script.IsValue(reference) && reference.Length > 1)
                    {
                        item.ColumnNo = FindSourceInLine(reference[1], source);
                    }
                }

                if (funcs.ContainsKey(functionText))
                {
                    recursion = true;
                }
                else
                {
                    funcs[functionText] = true;
                }

                locations.Push(item);
            }

            StackTrace result = new StackTrace(StackTraceMode.Callers, e.Message);
            result.Name = (string)err.GetData("name");
            result.Locations = locations;
            AugmentStackTraceWithInitialElement(result,
                (string)ScriptEx.Value(err.GetData("sourceURL"), err.GetData("fileName")),
                (int)ScriptEx.Value(err.GetData("line"), err.GetData("lineNumber")),
                (string)ScriptEx.Value(e.Message, err.GetData("description")));
            return result;
        }

        private static StackLocation FindSourceByFunctionBody(object func)
        {
            JsArray<URLStr> urls = new JsArray<URLStr>(Window.Location.Href);
            ElementCollection scripts = Document.GetElementsByTagName("script");
            string code = func.ToString();
            Regex codeMatcher = new Regex("");
            Regex matcher;

            for (int i = 0; i < scripts.Length; i++)
            {
                Element script = (Element)scripts[i];
                if (script.HasAttribute("src") && Script.IsValue(script.GetAttribute("src")))
                {
                    urls.Push((string)script.GetAttribute("src"));
                }
            }

            RegexMatch parts = codeMatcher.Exec(code);
            if (Script.IsValue(parts) && parts.Length > 0)
            {
                matcher = new Regex(EscapeRegexp(code).Replace(new Regex("\\s+", "g"), "\\\\s+"));
            }
            else
            {
                string name = parts.Length > 1 ? @"\\s+" + parts[1] : "";
                string args = parts[2].Split(",").Join(@"\\s*,\\s*");

                string body = EscapeRegexp(parts[3]).Replace(new Regex(";$"), ";?");
                matcher = new Regex("function" + name + @"\\s*\\(\\s*" + args + @"\\s*\\)\\s*{\\s*" + body + @"\\s*}");
            }

            // look for a normal function definition
            StackLocation result = FindSourceInUrls(matcher, urls);
            if (Script.IsValue(result))
            {
                return result;
            }

            // TODO finish porting
            //Regex eventMatches = new Regex("");

            return null;
        }

        private static StackTrace ComputeStackTraceFromStackTraceProp(Exception e)
        {
            Error err = ((dynamic)e)._error;
            if (Script.IsNullOrUndefined(err) || Script.IsNullOrUndefined(err.Stack))
            {
                return null;
            }

            Regex chromeMatcher = new Regex(@"^\s*at (?:((?:\[object object\])?\S+(?: \[as \S+\])?) )?\(?((?:file|http|https):.*?):(\d+)(?::(\d+))?\)?\s*$", "i");
            Regex geckoMatcher = new Regex(@"^\s*(\S*)(?:\((.*?)\))?@((?:file|http|https).*?):(\d+)(?::(\d+))?\s*$", "i");
            Regex matcher = BrowserSupport.IsFF ? geckoMatcher : chromeMatcher;
            JsArray<string> lines = ((string)err.GetData("stack")).Split("\n");
            JsArray<StackLocation> locations = new JsArray<StackLocation>();
            RegexMatch reference = new Regex("^(.*) is undefined").Exec(e.Message);

            foreach (string line in lines)
            {
                RegexMatch parts = matcher.Exec(line);
                if (Script.IsValue(parts) && parts.Length >= 5)
                {
                    string functionName = parts[1];
                    string url = parts[2];
                    string lineNumStr = parts[3];
                    string colNumStr = parts[4];
                    StackLocation element = new StackLocation(url, int.Parse(lineNumStr));
                    if (Script.IsValue(functionName))
                    {
                        element.FunctionName = functionName;
                    }
                    if (Script.IsValue(colNumStr))
                    {
                        element.ColumnNo = int.Parse(colNumStr);
                    }

                    if (Script.IsValue(element.LineNo))
                    {
                        if (Script.IsNullOrUndefined(element.FunctionName))
                        {
                            element.FunctionName = GuessFunctionName(element);
                        }

                        element.Context = GatherContext(element);
                    }

                    locations.Push(element);
                }
            }

            if (locations.Length > 0 && Script.IsValue(locations[0].LineNo) &&
                Script.IsNullOrUndefined(locations[0].ColumnNo) && Script.IsValue(reference) && reference.Length > 1)
            {
                locations[0].ColumnNo = FindSourceInLine(reference[1], locations[0]);
            }

            if (locations.Length == 0)
            {
                return null;
            }

            StackTrace stack = new StackTrace(StackTraceMode.Stack, e.Message);
            stack.Name = (string)((dynamic)e).name;
            stack.Locations = locations;
            return stack;
        }

        #endregion ComputeStackTrace

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
        /// <param name="rethrow">Whether to throw the exception again when done processing it (originally a default of true).</param>
        public static void Report(Exception e, bool rethrow)
        {
            if (Script.IsNullOrUndefined(rethrow))
            {
                rethrow = true;
            }
            if (Script.IsValue(LastExceptionStack))
            {
                if (LastException == e)
                {
                    return; // Already caught by an inner catch block, ignore
                }
                else
                {
                    StackTrace s = LastExceptionStack;
                    LastExceptionStack = null;
                    LastException = null;
                    queuedTraces.Push(s);
                }
            }

            StackTrace stack = GetStackTraceFor(e);
            LastExceptionStack = stack;
            LastException = e;

            // If the stack trace is incomplete, wait for 2 seconds for
            // slow slow IE to see if onerror occurs or not before reporting
            // this exception; otherwise, we will end up with an incomplete
            // stack trace
            Window.SetTimeout(() =>
            {
                if (LastException == e)
                {
                    LastExceptionStack = null;
                    LastException = null;
                    queuedTraces.Push(stack);
                }
            }, (stack.IsIncomplete ? 2000 : 0));

            if (rethrow)
            {
                throw e; // re-throw to propagate to the top level (and cause window.onerror)
            }
        }
    }

    public sealed class StackLocation
    {
        public URLStr Url;
        public int LineNo = 0;
        public int ColumnNo = 0;
        public string FunctionName = ErrorTrace.UnknownFunctionName;
        public JsArray<string> Context;

        public StackLocation(string url, int lineNo)
        {
            this.Url = url;
            this.LineNo = lineNo;
        }
    }

    public sealed class StackTrace
    {
        public readonly string UserAgent = Window.Navigator.UserAgent;
        public StackTraceMode TraceMode = StackTraceMode.OnError;
        public DisplayTextStr Message;
        public URLStr Url;
        public JsArray<StackLocation> Locations;
        public bool IsIncomplete = false;
        public bool IsPartial = false;
        public string Name;

        public StackTrace(StackTraceMode traceMode, DisplayTextStr message)
        {
            this.TraceMode = traceMode;
            this.Message = message;
            this.Url = Document.URL;
            this.Locations = new JsArray<StackLocation>();
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

        private static StackTraceAppender globalAppender;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

#if DEBUG

        static StackTraceAppender()
        {
            // by default we're going to enable server-side reporting for Warn+ in debug
            EnableLogging(delegate(Logger l, LoggerLevel ll)
            {
                return ll > LoggerLevel.Info;
            });
        }

#endif

        private StackTraceAppender()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Enables logging using this appender type.
        /// </summary>
        /// <param name="filter">The filter to apply to this appender or <c>null</c> to enable for all loggers</param>
        [Conditional("DEBUG")]
        public static void EnableLogging(Func<Logger, LoggerLevel, bool> filter)
        {
            if (Script.IsNullOrUndefined(globalAppender))
            {
                globalAppender = new StackTraceAppender();
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
            if (Script.IsValue(globalAppender))
            {
                Logger.RemoveAppender(globalAppender);
                globalAppender = null;
            }
        }

        protected override void LogInternal(Logger source, LoggerLevel level, string message, object[] args)
        {
            message = this.FormatMessage(message.Replace(@"\n", "<br />"), args);
            if (level > LoggerLevel.Info)
            {
                try
                {
                    throw new Exception("Logged(" + Logger.LoggerLevelNames[(int)level] + ", from " + source.Name + "): " + message);
                }
                catch (Exception e)
                {
                    ErrorTrace.Report(e, false);
                }
            }
        }
    }
}
