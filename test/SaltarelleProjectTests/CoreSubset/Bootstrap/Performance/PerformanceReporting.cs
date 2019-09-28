// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformanceReporting.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Bootstrap
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Documentation here https://developer.mozilla.org/en-US/docs/Web/API/User_Timing_API
    /// </summary>
    [Imported, IgnoreNamespace, ScriptName("performance")]
    public static class Performance
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static void Mark(string markName)
        {
            // Imported
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static void Measure(string measureName, string startMarkName, string endMarkName)
        {
            // Imported
        }
    }

    /// <summary>
    /// Safe wrapper for the UserTiming API, performance. If not supported, commands become no-ops.
    /// </summary>
    public static class PerformanceReporting
    {
        public static readonly bool SupportsPerfApi;

        static PerformanceReporting()
        {
            dynamic windowAsDynamic = Window.Instance;
            PerformanceReporting.SupportsPerfApi = (windowAsDynamic["performance"] != null && windowAsDynamic["performance"]["mark"] != null);
        }

        public static void Mark(string markName)
        {
            if (PerformanceReporting.SupportsPerfApi)
            {
                Performance.Mark(markName);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        public static void Measure(string measureName, string startMarkName, string endMarkName)
        {
            if (PerformanceReporting.SupportsPerfApi)
            {
                try
                {
                    Performance.Measure(measureName, startMarkName, endMarkName);
                }
#pragma warning disable 0168
                // Warning disabled since we don't use e in production code
                catch (Exception e) // Measure will throw if for whatever reason we didn't mark one side or the other. Don't allow this to break other code
#pragma warning restore 0168
                {
#if DEBUG
                    JsConsole.Error("Missing a performance mark", e);
#endif
                }
            }
        }
    }
}
