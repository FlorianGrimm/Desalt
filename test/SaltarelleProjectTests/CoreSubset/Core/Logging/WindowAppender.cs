// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowAppender.cs" company="Tableau Software">
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
    using jQueryApi;

    /// <summary>
    /// A logger that writes to a floating window.  Designed for use by the TiledViewerRegions with mouse.
    /// MobileWindowAppender.cs should probably be made a subclass of this one?
    /// </summary>
    public class WindowAppender : BaseLogAppender
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        public static readonly LogAppenderInstance<WindowAppender> GlobalAppender = new LogAppenderInstance<WindowAppender>(() => new WindowAppender());

        private jQueryObject logDiv;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

#if DEBUG

        static WindowAppender()
        {
            // by default we're going to enable console.log for Warn+ in debug
            GlobalAppender.EnableLogging((logger, _) => logger.Name == "WindowAppender");
        }

#endif

        private WindowAppender()
        {
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================
        protected override void LogInternal(Logger source, LoggerLevel level, string message, object[] args)
        {
            if (Script.IsNullOrUndefined(this.logDiv))
            {
                this.BuildLogDiv();
            }

            message = this.FormatMessage(message.Replace(@"\n", "<br />"), args);
            this.logDiv.Html(message);
        }

        private void BuildLogDiv()
        {
            this.logDiv = jQuery.FromHtml("<div class='log-window-appender'>Debug mode ON</div>");
            this.logDiv.CSS(new JsDictionary(
                "position", "absolute",
                "bottom", "0px",
                "right", "0px",
                "backgroundColor", "white",
                "opacity", ".8",
                "border", "1px solid black",
                "minWidth", "5px",
                "minHeight", "5px",
                "z-index", "100"));

            jQuery.Select("body").Append(this.logDiv);
        }
    }
}
