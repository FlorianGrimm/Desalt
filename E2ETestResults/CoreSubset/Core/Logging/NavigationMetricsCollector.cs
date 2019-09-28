// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="NavigationMetricsCollector.cs" company="Tableau Software">
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
    using System.Runtime.CompilerServices;
    using Tableau.JavaScript.Vql.Bootstrap;
    using Tableau.JavaScript.Vql.TypeDefs;
    using UnderscoreJs;

    [Imported]
    [NamedValues]
    public enum NavigationMetricsName
    {
        [ScriptName("navigationStart")]
        navigationStart,

        [ScriptName("unloadEventStart")]
        unloadEventStart,

        [ScriptName("unloadEventEnd")]
        unloadEventEnd,

        [ScriptName("redirectStart")]
        redirectStart,

        [ScriptName("redirectEnd")]
        redirectEnd,

        [ScriptName("fetchStart")]
        fetchStart,

        [ScriptName("domainLookupStart")]
        domainLookupStart,

        [ScriptName("domainLookupEnd")]
        domainLookupEnd,

        [ScriptName("connectStart")]
        connectStart,

        [ScriptName("connectEnd")]
        connectEnd,

        [ScriptName("secureConnectionStart")]
        secureConnectionStart,

        [ScriptName("requestStart")]
        requestStart,

        [ScriptName("responseStart")]
        responseStart,

        [ScriptName("responseEnd")]
        responseEnd,

        [ScriptName("domLoading")]
        domLoading,

        [ScriptName("domInteractive")]
        domInteractive,

        [ScriptName("domContentLoadedEventStart")]
        domContentLoadedEventStart,

        [ScriptName("domContentLoadedEventEnd")]
        domContentLoadedEventEnd,

        [ScriptName("domComplete")]
        domComplete,

        [ScriptName("loadEventStart")]
        loadEventStart,

        [ScriptName("loadEventEnd")]
        loadEventEnd
    }

    /// <summary>
    /// Static class for collecting navigation/performance timings and logging them
    /// </summary>
    public static class NavigationMetricsCollector
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        /// <summary>
        /// Array of names of navigation events, ordered by their occurrence in time
        /// </summary>
        private static NavigationMetricsName[] navigationMetricsOrder =
        {
            NavigationMetricsName.navigationStart,
            NavigationMetricsName.unloadEventStart,
            NavigationMetricsName.unloadEventEnd,
            NavigationMetricsName.redirectStart,
            NavigationMetricsName.redirectEnd,
            NavigationMetricsName.fetchStart,
            NavigationMetricsName.domainLookupStart,
            NavigationMetricsName.domainLookupEnd,
            NavigationMetricsName.connectStart,
            NavigationMetricsName.connectEnd,
            NavigationMetricsName.secureConnectionStart,
            NavigationMetricsName.requestStart,
            NavigationMetricsName.responseStart,
            NavigationMetricsName.responseEnd,
            NavigationMetricsName.domLoading,
            NavigationMetricsName.domInteractive,
            NavigationMetricsName.domContentLoadedEventStart,
            NavigationMetricsName.domContentLoadedEventEnd,
            NavigationMetricsName.domComplete,
            NavigationMetricsName.loadEventStart,
            NavigationMetricsName.loadEventEnd
        };

        private static JsDictionary<NavigationMetricsName, int> navMetrics = null;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static NavigationMetricsCollector()
        {
            dynamic w = typeof(Window);
            HtmlEventHandler loadHandler = ev => Underscore.Defer(NavigationMetricsCollector.CollectMetrics);
            if (Script.IsValue(w.addEventListener))
            {
                Window.AddEventListener("load", loadHandler, false);
            }
            else if (Script.IsValue(w.attachEvent))
            {
                w.attachEvent("load", loadHandler);
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Gets the timing metrics from the browser and logs them using the MetricsController
        /// </summary>
        public static void CollectMetrics()
        {
            // check if window.performance.timing is present
            if (Script.TypeOf(typeof(Window)) != "undefined" && Script.TypeOf(Window.Performance) != "undefined" && Script.TypeOf(Window.Performance.Timing) != "undefined" && Script.TypeOf(typeof(MetricsEvent)) != "undefined")
            {
                // grab the metrics
                navMetrics = Script.Reinterpret<JsDictionary<NavigationMetricsName, int>>(Window.Performance.Timing);
                if (Script.In(navMetrics, Script.Reinterpret<string>(NavigationMetricsName.navigationStart)))
                {
                    int start = navMetrics[navigationMetricsOrder[0]];
                    JsArray<int> metricArray = new JsArray<int>();

                    // iterate through them in order of occurrence and build array of values
                    foreach (NavigationMetricsName name in navigationMetricsOrder)
                    {
                        int metric = navMetrics[name];
                        metric = (metric == 0 ? 0 : metric - start);
                        metricArray.Push(metric);
                    }

                    // build up MetricsEvent object and log it
                    MetricsEventParameters parameters = new MetricsEventParameters { Values = metricArray };
                    MetricsEvent evt = new MetricsEvent(MetricsEventType.Navigation, MetricsSuites.Navigation, parameters);
                    MetricsController.LogEventInternalUse(evt);
                }
            }
        }
    }
}
