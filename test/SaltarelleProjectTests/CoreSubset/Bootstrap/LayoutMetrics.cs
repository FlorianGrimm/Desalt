// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="LayoutMetrics.cs" company="Tableau Software">
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
    using System.Text;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// UIMetricType enums
    /// </summary>
    [NamedValues]
    public enum UIMetricType
    {
        [ScriptName("scrollbar")]
        Scrollbar = 0,

        [ScriptName("qfixed")]
        QFixed = 1,

        [ScriptName("qslider")]
        QSlider = 2,

        [ScriptName("qreadout")]
        QReadout = 3,

        [ScriptName("cfixed")]
        CFixed = 4,

        [ScriptName("citem")]
        CItem = 5,

        [ScriptName("hfixed")]
        HFixed = 6,

        [ScriptName("hitem")]
        HItem = 7,

        [ScriptName("cmslider")]
        CmSlider = 8,

        [ScriptName("cmdropdown")]
        CmDropdown = 9,

        [ScriptName("cmpattern")]
        CmPattern = 10,

        [ScriptName("rdate")]
        RDate = 11,

        [ScriptName("rdatep")]
        RDateP = 12,

        [ScriptName("capply")]
        CApply = 13,

        [ScriptName("cmtypeinsearch")]
        CmTypeInSearch = 14,

        [ScriptName("ccustomitem")]
        CCustomItem = 15,
    }

    /// <summary>
    /// Contains the metrics for scrollbars, filters, and other items used to
    /// determine the initial layouts. Also contains utility methods for working with
    /// the hard-coded values.
    ///
    /// These values are currently maintained in TWO locations.
    /// C++: SetPortSizeCommand::DefaultMetrics() and
    /// Saltarelle: Tableau.JavaScript.Vql.Bootstrap.LayoutMetrics.LayoutMetrics.
    /// Make sure to keep both of them in sync.
    /// </summary>
    public class LayoutMetrics
    {
        private static JsDictionary<UIMetricType, Metric> defaultDb;

        private static JsDictionary<UIMetricType, ClientUIMetricType> metricToClientMetric = new JsDictionary<UIMetricType, ClientUIMetricType>();

        static LayoutMetrics()
        {
            defaultDb = new JsDictionary<UIMetricType, Metric>();

            // These metrics were measured on IE8, IE9, Firefox, Chrome, and Safari
            // and were found to match on all of the browsers (as of 2/17/2011).
            defaultDb[UIMetricType.Scrollbar] = new Metric(17, 17);
            defaultDb[UIMetricType.QFixed] = new Metric(0, 5);
            defaultDb[UIMetricType.QSlider] = new Metric(0, 18);
            defaultDb[UIMetricType.QReadout] = new Metric(0, 20);
            defaultDb[UIMetricType.CFixed] = new Metric(0, 6);
            defaultDb[UIMetricType.CItem] = new Metric(0, 18);         // minimum height of check/radiolist item
            defaultDb[UIMetricType.CCustomItem] = new Metric(0, 9);    // minimum height of custom list item
            defaultDb[UIMetricType.HFixed] = new Metric(0, 21);
            defaultDb[UIMetricType.HItem] = new Metric(0, 18);         // minimum height of hierarchical list item
            defaultDb[UIMetricType.CmSlider] = new Metric(0, 49);
            defaultDb[UIMetricType.CmDropdown] = new Metric(0, 29);
            defaultDb[UIMetricType.CmPattern] = new Metric(0, 29);
            defaultDb[UIMetricType.CApply] = new Metric(0, 21);
            defaultDb[UIMetricType.CmTypeInSearch] = new Metric(0, 22);
            defaultDb[UIMetricType.RDate] = new Metric(0, 28);

            if (TsConfig.IsMobile)
            {
                defaultDb[UIMetricType.Scrollbar] = new Metric(0, 0);
            }

            // map UIMetricType to ClientUIMetricType
            metricToClientMetric[UIMetricType.Scrollbar] = ClientUIMetricType.ScrollbarMetric;
            metricToClientMetric[UIMetricType.QFixed] = ClientUIMetricType.QFilterFixedMetric;
            metricToClientMetric[UIMetricType.QSlider] = ClientUIMetricType.QFilterSliderMetric;
            metricToClientMetric[UIMetricType.QReadout] = ClientUIMetricType.QFilterReadoutMetric;
            metricToClientMetric[UIMetricType.CFixed] = ClientUIMetricType.CFilterFixedMetric;
            metricToClientMetric[UIMetricType.CItem] = ClientUIMetricType.CFilterItemMetric;
            metricToClientMetric[UIMetricType.CApply] = ClientUIMetricType.CFilterApplyMetric;
            metricToClientMetric[UIMetricType.HFixed] = ClientUIMetricType.HFilterFixedMetric;
            metricToClientMetric[UIMetricType.HItem] = ClientUIMetricType.HFilterItemMetric;
            metricToClientMetric[UIMetricType.CmSlider] = ClientUIMetricType.CmSliderFilterMetric;
            metricToClientMetric[UIMetricType.CmDropdown] = ClientUIMetricType.CmDropdownFilterMetric;
            metricToClientMetric[UIMetricType.CmPattern] = ClientUIMetricType.CmPatternFilterMetric;
            metricToClientMetric[UIMetricType.RDate] = ClientUIMetricType.RDateFilterMetric;
            metricToClientMetric[UIMetricType.RDateP] = ClientUIMetricType.RDatePFilterMetric;
            metricToClientMetric[UIMetricType.CmTypeInSearch] = ClientUIMetricType.CmTypeInSearchMetric;
            metricToClientMetric[UIMetricType.CCustomItem] = ClientUIMetricType.CFilterCustomItemMetric;
        }

        /// <summary>
        /// Initializes a new instance of the LayoutMetrics class.
        /// </summary>
        /// <param name="scrollbarSize">If not supplied the scrollbar sizes are measured, which can be expensive.
        /// </param>
        public LayoutMetrics(Metric scrollbarSize)
        {
            this.CloneDefaultDb();

            // populate class members with the metrics database
            if (Script.IsNullOrUndefined(scrollbarSize))
            {
                int size = LayoutMetrics.GetScrollbarSize();
                scrollbarSize = new Metric(size, size);
            }

            ((dynamic)this)[UIMetricType.Scrollbar] = new Metric(scrollbarSize.W, scrollbarSize.H);
        }

        public static LayoutMetrics Clone(LayoutMetrics other)
        {
            LayoutMetrics obj = (LayoutMetrics)new LayoutMetrics(null);

            foreach (KeyValuePair<UIMetricType, Metric> entry in LayoutMetrics.defaultDb)
            {
                Metric v = (Metric)((dynamic)other)[entry.Key];
                if (Script.IsValue(v))
                {
                    ((dynamic)obj)[entry.Key] = v;
                }
            }

            return obj;
        }

        [SuppressMessage("Microsoft.Design", "CA1024", Justification = "Function to be called by other script")]
        public static int GetScrollbarSize()
        {
            if (TsConfig.IsMobile)
            {
                return 0;
            }

            Element outer = (Element)Document.CreateElement("div");
            Style style = outer.Style;
            style.Width = "100px";
            style.Height = "100px";
            style.Overflow = "scroll";
            style.Position = "absolute";
            style.Top = "0px";
            style.Filter = "alpha(opacity=0)";
            style.Opacity = "0";
            style.Left = "0px";

            Element inner = (Element)Document.CreateElement("div");
            inner.Style.Width = "400px";
            inner.Style.Height = "400px";
            outer.AppendChild(inner);
            Document.Body.AppendChild(outer);

            int width = outer.OffsetWidth - outer.ClientWidth;

            Document.Body.RemoveChild(outer);
            outer.RemoveChild(inner);
            outer = inner = null;

            // myork-2011-11-17: In OS X Lion, scroll bars take up 0 width because they are overlayed
            // on elements. But, we still want to show them. If we have a 0 width then set it to 9
            // pixels (current size of mac scroll bar)
            width = width > 0 ? width : 9;
            return width;
        }

        // $NOTE-jrockwood-2011-02-23:
        // Yes, we could write a general-purpose JSON.stringify method or use the
        // built-in browser JSON object, but it's a lot of code for a simple one-
        // time use here. Note that the use of spaces and newlines here is critical
        // since the server uses a YAML parser to decode the JSON. Why? Good question,
        // to which I don't know the answer.
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");

            // count # of fields in defaultDb
            // note: not using Dictionary.Count to avoid referencing mscorlib.js
            int length = 0;
            foreach (KeyValuePair<UIMetricType, Metric> entry in LayoutMetrics.defaultDb)
            {
                length++;
            }

            int i = 0;
            foreach (KeyValuePair<UIMetricType, Metric> entry in LayoutMetrics.defaultDb)
            {
                if (Script.IsValue(((dynamic)this)[entry.Key]))
                {
                    Metric m = (Metric)((dynamic)this)[entry.Key];
                    sb.Append("\t\"" + entry.Key + "\": {\n");
                    sb.Append("\t\t\"w\": " + m.W + ",\n");
                    sb.Append("\t\t\"h\": " + m.H + "\n");
                    sb.Append("\t}");
                    if (i < length - 1)
                    {
                        sb.Append(",");
                    }

                    sb.Append("\n");
                }
                ++i;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private void CloneDefaultDb()
        {
            foreach (KeyValuePair<UIMetricType, Metric> entry in defaultDb)
            {
                ((dynamic)this)[entry.Key] = entry.Value;
            }
        }
    }

    public sealed class Metric : Record
    {
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "W",
            Justification = "W is the correct spelling")]
        public int W;

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "H",
            Justification = "H is the correct spelling")]
        public int H;

        [ObjectLiteral]
        public Metric(int w, int h)
        {
            this.W = w;
            this.H = h;
        }
    }
}
