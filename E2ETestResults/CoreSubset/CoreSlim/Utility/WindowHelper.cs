// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowHelper.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.CoreSlim
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Html.Editing;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Contains a set of helper functions for a <see cref="WindowInstance"/>.
    /// </summary>
    public class WindowHelper
    {
        //// ===========================================================================================================
        //// Member Variables
        //// ===========================================================================================================

        private static readonly Func<WindowInstance, int> InnerWidthFunc;

        private static readonly Func<WindowInstance, int> InnerHeightFunc;

        private static readonly Func<WindowInstance, int> ClientWidthFunc;

        private static readonly Func<WindowInstance, int> ClientHeightFunc;

        private static readonly Func<WindowInstance, int> PageXOffsetFunc;

        private static readonly Func<WindowInstance, int> PageYOffsetFunc;

        private static readonly Func<WindowInstance, int> ScreenLeftFunc;

        private static readonly Func<WindowInstance, int> ScreenTopFunc;

        private static readonly Func<WindowInstance, int> OuterWidthFunc;

        private static readonly Func<WindowInstance, int> OuterHeightFunc;

        private static Func<Action, int> requestAnimationFrameFunc;
        private static Action<int> cancelAnimationFrameFunc;

        private readonly WindowInstance window;

        static WindowHelper()
        {
            // polyfills for various window properties - http://www.quirksmode.org/mobile/viewports2.html

            // InnerWidth
            if (Script.In(typeof(Window), "innerWidth"))
            {
                InnerWidthFunc = delegate(WindowInstance w) { return w.InnerWidth; };
            }
            else
            {
                InnerWidthFunc = delegate(WindowInstance w) { return Script.Reinterpret<HtmlElement>(w.Document.DocumentElement).OffsetWidth; };
            }

            // OuterWidth
            if (Script.In(typeof(Window), "outerWidth"))
            {
                OuterWidthFunc = delegate(WindowInstance w) { return w.OuterWidth; };
            }
            else
            {
                // TODO: This is not correct, but it'll do for now. B147312
                OuterWidthFunc = InnerWidthFunc;
            }

            // InnerHeight
            if (Script.In(typeof(Window), "innerHeight"))
            {
                InnerHeightFunc = delegate(WindowInstance w) { return w.InnerHeight; };
            }
            else
            {
                InnerHeightFunc = delegate(WindowInstance w) { return Script.Reinterpret<HtmlElement>(w.Document.DocumentElement).OffsetHeight; };
            }

            // OuterHeight
            if (Script.In(typeof(Window), "outerHeight"))
            {
                OuterHeightFunc = delegate(WindowInstance w) { return w.OuterHeight; };
            }
            else
            {
                // TODO: This is not correct, but it'll do for now. B147312
                OuterHeightFunc = InnerHeightFunc;
            }

            // ClientWidth
            if (Script.In(typeof(Window), "clientWidth"))
            {
                ClientWidthFunc = delegate(WindowInstance w) { return TypeUtil.GetField<int>(w, "clientWidth"); };
            }
            else
            {
                ClientWidthFunc = delegate(WindowInstance w) { return w.Document.DocumentElement.ClientWidth; };
            }

            // ClientHeight
            if (Script.In(typeof(Window), "clientHeight"))
            {
                ClientHeightFunc = delegate(WindowInstance w) { return TypeUtil.GetField<int>(w, "clientHeight"); };
            }
            else
            {
                ClientHeightFunc = delegate(WindowInstance w) { return w.Document.DocumentElement.ClientHeight; };
            }

            // PageXOffsetFunc
            if (Script.IsValue(Window.Self.PageXOffset))
            {
                PageXOffsetFunc = delegate(WindowInstance w) { return w.PageXOffset; };
            }
            else
            {
                PageXOffsetFunc = delegate(WindowInstance w) { return w.Document.DocumentElement.ScrollLeft; };
            }

            // PageYOffsetFunc
            if (Script.IsValue(Window.Self.PageYOffset))
            {
                PageYOffsetFunc = delegate(WindowInstance w) { return w.PageYOffset; };
            }
            else
            {
                PageYOffsetFunc = delegate(WindowInstance w) { return w.Document.DocumentElement.ScrollTop; };
            }

            // ScreenLeftFunc
            if (Script.In(typeof(Window), "screenLeft"))
            {
                ScreenLeftFunc = delegate(WindowInstance w) { return ((dynamic)w).screenLeft; };
            }
            else
            {
                ScreenLeftFunc = delegate(WindowInstance w) { return w.ScreenX; };
            }

            // ScreenTopFunc
            if (Script.In(typeof(Window), "screenTop"))
            {
                ScreenTopFunc = delegate(WindowInstance w) { return ((dynamic)w).screenTop; };
            }
            else
            {
                ScreenTopFunc = delegate(WindowInstance w) { return w.ScreenY; };
            }

            // Polyfill for RequestAnimationFrame - http://paulirish.com/2011/requestanimationframe-for-smart-animating/
            {
                const string DefaultRequestName = "requestAnimationFrame";
                const string DefaultCancelName = "cancelAnimationFrame";
                string[] vendors = new string[] { "ms", "moz", "webkit", "o" };
                string requestFuncName = null;
                string cancelFuncName = null;

                if (Script.In(typeof(Window), DefaultRequestName))
                {
                    requestFuncName = DefaultRequestName;
                }

                if (Script.In(typeof(Window), DefaultCancelName))
                {
                    cancelFuncName = DefaultCancelName;
                }

                for (int ii = 0; ii < vendors.Length && (requestFuncName == null || cancelFuncName == null); ++ii)
                {
                    string vendor = vendors[ii];
                    string funcName = vendor + "RequestAnimationFrame";
                    if (requestFuncName == null && Script.In(typeof(Window), funcName))
                    {
                        requestFuncName = funcName;
                    }

                    if (cancelFuncName == null)
                    {
                        //string cancelFuncName = (Func<Action<int>, int>)Script.Literal("window[{0}]", requestFuncName);
                        funcName = vendor + "CancelAnimationFrame";
                        if (Script.In(typeof(Window), funcName))
                        {
                            cancelFuncName = funcName;
                        }
                        funcName = vendor + "CancelRequestAnimationFrame";
                        if (Script.In(typeof(Window), funcName))
                        {
                            cancelFuncName = funcName;
                        }
                    }
                }

                if (requestFuncName != null)
                {
                    requestAnimationFrameFunc = delegate(Action callback)
                    {
                        return TypeUtil.GetField<Func<Action, int>>(typeof(Window), requestFuncName)(callback);
                    };
                }
                else
                {
                    SetDefaultRequestAnimationFrameImpl();
                }

                if (cancelFuncName != null)
                {
                    cancelAnimationFrameFunc = delegate(int animationId)
                    {
                        TypeUtil.GetField<Action<int>>(typeof(Window), cancelFuncName)(animationId);
                    };
                }
                else
                {
                    cancelAnimationFrameFunc = delegate(int id) { Window.ClearTimeout(id); };
                }
            }
        }

        //// ===========================================================================================================
        //// Constructors
        //// ===========================================================================================================

        public WindowHelper(WindowInstance window)
        {
            this.window = window;
        }

        //// ===========================================================================================================
        //// Properties
        //// ===========================================================================================================

        /// <summary>
        /// Gets the window pageXOffset, or equivalent.
        /// </summary>
        public int PageXOffset
        {
            get
            {
                return PageXOffsetFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window pageYOffset, or equivalent.
        /// </summary>
        public int PageYOffset
        {
            get
            {
                return PageYOffsetFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window pageXOffset, or equivalent.
        /// </summary>
        public int ClientWidth
        {
            get
            {
                return ClientWidthFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window pageYOffset, or equivalent.
        /// </summary>
        public int ClientHeight
        {
            get
            {
                return ClientHeightFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window innerWidth, or equivalent.
        /// </summary>
        public int InnerWidth
        {
            get
            {
                return InnerWidthFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window outerWidth, or innerWidth if unavailable
        /// </summary>
        public int OuterWidth
        {
            get
            {
                return OuterWidthFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window innerHeight, or equivalent.
        /// </summary>
        public int InnerHeight
        {
            get
            {
                return InnerHeightFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window outerHeight, or innerHeight if unavailable.
        /// </summary>
        public int OuterHeight
        {
            get
            {
                return OuterHeightFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window screenLeft, or equivalent.
        /// </summary>
        public int ScreenLeft
        {
            get
            {
                return ScreenLeftFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window screenTop, or equivalent
        /// </summary>
        public int ScreenTop
        {
            get
            {
                return ScreenTopFunc(this.window);
            }
        }

        /// <summary>
        /// Gets the window's self.
        /// This method only exists to support testing.
        /// </summary>
        public static WindowInstance WindowSelf
        {
            get
            {
                return Window.Self;
            }
        }

        /// <summary>
        /// Get the current window or document Selection object
        /// </summary>
        /// <returns>The selection object, or null if none</returns>
        public static Selection Selection
        {
            get
            {
                if (TypeUtil.HasMethod(typeof(Window), "getSelection"))
                {
                    return Window.GetSelection();
                }
                else if (TypeUtil.HasMethod(typeof(Document), "getSelection"))
                {
                    return Document.GetSelection();
                }

                return null;
            }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Calls <see cref="Window.Close"/> on the given window.
        /// This method only exists to support testing.
        /// </summary>
        public static void Close(WindowInstance window)
        {
            window.Close();
        }

        /// <summary>
        /// Gets the window opener.
        /// This method only exists to support testing.
        /// </summary>
        public static WindowInstance GetOpener(WindowInstance window)
        {
            return window.Opener;
        }

        /// <summary>
        /// Gets the current window location.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        public static Location GetLocation(WindowInstance window)
        {
            return window.Location;
        }

        /// <summary>
        /// Gets the current window location.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        public static string GetPathAndSearch(WindowInstance window)
        {
            return window.Location.Pathname + window.Location.Search;
        }

        /// <summary>
        /// Sets the href property on the window's location.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        /// <param name="window">The window containing the location</param>
        /// <param name="href">The href to set</param>
        public static void SetLocationHref(WindowInstance window, string href)
        {
            window.Location.Href = href;
        }

        /// <summary>
        /// Calls <see cref="Location.Replace"/> on the given window.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        public static void LocationReplace(WindowInstance window, string url)
        {
            window.Location.Replace(url);
        }

        [AlternateSignature]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static extern WindowInstance Open(string href, string target);

        /// <summary>
        /// Calls <see cref="Window.Open(string,string,string)"/>.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        /// <param name="href"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        public static WindowInstance Open(string href, string target, string options)
        {
            return Window.Open(href, target, options);
        }

        /// <summary>
        /// Calls window.location.reload.
        /// This method only exists to support testing as the standard window.location cannot be mocked safely.
        /// </summary>
        public static void Reload(WindowInstance w, bool forceGet = false)
        {
            w.Location.Reload(forceGet);
        }

        /// <summary>
        /// Requests an animation frame
        /// Falls back to using Window.SetTimeout if the browser does not support RequestAnimationFrame
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <returns>animation id used to cancel the animation</returns>
        public static int RequestAnimationFrame(Action action)
        {
            return requestAnimationFrameFunc(action);
        }

        /// <summary>
        /// Cancels an animation
        /// </summary>
        /// <param name="animationId">id of animation to cancel</param>
        public static void CancelAnimationFrame(int animationId)
        {
            if (Script.IsValue(animationId))
            {
                cancelAnimationFrameFunc(animationId);
            }
        }

        /// <summary>
        /// This method exists so that Window.SetTimeout can be mocked
        /// </summary>
        public static void SetTimeout(Action callback, int milliseconds)
        {
            Window.SetTimeout(callback, milliseconds);
        }

        public static void AddListener(EventTarget windowParam, string eventName, HtmlEventHandler messageListener)
        {
            if (Script.In(windowParam, "addEventListener"))
            {
                windowParam.AddEventListener(eventName, messageListener, false);
            }
            else
            {
                ((dynamic)windowParam).attachEvent("on" + eventName, messageListener);
            }
        }

        public static void RemoveListener(EventTarget window, string eventName, HtmlEventHandler messageListener)
        {
            if (Script.In(window, "removeEventListener"))
            {
                window.RemoveEventListener(eventName, messageListener, false);
            }
            else
            {
                ((dynamic)window).detachEvent("on" + eventName, messageListener);
            }
        }

        /// <summary>
        /// Sets the RequestAnimationFrame implementation to the fallback if there is no browser support.
        /// This is in its own method so the closure over 'lastTime' is smaller.
        /// </summary>
        private static void SetDefaultRequestAnimationFrameImpl()
        {
            int lastTime = 0;
            requestAnimationFrameFunc = delegate(Action callback)
            {
                int curTime = (int)(new JsDate().GetTime());
                int timeToCall = Math.Max(0, 16 - (curTime - lastTime));
                lastTime = curTime + timeToCall;
                int id = Window.SetTimeout(callback, timeToCall);
                return id;
            };
        }

        /// <summary>
        /// Clears the selection object at the window/document level
        /// </summary>
        public static void ClearSelection()
        {
            var selection = Selection;

            if (selection != null)
            {
                if (TypeUtil.HasMethod(selection, "removeAllRanges"))
                {
                    selection.RemoveAllRanges();
                }
                else if (TypeUtil.HasMethod(selection, "empty"))
                {
                    Script.InvokeMethod(selection, "empty");
                }
            }
        }
    }
}
