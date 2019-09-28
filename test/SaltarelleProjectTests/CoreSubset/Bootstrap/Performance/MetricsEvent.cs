// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="MetricsEvent.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Tableau.JavaScript.Vql.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

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

    /// <summary>
    /// To make the MetricsLogger happy, the ScriptNames here MUST match those of the
    /// MetricsParameterName enum.
    /// </summary>
    [Imported, Serializable]
    public class MetricsEventParameters
    {
        [ScriptName("d")]
        public string Description;

        /// <summary>
        /// This number represents miliseconds
        /// </summary>
        [ScriptName("t")]
        public double Time;

        /// <summary>
        /// This number represents miliseconds
        /// </summary>
        [ScriptName("e")]
        public double Elapsed;

        [ScriptName("id")]
        public string TabSessionId;

        [ScriptName("v")]
        public JsArray<int> Values;

        [ScriptName("sid")]
        public string MetricsSessionId;

        [ScriptName("ei")]
        public string ExtraInfo;

        [ScriptName("wb")]
        public string WorkbookName;

        [ScriptName("s")]
        public string SheetName;

        [ScriptName("m")]
        public bool IsMobile;
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
        public readonly MetricsEventParameters Parameters;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public MetricsEvent(MetricsEventType evtType, MetricsSuites suite, MetricsEventParameters eventParams)
        {
            this.EventType = evtType;
            this.MetricSuite = suite;
            this.Parameters = eventParams;
        }
    }
}
