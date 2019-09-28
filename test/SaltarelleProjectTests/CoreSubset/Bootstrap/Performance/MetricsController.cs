// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsController.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
    using Tableau.JavaScript.Vql.TypeDefs;

    // $NOTE -- if you add an item to the enum below, ensure you add an entry
    // to the lookup table MetricsController.suiteNameLookup
    // Suppress the "usage" warning, because not all possible flag values exist.
    [Flags]
    [SuppressMessage("Microsoft.Usage", "CA2217")]
    public enum MetricsSuites
    {
        None = 0x0000, // No metrics (disable reporting)

        // $NOTE-jking: in general, you should not be adding new metrics using the
        // following group of suite names.  These have been carefully selected to provide
        // data but not to overwhelm the server if/when we report them back
        Navigation = 0x0001, // Window.performance.navigation timings
        Bootstrap = 0x0002, // Bootstrap stages/timings
        Commands = 0x0004, // Local+remote execution of commands
        // Unused   = 0x0008, // Currently Unused. Included for completeness
        Rendering = 0x0010, // Rendering of viz panetable

        // In general, YOU SHOULD USE THIS IF YOU ADD NEW METRICS
        Debug = 0x0020, // Generic, debugging metrics.

        Toolbar = 0x0040, // Toolbar events
        Fonts = 0x0080, // Font metrics
        HitTest = 0x0100, // Hit-testing/responsive feedback
        Maps = 0x0200, // Maps metrics
        Exporting = 0x0400, // Exporting metrics

        // Combination suites
        // $NOTE-jking: if you're thinking about modifying these, you probably shouldn't :)
        Min = 0x0003, // Navigation | Bootstrap
        Core = 0x000F, // Min | Commands | Unused

        // report everything/DOS the server!
        All = 0xFFFF, // All metrics/no filtering
    }

    [Imported]
    [NamedValues]
    public enum MetricsEventType
    {
        [ScriptName("nav")]
        Navigation,

        [ScriptName("wp")]
        ContextEnd,

        [ScriptName("gen")]
        Generic,

        [ScriptName("init")]
        SessionInit,
    }

    internal class LocalWebClientMetricsLogger : IWebClientMetricsLogger
    {
        private const string AppStartMarker = "AppStartEpochMarker";

        public void LogEvent(MetricsEvent evt)
        {
            // Chrome won't show just marks, so fire off 0 time measures
            string desc = BuildDescriptionName(evt.Parameters.Description, evt.Parameters.ExtraInfo);
            string startMarkName;
            if (evt.MetricSuite == MetricsSuites.Bootstrap && Script.IsValue(evt.Parameters.Elapsed))
            {
                startMarkName = AppStartMarker;
            }
            else
            {
                startMarkName = LogLocalMetricStart(desc);
            }
            LogLocalMetricEnd(desc, startMarkName);
        }

        internal static string LogLocalMetricStart(string metricName)
        {
            string startMarkName = BuildStartName(metricName);
            PerformanceReporting.Mark(startMarkName);
            return startMarkName;
        }

        internal static void MarkAppStart()
        {
            PerformanceReporting.Mark(AppStartMarker);
        }

        internal static void LogLocalMetricEnd(string metricName, string startMarkName = null)
        {
            string endMarkName = BuildEndName(metricName);
            startMarkName = startMarkName ?? BuildStartName(metricName);

            PerformanceReporting.Mark(endMarkName);
            // The "Heavy Greek Cross" symbol is designed to make the measure names clear they are comming from Tableau code
            // React uses ⚛ for the same reason
            PerformanceReporting.Measure("✚ " + metricName, startMarkName, endMarkName);
        }

        internal static string BuildDescriptionName(string message, string extraInfo)
        {
            message = MetricsController.GetFriendlyEventDescription(message);
            if (extraInfo != null)
            {
                message += " " + extraInfo;
            }
            return message;
        }

        private static string BuildStartName(string desc)
        {
            return "__start__" + desc;
        }

        private static string BuildEndName(string desc)
        {
            return "__end__" + desc;
        }
    }

    /// <summary>
    /// Singleton class for measuring and logging code performance
    /// </summary>
    public class MetricsController
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly JsDictionary<ClientMetric, string> FullMetricNameLookup = new JsDictionary<ClientMetric, string>(
                    ClientMetric.BTSTRP, "Bootstrap Request",
                    ClientMetric.PROPRI, "Process Primary Payload",
                    ClientMetric.PROSEC, "Process Secondary Payload",
                    ClientMetric.MDLINI, "Initialize Models",
                    ClientMetric.MDLEVT, "Handle Model Events",
                    ClientMetric.EXELOC, "Execute Local Command",
                    ClientMetric.EXEREM, "Execute Remote Command",
                    ClientMetric.PROLOC, "Process Local Command Response",
                    ClientMetric.PROREM, "Process Remote Command Response",
                    ClientMetric.RNDRPT, "Render Panetable",
                    ClientMetric.RNDRRG, "Render Region",
                    ClientMetric.RTCONV, "Runtime model presmodel conversion",
                    ClientMetric.CLNTLD, "Client Loaded",
                    ClientMetric.APPSTR, "Application Startup",
                    ClientMetric.APPINT, "Application Interactive",
                    ClientMetric.ALLZNS, "All Zones Loaded",
                    ClientMetric.TBRLAY, "Toolbar Layout",
                    ClientMetric.TBRHNT, "Toolbar HandleNewToolbar",
                    ClientMetric.TBRADD, "Toolbar AddToolbar",
                    ClientMetric.TBRHRE, "Toolbar HandleResize",
                    ClientMetric.MDLOAD, "Load js async",
                    ClientMetric.EMLOAD, "Emscripten load",
                    ClientMetric.RTLOAD, "Runtime load",
                    ClientMetric.RTLPRC, "Runtime command local processing");

        public static Func<double> GetTiming;
        public static readonly double RecordingStart;
        internal static readonly bool ReportLocalMetrics;
        private static readonly JsDictionary<string, MetricsSuites> SuiteNameLookup;
        private static readonly double AppStartEpoch;
        private static MetricsController instance;
        private int nextContextID = 0;
        private readonly JsArray<MetricsContext> contextStack = new JsArray<MetricsContext>();
        private JsArray<MetricsEvent> eventBuffer = new JsArray<MetricsEvent>();
        private JsArray<IWebClientMetricsLogger> eventLoggers = new JsArray<IWebClientMetricsLogger>();
        private static readonly IWebClientMetricsLogger LocalEventLogger = new LocalWebClientMetricsLogger();

        private string sessionId = "";
        private string workbookName = "";
        private string sheetName = "";
        private string metricSessionId = "";

        private readonly MetricsSuites metricsFilter = MetricsSuites.None;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static MetricsController()
        {
            // Right now this is kept simple as this is in bootstrap level code though it would be nice to allow configuration of what metrics are reported
            // The primary use case of this is in a timeline perf view, so filtering can be done there
            ReportLocalMetrics = PerformanceReporting.SupportsPerfApi;
            RecordingStart = new JsDate().GetTime();
            if (Script.IsValue(typeof(Window)) && Script.IsValue(Window.Self.Performance) && Script.IsValue(((dynamic)Window.Self.Performance)["now"]))
            {
                long epoch;
                // window.performance.now returns time in ms since navigation start.  Use "responseStart" instead
                // to ignore previous page's unloadEvent handling and any redirects, and to try to align the 'zero'
                // point of these two cases as closely as possible
                if (Script.IsValue(Window.Self.Performance.Timing))
                {
                    epoch = (long)(Window.Self.Performance.Timing.ResponseStart - Window.Self.Performance.Timing.NavigationStart);
                }
                else
                {
                    epoch = 0;
                }
                GetTiming = () => Window.Self.Performance.Now() - epoch;
            }
            else
            {
                // because window.performance.start is relative to start of page navigation, do something
                // similar, but we need to track our own Epoch
                double pageNavigationEpoch = new JsDate().GetTime();
                GetTiming = () => new JsDate().GetTime() - pageNavigationEpoch;
            }
            AppStartEpoch = GetTiming();
            if (ReportLocalMetrics)
            {
                LocalWebClientMetricsLogger.MarkAppStart();
            }

            // build lookup table to parse metrics filter config option
            SuiteNameLookup = new JsDictionary<string, MetricsSuites>();
            SuiteNameLookup["none"] = MetricsSuites.None;
            SuiteNameLookup["navigation"] = MetricsSuites.Navigation;
            SuiteNameLookup["bootstrap"] = MetricsSuites.Bootstrap;
            SuiteNameLookup["rendering"] = MetricsSuites.Rendering;
            SuiteNameLookup["commands"] = MetricsSuites.Commands;
            SuiteNameLookup["toolbar"] = MetricsSuites.Toolbar;
            SuiteNameLookup["hittest"] = MetricsSuites.HitTest;
            SuiteNameLookup["debug"] = MetricsSuites.Debug;
            SuiteNameLookup["fonts"] = MetricsSuites.Fonts;
            SuiteNameLookup["maps"] = MetricsSuites.Maps;
            SuiteNameLookup["exporting"] = MetricsSuites.Exporting;
            SuiteNameLookup["min"] = MetricsSuites.Min;
            SuiteNameLookup["core"] = MetricsSuites.Core;
            SuiteNameLookup["all"] = MetricsSuites.All;
        }

        [SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
        private MetricsController()
        {
            if (Script.IsValue(TsConfig.MetricsFilter) && TsConfig.MetricsFilter != "")
            {
                // iterate through possibly '|'-delimited list of suites to enable
                MetricsSuites filter = MetricsSuites.None;
                string[] filters = TsConfig.MetricsFilter.Split('|');
                foreach (string suite in filters)
                {
                    // B156978: IE8 doesn't support String.trim(), so test for existence before calling
                    string trimmedSuite = TypeUtil.HasMethod(suite, "trim") ? suite.Trim() : suite;
                    trimmedSuite = trimmedSuite.ToLowerCase();

                    // Saltarelle code may not be loaded yet, so use something that compiles into
                    // reasonable built-in javascript
                    if (SuiteNameLookup.ContainsKey(trimmedSuite))
                    {
                        filter |= SuiteNameLookup[trimmedSuite];
                    }
                }
                this.metricsFilter = filter;
            }
        }

        //// ===========================================================================================================
        //// Events
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets reference to instance of MetricsController class. Creates instance if it doesn't exist.
        /// </summary>
        internal static MetricsController Instance
        {
            get
            {
                if (MetricsController.instance == null)
                {
                    MetricsController.instance = new MetricsController();
                }
                return MetricsController.instance;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Creates and returns a new timing/measurement context with the provided description.
        /// In general, use with a Using() statement, such as:
        ///     using (var mc = MetricsController.CreateContext("stuff to measure", MetricsSuites.Debug))
        ///     {
        ///         ...
        ///     }
        /// Or instead explicitly call the dispose() method on the returned MetricsContext object to complete the timing
        /// of this context.
        /// </summary>
        public static MetricsContext CreateContext(
            string description,
            MetricsSuites suite = MetricsSuites.Debug,
            string extraInfo = null)
        {
            if (Script.IsNullOrUndefined(suite))
            {
                suite = MetricsSuites.Debug;
            }

            // if this metrics type is currently filtered out
            bool filteredMetric = (suite == MetricsSuites.None || (suite & Instance.metricsFilter) != suite);

            MetricsContext newContext;
            if (filteredMetric)
            {
                if (ReportLocalMetrics)
                {
                    // User has turned on local metrics reporting, so use JsConsole
                    newContext = new LocalMetricsContext(suite, description, extraInfo);
                }
                else
                {
                    // if this metric is filtered out, we want to return a 'valid' object to the caller, but
                    // not do any actual work behind the scenes
                    newContext = NullMetricsContext.Instance;
                }
            }
            else
            {
                newContext = new MetricsContext(Instance.GetNextContextID(), suite, description, extraInfo);
                Instance.contextStack.Push(newContext);
            }
            return newContext;
        }

        /// <summary>
        /// Most consumers who want to log a metrics should use this simplified method.
        /// Logs the given event to the chrome dev tools and any reporters wired up
        /// </summary>
        /// <param name="description"></param>
        /// <param name="metricsSuite"></param>
        public static void LogMetricsEvent(string description, MetricsSuites metricsSuite)
        {
            var parameters = new MetricsEventParameters
            {
                Time = MetricsController.GetTiming(),
                Description = description
            };
            if (metricsSuite == MetricsSuites.Bootstrap)
            {
                // for the bootstrap events, set a start time of the Epoch start
                parameters.Elapsed = parameters.Time - AppStartEpoch;
            }
            MetricsController.LogEventInternalUse(new MetricsEvent(MetricsEventType.Generic, metricsSuite, parameters));
        }

        /// <summary>
        /// This is primarily for inernal and special event logging
        /// Logs the given event to the chrome dev tools and any reporters wired up
        ///
        /// This method would be private if not for the uses in tests and NavigationMetricsCollector
        /// </summary>
        public static void LogEventInternalUse(MetricsEvent evt)
        {
            if (ReportLocalMetrics && evt.EventType != MetricsEventType.SessionInit)
            {
                LocalEventLogger.LogEvent(evt);
            }
            // if this metrics suite is currently filtered out, simply ignore
            if (evt.MetricSuite == MetricsSuites.None || (evt.MetricSuite & Instance.metricsFilter) != evt.MetricSuite)
            {
                return;
            }

            if (Instance.eventLoggers.Length > 0)
            {
                SendMetricToAllLoggers(evt);
            }
            else
            {
                // if we the javascript hasn't loaded yet, simply buffer the events
                Instance.eventBuffer.Push(evt);
            }
        }

        private static void SendMetricToAllLoggers(MetricsEvent evt)
        {
            // inject sessionId
            evt.Parameters.MetricsSessionId = Instance.metricSessionId;
            foreach (IWebClientMetricsLogger logger in Instance.eventLoggers)
            {
                logger.LogEvent(evt);
            }
        }

        /// <summary>
        /// Sets the logging interface, and outputs any buffered MetricsEvents
        /// that are pending
        /// </summary>
        public static void SetEventLoggers(JsArray<IWebClientMetricsLogger> loggers)
        {
            Debug.Assert(loggers != null, "Don't pass in a null set of loggers");
            Instance.eventLoggers = loggers;

            // if a valid logger was passed in and we have buffered events, log them
            // and clear the buffer
            if (Script.IsValue(loggers) && (instance.eventBuffer.Length > 0))
            {
                foreach (MetricsEvent bufferedEvt in instance.eventBuffer)
                {
                    SendMetricToAllLoggers(bufferedEvt);
                }
                instance.eventBuffer = new JsArray<MetricsEvent>();
            }
        }

        /// <summary>
        /// (Re-)Initialize the session identifying information so we can associate metrics events
        /// with a given session + workbook + dashboard.
        /// </summary>
        public static void InitSessionInfo()
        {
            // ensure controller instance is created, store in local so can avoid func call each time
            MetricsController localInstance = Instance;

            string currentSheet = string.IsNullOrEmpty(TsConfig.CurrentSheetName) ? TsConfig.SheetId : TsConfig.CurrentSheetName;
            if (localInstance.sessionId == TsConfig.SessionId && localInstance.workbookName == TsConfig.WorkbookName && localInstance.sheetName == currentSheet)
            {
                // if none of the properties have changed, there's no need to generate a new session id
                return;
            }

            localInstance.sessionId = TsConfig.SessionId;
            localInstance.workbookName = TsConfig.WorkbookName;
            localInstance.sheetName = currentSheet;

            // sessionID is just a semi-unique value representing the time the session was instantiated.
            // Last 6 digits of base-36 encoding should give 25 days of unique values. good enough
            JsDate now = new JsDate();
            localInstance.metricSessionId = now.GetTime().ToString(36);
            localInstance.metricSessionId = localInstance.metricSessionId.Substr(localInstance.metricSessionId.Length - 6);

            // add bits of sessionID to distinguish between clients with same timestamp
            if (localInstance.sessionId.Length >= 5) // should be 35 chars
            {
                localInstance.metricSessionId = localInstance.metricSessionId + localInstance.sessionId.Substr(1, 1);
                localInstance.metricSessionId = localInstance.metricSessionId + localInstance.sessionId.Substr(4, 1);
            }

            localInstance.LogSessionInfo();
        }

        public static string GetFriendlyEventDescription(string desc)
        {
            return FullMetricNameLookup.ReinterpretAs<JsDictionary<string, string>>()[desc] ?? desc;
        }

        /// <summary>
        /// Close the provided MetricsContext object and stop the timing.  Removes any orphaned
        /// nested contexts in the process.
        /// </summary>
        internal static void CloseContext(MetricsContext context)
        {
            int id = context.Id;
            int pos = -1;

            for (int i = (Instance.contextStack.Length - 1); i >= 0; i--)
            {
                if (instance.contextStack[i].Id == id)
                {
                    pos = i;
                    break;
                }
            }

            if (pos != -1)
            {
                int cnt = instance.contextStack.Length - pos;
                for (int i = 0; i < cnt; i++)
                {
                    instance.contextStack.Pop();
                }
            }

            instance.LogContextEnd(context);
        }

        private static MetricsEventParameters BuildMetricsEventCommonParameters(MetricsContext context)
        {
            MetricsEventParameters parameters = new MetricsEventParameters
            {
                TabSessionId = context.Id.ToString(),
                Description = context.Description
            };
            if (context.ExtraInfo != null)
            {
                // don't set this unless it's interesting - that will leave it as undefined and not get serialized
                parameters.ExtraInfo = context.ExtraInfo;
            }

            return parameters;
        }

        /// <summary>
        /// Returns the next unique ID to be assigned to a new MetricsContext
        /// </summary>
        internal int GetNextContextID()
        {
            int id = this.nextContextID;
            ++this.nextContextID;
            return id;
        }

        /// <summary>
        /// Log the current session identifying information so we can correlate it with the metrics events
        /// </summary>
        private void LogSessionInfo()
        {
            MetricsEventParameters parameters = new MetricsEventParameters
            {
                TabSessionId = this.sessionId,
                MetricsSessionId = this.metricSessionId,
                WorkbookName = this.workbookName,
                SheetName = this.sheetName,
                IsMobile = TsConfig.IsMobile
            };
            LogEventInternalUse(new MetricsEvent(MetricsEventType.SessionInit, MetricsSuites.Bootstrap, parameters));
        }

        /// <summary>
        /// Logs an event for the close of a MetricsContext
        /// </summary>
        private void LogContextEnd(MetricsContext context)
        {
            MetricsEventParameters parameters = BuildMetricsEventCommonParameters(context);
            parameters.Time = context.EndTime;
            parameters.Elapsed = context.ElapsedMS();

            LogEventInternalUse(new MetricsEvent(MetricsEventType.ContextEnd, context.MetricSuite, parameters));
        }
    }

    /// <summary>
    /// Class representing a single timing context for the MetricsController
    /// </summary>
    public class MetricsContext : IDisposable
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        internal readonly int Id;
        internal readonly MetricsSuites MetricSuite;
        internal readonly string Description;
        internal readonly string ExtraInfo;
        protected double start;
        protected double end;
        protected bool open;
        private readonly LocalMetricsContext localReporter = null;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public MetricsContext(int contextID, MetricsSuites suite, string desc, string extraInfo)
            : this(contextID, suite, desc)
        {
            this.ExtraInfo = extraInfo;

            if (MetricsController.ReportLocalMetrics)
            {
                this.localReporter = new LocalMetricsContext(suite, desc, extraInfo);
            }
        }

        protected MetricsContext(int contextID, MetricsSuites suite, string desc)
        {
            this.Id = contextID;
            this.MetricSuite = suite;
            this.Description = desc;
            this.start = MetricsController.GetTiming();
            this.open = true;
        }

        //// ===========================================================================================================
        //// Events
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the End date/time of this context (if it's been closed)
        /// </summary>
        internal double EndTime
        {
            get { return this.end; }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Closes the current context and stops any existing timing.
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// Closes the current context and stops any existing timing.
        /// </summary>
        protected virtual void Close()
        {
            if (this.open)
            {
                this.end = MetricsController.GetTiming();
                MetricsController.CloseContext(this);
                this.open = false;
                if (this.localReporter != null)
                {
                    this.localReporter.Close();
                }
            }
        }

        /// <summary>
        /// Returns the number of milliseconds elapsed since this context was opened.
        /// If the context has been closed, returns the milliseconds that elapsed between
        /// creation and close of the context
        /// </summary>
        public double ElapsedMS()
        {
            if (!this.open)
            {
                return this.end - this.start;
            }

            return MetricsController.GetTiming() - this.start;
        }
    }

    /// <summary>
    /// Class representing a single timing context for the MetricsController
    /// </summary>
    internal class LocalMetricsContext : MetricsContext
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private readonly string metricName;
        private static int eventCount = 0;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        internal LocalMetricsContext(MetricsSuites suite, string desc, string extraInfo)
            : base(MetricsController.Instance.GetNextContextID(), suite, desc)
        {
            desc = LocalWebClientMetricsLogger.BuildDescriptionName(desc, extraInfo);
            this.open = true;

            this.metricName = desc + "#" + LocalMetricsContext.eventCount++;
            LocalWebClientMetricsLogger.LogLocalMetricStart(this.metricName);
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Closes the current context and stops any existing timing.
        /// </summary>
        protected override void Close()
        {
            if (this.open)
            {
                this.open = false;
                LocalWebClientMetricsLogger.LogLocalMetricEnd(this.metricName);
            }
        }
    }

    /// <summary>
    /// A singleton class that 'implements' the MetricsContext interface but doesn't do/store any timing
    /// or metrics events in practice
    /// </summary>
    internal class NullMetricsContext : MetricsContext
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static NullMetricsContext instance;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        private NullMetricsContext()
            : base(-1, MetricsSuites.None, "", null)
        {
            this.open = false;
            this.start = 0;
            this.end = 0;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        internal static NullMetricsContext Instance
        {
            get
            {
                if (Script.IsNullOrUndefined(NullMetricsContext.instance))
                {
                    NullMetricsContext.instance = new NullMetricsContext();
                }

                return NullMetricsContext.instance;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Closes the current context and stops any existing timing.
        /// </summary>
        protected override void Close()
        {
            // do nothing -- we're a shell
        }
    }
}
