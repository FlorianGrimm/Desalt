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
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
    using Tableau.JavaScript.Vql.TypeDefs;

    // $NOTE -- if you add an item to the enum below, ensure you add an entry
    // to the lookup table MetricsController.suiteNameLookup
    [Flags]
    public enum MetricsSuites
    {
        None        = 0x00, // No metrics (disable reporting)

        // $NOTE-jking: in general, you should not be adding new metrics using the
        // following group of suite names.  These have been carefully selected to provide
        // data but not to overwhelm the server if/when we report them back
        Navigation  = 0x01, // Window.performance.navigation timings
        Bootstrap   = 0x02, // Bootstrap stages/timings
        Rendering   = 0x04, // Rendering of viz panetable/axes
        Commands    = 0x08, // Local+remote execution of commands
        HitTest     = 0x10, // Hit-testing/responsive feedback

        // In general, YOU SHOULD USE THIS IF YOU ADD NEW METRICS
        Debug       = 0x20, // Generic, debugging metrics.
        Toolbar     = 0x40, // Toolbar events
        Fonts       = 0x80, // Font metrics

        // Combination suites
        // $NOTE-jking: if you're thinking about modifying these, you probably shouldn't :)
        Min         = 0x03, // Navigation | Bootstrap
        Core        = 0x0F, // Min | Commands | Rendering

        // report everything/DOS the server!
        All         = 0xFF, // All metrics/no filtering
    }

    [Imported]
    [NamedValues]
    public enum MetricsParameterName
    {
        [ScriptName("d")]
        description,

        [ScriptName("t")]
        time,

        [ScriptName("id")]
        id,

        [ScriptName("e")]
        elapsed,

        [ScriptName("v")]
        values,

        [ScriptName("sid")]
        sessionId,

        [ScriptName("wb")]
        workbook,

        [ScriptName("s")]
        sheet,

        [ScriptName("m")]
        isMobile,

        [ScriptName("p")]
        properties,
    }

    [Imported]
    [NamedValues]
    public enum MetricsEventType
    {
        [ScriptName("nav")]
        Navigation,

        [ScriptName("wps")]
        ContextStart,

        [ScriptName("wp")]
        ContextEnd,

        [ScriptName("gen")]
        Generic,

        [ScriptName("init")]
        SessionInit,
    }

    /// <summary>
    /// Singleton class for measuring and logging code performance
    /// </summary>
    public class MetricsController
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static Func<double> GetTiming;
        private static long epoch;
        private static MetricsController instance;
        private static JsDictionary<string, MetricsSuites> suiteNameLookup;
        private int nextContextID = 0;
        private JsArray<MetricsContext> contextStack = new JsArray<MetricsContext>();
        private JsArray<MetricsEvent> eventBuffer = new JsArray<MetricsEvent>();
        private Action<MetricsEvent> eventLogger = null;

        private string sessionId = "";
        private string workbookName = "";
        private string sheetName = "";
        private string metricSessionId = "";

        private MetricsSuites metricsFilter = MetricsSuites.None;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static MetricsController()
        {
            if (Script.IsValue(typeof(Window)) && Script.IsValue(Window.Self.Performance) && Script.IsValue(((dynamic)Window.Self.Performance)["now"]))
            {
                // window.performance.now returns time in ms since navigation start.  Use "responseStart" instead
                // to ignore previous page's unloadEvent handling and any redirects,, and to try to align the 'zero'
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
                // similar, but we need to track our own epoch
                epoch = new JsDate().GetTime();
                GetTiming = () => new JsDate().GetTime() - epoch;
            }

            // build lookup table to parse metrics filter config option
            suiteNameLookup = new JsDictionary<string, MetricsSuites>();
            suiteNameLookup["none"]         = MetricsSuites.None;
            suiteNameLookup["navigation"]   = MetricsSuites.Navigation;
            suiteNameLookup["bootstrap"]    = MetricsSuites.Bootstrap;
            suiteNameLookup["rendering"]    = MetricsSuites.Rendering;
            suiteNameLookup["commands"]     = MetricsSuites.Commands;
            suiteNameLookup["toolbar"]      = MetricsSuites.Toolbar;
            suiteNameLookup["hittest"]      = MetricsSuites.HitTest;
            suiteNameLookup["debug"]        = MetricsSuites.Debug;
            suiteNameLookup["fonts"]        = MetricsSuites.Fonts;
            suiteNameLookup["min"]          = MetricsSuites.Min;
            suiteNameLookup["core"]         = MetricsSuites.Core;
            suiteNameLookup["all"]          = MetricsSuites.All;
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
                    if (suiteNameLookup.ContainsKey(trimmedSuite))
                    {
                        filter |= suiteNameLookup[trimmedSuite];
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
        private static MetricsController Instance
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
            JsDictionary<string, object> props = null)
        {
            if (Script.IsNullOrUndefined(suite))
            {
                suite = MetricsSuites.Debug;
            }

            // if this metrics type is currently filtered out
            bool filteredMetric = (suite == MetricsSuites.None || (suite & Instance.metricsFilter) != suite);

            if (Script.IsNullOrUndefined(props))
            {
                props = new JsDictionary<string, object>();
            }

            MetricsContext newContext;
            if (filteredMetric)
            {
                // if this metric is filtered out, we want to return a 'valid' object to the caller, but
                // not do any actual work behind the scenes
                newContext = NullMetricsContext.Instance;
            }
            else
            {
                newContext = new MetricsContext(Instance.GetNextContextID(), suite, description, props);
                Instance.contextStack.Push(newContext);
            }
            return newContext;
        }

        /// <summary>
        /// Logs the given event by outputting it to the console and/or sending it to
        /// the server for recording in the log
        /// </summary>
        public static void LogEvent(MetricsEvent evt)
        {
            // if this metrics suite is currently filtered out, simply ignore
            if (evt.MetricSuite == MetricsSuites.None || (evt.MetricSuite & Instance.metricsFilter) != evt.MetricSuite)
            {
                return;
            }

            if (Script.IsValue(Instance.eventLogger))
            {
                // inject sessionId
                evt.Parameters[MetricsParameterName.sessionId] = Instance.metricSessionId;
                Instance.eventLogger(evt);
            }
            else
            {
                // if we the javascript hasn't loaded yet, simply buffer the events
                Instance.eventBuffer.Push(evt);
            }
        }

        /// <summary>
        /// Sets the logging interface, and outputs any buffered MetricsEvents
        /// that are pending
        /// </summary>
        public static void SetEventLogger(Action<MetricsEvent> logger)
        {
            Instance.eventLogger = logger;

            // if a valid logger was passed in and we have buffered events, log them
            // and clear the buffer
            if (Script.IsValue(logger) && (instance.eventBuffer.Length > 0))
            {
                foreach (MetricsEvent bufferedEvt in instance.eventBuffer)
                {
                    // inject sessionId for events buffered before client init
                    bufferedEvt.Parameters[MetricsParameterName.sessionId] = instance.metricSessionId;
                    instance.eventLogger(bufferedEvt);
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

        /// <summary>
        /// Close the provided MetricsContext object and stop the timing.  Removes any orphaned
        /// nested contexts in the process.
        /// </summary>
        public static void CloseContext(MetricsContext context)
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

        private static JsDictionary<MetricsParameterName, object> BuildMetricsEventCommonParameters(MetricsContext context)
        {
            JsDictionary<MetricsParameterName, object> parameters = new JsDictionary<MetricsParameterName, object>();
            parameters[MetricsParameterName.id] = context.Id;
            parameters[MetricsParameterName.description] = context.Description;

            if (context.Properties.Count > 0)
            {
                parameters[MetricsParameterName.properties] = context.Properties;
            }

            return parameters;
        }

        /// <summary>
        /// Returns the next unique ID to be assigned to a new MetricsContext
        /// </summary>
        private int GetNextContextID()
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
            JsDictionary<MetricsParameterName, object> parameters = new JsDictionary<MetricsParameterName, object>();
            parameters[MetricsParameterName.id] = this.sessionId;
            parameters[MetricsParameterName.sessionId] = this.metricSessionId;
            parameters[MetricsParameterName.workbook] = this.workbookName;
            parameters[MetricsParameterName.sheet] = this.sheetName;
            parameters[MetricsParameterName.isMobile] = TsConfig.IsMobile;
            LogEvent(new MetricsEvent(MetricsEventType.SessionInit, MetricsSuites.Bootstrap, parameters));
        }

        /// <summary>
        /// Logs an event for the close of a MetricsContext
        /// </summary>
        private void LogContextEnd(MetricsContext context)
        {
            JsDictionary<MetricsParameterName, object> parameters = BuildMetricsEventCommonParameters(context);
            parameters[MetricsParameterName.time] = context.EndTime;
            parameters[MetricsParameterName.elapsed] = context.ElapsedMS();

            LogEvent(new MetricsEvent(MetricsEventType.ContextEnd, context.MetricSuite, parameters));
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

        public readonly int Id;
        public readonly MetricsSuites MetricSuite;
        public readonly string Description;
        private readonly JsDictionary<string, object> propBag;
        protected double start;
        protected double end;
        protected bool open;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public MetricsContext(int contextID, MetricsSuites suite, string desc, JsDictionary<string, object> props)
        {
            this.Id = contextID;
            this.MetricSuite = suite;
            this.Description = desc;
            this.start = MetricsController.GetTiming();
            this.open = true;
            this.propBag = props;
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
        public double EndTime
        {
            get { return this.end; }
        }

        public JsDictionary<string, object> Properties
        {
            get { return this.propBag; }
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
            : base(-1, MetricsSuites.None, "", new JsDictionary<string, object>())
        {
            this.open = false;
            this.start = 0;
            this.end = 0;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        public static NullMetricsContext Instance
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

    /// <summary>
    /// Class representing a single timing event for the MetricsController
    /// </summary>
    public class MetricsEvent
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public readonly MetricsEventType EventType;
        public readonly MetricsSuites MetricSuite;
        public readonly JsDictionary<MetricsParameterName, object> Parameters;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public MetricsEvent(MetricsEventType evtType, MetricsSuites suite, JsDictionary<MetricsParameterName, object> eventParams)
        {
            this.EventType = evtType;
            this.MetricSuite = suite;
            this.Parameters = eventParams;
        }
    }
}
