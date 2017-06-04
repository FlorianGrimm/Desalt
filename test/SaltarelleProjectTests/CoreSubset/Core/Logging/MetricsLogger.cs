// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsLogger.cs" company="Tableau Software">
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
    using System.Runtime.CompilerServices;
    using System.Text;
    using jQueryApi;
    using Tableau.JavaScript.Vql.Bootstrap;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// class for logging metrics events
    /// </summary>
    public class MetricsLogger
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Cap on # of image elements we keep around for beacon messages
        /// simply to avoid out of memory issues.
        /// </summary>
        private const int MaxBeaconElementArraySize = 100;

        /// <summary>
        /// Cap on # of metrics events sitting in buffer/waiting to be reported
        /// </summary>
        private const int MaxEventBufferSize = 400;

        /// <summary>
        /// Cap on # of metrics events to report/log in a given time slice
        /// </summary>
        private const int MaxEventsToProcess = 20;

        /// <summary>
        /// Default delay to wait between event being logged and reporting the metrics
        /// (to allow for a few to collect so they can be batched)
        /// </summary>
        private const int DefaultProcessingDelay = 250;

        /// <summary>
        /// Amount of time to delay between processing batches of events
        /// (eg if we've processed the first batch but still have more queued)
        /// </summary>
        private const int OverflowProcessingDelay = 5;

        /// <summary>
        /// Interval(ms) at which to attempt to clean up image elements
        /// created for sending web beacon
        /// </summary>
        private const int BeaconCleanupDelay = 250;

        /// <summary>
        /// Mapping of metrics parameter names to verbose/debugging names
        /// </summary>
        private static readonly JsDictionary<MetricsParameterName, string> debugParamNames;

        /// <summary>
        /// Mapping of metrics event types to verbose/debugging type names
        /// </summary>
        private static readonly JsDictionary<MetricsEventType, string> debugEventNames;

        /// <summary>
        /// Singleton instance for the class
        /// </summary>
        private static MetricsLogger instance;

        /// <summary>
        /// Logged events waiting to be transmitted to the server
        /// </summary>
        private JsArray<MetricsEvent> eventBuffer;

        /// <summary>
        /// Logger object for outputting metrics to the console
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Collection of image elements used to report metrics events back to the server
        /// We must hold on to these until they are "complete", ie the src attribute is updated
        /// and the browser has made the request/gotten the response
        /// </summary>
        private JsArray<ImageElement> beaconImages;

        /// <summary>
        /// Cached id of timer (via window.setTimeout) for callback to clean up beacon
        /// images that are no longer needed
        /// </summary>
        private int? beaconCleanupTimerId;

        /// <summary>
        /// Cached timerID for buffer processing timer
        /// </summary>
        private int? bufferProcessTimerId;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static MetricsLogger()
        {
            debugParamNames = new JsDictionary<MetricsParameterName, string>();
            debugParamNames[MetricsParameterName.description] = "DESC";
            debugParamNames[MetricsParameterName.time] = "TIME";
            debugParamNames[MetricsParameterName.id] = "ID";
            debugParamNames[MetricsParameterName.sessionId] = "SESSION_ID";
            debugParamNames[MetricsParameterName.elapsed] = "ELAPSED";
            debugParamNames[MetricsParameterName.values] = "VALS";
            debugParamNames[MetricsParameterName.workbook] = "WORKBOOK";
            debugParamNames[MetricsParameterName.sheet] = "SHEET_NAME";
            debugParamNames[MetricsParameterName.properties] = "PROPS";
            debugParamNames[MetricsParameterName.isMobile] = "MOBILE";

            debugEventNames = new JsDictionary<MetricsEventType, string>();
            debugEventNames[MetricsEventType.Navigation] = "Navigation";
            debugEventNames[MetricsEventType.ContextStart] = "ProfileStart";
            debugEventNames[MetricsEventType.ContextEnd] = "ProfileEnd";
            debugEventNames[MetricsEventType.Generic] = "Generic";
            debugEventNames[MetricsEventType.SessionInit] = "SessionInit";
        }

        private MetricsLogger()
        {
            this.eventBuffer = new JsArray<MetricsEvent>();
            this.beaconImages = new JsArray<ImageElement>();
            this.bufferProcessTimerId = null;
            this.beaconCleanupTimerId = null;
        }

        //// ===========================================================================================================
        //// Events
        //// ===========================================================================================================

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets singleton instance of the MetricsLogger, creating it if necessary
        /// </summary>
        public static MetricsLogger Instance
        {
            get
            {
                if (!Script.IsValue(MetricsLogger.instance))
                {
                    MetricsLogger.instance = new MetricsLogger();
                }

                return MetricsLogger.instance;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Logs the specified metric event
        /// </summary>
        public void LogEvent(MetricsEvent evt)
        {
            // if buffer is full, toss out the oldest item before adding new
            if (this.eventBuffer.Length >= MetricsLogger.MaxEventBufferSize)
            {
                this.eventBuffer.Shift();
            }

            this.eventBuffer.Push(evt);

            // kick off timer to process events once we've popped
            // the stack/aren't currently processing anything
            this.StartProcessingTimer();
        }

        /// <summary>
        /// Attach the Logger to the MetricsController
        /// </summary>
        public void Attach()
        {
            MetricsController.SetEventLogger(this.LogEvent);
        }

        [AlternateSignature, SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        private extern void StartProcessingTimer();

        private void StartProcessingTimer(int delay)
        {
            if (this.bufferProcessTimerId.HasValue) { return; }

            delay = Script.Coalesce(delay, MetricsLogger.DefaultProcessingDelay);
            this.bufferProcessTimerId = Window.SetTimeout(this.ProcessBufferedEvents, delay);
        }

        /// <summary>
        /// Process our eventBuffer and output each event
        /// </summary>
        private void ProcessBufferedEvents()
        {
            this.bufferProcessTimerId = null;

            // to protect against spinning through a ton of events and
            // blocking the thread and making a bunch of remote requests,
            // cap the # of events we'll process
            JsArray<MetricsEvent> metricsToProcess;
            if (this.eventBuffer.Length > MetricsLogger.MaxEventsToProcess)
            {
                // if we have more events than we can process, slice the first few off the array
                // and then kick off a timer to ensure we come back to process the rest
                metricsToProcess = (JsArray<MetricsEvent>)this.eventBuffer.Slice(0, MetricsLogger.MaxEventsToProcess);
                this.eventBuffer = (JsArray<MetricsEvent>)this.eventBuffer.Slice(MetricsLogger.MaxEventsToProcess);

                this.StartProcessingTimer(MetricsLogger.OverflowProcessingDelay);
            }
            else
            {
                metricsToProcess = this.eventBuffer;
                this.eventBuffer = new JsArray<MetricsEvent>();
            }

            this.OutputEventsToConsole(metricsToProcess);

            if (TsConfig.MetricsReportingEnabled)
            {
                try
                {
                    this.OutputEventsToServer(metricsToProcess);
                }
                catch
                {
                    // just ignore exceptions here - no point in bringing down the client
                }
            }
        }

        /// <summary>
        /// Output the given event to the console, if appropriate
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        private void OutputEventsToConsole(JsArray<MetricsEvent> evts)
        {
            this.logger = ScriptEx.Value(this.logger, Logger.LazyGetLogger(typeof(MetricsLogger)));
            foreach (MetricsEvent evt in evts)
            {
                this.logger.Debug(FormatEvent(evt, true));
            }
        }

        /// <summary>
        /// Output the given event to the server, if appropriate
        /// </summary>
        private void OutputEventsToServer(JsArray<MetricsEvent> evts)
        {
            // IE can only handle 2048 bytes in URL.  Choosing a smaller # to be safe
            const int MaxPayloadLength = 1500;

            int numEvents = evts.Length;
            string payload = "";

            // bail if nothing to do
            if (numEvents == 0) { return; }

            // iterate through the events, formatting them to string and concatenating them
            // until we exceed max length
            for (int i = 0; i < numEvents; i++)
            {
                MetricsEvent evt = evts[i];

                // ignore context start events - they don't really provide value at the moment
                if (evt.EventType == MetricsEventType.ContextStart) { continue; }
                string formattedEvent = FormatEvent(evt, false);
                if (payload.Length > 0 && payload.Length + formattedEvent.Length > MaxPayloadLength)
                {
                    this.SendBeacon(TsConfig.MetricsServerHostname, payload);
                    payload = "";
                }
                else if (payload.Length > 0)
                {
                    payload += "&";
                }
                payload += formattedEvent;
            }

            // send any remaining payload
            if (payload.Length > 0)
            {
                this.SendBeacon(TsConfig.MetricsServerHostname, payload);
            }

            // if no cleanup is pending, set timer to clean up any beacon
            // images that have served their purpose
            if (this.beaconCleanupTimerId == null)
            {
                this.beaconCleanupTimerId = Window.SetTimeout(this.CleanupBeaconImages, BeaconCleanupDelay);
            }
        }

        /// <summary>
        /// Formats the specified MetricsEvent to a string.  If 'verbose' is true then
        /// uses more user-readable formatting
        /// </summary>
        private static string FormatEvent(MetricsEvent evt, bool verbose)
        {
            string delimiter = verbose ? ", " : ",";

            // build up string with description and key+value pairs
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(verbose ? debugEventNames[evt.EventType] : evt.EventType.ToString());

            int count = evt.Parameters.Count;
            if (count > 0)
            {
                strBuilder.Append("=");
                strBuilder.Append("{");

                int i = 0;

                string propSeparator = verbose ? ": " : ":";
                foreach (MetricsParameterName key in evt.Parameters.Keys)
                {
                    // don't output context ID unless it's a session init event
                    if (key == MetricsParameterName.id && evt.EventType != MetricsEventType.SessionInit)
                    {
                        continue;
                    }

                    if (i++ > 0)
                    {
                        strBuilder.Append(delimiter);
                    }
                    strBuilder.Append(verbose ? debugParamNames[key] : key.ToString());
                    strBuilder.Append(propSeparator);

                    object val = evt.Parameters[key];
                    FormatValue(strBuilder, val, verbose);
                }

                strBuilder.Append("}");
            }

            return strBuilder.ToString();
        }

        /// <summary>
        /// Given a dictionary of values, output the list of name+value pairs to the string builder
        /// </summary>
        private static void FormatDictionaryValues(StringBuilder strBuilder, JsDictionary<string, object> dict, bool verbose)
        {
            string delimiter = verbose ? ", " : ",";
            string propSeparator = verbose ? ": " : ":";
            int propCount = dict.Count;
            int j = 0;
            foreach (string propertyName in dict.Keys)
            {
                if (MiscUtil.HasOwnProperty(dict, propertyName))
                {
                    object propertyVal = dict[propertyName];

                    strBuilder.Append(propertyName);
                    strBuilder.Append(propSeparator);
                    FormatValue(strBuilder, propertyVal, verbose);
                    if (++j < propCount)
                    {
                        strBuilder.Append(delimiter);
                    }
                }
            }
        }

        /// <summary>
        /// Given a specific object, output it's value to the stringbuilder with the appropriate format
        /// </summary>
        private static void FormatValue(StringBuilder strBuilder, object value, bool verbose)
        {
            string type = Script.TypeOf(value);

            // special handling for floating point numbers (round to tenths if not whole number)
            if (type == "number" && Math.Floor((double)value) != (double)value)
            {
                strBuilder.Append(((float)value).ToFixed(1));
            }
            else if (type == "string")
            {
                if (verbose)
                {
                    strBuilder.Append("\"");
                    strBuilder.Append(value);
                    strBuilder.Append("\"");
                }
                else
                {
                    strBuilder.Append(string.EncodeUriComponent((string)value));
                }
            }
            else if (jQuery.IsArray(value))
            {
                // enclose arrays in brackets
                strBuilder.Append("[");
                strBuilder.Append(value);
                strBuilder.Append("]");
            }
            else if (type == "object")
            {
                // StringBuilder can't handle objects, so iterate through each element ourselves
                strBuilder.Append("{");
                JsDictionary dict = JsDictionary.GetDictionary(value);
                FormatDictionaryValues(strBuilder, dict, verbose);
                strBuilder.Append("}");
            }
            else
            {
                strBuilder.Append(value);
            }
        }

        /// <summary>
        /// A method of sending data back to the server for logging.
        /// With the right apache configuration, the image request simply gets logged,
        /// and we get a 204 in response.
        /// </summary>
        private void SendBeacon(string hostname, string payload)
        {
            const int Version = 1;

            // if no hostname, nothing to do
            if (MiscUtil.IsNullOrEmpty(hostname) || MiscUtil.IsNullOrEmpty(payload))
            {
                return;
            }

            // create a new image element to send the message to the server.
            // can't re-use an element as this function may get called in a tight loop,
            // and updating the same element's src attribute could result in some
            // requests getting lost
            ImageElement beaconImg = (ImageElement)Document.CreateElement("img");
            string versionStr = "v=" + Version.ToString();
            string beaconStr = hostname;
            beaconStr += "?" + versionStr + "&" + payload;
            beaconImg.Src = beaconStr;

            // add element to array which will get cleaned up later
            this.beaconImages.Push(beaconImg);

            // bound array.  If we're too large, just throw out the first/oldest element
            if (this.beaconImages.Length > MaxBeaconElementArraySize)
            {
                this.beaconImages.Shift();
            }
        }

        /// <summary>
        /// Cleans up dynamically created beacon images that have served their purpose
        /// ie, request has been sent and response received
        /// </summary>
        private void CleanupBeaconImages()
        {
            try
            {
                this.beaconCleanupTimerId = null;

                int index = 0;
                while (index < this.beaconImages.Length)
                {
                    if (this.beaconImages[index].Complete)
                    {
                        this.beaconImages.Splice(index, 1);
                    }
                    else
                    {
                        index++;
                    }
                }

                // kick off timer to ensure cleanup of any images left over
                if (this.beaconImages.Length > 0)
                {
                    this.beaconCleanupTimerId = Window.SetTimeout(this.CleanupBeaconImages, BeaconCleanupDelay);
                }
            }
            catch
            {
                // just ignore exceptions here - no point in bringing down the client
            }
        }
    }
}
