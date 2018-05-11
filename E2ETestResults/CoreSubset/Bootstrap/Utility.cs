// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Tableau Software">
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
    using System.Html;
    using System.Html.Media.Graphics;
    using System.Net;
    using System.Net.Messaging;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using Tableau.JavaScript.Vql.TypeDefs;

    /// <summary>
    /// The different scenarios in which the current viz can be embedded.
    /// </summary>
    [NamedValues]
    public enum EmbedMode
    {
        /// <summary>
        /// The only thing on the page.  No iframe.
        /// </summary>
        NotEmbedded = 0,

        /// <summary>
        /// Embedded via an iFrame from WG server.  Specifically WG server, VizHub, VizPortal, etc don't count.
        /// </summary>
        EmbeddedInWg = 1,

        /// <summary>
        /// Embedded via an iFrame from something other than WG.  Could be anything.
        /// </summary>
        EmbeddedNotInWg = 2
    }

    /// <summary>
    /// Utility functions for Bootstrap code
    /// </summary>
    // This is public because the test code uses it.
    public static class Utility
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        // amount by which iOS on Safari7 misreports clientHeight in landscape mode (BUGZID 85078, BUGZID 148694)
        private const int Safari7ClientHeightErrorPixels = 20;

        private static readonly Regex RegexNotwhite = new Regex(@"\s");
        private static readonly Regex RegexTrimLeft = new Regex(@"^\s+");
        private static readonly Regex RegexTrimRight = new Regex(@"\s+$");

        private static readonly EmbedMode EmbedModeVar;

        public static string CLIENTNO = "cn";

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        static Utility()
        {
            // IE doesn't match non-breaking spaces with \s
            if (RegexNotwhite.Test(@"\xA0"))
            {
                RegexTrimLeft = new Regex(@"^[\s\xA0]+");
                RegexTrimRight = new Regex(@"[\s\xA0]+$");
            }

            EmbedModeVar = CalculateEmbedMode();
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        internal static bool NeedsSafari7HackFix
        {
            get
            {
                // B85078: Helper  for work around for iOS7/Safari bug where in landscape mode, the clientHeight
                //  is getting mis-reported and is to large by 20 units
                if (!TsConfig.IsMobileDevice || TsConfig.Embedded)
                {
                    return false;
                }

                bool isAndroid = (Window.Navigator.UserAgent.IndexOf("Android") != -1);
                if (isAndroid)
                {
                    return false;
                }

                bool isSafari7 = (Window.Navigator.UserAgent.IndexOf("Safari") != -1) && (Window.Navigator.UserAgent.IndexOf("OS 7") != -1);
                return isSafari7;
            }
        }

        /// <summary>
        /// This method should only be used within Utility, and only to find out the orientation for Safari Mobile.
        /// In general, please use Spiff.OrientationHandler for all things orientation.
        /// 
        /// This method lives here so it can be in the Bootstrap code that's loaded in the page
        /// </summary>
        private static bool InLandscapeMode
        {
            get
            {
                try
                {
                    // Embedded frames don't report the actual orientation, so we need to get info from topmost window we can find
                    WindowInstance win = Utility.GetTopmostWindow();
                    WindowOrientation orientation = win.GetOrientation();
                    return Script.IsValue(orientation) && (orientation == WindowOrientation.LeftLandscape || orientation == WindowOrientation.RightLandscape);
                }
                catch
                {
                    // potential security exception if embedded cross-domain
                }

                return false;
            }
        }

        public static JsDictionary<string, string> UrlLocationSearchParams
        {
            get { return ParseQueryParamString(UrlLocationSearch.Substring(1)); }
        }

        public static JsDictionary<string, string> UrlLocationHashData
        {
            get
            {
                JsDictionary<string, string> urlHashData = new JsDictionary<string, string>();
                string fragmentId = UrlLocationHash;
                //if Url hash string is empty string or just prefix "#", return empty dictionary
                if (fragmentId.Length < 2)
                {
                    return new JsDictionary<string, string>();
                }
                fragmentId = fragmentId.Substr(1);
                string[] pairs = fragmentId.Split("&");
                foreach (string pair in pairs)
                {
                    string[] keyVal = pair.Split("=");
                    if (keyVal.Length == 1)
                    {
                        urlHashData[CLIENTNO] = keyVal[0];
                    }
                    else if (keyVal.Length == 2)
                    {
                        string key = string.DecodeUriComponent(keyVal[0]);
                        string value = string.DecodeUriComponent(keyVal[1]);
                        urlHashData[key] = value;
                    }
                }
                return urlHashData;
            }

            set
            {
                StringBuilder newFragmentId = new StringBuilder();
                bool first = true;
                Action appendSeparator = delegate
                {
                    newFragmentId.Append(first ? "#" : "&");
                    first = false;
                };

                foreach (KeyValuePair<string, string> pairs in value)
                {
                    string keyEncoded = string.EncodeUriComponent(pairs.Key);
                    appendSeparator();
                    if (keyEncoded == CLIENTNO)
                    {
                        newFragmentId.Append(pairs.Value);
                    }
                    else if (Script.IsNullOrUndefined(pairs.Value))
                    {
                        newFragmentId.Append(keyEncoded);
                    }
                    else
                    {
                        newFragmentId.Append(keyEncoded).Append("=").Append(string.EncodeUriComponent(pairs.Value));
                    }
                }
                if (Script.IsValue(newFragmentId))
                {
                    WindowInstance window = LocationWindow;
                    if (HistoryApiSupported())
                    {
                        ReplaceState(window, null, null, newFragmentId.ToString());
                    }
                    else
                    {
                        window.Location.Hash = newFragmentId.ToString();
                    }
                }
            }
        }

        public static string UrlLocationHash
        {
            get
            {
                WindowInstance window = LocationWindow;
                return window.Location.Hash;
            }
        }

        public static string UrlLocationSearch
        {
            get
            {
                WindowInstance window = LocationWindow;
                return window.Location.Search;
            }
        }

        public static EmbedMode EmbedMode
        {
            get
            {
                return EmbedModeVar;
            }
        }

        /// <summary>
        /// Gets the WindowInstance that corresponds to the viz location. If in WG then it's the WG parent frame.
        /// Otherwise it's the viz's frame.
        /// </summary>
        public static WindowInstance LocationWindow
        {
            get
            {
                return EmbedMode == EmbedMode.EmbeddedInWg ? Window.Parent : Window.Self;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        private static JsDictionary<string, string> ParseQueryParamString(string urlStr)
        {
            var urlData = new JsDictionary<string, string>();
            string[] pairs = urlStr.Split("&");
            foreach (string pair in pairs)
            {
                string[] keyVal = pair.Split("=");
                if (keyVal.Length == 2)
                {
                    string key = string.DecodeUriComponent(keyVal[0]);
                    string value = string.DecodeUriComponent(keyVal[1]);
                    urlData[key] = value;
                }
            }
            return urlData;
        }

        public static void XhrPostJsonChunked(
            URLStr uri,
            string param,
            Action<object> firstChunkCallback,
            Action<object> secondaryChunkCallback,
            Action<object> errBack,
            bool asynchronous)
        {
            XmlHttpRequest xhr = (XmlHttpRequest)Utility.CreateXhr();
            xhr.Open("POST", uri, asynchronous);
            xhr.SetRequestHeader("Accept", "text/javascript");
            xhr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            if (!Script.IsNullOrUndefined(TsConfig.SheetId))
            {
                xhr.SetRequestHeader("X-Tsi-Active-Tab", TsConfig.SheetId);
            }
            Action<object> invokeError = GetInvokeErrorDelegate(xhr, errBack);

            int byteOffset = 0;
            Action consumeJSONChunks = delegate
            {
                // Check our response text and see if we can parse the size of the next chunk.
                // If so, see if that entire chunk has arrived. If not, do nothing, and wait to be called
                // back again to do the same check. If it has arrive, attempt to parse it as JSON and pass
                // it to the appropriate callback

                string buffer = "";
                try
                {
                    buffer = xhr.ResponseText;
                }
                catch (Exception)
                {
                    // ignore, IE in ready state 3 will throw an exception when accessing ResponseText.
                }

                int bufferLength = buffer.Length;
                while (byteOffset < bufferLength)
                {
                    string newData = buffer.Substr(byteOffset);
                    Regex regex = new Regex(@"^(\d+);");
                    string[] match = newData.Match(regex);
                    if (!Script.IsValue(match)) { return; }

                    int chunkStart = match[0].Length;
                    int chunkLength = int.Parse(match[1]);
                    if (chunkStart + chunkLength > newData.Length) { return; }

                    newData = newData.Substr(chunkStart, chunkLength);
                    string json = null;
                    try
                    {
                        string contextStr = "Parse " + ((byteOffset == 0) ? "Primary" : "Secondary") + " JSON";
                        using (var mc = MetricsController.CreateContext(contextStr, MetricsSuites.Debug))
                        {
                            json = ParseJson(newData);
                        }
                    }
                    catch (Exception)
                    {
                        // Bad JSON, ignore chunk
                        // $NOTE: The error handler in CommandController relies
                        // on this error text
                        invokeError(new Exception("Invalid JSON"));
                    }

                    if (byteOffset == 0)
                    {
                        firstChunkCallback(json);
                    }
                    else
                    {
                        secondaryChunkCallback(json);
                    }

                    byteOffset += chunkStart + chunkLength;
                }
            };

            int intervalID = -1;
            bool isReceiving = false;

            bool cannotTouchXhrWhileDownloading = // B64984 Older IEs choke on accessing xhr.Status in ReadyState 3?
                (Window.Navigator.UserAgent.IndexOf("MSIE") >= 0 && // from BrowserSupport::DetectBrowser
                 double.Parse(Window.Navigator.AppVersion.Split("MSIE ")[1]) < 10); // from Dojo

            xhr.OnReadyStateChange = delegate
            {
                try
                {
                    // ReadyState change firing across the browsers is a mess.
                    // IE10 (all IEs?) will fire Receiving every time it gets a new 4KB chunk.
                    // Some other browsers will only fire Receiving once.
                    // Chrome (at least) even fires Loaded more than once.
                    // Every browser fires Receiving, even if you can't read the responseText (IE < 10)
                    // but in that case, we try to be extra safe with cannotTouchXhrWhileDownloading
                    if (!cannotTouchXhrWhileDownloading &&
                        xhr.ReadyState == ReadyState.Loading &&
                        xhr.Status == 200 &&
                        !isReceiving) // we only want to enter this once
                    {
                        consumeJSONChunks();
                        if (intervalID == -1) // extra paranoia to make sure we only set one interval
                        {
                            intervalID = Window.SetInterval(consumeJSONChunks, 10);
                        }
                        isReceiving = true;
                        return;
                    }
                    if (xhr.ReadyState != ReadyState.Done)
                    {
                        return;
                    }

                    if (intervalID != -1)
                    {
                        Window.ClearInterval(intervalID);
                        intervalID = -1;
                    }

                    if (IsSuccessStatus(xhr))
                    {
                        consumeJSONChunks();
                    }
                    else
                    {
                        invokeError(new Exception("Unable to load " + uri + "; status: " + xhr.Status));
                    }
                }
                catch (Exception ex)
                {
                    // BUGZID: 127983 cleanly nav away from embedded viz in IE in single-page app like AngularJS.
                    // IE will continue calling the OnReadyStateChange for any outstanding XHRs, so abort.
                    // detect we're leaving the app/viz based on if javascript global variable has become undefined,
                    // (undefined can truly become 'undefined')
                    if (Script.IsUndefined(Type.GetType("ss")))
                    {
                        xhr.Abort();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            };

            try
            {
                xhr.Send(param);
            }
            catch (Exception e)
            {
                invokeError(e);
            }
        }

        /// <summary>A Simple synchronous GET to an XML-returning URI</summary>
        ///
        public static string XhrGetXmlSynchronous(
            URLStr uri,
            Action<object> errBack)
        {
            XmlHttpRequest xhr = (XmlHttpRequest)Utility.CreateXhr();
            xhr.Open("GET", uri, false);
            xhr.SetRequestHeader("Accept", "text/xml");

            try
            {
                xhr.Send();
            }
            catch (Exception e)
            {
                InvokeErrorDelegate(xhr, errBack, e);
                return null;
            }

            return xhr.ResponseText;
        }

        public static void XhrPostJson(
            URLStr uri,
            string param,
            Action<object> callback,
            Action<object> errBack,
            bool asynchronous)
        {
            XmlHttpRequest xhr = (XmlHttpRequest)Utility.CreateXhr();
            xhr.Open("POST", uri, asynchronous);
            xhr.SetRequestHeader("Accept", "text/javascript");
            xhr.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            if (Script.IsValue(TsConfig.SheetId))
            {
                xhr.SetRequestHeader("X-Tsi-Active-Tab", TsConfig.SheetId);
            }
            Action<object> invokeError = GetInvokeErrorDelegate(xhr, errBack);

            xhr.OnReadyStateChange = delegate
            {
                // we don't care about this
                if (xhr.ReadyState != ReadyState.Done)
                {
                    return;
                }

                if (IsSuccessStatus(xhr))
                {
                    try
                    {
                        string json = Utility.ParseJson(xhr.ResponseText);
                        callback(json);
                    }
                    catch (Exception x)
                    {
                        invokeError(x);
                    }
                }
                else
                {
                    invokeError(new Exception("Unable to load " + uri + "; status: " + xhr.Status));
                }
            };

            try
            {
                xhr.Send(param);
            }
            catch (Exception e)
            {
                invokeError(e);
            }
        }

        public static void ApplySafari7CSSHackFix()
        {
            // B85078: Work around iOS7/Safari bug where in landscape mode, the clientHeight is getting mis-reported and is
            // too large by 20 units
            if (NeedsSafari7HackFix)
            {
                if (InLandscapeMode)
                {
                    Window.Document.Body.Style.Height = (Window.OuterHeight - Safari7ClientHeightErrorPixels) + "px";
                }
                else
                {
                    Window.Document.Body.Style.Height = "";
                }
            }
        }

        /// <summary>
        /// Attaches an event handler for 'message' event, and calls the specified handler
        /// function when event is received. If the passed handler handles the event (returns true) the
        /// event handler will be removed
        /// </summary>
        /// <param name="eventHandler"></param>
        public static void AttachOneTimeMessageHandler(Func<MessageEvent, bool> eventHandler)
        {
            HtmlEventHandler messageListener = null;
            messageListener = delegate(Event ev)
            {
                MessageEvent e = (MessageEvent)ev;
                if (eventHandler(e))
                {
                    if (Script.IsValue(((dynamic)Window.Self).removeEventListener))
                    {
                        Window.RemoveEventListener("message", messageListener, false);
                    }
                    else
                    {
                        ((dynamic)Window.Self).detachEvent("onmessage", messageListener);
                    }
                }
            };

            if (Script.IsValue(((dynamic)Window.Self).addEventListener))
            {
                Window.AddEventListener("message", messageListener, false);
            }
            else
            {
                ((dynamic)Window.Self).attachEvent("onmessage", messageListener);
            }
        }

        public static bool DoPostMessageWithContext(string message)
        {
            bool success = false;

            // this.loadOrderID should only be >= 0 when we're loaded by viz_v1.js
            // this.loadOrderID indicates the order in which this viz gets loaded
            if (TsConfig.LoadOrderID >= 0)
            {
                message += "," + TsConfig.LoadOrderID.As<string>();
            }

            // Each viz in the API is given a unique ID so that multiple vizs
            // on a page can be supported. It's passed as a URL query parameter
            // when loaded from tableau_v8.js
            if (!string.IsNullOrEmpty(TsConfig.ApiId))
            {
                // if loadOrderID < 0, above code will not put it in the msg.
                // we need to put it there to fill up the space, Tableau_v8.js knows how to deal with it.
                if (TsConfig.LoadOrderID < 0)
                {
                    message += "," + TsConfig.LoadOrderID.As<string>();
                }
                message += "," + TsConfig.ApiId.As<string>();
            }

            success = DoPostMessage(message);
            return success;
        }

        public static bool DoPostMessage(string message)
        {
            bool success = false;
            if (Script.In(typeof(Window), "postMessage"))
            {
                try
                {
                    Window.Parent.PostMessage(message, "*");
                    success = true;
                }
                catch
                {
                    // possible to get security exception
                }
            }
            return success;
        }

        internal static EmbedMode CalculateEmbedMode()
        {
            bool parentIsSelf = false;
            try
            {
                parentIsSelf = Window.Self == Window.Parent;
            }
            catch
            {
                // possible to get security exception
            }

            if (parentIsSelf)
            {
                return EmbedMode.NotEmbedded;
            }
            if (TsConfig.SingleFrame)
            {
                return EmbedMode.EmbeddedNotInWg;
            }
            return EmbedMode.EmbeddedInWg;
        }

        internal static string Trim(string text)
        {
            if (Script.IsNullOrUndefined(text))
            {
                return "";
            }

            // Use native String.trim function wherever possible
            Function nativeTrimFunction = typeof(string).Prototype["trim"].As<Function>();
            if (nativeTrimFunction != null)
            {
                string result = nativeTrimFunction.Call(text).As<string>();
                return result;
            }
            else
            {
                // Otherwise use our own trimming functionality
                return text.Replace(RegexTrimLeft, "").Replace(RegexTrimRight, "");
            }
        }

        internal static string ParseJson(JSONStr data)
        {
            // typeof data !== "string" || !data)
            if (Script.IsNullOrUndefined(data) || Script.TypeOf(data) != "string")
            {
                return null;
            }

            // Make sure leading/trailing whitespace is removed (IE can't handle it)
            data = Utility.Trim(data);

            // Use the native JSON parser if it exists
            string result = NativeParseJson(data);
            return result;
        }

        [InlineCode("window.JSON && window.JSON.parse ? window.JSON.parse({data}) : (new Function('return ' + {data}))()")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Inline code")]
        private static string NativeParseJson(JSONStr data)
        {
            return null;
        }

        // XHR Utilities
        // $NOTE-jrockwood-2011-02-08: Assumes that the request is remote (does not use
        // file:// protocol), since IE7 doesn't support that with the XMLHttpRequest.
        internal static object CreateXhr()
        {
            try
            {
                return new XmlHttpRequest();
            }
            catch (Exception)
            {
            }

            try
            {
                return new ActiveXObject("Microsoft.XMLHTTP");
            }
            catch (Exception)
            {
            }

            throw new Exception("XMLHttpRequest not supported");
        }

        // $NOTE-jrockwood-2011-02-08: Assumes that we are in standards mode (have a
        // valid DOCTYPE). This does not need to be called after the window.onload and
        // is available immediately.
        internal static Metric GetViewport()
        {
            Element docElem = (Element)Window.Document.DocumentElement;
            return new Metric(docElem.ClientWidth, docElem.ClientHeight);
        }

        internal static WindowInstance GetTopmostWindow()
        {
            WindowInstance win = Window.Self;

            while (Script.IsValue(win.Parent) && win.Parent != win)
            {
                win = win.Parent;
            }

            return win;
        }

        // $NOTE-adahl-2011-04-23: This routine is here because there was an infinite
        // loop bug on the iPad for non-embedded views where we would continually
        // grow the view. After some investigation, I believe the root cause is that
        // mobile Safari does not enforce sizes of iframes.  You can see why they did
        // this -- they don't want to have embedded scrollbars.  This means the inner
        // iframe will take up as much room as it needs.  For our situation, we would
        // end up the inner part being set to be wider than the outer part and it
        // just starts resizing indefinately.
        // The tact for the fix is to cap the viewport calculation to the physical
        // width of the device so it doesn't keep getting wider.
        internal static Metric GetNonEmbeddedMobileViewport()
        {
            int temp, chromeSpace;
            int w = Window.Document.DocumentElement.ClientWidth;
            int h = Window.Document.DocumentElement.ClientHeight;
            bool isAndroid = (Window.Navigator.UserAgent.IndexOf("Android") != -1);

            if (isAndroid)
            {
                if (w == Window.Screen.Height)
                {
                    chromeSpace = Window.Screen.Width - h;
                    temp = w - chromeSpace;
                    w = h + chromeSpace;
                    h = temp;
                }
            }
            else if (InLandscapeMode)
            {
                // BUG 85078: Work around iOS7/Safari bug where in landscape mode, the clientHeight is getting mis-reported and is
                // too large by 20 units. BUG 148694 Can't set to Windows.InnerHeight directly because it changes when virtual keyboard is visible.
                if ((Window.InnerHeight < h) && NeedsSafari7HackFix)
                {
                    h -= Safari7ClientHeightErrorPixels;
                }

                // Mobile Safari reports screen height and width from a portrait perspective,
                // so we need to adjust based on the orientation.
                if (w == Window.Screen.Width)
                {
                    chromeSpace = Window.Screen.Height - h;
                    temp = w - chromeSpace;
                    w = h + chromeSpace;
                    h = temp;
                }
            }
            else
            {
                if (w == Window.Screen.Height)
                {
                    chromeSpace = Window.Screen.Width - h;
                    temp = w - chromeSpace;
                    w = h + chromeSpace;
                    h = temp;
                }
            }

            return new Metric(w, h);
        }

        // Detect browser support for Canvas, including the Text API (http://caniuse.com/canvas-text)
        // Should be true for IE9+, FF, Chrome, Safari, AndroidBrowser, MobileSafari and false for <=IE8.
        // Indifferently support Opera 10.5+, other mobile/web browsers if they claim support
        // Compiled js, for comparison to http://diveintohtml5.info/detect.html#canvas
        // var canvas = document.createElement('canvas');
        // if (!tabBootstrap.Utility._isValue(canvas, 'getContext')) {
        //     return false;
        // }
        // var context = canvas.getContext('2d');
        // return typeof context.fillText === 'function';
        internal static bool IsCanvasSupported()
        {
            var canvas = Document.CreateElement("canvas");
            if (Script.IsNullOrUndefined(canvas) || Script.IsNullOrUndefined(((dynamic)canvas)["getContext"]))
            {
                return false;
            }

            CanvasRenderingContext context = ((CanvasElement)canvas).GetContext(CanvasContextId.Render2D);
            // add a condition that calling measureText returns something. This was an issue for opera mini - bug 148514
            return TypeUtil.HasMethod(context, "fillText") && Script.IsValue(Script.InvokeMethod(context, "measureText", "foo"));
        }

        public static string HashClientNumber
        {
            get
            {
                JsDictionary<string, string> info = UrlLocationHashData;
                return Script.IsValue(info) && Script.IsValue(info[CLIENTNO]) ? info[CLIENTNO] : "";
            }
        }

        public static void AddToUrlHash(string key, string value)
        {
            JsDictionary<string, string> urlHash = UrlLocationHashData;
            urlHash[key] = value;
            UrlLocationHashData = urlHash;
        }

        public static bool HistoryApiSupported()
        {
            return TypeUtil.HasMethod(Window.History, "pushState") &&
                TypeUtil.HasMethod(Window.History, "replaceState");
        }

        public static void ReplaceState(WindowInstance window, object state, string title, URLStr url)
        {
            try
            {
                window.History.ReplaceState(state, title, url);
            }
            catch (Exception)
            {
                // BUG 76629 - In IE 10, when hosted in the preview window, this method throws as exception
            }
        }

        public static string GetValueFromUrlHash(string key)
        {
            JsDictionary<string, string> urlHash = UrlLocationHashData;
            return urlHash.ContainsKey(key) ? urlHash[key] : "";
        }

        public static void RemoveEntryFromUrlHash(string key)
        {
            JsDictionary<string, string> fragInfo = UrlLocationHashData;
            fragInfo.Remove(key);
            UrlLocationHashData = fragInfo;
        }

        /// <summary>
        /// Gets the pixel ratio of the device. We will fall back to 1.0f if it's not supported on
        /// the browser. This should be safe because at worse, we will not render at high resolution.
        ///
        /// </summary>
        internal static float GetDevicePixelRatio()
        {
            float devicePixelRatio = 1.0f;

            if (Script.IsValue(TsConfig.HighDpi) && TsConfig.HighDpi)
            {
                if (Script.IsValue(TsConfig.PixelRatio))
                {
                    devicePixelRatio = TsConfig.PixelRatio;
                }
                else
                {
                    devicePixelRatio = TypeUtil.GetField<float?>(Window.Self, "devicePixelRatio") ?? 1.0f;
                }
            }

            return devicePixelRatio;
        }

        private static bool IsSuccessStatus(XmlHttpRequest xhr)
        {
            int status = Script.IsValue(xhr.Status) ? xhr.Status : 0;
            if ((status >= 200 && status < 300) ||
                 status == 304 ||  // Not Modified

                 // IE misinterpreting HTTP status code 204 as 1223
                // (http://www.mail-archive.com/jquery-en@googlegroups.com/msg13093.html)
                 status == 1223 ||

                 // IE mangled the status code
                (0 == status && (Window.Location.Protocol == "file:" ||
                                 Window.Location.Protocol == "chrome:")))
            {
                return true;
            }

            return false;
        }

        private static void InvokeErrorDelegate(XmlHttpRequest xhr, Action<object> errBack, Exception e)
        {
            if (errBack == null)
            {
                return;
            }

            Action<object> invokeError = GetInvokeErrorDelegate(xhr, errBack);
            invokeError(e);
        }

        private static Action<object> GetInvokeErrorDelegate(XmlHttpRequest xhr, Action<object> errBack)
        {
            return delegate(dynamic err)
            {
                err.status = xhr.Status;
                err.responseText = xhr.ResponseText;
                errBack(err);
            };
        }
    }
}
