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
    using System.Html;
    using System.Net;
    using System.Net.Messaging;
    using System.Serialization;
    using System.Text.RegularExpressions;
    using TypeDefs;

    /// <summary>
    /// Utility functions for Bootstrap code
    /// </summary>
    // This is public because the test code uses it.
    public static class Utility
    {
        /// <summary>
        /// Gets the WindowInstance that corresponds to the viz location. If in WG then it's the WG parent frame.
        /// Otherwise it's the viz's frame.
        /// </summary>
        public static WindowInstance LocationWindow
        {
            get
            {
                return Window.Self;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        public static void XhrPostJsonChunked(
            URLStr uri,
            string param,
            Action<BootstrapResponse> firstChunkCallback,
            Action<SecondaryBootstrapResponse> secondaryChunkCallback,
            Action<object> errBack,
            bool asynchronous)
        {
            var xhr = new XmlHttpRequest();
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
                    var regex = new Regex(@"^(\d+);");
                    string[] match = newData.Match(regex);
                    if (!Script.IsValue(match)) { return; }

                    int chunkStart = match[0].Length;
                    int chunkLength = Script.ParseInt(match[1]).ReinterpretAs<int>();
                    if (chunkStart + chunkLength > newData.Length) { return; }

                    newData = newData.Substr(chunkStart, chunkLength);
                    object json = null;
                    try
                    {
                        string contextStr = "Parse " + ((byteOffset == 0) ? "Primary" : "Secondary") + " JSON";
                        using (var mc = MetricsController.CreateContext(contextStr, MetricsSuites.Debug))
                        {
                            json = ParseJson<object>(newData);
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
                        firstChunkCallback(json.ReinterpretAs<BootstrapResponse>());
                    }
                    else
                    {
                        secondaryChunkCallback(json.ReinterpretAs<SecondaryBootstrapResponse>());
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
        internal static string XhrGetXmlSynchronous(
            URLStr uri,
            Action<object> errBack)
        {
            var xhr = new XmlHttpRequest();
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

        internal static void XhrPostJson(
            URLStr uri,
            string param,
            Action<object> callback,
            Action<object> errBack,
            bool asynchronous)
        {
            var xhr = new XmlHttpRequest();
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
                        object json = ParseJson<object>(xhr.ResponseText);
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

        /// <summary>
        /// Attaches an event handler for 'message' event, and calls the specified handler
        /// function when event is received. If the passed handler handles the event (returns true) the
        /// event handler will be removed
        /// </summary>
        /// <param name="eventHandler"></param>
        internal static void AttachOneTimeMessageHandler(Func<MessageEvent, bool> eventHandler)
        {
            HtmlEventHandler messageListener = null;
            messageListener = (Event ev) =>
            {
                var e = (MessageEvent)ev;
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

        public static bool DoPostMessage(object message)
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

        internal static T ParseJson<T>(string data)
        {
            // typeof data !== "string" || !data)
            if (Script.IsNullOrUndefined(data) || Script.TypeOf(data) != "string")
            {
                return default(T);
            }

            // Make sure leading/trailing whitespace is removed (IE can't handle it)
            data = data.Trim();

            return Json.Parse<T>(data);
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

        private static bool IsSuccessStatus(XmlHttpRequest xhr)
        {
            int status = Script.IsValue(xhr.Status) ? xhr.Status : 0;
            if ((status >= 200 && status < 300) ||
                 status == 304 || // Not Modified

                 // IE misinterpreting HTTP status code 204 as 1223
                 // (http://www.mail-archive.com/jquery-en@googlegroups.com/msg13093.html)
                 status == 1223 ||

                // IE mangled the status code
                (status == 0 && (Window.Location.Protocol == "file:" ||
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
            return (dynamic err) =>
            {
                // TFS 927164: Passed in exception could be undefined
                err = err ?? new object();

                err.status = xhr.Status;
                err.responseText = xhr.ResponseText;
                errBack(err);
            };
        }
    }
}
