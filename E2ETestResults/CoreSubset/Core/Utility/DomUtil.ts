import { BrowserEventName, tsConfig } from 'TypeDefs';

import { BrowserSupport } from 'vqlbrowsersupport';

import { CssDictionary } from './CssDictionary';

import 'mscorlib';

import { DoubleUtil } from './DoubleUtil';

import { HtmlExtensions, JsNativeExtensionMethods, Number, JsTextRange, TypeUtil } from 'NativeJsTypeDefs';

import { $, jQueryPosition } from 'jQuery';

import { Logger } from '../../CoreSlim/Logging/Logger';

import { MiscUtil } from './MiscUtil';

import { Param } from './Param';

import { Point, PointD } from './Point';

import { PointUtil } from './PointUtil';

import { Rect, RectXY } from './Rect';

import { ScriptEx } from '../../CoreSlim/Utility/ScriptEx';

import { Size } from './Size';

import { _ } from 'Underscore';

import { VisibleRoom } from './IBrowserViewport';

/**
 * Contains utility methods for DOM.
 */
export class DomUtil {
  private static readonly testWaitAttributeName: string = 'data-tab-test-wait';

  private static readonly translationFuncIndexer: { [key: string]: number } = {
    'matrix': 4,
    'matrix3d': 12,
    'translate': 0,
    'translate3d': 0,
    'translateX': 0,
    'translateY': -1
  };

  private static uniqueId: number = new Date().getTime();

  private static get log(): Logger {
    return Logger.lazyGetLogger(DomUtil);
  }

  /**
   * Document Body element. Intended to be used in places where a fake body needs to be returned in tests
   */
  public static get documentBody(): HTMLElement {
    return document.body;
  }

  /**
   * Gives the final used values of all the CSS properties of an element.
   * https://developer.mozilla.org/en-US/docs/DOM/window.getComputedStyle
   */
  public static getComputedStyle(e: HTMLElement): CSSStyleDeclaration {
    if (BrowserSupport.GetComputedStyle()) {
      let s: CSSStyleDeclaration = window.getComputedStyle(e);
      if (ss.isValue(s)) {
        return s;
      }
    }
    DomUtil.log.warn('Calling GetComputedStyle but is unsupported');
    return e.style;
  }

  /**
   * Gets the computed z-index of the given element.  This is done by querying the CSS z-index property of this
   * element and absolutely positioned ancestors, finding the z-index of the last before root. Handles the case
   * where IE7 reports an "auto" z-index property value as "0".
   * @param child The child element to calculate a z-index for
   * @returns The computed z-index of the element, defaults to 0
   */
  public static getComputedZIndex(child: HTMLElement): number {
    Param.verifyValue(child, 'child');
    let iter: Object = $.$(child);
    let lastPositioned: Object = iter;
    let html: HTMLElement = document.documentElement;
    let body: HTMLElement = document.body;
    while (iter.length !== 0 && iter[0] !== body && iter[0] !== html) {
      let pos: string = iter.css('position');
      if (pos === 'absolute' || pos === 'fixed') {
        lastPositioned = iter;
      }
      iter = iter.offsetParent();
    }
    return DomUtil.parseZIndexProperty(lastPositioned);
  }

  /**
   * Ported from tableau.util.resize.
   * @param e 
   * @param rect 
   */
  public static resize(e: any, rect: Rect): void {
    if ((typeof (e['resize']) === 'function')) {
      e.resize(rect);
    } else {
      DomUtil.setMarginBox(<HTMLElement>(e.domNode) || (e), rect);
    }
  }

  /**
   * Gets the equivalent of dojo.contentBox(e).
   */
  public static getContentBox(e: HTMLElement): Rect {
    let obj: Object = $.$(e);
    return new Rect((ss.parseInt(obj.css('padding-left'), 10)) || (0), (ss.parseInt(obj.css('padding-top'), 10)) || (0), DoubleUtil.roundToInt(obj.width()), DoubleUtil.roundToInt(obj.height()));
  }

  /**
   * Sets the equivalent of dojo.contentBox(e, rect).
   */
  public static setContentBox(e: HTMLElement, r: Rect): void {
    $.$(e).width(r.w).height(r.h);
  }

  /**
   * Sets the equivalent of dojo.marginBox(e, rect).
   */
  public static setMarginBox(e: HTMLElement, size: Size): void;

  /**
   * Sets the equivalent of dojo.marginBox(e, rect).
   */
  public static setMarginBox(e: HTMLElement, r: Rect | Size): void {
    DomUtil.setMarginBoxJQ($.$(e), r);
  }

  /**
   * Sets the equivalent of dojo.marginBox(o, rect).
   */
  public static setMarginBoxJQ(o: Object, r: Rect): void {
    let rawElement: HTMLElement = o[0];
    let elementStyle: CSSStyleDeclaration = rawElement.style;
    let computedStyle: CSSStyleDeclaration = window.getComputedStyle(rawElement);
    DomUtil.setMarginSizeJQ(computedStyle, r, rawElement);
    if (!number.isNaN(r.t)) {
      elementStyle.top = r.t + 'px';
    }
    if (!number.isNaN(r.l)) {
      elementStyle.left = r.l + 'px';
    }
  }

  /**
   * Set the top, left, width and height styles based on the given rect to position the given jquery object
   */
  public static setAbsolutePositionBox(o: Object, r: Rect): void {
    o.css(new CssDictionary());
  }

  /**
   * Gets the equivalent of dojo.marginBox(e).
   */
  public static getMarginBox(e: HTMLElement): Rect {
    return DomUtil.getMarginBoxJQ($.$(e));
  }

  /**
   * Gets the equivalent of dojo.marginBox(e).
   */
  public static getMarginBoxJQ(o: Object): Rect {
    let p: jQueryPosition = o.position();
    return new Rect(DoubleUtil.roundToInt(p.left), DoubleUtil.roundToInt(p.top), DoubleUtil.roundToInt(o.outerWidth(true)), DoubleUtil.roundToInt(o.outerHeight(true)));
  }

  /**
   * Gets the location and area of the element relative to the document itself.  I.e. in the coordinate space
   * of the document, not the viewport.  So it will be the same no matter how much you have scrolled or zoomed
   * in.
   * 
   * This is basically equivalent to the old dojo.coords(e) function.
   */
  public static getRectXY(o: Object): RectXY {
    let x: number = DoubleUtil.roundToInt(DomUtil.getPageOffset(o).left);
    let y: number = DoubleUtil.roundToInt(DomUtil.getPageOffset(o).top);
    let w: number = DoubleUtil.roundToInt(o.outerWidth(true));
    let h: number = DoubleUtil.roundToInt(o.outerHeight(true));
    return new RectXY(x, y, w, h);
  }

  /**
   * Similar to GetRectXY (above),
   * but it also supports includeScroll parameter
   * (also ported from Dojo)
   */
  public static getRectXY$1(o: Object, includeScroll: boolean): RectXY {
    let result: RectXY = DomUtil.getRectXY(o);
    if (includeScroll) {
      let scroll: Point = DomUtil.docScroll();
      result.x += scroll.x;
      result.y += scroll.y;
    }
    return result;
  }

  /**
   * Port of dojo's docScroll function
   * http://dojotoolkit.org/reference-guide/1.10/dojo/dom-geometry/docScroll.html
   * @returns Returns a normalized object with {x, y} with corresponding offsets for the scroll position for the current document.
   */
  public static docScroll(): Point {
    let x: number = ss.coalesce(window.pageXOffset, ss.coalesce(document.documentElement.scrollLeft, ss.coalesce(DomUtil.documentBody.scrollLeft, 0)));
    let y: number = ss.coalesce(window.pageYOffset, ss.coalesce(document.documentElement.scrollTop, ss.coalesce(DomUtil.documentBody.scrollTop, 0)));
    return new Point(x, y);
  }

  /**
   * return true if 'ancestor' is the ancestor of 'child'
   * @param ancestor DOM element of the ancestor
   * @param child DOM element of the child
   * @returns return true if 'ancestor' is the ancestor or 'child'
   */
  public static isAncestorOf(ancestor: HTMLElement, child: HTMLElement): boolean {
    if (ss.isNullOrUndefined(ancestor) || ss.isNullOrUndefined(child)) {
      return false;
    }
    return $.$(child).parents().index(ancestor) >= 0;
  }

  /**
   * Tests whether me is the testElement or if testElement is a child of me
   * @returns true if me is the given testElement or if testElement is a child of me
   */
  public static isEqualOrAncestorOf(ancestor: HTMLElement, child: HTMLElement): boolean {
    if (ss.isNullOrUndefined(ancestor) || ss.isNullOrUndefined(child)) {
      return false;
    }
    return (ancestor === child || DomUtil.isAncestorOf(ancestor, child));
  }

  /**
   * Sets an elements position using either CSS transform or absolute positioning, depending on what
   * is supported.  Assumes element is added to body.
   */
  public static setElementPosition(e: Object, pageX: number, pageY: number, duration: string = null, useTransform: boolean | null = null): void {
    if ((!useTransform.hasValue || useTransform.value)) {
      let styling: { [key: string]: any } = {
        'top': '0px',
        'left': '0px'
      };
      let transformVal: string = new ss.StringBuilder('translate3d(').append(pageX).append('px,').append(pageY).append('px,').append('0px)').toString();
      styling[BrowserSupport.CssTransformName()] = transformVal;
      if (ss.isValue(duration)) {
        styling[BrowserSupport.CssTransitionName() + '-duration'] = duration;
      }
      e.css(styling);
      return;
    }
    let css: { [key: string]: any } = {
      'position': 'absolute',
      'top': pageY + 'px',
      'left': pageX + 'px'
    };
    css[BrowserSupport.CssTransformName()] = '';
    e.css(css);
  }

  /**
   * Gets the page position for an element -- NOTE: rounds to ints
   * @param e The element whose page position is needed
   * @returns a Point with the X and Y offsets from the left/top of the page
   */
  public static getElementPosition(e: Object): Point {
    return PointUtil.fromPosition(e.offset());
  }

  /**
   * Gets an absolute/client position for an element -- NOTE: rounds to ints
   * @param e The element whose client position is needed
   * @returns a Point with the X and Y offsets from the left/top of the client window
   */
  public static getElementClientPosition(e: Object): Point {
    let p: Point = DomUtil.getElementPosition(e);
    p.x -= $.$(<HTMLElement>document.documentElement).scrollLeft();
    p.y -= DoubleUtil.roundToInt($.$(<HTMLElement>document.documentElement).scrollTop());
    return p;
  }

  /**
   * Extracts (only) any CSS-"transform" based 2d translation. (ignores any Z-component) n.b. ASSUMES PIXEL VALUES (i.e. even "12em" means "12px"!)
   */
  public static getTransformOffset(element: Object): jQueryPosition {
    if (element === null) {
      DomUtil.log.warn('Attempting to get transformation on null element!');
      return new jQueryPosition(0, 0);
    }
    let fullTransform: string = element.css('transform');
    if (ss.isNullOrEmptyString(fullTransform)) {
      return new jQueryPosition(0, 0);
    }
    let transform: string[] = fullTransform.split('(');
    let index: number | null = DomUtil.translationFuncIndexer[transform[0]];
    if (index === null) {
      return new jQueryPosition(0, 0);
    }
    let vals: string[] = transform[1].split(',');
    return new jQueryPosition(DoubleUtil.parseDouble(vals[index.value]) || 0, DoubleUtil.parseDouble(vals[index.value + 1]) || 0);
  }

  /**
   * Extracts the scaling factor from a CSS transform -- assumes symmetric scaling between x- and y-axes
   * @returns the extracted scaling factor from 'transform' if present, otherwise 1
   */
  public static getTransformScale(element: Object): number {
    if (element === null) {
      DomUtil.log.warn('Attempting to get transformation on null element!');
      return 1;
    }
    let fullTransform: string = element.css('transform');
    if (ss.isNullOrEmptyString(fullTransform)) {
      return 1;
    }
    let transform: string[] = fullTransform.split('(');
    if (transform[0] === 'scale' || transform[0] === 'matrix' || transform[0] === 'matrix3d') {
      return DoubleUtil.parseDouble(transform[1]) || 1;
    } else {
      return 1;
    }
  }

  /**
   * Returns the offset of the element relative to the document, without integer-truncating
   * any results. Works when pinch-to-zoomed as well, unlike normal Offset()
   */
  public static getPageOffset(e: Object): jQueryPosition {
    let pageOffset: PointD = DomUtil.getPageOffset$1(e[0]);
    return new jQueryPosition(pageOffset.x, pageOffset.y);
  }

  /**
   * Non-jQuery version of {@link DomUtil.GetPageOffset}.
   */
  public static getPageOffset$1(e: Element): PointD {
    if (e === null) {
      ss.Debug.assert(false, 'Tried to getPageOffset of null element');
      return new PointD(0, 0);
    }
    let elementRect: DOMRect = e.getBoundingClientRect();
    let documentElementRect: DOMRect = document.documentElement.getBoundingClientRect();
    return new PointD(elementRect.left - documentElementRect.left, elementRect.top - documentElementRect.top);
  }

  public static getScrollPosition(o: Object): jQueryPosition {
    return new jQueryPosition(o[0].scrollLeft, o[0].scrollTop);
  }

  public static scrollPosition(o: Object, pos: jQueryPosition): void {
    o[0].scrollLeft = pos.left;
    o[0].scrollTop = pos.top;
  }

  /**
   * Sets focus to a child node and resets the scroll position of its scrollable container afterward.
   * The user-facing effect is that focus can be set on a child element without the default "scroll to center" browser behavior.
   * @param focusMethod Callback that sets focus on the relevant child node
   * @param scrollNode The scrollable ancestor node of the child
   */
  public static focusWithoutScrolling(focusMethod: () => void, scrollNode: Object): void {
    if (scrollNode === null || scrollNode.length === 0) {
      focusMethod();
    } else {
      scrollNode.attr(DomUtil.testWaitAttributeName, '');
      window.setTimeout(() => {
        let scrollPos: jQueryPosition = DomUtil.getScrollPosition(scrollNode);
        focusMethod();
        DomUtil.scrollPosition(scrollNode, scrollPos);
        scrollNode.removeAttr(DomUtil.testWaitAttributeName);
      }, 200);
    }
  }

  /**
   * Returns the available viewport space around a position. Does not account for parent-frame scrolling,
   * for that use the SpiffBrowserViewport callbacks.
   */
  public static roomAroundPosition(p: jQueryPosition): VisibleRoom {
    let roomAbove: number = p.top - window.pageYOffset;
    let roomBelow: number = window.pageYOffset + window.innerHeight - p.top;
    let roomLeft: number = p.left - window.pageXOffset;
    let roomRight: number = window.pageXOffset + window.innerWidth - p.left;
    return new VisibleRoom(roomAbove, roomBelow, roomLeft, roomRight);
  }

  /**
   * Calculates the difference in position between an element and a parent/ancestor element
   * @param e The element whose offset is needed
   * @param p The element that should be used as the baseline for the offset
   * @returns a Point with the X and Y distances between the two elements
   */
  public static getElementRelativePosition(e: Object, p: Object): Point {
    if (ss.isNullOrUndefined(p)) {
      p = e.parent();
    }
    let ep: jQueryPosition = e.offset();
    let pp: jQueryPosition = p.offset();
    return new Point(DoubleUtil.roundToInt(ep.left) - DoubleUtil.roundToInt(pp.left), DoubleUtil.roundToInt(ep.top) - DoubleUtil.roundToInt(pp.top));
  }

  /**
   * Parse the width from a css style into an integer.
   * Returns NaN if the width style doesn't have numbers in it.
   */
  public static parseWidthFromStyle(style: CSSStyleDeclaration): number {
    if (ss.isValue(style) && !MiscUtil.isNullOrEmpty$1(style.width)) {
      return ss.parseInt(style.width);
    }
    return Number.NaN;
  }

  /**
   * Parse the height from a css style into an integer.
   * Returns NaN if the height style doesn't have numbers in it.
   */
  public static parseHeightFromStyle(style: CSSStyleDeclaration): number {
    if (ss.isValue(style) && !MiscUtil.isNullOrEmpty$1(style.height)) {
      return ss.parseInt(style.height);
    }
    return Number.NaN;
  }

  /**
   * Creates a jQuery namespaced event name, of the following form:
   * eventName.instanceId_className
   * For example, keydown.1_Dialog, where eventName is keydown and eventNamespace is ".1_Dialog"
   * @param eventName The browser event name.
   * @param eventNamespace The namespace to be appended.
   * @returns A jQuery namespaced event name if the eventNamespace has a value, otherwise the eventName with no change
   */
  public static createNamespacedEventName(eventName: BrowserEventName, eventNamespace: string): string {
    if (ss.isValue(eventNamespace)) {
      return eventName + eventNamespace;
    }
    return eventName.toString();
  }

  /**
   * Stop propagation of mouse and touch input events.
   * @param o The element on which we should stop propagation.
   * @param eventNamespace The event namespace to be used in the binding.
   */
  public static stopPropagationOfInputEvents(o: Object, eventNamespace: string): void {
    let stopPropagation: jQueryEventHandler = (e: Object) => e.stopPropagation();
    DomUtil.handleInputEvents(o, eventNamespace, stopPropagation);
  }

  /**
   * Adds a handler for all mouse and touch input events on the object.
   * @param o The element on which we should bind to events
   * @param eventNamespace The event namespace to be used in the binding.
   * @param handler The event handler method that will be called for each input event.
   */
  public static handleInputEvents(o: Object, eventNamespace: string, handler: jQueryEventHandler): void {
    o.on(DomUtil.createNamespacedEventName(BrowserEventName.TouchStart, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.TouchCancel, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.TouchEnd, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.TouchMove, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.Click, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.MouseDown, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.MouseMove, eventNamespace), handler).on(DomUtil.createNamespacedEventName(BrowserEventName.MouseUp, eventNamespace), handler);
  }

  /**
   * Returns true if domElement is a focusable input element
   * @param domElement The domElement
   * @returns true or false
   */
  public static isFocusableTextElement(domElement: Element): boolean {
    if (ss.isValue(domElement) && ss.isValue(domElement.tagName)) {
      let targetTagName: string = domElement.tagName.toLowerCase();
      if ((targetTagName === 'textarea') || (targetTagName === 'input') || (targetTagName === 'select')) {
        return true;
      }
    }
    return false;
  }

  /**
   * Returns true if domElement is a checkBox, else false
   * @param domElement The domElement
   * @returns true or false
   */
  public static isCheckboxElement(domElement: HTMLElement): boolean {
    if (ss.isValue(domElement) && ss.isValue(domElement.tagName)) {
      let targetTagName: string = domElement.tagName.toLowerCase();
      let typeAttributeValue: string = $.$(domElement).attr('type');
      if (targetTagName === 'input' && typeAttributeValue === 'checkbox') {
        return true;
      }
    }
    return false;
  }

  /**
   * Returns true if input events on domElement should be handled as touchEvents, else false.
   * @param domElement The domElement that is the target of mouse/touch events
   * @returns true or false
   */
  public static handleTouchEvents(domElement: HTMLElement): boolean {
    if (DomUtil.isCheckboxElement(domElement)) {
      return false;
    }
    if (DomUtil.isFocusableTextElement(domElement)) {
      return false;
    }
    return true;
  }

  /**
   * If supported calls setCapture on the given Element.  No-op if unsupported.
   * https://developer.mozilla.org/en-US/docs/Web/API/Element.setCapture
   * @param e The capturing element
   * @param retargetToElement If true, all events are targeted directly to this element; if
   * false, events can also fire at descendants of this element.
   */
  public static setCapture(e: Element, retargetToElement: boolean): void {
    if (!BrowserSupport.MouseCapture()) {
      return;
    }
    e.setCapture(retargetToElement);
  }

  /**
   * Calls document.releaseCapture.  No-op if unsupported.
   * https://developer.mozilla.org/en-US/docs/Web/API/document.releaseCapture
   */
  public static releaseCapture(): void {
    if (!BrowserSupport.MouseCapture()) {
      return;
    }
    document.releaseCapture();
  }

  /**
   * calls Blur on the document's active element
   */
  public static blur(): void {
    let activeElem: HTMLElement = document.activeElement;
    if (ss.isValue(activeElem) && activeElem !== DomUtil.documentBody && (typeof (activeElem['blur']) === 'function')) {
      activeElem.blur();
    }
  }

  private static convertCssToInt(cssValue: string, defaultValue: number): number {
    let x: number = ss.parseInt(cssValue, 10);
    return number.isNaN(x) ? defaultValue : x;
  }

  public static getSizeFromCssPixelProperty(element: Object, propertyName: string): number {
    let strValue: string = element.css(propertyName);
    return DomUtil.convertCssToInt(strValue, 0);
  }

  private static setOuterWidth(computedStyle: CSSStyleDeclaration, outerWidth: number, element: HTMLElement): void {
    let marginLeft: number = DomUtil.convertCssToInt(computedStyle.marginLeft, 0);
    let borderLeft: number = DomUtil.convertCssToInt(computedStyle.borderLeftWidth, 0);
    let paddingLeft: number = DomUtil.convertCssToInt(computedStyle.paddingLeft, 0);
    let paddingRight: number = DomUtil.convertCssToInt(computedStyle.paddingRight, 0);
    let borderRight: number = DomUtil.convertCssToInt(computedStyle.borderRightWidth, 0);
    let marginRight: number = DomUtil.convertCssToInt(computedStyle.marginRight, 0);
    let newVal: number = Math.max(outerWidth - marginLeft - borderLeft - paddingLeft - paddingRight - borderRight - marginRight, 0);
    element.style.width = newVal + 'px';
  }

  private static setOuterHeight(computedStyle: CSSStyleDeclaration, outerHeight: number, element: HTMLElement): void {
    let marginTop: number = DomUtil.convertCssToInt(computedStyle.marginTop, 0);
    let borderTop: number = DomUtil.convertCssToInt(computedStyle.borderTopWidth, 0);
    let paddingTop: number = DomUtil.convertCssToInt(computedStyle.paddingTop, 0);
    let paddingBottom: number = DomUtil.convertCssToInt(computedStyle.paddingBottom, 0);
    let borderBottom: number = DomUtil.convertCssToInt(computedStyle.borderBottomWidth, 0);
    let marginBottom: number = DomUtil.convertCssToInt(computedStyle.marginBottom, 0);
    let newVal: number = Math.max(outerHeight - marginTop - borderTop - paddingTop - paddingBottom - borderBottom - marginBottom, 0);
    element.style.height = newVal + 'px';
  }

  /**
   * Sets the equivalent of dojo.marginBox(o, size) given size-only values.
   */
  private static setMarginSizeJQ(computedStyle: CSSStyleDeclaration, s: Size, element: HTMLElement): void {
    if (s.w >= 0) {
      DomUtil.setOuterWidth(computedStyle, s.w, element);
    }
    if (s.h >= 0) {
      DomUtil.setOuterHeight(computedStyle, s.h, element);
    }
  }

  /**
   * The z-index property for modern browsers returns a string but for IE older than IE9 an integer is returned.
   * This is a helper function for those return values.
   * @param o The jQuery object to query for a z-index
   * @returns The z-index of the parameter, defaults to zero
   */
  private static parseZIndexProperty(o: Object): number {
    Param.verifyValue(o, 'o');
    let zindexProperty: any = o.css('z-index');
    if (_.isNumber(zindexProperty)) {
      return <number>zindexProperty;
    }
    if (_.isString(zindexProperty)) {
      if (!ss.isNullOrEmptyString(<string>zindexProperty) && <string>zindexProperty !== 'auto' && <string>zindexProperty !== 'inherits') {
        return ss.parseInt(<string>zindexProperty, 10);
      }
    }
    return 0;
  }

  public static makeHtmlSafeId(value: string): string {
    return ss.replaceAllString(string.encodeURIComponent(value), '.', 'dot');
  }

  /**
   * Intended to be used on Safari mobile to select a range of text in an input element
   * Use on Safari mobile in places where you would use .select() to select text in other browsers
   * @param inputElement The input element
   * @param selectionStart The index of the first selected character
   * @param selectionEnd The index of the character after the last selected character
   */
  public static setSelectionRangeOnInput(inputElement: HTMLElement, selectionStart: number, selectionEnd: number): void {
    if (BrowserSupport.SetSelectionRange()) {
      try {
        inputElement.setSelectionRange(selectionStart, selectionEnd);
      } catch { }
    }
  }

  /**
   * Select all text in an input element.
   * @param inputElement The input element
   */
  public static selectAllInputText(inputElement: Object): void {
    try {
      if (BrowserSupport.SetSelectionRange()) {
        inputElement.get(0).setSelectionRange(0, inputElement.val().length);
      } else {
        inputElement.select();
      }
    } catch { }
  }

  public static setCursorPosition(input: Element, pos: number): void {
    if ((typeof (input['createTextRange']) === 'function')) {
      let rng: JsTextRange = input.createTextRange ? input.createTextRange() : null;
      rng.move(JsTextRange.Unit.Character, pos);
      rng.select();
      rng.scrollIntoView();
      input.focus();
    } else
      if (BrowserSupport.IsSafari() && BrowserSupport.SetSelectionRange()) {
        input.focus();
        input.setSelectionRange(pos, pos);
      } else {
        input.blur();
        input.selectionStart = input.selectionEnd = pos;
        input.focus();
      }
  }

  public static replaceSelection(input: Element, text: string): void {
    let oldVal: string = input.value;
    let prefix: string = oldVal.substr(0, input.selectionStart);
    let suffix: string = oldVal.substring(input.selectionEnd);
    let newCursorPos: number = input.selectionStart + text.length;
    input.value = prefix + text + suffix;
    DomUtil.setCursorPosition(input, newCursorPos);
  }

  /**
   * Sets the hover tooltip of an element. Normally uses the standard HTML 'title' attribute.
   * On mobile, this will add a lightweight div that is appeared (always to the right) using
   * a CSS animation.
   * @param obj The DOM node to set the tooltip on
   * @param tooltipText The text to display in the tooltip
   */
  public static setNativeTooltip(obj: Object, tooltipText: string): void {
    let empty: boolean = ss.isNullOrEmptyString(tooltipText);
    if (empty) {
      obj.removeAttr('title');
    } else {
      obj.attr('title', tooltipText);
    }
    if (tsConfig.is_mobile) {
      obj.children('.tab-mobileTooltip').remove();
      if (!empty) {
        let tooltipDiv: Object = $.$('<div class=\'tab-mobileTooltip\'/>').text(tooltipText);
        obj.append(tooltipDiv);
      }
    }
  }

  public static nodeHasTextSelection(node: Node): boolean {
    let windowSelection: Selection = window.getSelection();
    for (let ii = windowSelection.rangeCount - 1; ii >= 0; --ii) {
      let range: Range = windowSelection.getRangeAt(ii);
      if (node === range.startContainer || node.contains(range.startContainer) || node === range.endContainer || node.contains(range.endContainer)) {
        return true;
      }
    }
    return false;
  }

  /**
   * Get a scoped unique id, intended to be used when elements need a DOM id, e.g. for associating a label
   * element with an input element
   */
  public static generateUniqueId(): string {
    return 'tab-ui-id-' + DomUtil.uniqueId++;
  }

  /**
   * Get a string array containing the id attributes of a jQueryObject collection.
   */
  public static getIds(collection: Object): string[] {
    if (collection === null) {
      return [];
    }
    return collection.map((index, element) => element.id).get();
  }

  public static asIDSelector(selector: string): string {
    return '#' + selector;
  }

  public static asClassSelector(selector: string): string {
    return '.' + selector;
  }

  public static toMsString(value: number): string {
    return value + 'ms';
  }

  public static toPoint(pos: jQueryPosition): Point {
    return new Point(<number>pos.left, <number>pos.top);
  }
}
