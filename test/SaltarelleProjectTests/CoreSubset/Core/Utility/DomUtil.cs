// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="DomUtil.cs" company="Tableau Software">
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
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Html;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Xml;
    using jQueryApi;
    using Tableau.JavaScript.Vql.TypeDefs;
    using UnderscoreJs;

    /// <summary>
    /// Contains utility methods for DOM.
    /// </summary>
    public static class DomUtil
    {
        private const string TestWaitAttributeName = "data-tab-test-wait";

        // used by GetTransformOffset, StyleCop forces us to put it up here
        // see https://developer.mozilla.org/en-US/docs/Web/CSS/transform-function#matrix%28%2
        private static readonly JsDictionary<string, int> translationFuncIndexer = new JsDictionary<string, int>(
            "matrix", 4, "matrix3d", 12, "translate", 0, "translate3d", 0, "translateX", 0, "translateY", -1);

        private static long uniqueId = new JsDate().GetTime();

        private static Logger Log
        {
            get { return Logger.LazyGetLogger(typeof(DomUtil)); }
        }

        /// <summary>
        /// Document Body element. Intended to be used in places where a fake body needs to be returned in tests
        /// </summary>
        public static Element DocumentBody
        {
            get { return Document.Body; }
        }

        //// ===========================================================================================================
        //// Methods
        //// ===========================================================================================================

        /// <summary>
        /// Gives the final used values of all the CSS properties of an element.
        /// https://developer.mozilla.org/en-US/docs/DOM/window.getComputedStyle
        /// </summary>
        public static Style GetComputedStyle(Element e)
        {
            if (BrowserSupport.GetComputedStyle())
            {
                Style s = Window.GetComputedStyle(e);
                if (Script.IsValue(s))
                {
                    return s;
                }
            }

            Log.Warn("Calling GetComputedStyle but is unsupported");
            return e.Style;
        }

        /// <summary>
        /// Gets the computed z-index of the given element.  This is done by querying the CSS z-index property of this
        /// element and absolutely positioned ancestors, finding the z-index of the last before root. Handles the case
        /// where IE7 reports an "auto" z-index property value as "0".
        /// </summary>
        /// <param name="child">The child element to calculate a z-index for</param>
        /// <returns>The computed z-index of the element, defaults to 0</returns>
        public static int GetComputedZIndex(Element child)
        {
            // Given a child element
            Param.VerifyValue(child, "child");

            // Start with the child
            jQueryObject iter = jQuery.FromElement(child);
            jQueryObject lastPositioned = iter;
            Element html = Document.DocumentElement;
            Element body = Document.Body;

            // Iterate upward, stop when reaching the root
            while (iter.Length != 0 && iter[0] != body && iter[0] != html)
            {
                string pos = iter.GetCSS("position");
                if (pos == "absolute" || pos == "fixed")
                {
                    lastPositioned = iter; // store the last absolutely positioned element prior to reaching the root
                }

                iter = iter.OffsetParent(); // OffsetParent finds the closest _positioned_ ancestor, otherwise returns <html> or <body>
            }
            return ParseZIndexProperty(lastPositioned); // returns 0 by default
        }

        /// <summary>
        /// Ported from tableau.util.resize.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="rect"></param>
        public static void Resize(dynamic e, Rect rect)
        {
            if (TypeUtil.HasMethod(e, "resize"))
            {
                e.resize(rect);
            }
            else
            {
                SetMarginBox((Element)ScriptEx.Value(e.domNode, e), rect);
            }
        }

        /// <summary>
        /// Gets the equivalent of dojo.contentBox(e).
        /// </summary>
        public static Rect GetContentBox(Element e)
        {
            jQueryObject obj = jQuery.FromElement(e);
            return new Rect(
                ScriptEx.Value(Script.ParseInt(obj.GetCSS("padding-left"), 10).ReinterpretAs<int>(), 0),
                ScriptEx.Value(Script.ParseInt(obj.GetCSS("padding-top"), 10).ReinterpretAs<int>(), 0),
                obj.GetWidth().RoundToInt(),
                obj.GetHeight().RoundToInt());
        }

        /// <summary>
        /// Sets the equivalent of dojo.contentBox(e, rect).
        /// </summary>
        public static void SetContentBox(Element e, Rect r)
        {
            jQuery.FromElement(e)
                .Width(r.Width)
                .Height(r.Height);
        }

        /// <summary>
        /// Sets the equivalent of dojo.marginBox(e, rect).
        /// </summary>
        [AlternateSignature, SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
        public static extern void SetMarginBox(Element e, Size size);

        /// <summary>
        /// Sets the equivalent of dojo.marginBox(e, rect).
        /// </summary>
        [PreserveName]
        public static void SetMarginBox(Element e, Rect r)
        {
            SetMarginBoxJQ(jQuery.FromElement(e), r);
        }

        /// <summary>
        /// Sets the equivalent of dojo.marginBox(o, rect).
        /// </summary>
        public static void SetMarginBoxJQ(jQueryObject o, Rect r)
        {
            Element rawElement = o[0];
            Style elementStyle = rawElement.Style;
            Style computedStyle = Window.GetComputedStyle(rawElement);
            DomUtil.SetMarginSizeJQ(computedStyle, r.ReinterpretAs<Size>(), rawElement);

            if (!double.IsNaN(r.Top))
            {
                elementStyle.Top = r.Top + "px";
            }

            if (!double.IsNaN(r.Left))
            {
                elementStyle.Left = r.Left + "px";
            }
        }

        /// <summary>
        /// Set the top, left, width and height styles based on the given rect to position the given jquery object
        /// </summary>
        public static void SetAbsolutePositionBox(jQueryObject o, Rect r)
        {
            o.CSS(new CssDictionary
            {
                Left = ScriptEx.Value(r.Left, 0).AsPx(),
                Top = ScriptEx.Value(r.Top, 0).AsPx(),
                Width = ScriptEx.Value(r.Width, 0).AsPx(),
                Height = ScriptEx.Value(r.Height, 0).AsPx()
            });
        }

        /// <summary>
        /// Gets the equivalent of dojo.marginBox(e).
        /// </summary>
        public static Rect GetMarginBox(Element e)
        {
            return DomUtil.GetMarginBoxJQ(jQuery.FromElement(e));
        }

        /// <summary>
        /// Gets the equivalent of dojo.marginBox(e).
        /// </summary>
        public static Rect GetMarginBoxJQ(jQueryObject o)
        {
            jQueryPosition p = o.Position();
            return new Rect(
                p.Left.RoundToInt(),
                p.Top.RoundToInt(),
                o.GetOuterWidth(true).RoundToInt(),
                o.GetOuterHeight(true).RoundToInt());
        }

        /// <summary>
        /// Gets the location and area of the element relative to the document itself.  I.e. in the coordinate space
        /// of the document, not the viewport.  So it will be the same no matter how much you have scrolled or zoomed
        /// in.
        /// </summary>
        /// <remarks>This is basically equivalent to the old dojo.coords(e) function.</remarks>
        public static RectXY GetRectXY(jQueryObject o)
        {
            int x = o.GetPageOffset().Left.RoundToInt();
            int y = o.GetPageOffset().Top.RoundToInt();
            int w = o.GetOuterWidth(true).RoundToInt();
            int h = o.GetOuterHeight(true).RoundToInt();

            return new RectXY(x, y, w, h);
        }

        /// <summary>
        /// Similar to GetRectXY (above),
        /// but it also supports includeScroll parameter
        /// (also ported from Dojo)
        /// </summary>
        public static RectXY GetRectXY(jQueryObject o, bool includeScroll)
        {
            RectXY result = GetRectXY(o);
            if (includeScroll)
            {
                Point scroll = DocScroll();
                result.X += scroll.X;
                result.Y += scroll.Y;
            }

            return result;
        }

        /// <summary>
        /// Port of dojo's docScroll function
        /// http://dojotoolkit.org/reference-guide/1.10/dojo/dom-geometry/docScroll.html
        /// </summary>
        /// <returns>
        /// Returns a normalized object with {x, y} with corresponding offsets for the scroll position for the current document.
        /// </returns>
        public static Point DocScroll()
        {
            int x = Script.Coalesce(Window.PageXOffset,
                Script.Coalesce(Document.DocumentElement.ScrollLeft, Script.Coalesce(DocumentBody.ScrollLeft, 0)));

            int y = Script.Coalesce(Window.PageYOffset,
                Script.Coalesce(Document.DocumentElement.ScrollTop, Script.Coalesce(DocumentBody.ScrollTop, 0)));

            return new Point(x, y);
        }

        /// <summary>
        /// return true if 'ancestor' is the ancestor of 'child'
        /// </summary>
        /// <param name="ancestor">DOM element of the ancestor</param>
        /// <param name="child">DOM element of the child</param>
        /// <returns>return true if 'ancestor' is the ancestor or 'child'</returns>
        [SuppressMessage("Microsoft.Performance", "CA1811", Justification = "Function to be called by other script")]
        public static bool IsAncestorOf(Element ancestor, Element child)
        {
            if (Script.IsNullOrUndefined(ancestor) || Script.IsNullOrUndefined(child))
            {
                return false;
            }

            return jQuery.FromElement(child).Parents().Index(ancestor) >= 0;
        }

        /// <summary>
        /// Tests whether me is the testElement or if testElement is a child of me
        /// </summary>
        /// <returns>true if me is the given testElement or if testElement is a child of me</returns>
        public static bool IsEqualOrAncestorOf(Element ancestor, Element child)
        {
            if (Script.IsNullOrUndefined(ancestor) || Script.IsNullOrUndefined(child))
            {
                return false;
            }

            return (ancestor == child || IsAncestorOf(ancestor, child));
        }

        /// <summary>
        /// Sets an elements position using either CSS transform or absolute positioning, depending on what
        /// is supported.  Assumes element is added to body.
        /// </summary>
        public static void SetElementPosition(jQueryObject e, int pageX, int pageY, string duration = null, bool? useTransform = null)
        {
            if ((!useTransform.HasValue || useTransform.Value))
            {
                // This prevents visibility: hidden elements from displacing the translation.
                var styling = new JsDictionary("top", "0px", "left", "0px");

                string transformVal = new StringBuilder("translate3d(")
                    .Append(pageX).Append("px,")
                    .Append(pageY).Append("px,")
                    .Append("0px)").ToString();
                styling[BrowserSupport.CssTransformName()] = transformVal;
                if (Script.IsValue(duration))
                {
                    styling[BrowserSupport.CssTransitionName() + "-duration"] = duration;
                }
                e.CSS(styling);
                return;
            }
            var css = new JsDictionary("position", "absolute", "top", pageY + "px", "left", pageX + "px");
            css[BrowserSupport.CssTransformName()] = "";
            e.CSS(css);
        }

        /// <summary>
        /// Gets the page position for an element -- NOTE: rounds to ints
        /// </summary>
        /// <param name="e">The element whose page position is needed</param>
        /// <returns>a Point with the X and Y offsets from the left/top of the page</returns>
        public static Point GetElementPosition(jQueryObject e)
        {
            return PointUtil.FromPosition(e.GetOffset());
        }

        /// <summary>
        /// Gets an absolute/client position for an element -- NOTE: rounds to ints
        /// </summary>
        /// <param name="e">The element whose client position is needed</param>
        /// <returns>a Point with the X and Y offsets from the left/top of the client window</returns>
        public static Point GetElementClientPosition(jQueryObject e)
        {
            Point p = GetElementPosition(e);

            // Getting the element position relative to the body results in page-based coordinates,
            // so we need to subtract out any scroll position for the page to get the client coordinates.
            p.X -= jQuery.FromElement((Element)Document.DocumentElement).GetScrollLeft();
            p.Y -= jQuery.FromElement((Element)Document.DocumentElement).GetScrollTop().RoundToInt();

            return p;
        }

        /// <summary>
        /// Extracts (only) any CSS-"transform" based 2d translation. (ignores any Z-component) n.b. ASSUMES PIXEL VALUES (i.e. even "12em" means "12px"!)
        /// </summary>
        public static jQueryPosition GetTransformOffset(jQueryObject element)
        {
            if (element == null)
            {
                Log.Warn("Attempting to get transformation on null element!");
                return new jQueryPosition(0, 0);
            }

            string fullTransform = element.GetCSS("transform");

            // Our transform may not exist, just return early.
            if (string.IsNullOrEmpty(fullTransform))
            {
                return new jQueryPosition(0, 0);
            }

            string[] transform = fullTransform.Split("(");

            int? index = translationFuncIndexer[transform[0]];
            if (index == null)
            {
                return new jQueryPosition(0, 0);
            }

            string[] vals = transform[1].Split(",");
            return new jQueryPosition(
                // we rely on the nulling behavior of DoubleUtil.ParseDouble in the case that index is negative
                DoubleUtil.ParseDouble(vals[index.Value]) ?? 0,      // x
                DoubleUtil.ParseDouble(vals[index.Value + 1]) ?? 0); // y
        }

        /// <summary>
        /// Extracts the scaling factor from a CSS transform -- assumes symmetric scaling between x- and y-axes
        /// </summary>
        /// <returns>the extracted scaling factor from 'transform' if present, otherwise 1</returns>
        public static double GetTransformScale(jQueryObject element)
        {
            if (element == null)
            {
                Log.Warn("Attempting to get transformation on null element!");
                return 1;
            }

            string fullTransform = element.GetCSS("transform");
            if (string.IsNullOrEmpty(fullTransform))
            {
                return 1;
            }

            string[] transform = fullTransform.Split("(");

            if (transform[0] == "scale" || transform[0] == "matrix" || transform[0] == "matrix3d")
            {
                return DoubleUtil.ParseDouble(transform[1]) ?? 1;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Returns the offset of the element relative to the document, without integer-truncating
        /// any results. Works when pinch-to-zoomed as well, unlike normal Offset()
        /// </summary>
        public static jQueryPosition GetPageOffset(this jQueryObject e)
        {
            PointD pageOffset = e[0].GetPageOffset();

            return new jQueryPosition(pageOffset.X, pageOffset.Y);
        }

        /// <summary>
        /// Non-jQuery version of <see cref="GetPageOffset(jQueryObject)"/>.
        /// </summary>
        public static PointD GetPageOffset(this XmlElement e)
        {
            if (e == null)
            {
                Debug.Assert(false, "Tried to getPageOffset of null element");
                return new PointD(0, 0);
            }

            // 2017-04-17: You may be tempted to just return window.pageX/YOffset + elementRect, but this gives bad values for
            // some browsers (currently, this means Chrome and Edge) when pinched-to-zoom.
            // The reason is that, under their viewport models, getBoundingClientRect and window.pageX/YOffset refer to different
            // coordinate systems (but this will likely change in the future to be the same coordinate system).
            // See https://bugs.chromium.org/p/chromium/issues/detail?id=489206 for more context.
            // So, instead, we make sure we're always working in the same coordinate system by comparing the element's client
            // rect with the document element's client rect.
            DOMRect elementRect = e.GetBoundingClientRect();
            DOMRect documentElementRect = Document.DocumentElement.GetBoundingClientRect();

            return new PointD(elementRect.Left - documentElementRect.Left, elementRect.Top - documentElementRect.Top);
        }

        public static jQueryPosition GetScrollPosition(this jQueryObject o)
        {
            return new jQueryPosition(left: o[0].ScrollLeft, top: o[0].ScrollTop);
        }

        public static void ScrollPosition(this jQueryObject o, jQueryPosition pos)
        {
            o[0].ScrollLeft = pos.Left.ReinterpretAs<int>();
            o[0].ScrollTop = pos.Top.ReinterpretAs<int>();
        }

        /// <summary>
        /// Sets focus to a child node and resets the scroll position of its scrollable container afterward.
        /// The user-facing effect is that focus can be set on a child element without the default "scroll to center" browser behavior.
        /// </summary>
        /// <param name="focusMethod">Callback that sets focus on the relevant child node</param>
        /// <param name="scrollNode">The scrollable ancestor node of the child</param>
        public static void FocusWithoutScrolling(Action focusMethod, jQueryObject scrollNode)
        {
            if (scrollNode == null || scrollNode.Length == 0)
            {
                focusMethod();
            }
            else
            {
                scrollNode.Attribute(TestWaitAttributeName, "");
                Window.SetTimeout(() =>
                    {
                        var scrollPos = scrollNode.GetScrollPosition();
                        focusMethod();
                        scrollNode.ScrollPosition(scrollPos);
                        scrollNode.RemoveAttr(TestWaitAttributeName);
                    },
                    200); // TFSID 618960: allow some time for scrolling animation before reading the new scroll position
            }
        }

        /// <summary>
        /// Returns the available viewport space around a position. Does not account for parent-frame scrolling,
        /// for that use the SpiffBrowserViewport callbacks.
        /// </summary>
        public static VisibleRoom RoomAroundPosition(jQueryPosition p)
        {
            double roomAbove = p.Top - Window.PageYOffset;
            double roomBelow = Window.PageYOffset + Window.InnerHeight - p.Top;
            double roomLeft = p.Left - Window.PageXOffset;
            double roomRight = Window.PageXOffset + Window.InnerWidth - p.Left;

            return new VisibleRoom(roomAbove, roomBelow, roomLeft, roomRight);
        }

        /// <summary>
        /// Calculates the difference in position between an element and a parent/ancestor element
        /// </summary>
        /// <param name="e">The element whose offset is needed</param>
        /// <param name="p">The element that should be used as the baseline for the offset</param>
        /// <returns>a Point with the X and Y distances between the two elements</returns>
        public static Point GetElementRelativePosition(jQueryObject e, jQueryObject p)
        {
            if (Script.IsNullOrUndefined(p))
            {
                p = e.Parent();
            }

            jQueryPosition ep = e.GetOffset();
            jQueryPosition pp = p.GetOffset();

            return new Point(ep.Left.RoundToInt() - pp.Left.RoundToInt(), ep.Top.RoundToInt() - pp.Top.RoundToInt());
        }

        /// <summary>
        /// Parse the width from a css style into an integer.
        /// Returns NaN if the width style doesn't have numbers in it.
        /// </summary>
        public static int ParseWidthFromStyle(Style style)
        {
            if (Script.IsValue(style) && !MiscUtil.IsNullOrEmpty(style.Width))
            {
                return Script.ParseInt(style.Width).ReinterpretAs<int>();
            }

            return JsNumber.NaN;
        }

        /// <summary>
        /// Parse the height from a css style into an integer.
        /// Returns NaN if the height style doesn't have numbers in it.
        /// </summary>
        public static int ParseHeightFromStyle(Style style)
        {
            if (Script.IsValue(style) && !MiscUtil.IsNullOrEmpty(style.Height))
            {
                return Script.ParseInt(style.Height).ReinterpretAs<int>();
            }

            return JsNumber.NaN;
        }

        /// <summary>
        /// Creates a jQuery namespaced event name, of the following form:
        ///   eventName.instanceId_className
        /// For example, keydown.1_Dialog, where eventName is keydown and eventNamespace is ".1_Dialog"
        /// </summary>
        /// <param name="eventName">The browser event name.</param>
        /// <param name="eventNamespace">The namespace to be appended.</param>
        /// <returns>A jQuery namespaced event name if the eventNamespace has a value, otherwise the eventName with no change</returns>
        public static string CreateNamespacedEventName(BrowserEventName eventName, string eventNamespace)
        {
            if (Script.IsValue(eventNamespace))
            {
                return eventName + eventNamespace;
            }
            return eventName.ToString();
        }

        /// <summary>
        /// Stop propagation of mouse and touch input events.
        /// </summary>
        /// <param name="o">The element on which we should stop propagation.</param>
        /// <param name="eventNamespace">The event namespace to be used in the binding.</param>
        public static void StopPropagationOfInputEvents(jQueryObject o, string eventNamespace)
        {
            jQueryEventHandler stopPropagation = (jQueryEvent e) => e.StopPropagation();
            HandleInputEvents(o, eventNamespace, stopPropagation);
        }

        /// <summary>
        /// Adds a handler for all mouse and touch input events on the object.
        /// </summary>
        /// <param name="o">The element on which we should bind to events</param>
        /// <param name="eventNamespace">The event namespace to be used in the binding.</param>
        /// <param name="handler">The event handler method that will be called for each input event.</param>
        public static void HandleInputEvents(jQueryObject o, string eventNamespace, jQueryEventHandler handler)
        {
            o.On(CreateNamespacedEventName(BrowserEventName.TouchStart, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.TouchCancel, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.TouchEnd, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.TouchMove, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.Click, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.MouseDown, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.MouseMove, eventNamespace), handler)
             .On(CreateNamespacedEventName(BrowserEventName.MouseUp, eventNamespace), handler);
        }

        /// <summary>
        /// Returns true if domElement is a focusable input element
        /// </summary>
        /// <param name="domElement">The domElement</param>
        /// <returns>true or false</returns>
        public static bool IsFocusableTextElement(XmlElement domElement)
        {
            if (Script.IsValue(domElement) && Script.IsValue(domElement.TagName))
            {
                string targetTagName = domElement.TagName.ToLowerCase();
                if ((targetTagName == "textarea") || (targetTagName == "input") || (targetTagName == "select"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if domElement is a checkBox, else false
        /// </summary>
        /// <param name="domElement">The domElement</param>
        /// <returns>true or false</returns>
        public static bool IsCheckboxElement(Element domElement)
        {
            if (Script.IsValue(domElement) && Script.IsValue(domElement.TagName))
            {
                string targetTagName = domElement.TagName.ToLowerCase();
                string typeAttributeValue = jQuery.FromElement(domElement).GetAttribute("type");
                if (targetTagName == "input" && typeAttributeValue == "checkbox")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if input events on domElement should be handled as touchEvents, else false.
        /// </summary>
        /// <param name="domElement">The domElement that is the target of mouse/touch events</param>
        /// <returns>true or false</returns>
        public static bool HandleTouchEvents(Element domElement)
        {
            // If target is a checkbox, return false because checkboxes do not record the check/uncheck on
            // touchend if PreventDefault() is called. So when the target of an input event is a checkbox,
            // we must let mouse events fire in order for the check/uncheck to take effect.
            // Note: other inputs (input/textarea/select) seem unaffected by PreventDefault() being called.
            // Also, oddly, with mouse events, preventDefault() does not keep a checkbox from getting checked.
            if (IsCheckboxElement(domElement))
            {
                return false;
            }

            // If target is a focusable text element, return false because we need to avoid calling PreventDefault()
            // in order to let them have focus when we click or tap on them.
            if (IsFocusableTextElement(domElement))
            {
                return false;
            }

            // If we get to here, ok to handle touch events on this domElement.
            return true;
        }

        /// <summary>
        /// If supported calls setCapture on the given Element.  No-op if unsupported.
        /// https://developer.mozilla.org/en-US/docs/Web/API/Element.setCapture
        /// </summary>
        /// <param name="e">The capturing element</param>
        /// <param name="retargetToElement">If true, all events are targeted directly to this element; if
        /// false, events can also fire at descendants of this element.</param>
        public static void SetCapture(XmlElement e, bool retargetToElement)
        {
            if (!BrowserSupport.MouseCapture())
            {
                return;
            }
            e.SetCapture(retargetToElement);
        }

        /// <summary>
        /// Calls document.releaseCapture.  No-op if unsupported.
        /// https://developer.mozilla.org/en-US/docs/Web/API/document.releaseCapture
        /// </summary>
        public static void ReleaseCapture()
        {
            if (!BrowserSupport.MouseCapture())
            {
                return;
            }
            Document.ReleaseCapture();
        }

        /// <summary>
        /// calls Blur on the document's active element
        /// </summary>
        public static void Blur()
        {
            // we don't blur if active element is document.body
            // https://connect.microsoft.com/IE/feedback/details/790929/document-activeelement-blur
            // $NOTE-dscheidt-2016-09-08: IE11 has been seen occasionally returning an empty javascript object
            // instead of the document body or null when querying for the activeElement from within an iFrame.
            // This behavior is the cause of TFSID:558859, and is mitigated by the HasMethod check.

            var activeElem = Document.ActiveElement;
            if (Script.IsValue(activeElem) && activeElem != DocumentBody && TypeUtil.HasMethod(activeElem, "blur"))
            {
                activeElem.Blur();
            }
        }

        private static int ConvertCssToInt(string cssValue, int defaultValue)
        {
            int x = Script.ParseInt(cssValue, 10).ReinterpretAs<int>();
            return float.IsNaN(x) ? defaultValue : x;
        }

        public static int GetSizeFromCssPixelProperty(jQueryObject element, string propertyName)
        {
            string strValue = element.GetCSS(propertyName);
            return ConvertCssToInt(strValue, 0);
        }

        // These provide a safe (though slow) "outer" (including margin) setters
        // Dojo provided this functionality, jQuery does not
        private static void SetOuterWidth(Style computedStyle, int outerWidth, Element element)
        {
            int marginLeft = ConvertCssToInt(computedStyle.MarginLeft, 0);
            int borderLeft = ConvertCssToInt(computedStyle.BorderLeftWidth, 0);
            int paddingLeft = ConvertCssToInt(computedStyle.PaddingLeft, 0);
            int paddingRight = ConvertCssToInt(computedStyle.PaddingRight, 0);
            int borderRight = ConvertCssToInt(computedStyle.BorderRightWidth, 0);
            int marginRight = ConvertCssToInt(computedStyle.MarginRight, 0);
            int newVal = Math.Max(outerWidth - marginLeft - borderLeft - paddingLeft
                                  - paddingRight - borderRight - marginRight, 0);
            element.Style.Width = newVal + "px";
        }

        private static void SetOuterHeight(Style computedStyle, int outerHeight, Element element)
        {
            int marginTop = ConvertCssToInt(computedStyle.MarginTop, 0);
            int borderTop = ConvertCssToInt(computedStyle.BorderTopWidth, 0);
            int paddingTop = ConvertCssToInt(computedStyle.PaddingTop, 0);
            int paddingBottom = ConvertCssToInt(computedStyle.PaddingBottom, 0);
            int borderBottom = ConvertCssToInt(computedStyle.BorderBottomWidth, 0);
            int marginBottom = ConvertCssToInt(computedStyle.MarginBottom, 0);
            int newVal = Math.Max(outerHeight - marginTop - borderTop - paddingTop
                                  - paddingBottom - borderBottom - marginBottom, 0);
            element.Style.Height = newVal + "px";
        }

        /// <summary>
        /// Sets the equivalent of dojo.marginBox(o, size) given size-only values.
        /// </summary>
        private static void SetMarginSizeJQ(Style computedStyle, Size s, Element element)
        {
            if (s.Width >= 0) { DomUtil.SetOuterWidth(computedStyle, s.Width, element); }
            if (s.Height >= 0) { DomUtil.SetOuterHeight(computedStyle, s.Height, element); }
        }

        /// <summary>
        /// The z-index property for modern browsers returns a string but for IE older than IE9 an integer is returned.
        /// This is a helper function for those return values.
        /// </summary>
        /// <param name="o">The jQuery object to query for a z-index</param>
        /// <returns>The z-index of the parameter, defaults to zero</returns>
        private static int ParseZIndexProperty(jQueryObject o)
        {
            // Given an parameter
            Param.VerifyValue(o, "o");

            // In Firefox/Chrome/>=IE9 this returns a string, but in <IE9 it returns an int.
            object zindexProperty = o.GetCSS("z-index");

            if (Underscore.IsNumber(zindexProperty))
            {
                return (int)zindexProperty;
            }

            if (Underscore.IsString(zindexProperty))
            {
                // As per CSS spec the z-index property can be "auto", "inherits", or an integer.
                if (!string.IsNullOrEmpty((string)zindexProperty) && (string)zindexProperty != "auto" && (string)zindexProperty != "inherits")
                {
                    return Script.ParseInt((string)zindexProperty, 10).ReinterpretAs<int>();
                }
            }

            return 0;
        }

        public static string MakeHtmlSafeId(string value)
        {
            // uri encode the string to try and make the values generally safe (no spaces or ':'s)
            // replace '.'s with 'dot' so the css selectors don't get confused
            return string.EncodeUriComponent(value).Replace(".", "dot");
        }

        /// <summary>
        /// Intended to be used on Safari mobile to select a range of text in an input element
        /// Use on Safari mobile in places where you would use .select() to select text in other browsers
        /// </summary>
        /// <param name="inputElement">The input element</param>
        /// <param name="selectionStart">The index of the first selected character</param>
        /// <param name="selectionEnd">The index of the character after the last selected character</param>
        public static void SetSelectionRangeOnInput(Element inputElement, int selectionStart, int selectionEnd)
        {
            if (BrowserSupport.SetSelectionRange())
            {
                try
                {
                    inputElement.ReinterpretAs<InputElement>().SetSelectionRange(selectionStart, selectionEnd);
                }
                catch // not all input types support setSelectionRange
                {
                }
            }
        }

        /// <summary>
        /// Select all text in an input element.
        /// </summary>
        /// <param name="inputElement">The input element</param>
        public static void SelectAllInputText(jQueryObject inputElement)
        {
            try
            {
                if (BrowserSupport.SetSelectionRange())
                {
                    inputElement.GetElement(0).ReinterpretAs<InputElement>().SetSelectionRange(0, inputElement.GetValue().Length);
                }
                else
                {
                    inputElement.Select();
                }
            }
            catch // not all input types support setSelectionRange (e.g. datetime-local)
            {
            }
        }

        public static void SetCursorPosition(InputElement input, int pos)
        {
            if (TypeUtil.HasMethod(input, "createTextRange")) // Internet Explorer
            {
                var rng = input.CreateTextRange();
                rng.Move(JsTextRange.Unit.Character, pos);
                rng.Select();
                rng.ScrollIntoView();
                input.Focus();
            }
            else if (BrowserSupport.IsSafari() && BrowserSupport.SetSelectionRange())
            {
                input.Focus();
                input.SetSelectionRange(pos, pos);
            }
            else
            {
                input.Blur();
                input.SelectionStart = input.SelectionEnd = pos;
                input.Focus();
            }
        }

        public static void ReplaceSelection(InputElement input, string text)
        {
            var oldVal = input.Value;
            var prefix = oldVal.Substring(0, input.SelectionStart);
            var suffix = oldVal.Substring(input.SelectionEnd);
            var newCursorPos = input.SelectionStart + text.Length;
            input.Value = prefix + text + suffix;
            SetCursorPosition(input, newCursorPos);
        }

        /// <summary>
        /// Sets the hover tooltip of an element. Normally uses the standard HTML 'title' attribute.
        /// On mobile, this will add a lightweight div that is appeared (always to the right) using
        /// a CSS animation.
        /// </summary>
        /// <param name="obj">The DOM node to set the tooltip on</param>
        /// <param name="tooltipText">The text to display in the tooltip</param>
        public static void SetNativeTooltip(jQueryObject obj, string tooltipText)
        {
            bool empty = string.IsNullOrEmpty(tooltipText);
            if (empty)
            {
                obj.RemoveAttr("title");
            }
            else
            {
                obj.Attribute("title", tooltipText);
            }

            if (TsConfig.IsMobile)
            {
                obj.Children(".tab-mobileTooltip").Remove();
                if (!empty)
                {
                    var tooltipDiv = jQuery.FromHtml("<div class='tab-mobileTooltip'/>").Text(tooltipText);
                    obj.Append(tooltipDiv);
                }
            }
        }

        public static bool NodeHasTextSelection(XmlNode node)
        {
            var windowSelection = Window.GetSelection();

            for (int ii = windowSelection.RangeCount - 1; ii >= 0; --ii)
            {
                var range = windowSelection.GetRangeAt(ii);

                if (node == range.StartContainer ||
                    node.Contains(range.StartContainer) ||
                    node == range.EndContainer ||
                    node.Contains(range.EndContainer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a scoped unique id, intended to be used when elements need a DOM id, e.g. for associating a label
        /// element with an input element
        /// </summary>
        public static string GenerateUniqueId()
        {
            return "tab-ui-id-" + uniqueId++;
        }

        /// <summary>
        /// Get a string array containing the id attributes of a jQueryObject collection.
        /// </summary>
        public static string[] GetIds(jQueryObject collection)
        {
            if (collection == null)
            {
                return new string[0];
            }

            return collection.Map((index, element) => element.Id).GetItems().ReinterpretAs<string[]>();
        }

        public static string AsIDSelector(this string selector)
        {
            return "#" + selector;
        }

        public static string AsClassSelector(this string selector)
        {
            return "." + selector;
        }

        public static string ToMsString(this int value)
        {
            return value + "ms";
        }

        public static Point ToPoint(this jQueryPosition pos)
        {
            return new Point((int)pos.Left, (int)pos.Top);
        }
    }
}
