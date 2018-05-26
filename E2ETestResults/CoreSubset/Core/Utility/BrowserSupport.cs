// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowserSupport.cs" company="Tableau Software">
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
    using System.Html;
    using System.Html.Media.Graphics;
    using System.Serialization;
    using System.Text.RegularExpressions;
    using jQueryApi;
    using Tableau.JavaScript.Vql.Bootstrap;

    /// <summary>
    /// Contains information about what is currently supported in the browser or environment.
    /// </summary>
    public static class BrowserSupport
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static bool selectStart;
        private static bool touch = Script.In(typeof(Document), "ontouchend");
        private static bool fonts = Script.In(typeof(Document), "fonts");
        private static bool dataUri;
        private static bool postMessage;
        private static bool historyApi;
        private static bool consoleLogFormatting;
        private static string cssTransformName;
        private static string cssTransitionName;
        private static bool cssTranslate2d;
        private static bool cssTranslate3d;
        private static bool shouldUseAlternateHitStrategy = false;
        private static bool canvasLinePattern = false;
        private static bool isSafari = false;
        private static bool isChrome = false;
        private static bool isIE = false;
        private static double internetExplorerVersion = 0.0;
        private static double safariVersion = 0.0;
        private static double iosVersion = 0.0;
        private static bool isFF = false;
        private static bool isOpera = false;
        private static bool isKhtml = false;
        private static bool isWebKit = false;
        private static bool isMozilla = false;
        private static bool isIos = false;
        private static bool isAndroid = false;
        private static bool isMac = false;
        private static bool isWindows = false;
        private static float devicePixelRatio = 1.0f;
        private static float backingStoragePixelRatio = 1.0f;
        private static bool dateInput = false;
        private static bool dateTimeInput = false;
        private static bool dateTimeLocalInput = false;

        private static bool timeInput = false;

        private static bool setSelectionRange = false;

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static BrowserSupport()
        {
            DetectBrowser(); // FIXME Defect 535199
            jQuery.OnDocumentReady(DetectBrowserSupport);
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets a value indicating whether the browser supports getComputedStyle.
        /// https://developer.mozilla.org/en-US/docs/DOM/window.getComputedStyle
        /// </summary>
        public static bool GetComputedStyle
        {
            get { return Script.In(typeof(Window), "getComputedStyle"); }
        }

        /// <summary>
        /// Gets a value indicating whether the browser supports addEventListener.
        /// </summary>
        public static bool AddEventListener
        {
            get { return Script.In(typeof(Document), "addEventListener"); }
        }

        /// <summary>
        /// Gets a value indicating whether the selectstart event is supported.
        /// </summary>
        public static bool SelectStart
        {
            get { return selectStart; }
        }

        /// <summary>
        /// Gets a value indicating whether touch events are supported. Note that this does not imply that it's a mobile
        /// browser since many Windows 7 devices support touch and may still have a mouse.
        /// </summary>
        public static bool Touch
        {
            get { return touch; }
        }

        /// <summary>
        /// Gets a value indicating whether the browser supports the CSS Font Loader API.
        /// See https://drafts.csswg.org/css-font-loading
        /// </summary>
        public static bool FontLoaderApi
        {
            get { return fonts; }
        }

        /// <summary>
        /// Gets a value indicating whether the browser supports Data URIs.
        /// http://en.wikipedia.org/wiki/Data_URI_scheme
        /// </summary>
        public static bool DataUri
        {
            get { return dataUri; }
        }

        /// <summary>
        /// Gets a value indicating whether or not this browser supports postMessage.
        /// </summary>
        public static bool PostMessage
        {
            get { return postMessage; }
        }

        public static bool HistoryApi
        {
            get { return historyApi; }
        }

        public static bool ConsoleLogFormating
        {
            get { return consoleLogFormatting; }
        }

        public static bool IsMobile
        {
            get { return isAndroid || isIos; }
        }

        public static bool IsIos
        {
            get { return isIos; }
        }

        public static bool IsAndroid
        {
            get { return isAndroid; }
        }

        public static bool IsChrome
        {
            get { return isChrome; }
        }

        public static bool IsMac
        {
            get { return isMac; }
        }

        public static bool IsIE
        {
            get { return isIE; }
        }

        public static bool IsFF
        {
            get { return isFF; }
        }

        public static bool IsOpera
        {
            get { return isOpera; }
        }

        public static bool IsSafari
        {
            get { return isSafari; }
        }

        public static bool IsWindows
        {
            get { return isWindows; }
        }

        /// <summary>
        /// Returns the version of Internet Explorer that made this request.
        /// If the client is not IE, this returns 0.0
        /// </summary>
        public static double BrowserVersion
        {
            get { return internetExplorerVersion; }
        }

        public static double SafariVersion
        {
            get { return safariVersion; }
        }

        /// <summary>
        /// Returns the version of iOS that made this request.
        /// If the client is not iOS, this returns 0.0
        /// </summary>
        public static double IosVersion
        {
            get { return iosVersion; }
        }

        public static bool RaisesEventOnImageReassignment
        {
            // $NOTE-rbunker-2008-06-17: B17794, safari doesn't fire
            // onload if the src doesn't change
            get { return !isSafari; }
        }

        public static bool ImageLoadIsSynchronous
        {
            get { return isIE; }
        }

        /// <summary>
        /// Gets a value indicating whether document.elementFromPoint on the current browser
        /// requires screen vs. client coordinates for reasons of DPI, etc.
        /// See BUGZID 55280
        /// </summary>
        public static bool UseAlternateHitStrategy
        {
            get { return shouldUseAlternateHitStrategy; }
        }

        /// <summary>
        /// Gets a value indicating whether the CSS property "transform" is supported.
        /// http://caniuse.com/#search=transform
        /// </summary>
        public static bool CssTransform
        {
            get { return Script.IsValue(cssTransformName); }
        }

        /// <summary>
        /// Gets a value indicating the name of the CSS transform property.
        /// </summary>
        public static string CssTransformName
        {
            get { return cssTransformName; }
        }

        /// <summary>
        /// Gets a value indicating the name of the CSS transition property.
        /// </summary>
        public static string CssTransitionName
        {
            get { return cssTransitionName; }
        }

        /// <summary>
        /// Gets a value indicating whether the CSS "transform: translate()" is supported.
        /// http://caniuse.com/#search=transform
        /// </summary>
        public static bool CssTranslate2D
        {
            get { return cssTranslate2d; }
        }

        /// <summary>
        /// Gets a value indicating whether the CSS "transform: translate3d()" is supported.
        /// http://caniuse.com/#search=transform
        /// </summary>
        public static bool CssTranslate3D
        {
            get { return cssTranslate3d; }
        }

        public static float BackingStoragePixelRatio
        {
            get { return backingStoragePixelRatio; }
        }

        public static float DevicePixelRatio
        {
            get { return devicePixelRatio; }
        }

        public static bool CanvasLinePattern
        {
            get { return canvasLinePattern; }
        }

        /// <summary>
        /// Gets a value indicating whether the device supports a native HTML5 date picker.
        /// http://www.w3.org/TR/html-markup/input.date.html
        /// </summary>
        public static bool DateInput
        {
            get { return dateInput; }
        }

        /// <summary>
        /// Gets a value indicating whether the device supports a native HTML5 datetime picker.
        /// http://www.w3.org/TR/html-markup/input.datetime.html
        /// </summary>
        public static bool DateTimeInput
        {
            get { return dateTimeInput; }
        }

        /// <summary>
        /// Gets a value indicating whether the device supports a native HTML5 local datetime picker.
        /// http://www.w3.org/TR/html-markup/input.datetime-local.html
        /// </summary>
        public static bool DateTimeLocalInput
        {
            get { return dateTimeLocalInput; }
        }

        /// <summary>
        /// Gets a value indicating whether the device supports a native HTML5 local time picker.
        /// http://www.w3.org/TR/html-markup/input.time.html
        /// </summary>
        public static bool TimeInput
        {
            get { return timeInput; }
        }

        /// <summary>
        /// Indicates whether setSelectionRange is supported on an input element
        /// </summary>
        public static bool SetSelectionRange
        {
            get { return setSelectionRange; }
        }

        /// <summary>
        /// Gets the mousewheel event to use via feature detection.
        /// https://developer.mozilla.org/en-US/docs/Web/Reference/Events/wheel
        /// </summary>
        public static string MouseWheelEvent
        {
            get
            {
                string mouseWheelEvent;
                if (Script.In(Window.Document.DocumentElement, "onwheel"))
                {
                    mouseWheelEvent = "wheel";
                }
                else if (Script.In(Window.Document.DocumentElement, "onmousewheel"))
                {
                    mouseWheelEvent = "mousewheel";
                }
                else
                {
                    mouseWheelEvent = "MozMousePixelScroll";
                }

                return mouseWheelEvent;
            }
        }

        /// <summary>
        /// Tests if mouse capture support is present.  As of 6/1/14, only IE and Firefox support this.
        /// https://developer.mozilla.org/en-US/docs/Web/API/Element.setCapture
        /// </summary>
        ///
        /// <remarks>
        /// See the comments in <see cref="MouseCapture"/>) for how this is used during dragging. We have some
        /// special browser-specific logic there.
        /// </remarks>
        public static bool MouseCapture
        {
            get
            {
                return Script.In(typeof(Document), "releaseCapture");
            }
        }

        /// <summary>
        /// Indicates whether orientationchange event is supported by the browser
        /// </summary>
        public static bool OrientationChange
        {
            get
            {
                return Script.In(typeof(Window), "onorientationchange");
            }
        }

        /// <summary>
        /// Reports whether browser supports geolocation
        /// </summary>
        public static bool IsGeolocationSupported
        {
            get { return Script.IsValue(Window.Navigator.Geolocation); }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static void DetectBrowserSupport()
        {
            // Taken from jQuery UI
            Element body = Document.Body;
            Element div = Document.CreateElement("div");
            body.AppendChild(div);

            selectStart = Script.In(div, "onselectstart");

            // set display to none to avoid a layout bug in IE
            // http://dev.jquery.com/ticket/4014
            body.RemoveChild(div).Style.Display = "none";

            postMessage = Script.In(typeof(Window), "postMessage");

            historyApi = TypeUtil.HasMethod(Window.History, "pushState")
                && TypeUtil.HasMethod(Window.History, "replaceState");

            DetectDataUriSupport();
            DetectConsoleLogFormatting();
            DetectBrowser();
            DetectTransitionSupport();
            DetectTransformSupport();
            DetectDocumentElementFromPoint();
            DetectDevicePixelRatio();
            DetectBackingStoragePixelRatio();
            DetectDateInputSupport();
            DetectCanvasLinePattern();
            DetectSetSelectionRangeSupport();
        }

        /// <summary>
        /// Gets the location.origin property in a browser-independent way.
        /// </summary>
        /// <remarks>IE still does not support location.origin. Use a polyfill:
        /// <see href="http://stackoverflow.com/questions/1420881/javascript-jquery-method-to-find-base-url-from-a-string"/>
        /// </remarks>
        public static string GetOrigin(Location location)
        {
            string origin = location.Origin;
            if (Script.IsNullOrUndefined(origin))
            {
                origin = location.Protocol + "//" + location.Host;
            }

            return origin;
        }

        public static void DoPostMessageWithContext(string message)
        {
            // leverage implementation in DoPostMessage but add extra logging
            bool success = Utility.DoPostMessageWithContext(message);
            if (!success)
            {
                Logger.LazyGetLogger(typeof(BrowserSupport)).Debug("BrowserSupport::DoPostMessage failed.");
            }
        }

        private static void DetectDataUriSupport()
        {
            jQueryObject imgObj = jQuery.FromHtml("<img />");
            ImageElement img = (ImageElement)imgObj[0];
            imgObj.On("load error", delegate
            {
                // works if image was loaded as a single pixel
                dataUri = img.Width == 1 && img.Height == 1;
            });

            // single pixel image
            img.Src = "data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==";
        }

        /// <summary>
        /// Our default hit-testing uses coordinates which assume we aren't scrolled. This tests for a scroll possibility
        /// and then also checks an actual hit test on absolute-positioned temporary DOM element.
        /// Call this statically or rarely (once per drag at most).
        /// </summary>
        private static void DetectDocumentElementFromPoint()
        {
            Element body = Window.Document.Body;
            if (BrowserSupport.isWebKit && IsMobile)
            {
                // Perform a hit test far from origin to ensure that window.devicePixelRation is a factor.
                // BUGZID 63572 For Google Nexus tablets until fixed. This just tests brokenness.
                jQueryObject target = jQuery.FromHtml("<div></div>");
                // Coordinates fit within minimum viable dashboard size but far from origin for scaling bugs.
                // Also tests with a target large enough to matter for touch/pill/cursor size.
                target.CSS(new JsDictionary(
                    "position", "absolute",
                    "top", "300px",
                    "left", "300px",
                    "width", "25px",
                    "height", "25px",
                    "z-index", "10000")); // big number to ensure we're over the glass pane
                Element elem = target.GetElement(0);
                try
                {
                    body.AppendChild(elem);
                    shouldUseAlternateHitStrategy = Document.ElementFromPoint(310, 310) != elem;
                }
                catch
                {
                }
                finally
                {
                    target.Remove();
                }
            }
        }

        private static void DetectConsoleLogFormatting()
        {
            try
            {
                if (Script.IsValue(((dynamic)typeof(Window)).console && Script.IsValue(((dynamic)typeof(Window)).console.log)))
                {
                    // currently all consoles except iOS safari seem to work as expected
                    consoleLogFormatting = Window.Navigator.UserAgent.IndexOf("iPad") < 0;
                }
                else
                {
                    consoleLogFormatting = false;
                }
            }
            catch
            {
                // IE generates an error here, have to ignore
                consoleLogFormatting = false;
            }
        }

        /// <summary>
        /// NOTE-jrockwood-2012-11-29: Avoid browser detection like the plague.
        /// Instead use feature detection. However, there are some small cases
        /// where you have to sniff the browser user agent. Just be careful.
        /// </summary>
        private static void DetectBrowser()
        {
            string ua = GetUserAgent();

            isKhtml = ua.IndexOf("Konqueror") >= 0;
            isWebKit = ua.IndexOf("WebKit") >= 0;
            isChrome = ua.IndexOf("Chrome") >= 0;

            // NOTE-jfurdell-2015-01-21: Chrome's user agent string currently contains "Safari".
            // Check that the user agent string has "Safari" and not "Chrome"; if so, we're Safari.
            isSafari = ua.IndexOf("Safari") >= 0 && !isChrome;

            isOpera = ua.IndexOf("Opera") >= 0;

            // Check Safari version
            if (isSafari)
            {
                string[] versionMatches = ua.Match(new Regex("\\bVersion\\/(\\d+\\.\\d+)"));
                if (versionMatches != null)
                {
                    safariVersion = double.Parse(versionMatches[1]);
                }
            }

            // Check for IE versions (10 and earlier)
            internetExplorerVersion = 0.0;
            isIE = false;
            string[] oldIEVersions = ua.Match(new Regex("\\bMSIE (\\d+\\.\\d+)"));
            if (oldIEVersions != null)
            {
                isIE = true;
                // Element 0 is the whole "MSIE: #.#" string
                internetExplorerVersion = double.Parse(oldIEVersions[1]);
            }
            // Check for IE versions (11 and later)
            if (!isIE && !isOpera && (ua.IndexOf("Trident") >= 0 || ua.IndexOf("Edge/") >= 0))
            {
                string[] tridentIEVersions = ua.Match(new Regex("\\brv:(\\d+\\.\\d+)"));
                string[] edgeIEVersions = ua.Match(new Regex("Edge/(\\d+\\.\\d+)"));
                if (tridentIEVersions != null)
                {
                    isIE = true;
                    // Element 0 is the whole "rv:##.#" string
                    internetExplorerVersion = double.Parse(tridentIEVersions[1]);
                }
                else if (edgeIEVersions != null)
                {
                    isIE = true;
                    isChrome = false;
                    isSafari = false;
                    // Element 0 is the whole "rv:##.#" string
                    internetExplorerVersion = double.Parse(edgeIEVersions[1]);
                }
            }

            // NOTE-rshasan-2015-01-27: IE11's user agent string contains "Gecko", which
            // is historically associated with Mozilla.
            // To avoid identifying IE11 as Mozilla, we make sure that "IE words" are
            // absent ("MSIE" and "Trident").
            isMozilla = !isKhtml && !isWebKit && !isIE && ua.IndexOf("Gecko") >= 0;
            isFF = isMozilla || ua.IndexOf("Firefox") >= 0 || ua.IndexOf("Minefield") >= 0;

            Regex commandRegex = new Regex("iPhone|iPod|iPad");
            isIos = commandRegex.Test(ua);

            if (isIos)
            {
                string[] iosVersions = ua.Match(new Regex("\\bOS ([\\d+_?]+) like Mac OS X"));
                if (iosVersions != null)
                {
                    iosVersion = double.Parse(iosVersions[1].ReplaceFirst("_", ".").Replace("_", "")); // '9_0_2' --> 9.02
                }
            }

            isAndroid = ua.IndexOf("Android") >= 0 && !isIE;

            isMac = ua.IndexOf("Mac") >= 0;

            isWindows = ua.IndexOf("Windows") >= 0;
        }

        private static string GetUserAgent()
        {
            return Window.Navigator.UserAgent;
        }

        private static bool TrySettingCssProperty(string styleProp, string cssProp, string val)
        {
            Element e = (Element)Document.CreateElement("div");
            try
            {
                Document.Body.InsertBefore(e, null);

                if (!Script.In(e.Style, styleProp))
                {
                    return false;
                }

                e.Style[styleProp] = val;
                Style s = DomUtil.GetComputedStyle(e);
                string computedVal = s[cssProp];
                return !MiscUtil.IsNullOrEmpty(computedVal) && computedVal != "none";
            }
            finally
            {
                // set display to none to avoid a layout bug in IE
                // http://dev.jquery.com/ticket/4014
                ((Element)Document.Body.RemoveChild(e)).Style.Display = "none";
            }
        }

        private static void DetectTransitionSupport()
        {
            JsDictionary<string, string> transitions = new JsDictionary<string, string>(
                "transition", "transition",
                "webkitTransition", "-webkit-transition",
                "msTransition", "-ms-transition",
                "mozTransition", "-moz-transition",
                "oTransition", "-o-transition");

            foreach (KeyValuePair<string, string> t in transitions)
            {
                if (!Script.In(Document.Body.Style, t.Key))
                {
                    continue;
                }
                cssTransitionName = t.Value;
                break;
            }
        }

        private static void DetectTransformSupport()
        {
            JsDictionary<string, string> transforms = new JsDictionary<string, string>(
                "transform", "transform",
                "webkitTransform", "-webkit-transform",
                "msTransform", "-ms-transform",
                "mozTransform", "-moz-transform",
                "oTransform", "-o-transform");

            foreach (KeyValuePair<string, string> t in transforms)
            {
                if (!Script.In(Document.Body.Style, t.Key))
                {
                    continue;
                }
                cssTransformName = t.Value;
                cssTranslate2d = TrySettingCssProperty(t.Key, t.Value, "translate(1px,1px)");
                cssTranslate3d = TrySettingCssProperty(t.Key, t.Value, "translate3d(1px,1px,1px)");
                break;
            }
        }

        private static void DetectDevicePixelRatio()
        {
            devicePixelRatio = TypeUtil.GetField<float?>(Window.Self, "devicePixelRatio") ?? 1.0f;
        }

        private static void DetectBackingStoragePixelRatio()
        {
            var canvas = Document.CreateElement("canvas");

            if (Script.IsNullOrUndefined(canvas))
            {
                backingStoragePixelRatio = 1.0f;
                return;
            }

            CanvasRenderingContext context = null;
            if (TypeUtil.HasMethod(canvas.GetType(), "getContext"))
            {
                context = ((CanvasElement)canvas).GetContext("2d");
            }

            if (Script.IsNullOrUndefined(context))
            {
                backingStoragePixelRatio = 1.0f;
                return;
            }

            dynamic ctx = context;
            backingStoragePixelRatio = ctx.webkitBackingStorePixelRatio ||
                ctx.mozBackingStorePixelRatio ||
                ctx.msBackingStorePixelRatio ||
                ctx.oBackingStorePixelRatio ||
                1.0f;
        }

        /// <summary>
        /// Detects if the canvas supports custom line patterns.
        /// </summary>
        private static void DetectCanvasLinePattern()
        {
            var canvas = Document.CreateElement("canvas");

            if (Script.IsNullOrUndefined(canvas))
            {
                return;
            }

            CanvasRenderingContext context = null;
            if (TypeUtil.HasMethod(canvas, "getContext"))
            {
                context = ((CanvasElement)canvas).GetContext("2d");
            }

            if (Script.IsNullOrUndefined(context))
            {
                return;
            }

            canvasLinePattern =
                // Chrome
                TypeUtil.HasMethod(context, "setLineDash") ||
                // Mozilla Firefox
                Script.In(context, "mozDash") ||
                // Safari
                Script.In(context, "webkitLineDash");
        }

        private static void DetectSetSelectionRangeSupport()
        {
            jQueryObject inputObject = jQuery.FromHtml("<input>");
            setSelectionRange = TypeUtil.HasMethod(inputObject.GetElement(0), "setSelectionRange");
        }

        private static void DetectDateInputSupport()
        {
            BrowserSupport.dateInput = DetectCustomInputSupport("date");
            BrowserSupport.dateTimeInput = DetectCustomInputSupport("datetime");
            BrowserSupport.dateTimeLocalInput = DetectCustomInputSupport("datetime-local");
            BrowserSupport.timeInput = DetectCustomInputSupport("time");
        }

        private static bool DetectCustomInputSupport(string inputType)
        {
            jQueryObject inputObject = jQuery.FromHtml("<input type='" + inputType + "'>")
                    .CSS(new CssDictionary { Position = CssPosition.Absolute, Visibility = CssVisibility.Hidden })
                    .AppendTo(jQuery.FromElement(Document.Body));
            InputElement input = (InputElement)inputObject.GetElement(0);

            // if a device does not have an custom HTML5 input widget, this will usually get reset to "text".
            string reportedInputType = (string)input.GetAttribute("type");
            // a real custom input will filter out invalid data (e.g., http://www.w3.org/tr/html5/forms.html#date-state-%28type=date%29)
            const string InvalidDataString = "@inva/1d:)";
            input.Value = InvalidDataString;

            bool supportsInput = (reportedInputType == inputType && input.Value != InvalidDataString);

            inputObject.Remove();

            return supportsInput;
        }
    }
}
