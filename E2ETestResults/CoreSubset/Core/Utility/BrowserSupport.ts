import { CssDictionary } from './CssDictionary';

import { DomUtil } from './DomUtil';

import { $ } from 'Saltarelle.jQuery';

import { Logger } from '../Logging/Logger';

import { MiscUtil } from './MiscUtil';

import 'mscorlib';

import { TypeUtil } from 'NativeJsTypeDefs';

import { Utility } from '../../Bootstrap/Utility';

/**
 * Contains information about what is currently supported in the browser or environment.
 */
export class BrowserSupport {
  private static $selectStart: boolean;

  private static $touch: boolean = 'ontouchend' in document;

  private static fonts: boolean = 'fonts' in document;

  private static $dataUri: boolean;

  private static $postMessage: boolean;

  private static $historyApi: boolean;

  private static consoleLogFormatting: boolean;

  private static $cssTransformName: string;

  private static $cssTransitionName: string;

  private static cssTranslate2d: boolean;

  private static cssTranslate3d: boolean;

  private static shouldUseAlternateHitStrategy: boolean = false;

  private static $canvasLinePattern: boolean = false;

  private static $isSafari: boolean = false;

  private static $isChrome: boolean = false;

  private static $isIE: boolean = false;

  private static internetExplorerVersion: number = 0;

  private static $safariVersion: number = 0;

  private static $iosVersion: number = 0;

  private static $isFF: boolean = false;

  private static $isOpera: boolean = false;

  private static isKhtml: boolean = false;

  private static isWebKit: boolean = false;

  private static isMozilla: boolean = false;

  private static $isIos: boolean = false;

  private static $isAndroid: boolean = false;

  private static $isMac: boolean = false;

  private static $isWindows: boolean = false;

  private static $devicePixelRatio: number = 1;

  private static $backingStoragePixelRatio: number = 1;

  private static $dateInput: boolean = false;

  private static $dateTimeInput: boolean = false;

  private static $dateTimeLocalInput: boolean = false;

  private static $timeInput: boolean = false;

  private static $setSelectionRange: boolean = false;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    BrowserSupport.detectBrowser();
    $.$(BrowserSupport.detectBrowserSupport);
  }

  /**
   * Gets a value indicating whether the browser supports getComputedStyle.
   * https://developer.mozilla.org/en-US/docs/DOM/window.getComputedStyle
   */
  public static get getComputedStyle(): boolean {
    return 'getComputedStyle' in window;
  }

  /**
   * Gets a value indicating whether the browser supports addEventListener.
   */
  public static get addEventListener(): boolean {
    return 'addEventListener' in document;
  }

  /**
   * Gets a value indicating whether the selectstart event is supported.
   */
  public static get selectStart(): boolean {
    return BrowserSupport.$selectStart;
  }

  /**
   * Gets a value indicating whether touch events are supported. Note that this does not imply that it's a mobile
   * browser since many Windows 7 devices support touch and may still have a mouse.
   */
  public static get touch(): boolean {
    return BrowserSupport.$touch;
  }

  /**
   * Gets a value indicating whether the browser supports the CSS Font Loader API.
   * See https://drafts.csswg.org/css-font-loading
   */
  public static get fontLoaderApi(): boolean {
    return BrowserSupport.fonts;
  }

  /**
   * Gets a value indicating whether the browser supports Data URIs.
   * http://en.wikipedia.org/wiki/Data_URI_scheme
   */
  public static get dataUri(): boolean {
    return BrowserSupport.$dataUri;
  }

  /**
   * Gets a value indicating whether or not this browser supports postMessage.
   */
  public static get postMessage(): boolean {
    return BrowserSupport.$postMessage;
  }

  public static get historyApi(): boolean {
    return BrowserSupport.$historyApi;
  }

  public static get consoleLogFormating(): boolean {
    return BrowserSupport.consoleLogFormatting;
  }

  public static get isMobile(): boolean {
    return BrowserSupport.$isAndroid || BrowserSupport.$isIos;
  }

  public static get isIos(): boolean {
    return BrowserSupport.$isIos;
  }

  public static get isAndroid(): boolean {
    return BrowserSupport.$isAndroid;
  }

  public static get isChrome(): boolean {
    return BrowserSupport.$isChrome;
  }

  public static get isMac(): boolean {
    return BrowserSupport.$isMac;
  }

  public static get isIE(): boolean {
    return BrowserSupport.$isIE;
  }

  public static get isFF(): boolean {
    return BrowserSupport.$isFF;
  }

  public static get isOpera(): boolean {
    return BrowserSupport.$isOpera;
  }

  public static get isSafari(): boolean {
    return BrowserSupport.$isSafari;
  }

  public static get isWindows(): boolean {
    return BrowserSupport.$isWindows;
  }

  /**
   * Returns the version of Internet Explorer that made this request.
   * If the client is not IE, this returns 0.0
   */
  public static get browserVersion(): number {
    return BrowserSupport.internetExplorerVersion;
  }

  public static get safariVersion(): number {
    return BrowserSupport.$safariVersion;
  }

  /**
   * Returns the version of iOS that made this request.
   * If the client is not iOS, this returns 0.0
   */
  public static get iosVersion(): number {
    return BrowserSupport.$iosVersion;
  }

  public static get raisesEventOnImageReassignment(): boolean {
    return !BrowserSupport.$isSafari;
  }

  public static get imageLoadIsSynchronous(): boolean {
    return BrowserSupport.$isIE;
  }

  /**
   * Gets a value indicating whether document.elementFromPoint on the current browser
   * requires screen vs. client coordinates for reasons of DPI, etc.
   * See BUGZID 55280
   */
  public static get useAlternateHitStrategy(): boolean {
    return BrowserSupport.shouldUseAlternateHitStrategy;
  }

  /**
   * Gets a value indicating whether the CSS property "transform" is supported.
   * http://caniuse.com/#search=transform
   */
  public static get cssTransform(): boolean {
    return ss.isValue(BrowserSupport.$cssTransformName);
  }

  /**
   * Gets a value indicating the name of the CSS transform property.
   */
  public static get cssTransformName(): string {
    return BrowserSupport.$cssTransformName;
  }

  /**
   * Gets a value indicating the name of the CSS transition property.
   */
  public static get cssTransitionName(): string {
    return BrowserSupport.$cssTransitionName;
  }

  /**
   * Gets a value indicating whether the CSS "transform: translate()" is supported.
   * http://caniuse.com/#search=transform
   */
  public static get cssTranslate2D(): boolean {
    return BrowserSupport.cssTranslate2d;
  }

  /**
   * Gets a value indicating whether the CSS "transform: translate3d()" is supported.
   * http://caniuse.com/#search=transform
   */
  public static get cssTranslate3D(): boolean {
    return BrowserSupport.cssTranslate3d;
  }

  public static get backingStoragePixelRatio(): number {
    return BrowserSupport.$backingStoragePixelRatio;
  }

  public static get devicePixelRatio(): number {
    return BrowserSupport.$devicePixelRatio;
  }

  public static get canvasLinePattern(): boolean {
    return BrowserSupport.$canvasLinePattern;
  }

  /**
   * Gets a value indicating whether the device supports a native HTML5 date picker.
   * http://www.w3.org/TR/html-markup/input.date.html
   */
  public static get dateInput(): boolean {
    return BrowserSupport.$dateInput;
  }

  /**
   * Gets a value indicating whether the device supports a native HTML5 datetime picker.
   * http://www.w3.org/TR/html-markup/input.datetime.html
   */
  public static get dateTimeInput(): boolean {
    return BrowserSupport.$dateTimeInput;
  }

  /**
   * Gets a value indicating whether the device supports a native HTML5 local datetime picker.
   * http://www.w3.org/TR/html-markup/input.datetime-local.html
   */
  public static get dateTimeLocalInput(): boolean {
    return BrowserSupport.$dateTimeLocalInput;
  }

  /**
   * Gets a value indicating whether the device supports a native HTML5 local time picker.
   * http://www.w3.org/TR/html-markup/input.time.html
   */
  public static get timeInput(): boolean {
    return BrowserSupport.$timeInput;
  }

  /**
   * Indicates whether setSelectionRange is supported on an input element
   */
  public static get setSelectionRange(): boolean {
    return BrowserSupport.$setSelectionRange;
  }

  /**
   * Gets the mousewheel event to use via feature detection.
   * https://developer.mozilla.org/en-US/docs/Web/Reference/Events/wheel
   */
  public static get mouseWheelEvent(): string {
    let mouseWheelEvent: string;
    if ('onwheel' in window.document.documentElement) {
      mouseWheelEvent = 'wheel';
    } else
      if ('onmousewheel' in window.document.documentElement) {
        mouseWheelEvent = 'mousewheel';
      } else {
        mouseWheelEvent = 'MozMousePixelScroll';
      }
    return mouseWheelEvent;
  }

  /**
   * Tests if mouse capture support is present.  As of 6/1/14, only IE and Firefox support this.
   * https://developer.mozilla.org/en-US/docs/Web/API/Element.setCapture
   * 
   * See the comments in {@link BrowserSupport.MouseCapture}) for how this is used during dragging. We have some
   * special browser-specific logic there.
   */
  public static get mouseCapture(): boolean {
    return 'releaseCapture' in document;
  }

  /**
   * Indicates whether orientationchange event is supported by the browser
   */
  public static get orientationChange(): boolean {
    return 'onorientationchange' in window;
  }

  /**
   * Reports whether browser supports geolocation
   */
  public static get isGeolocationSupported(): boolean {
    return ss.isValue(window.navigator.geolocation);
  }

  public static detectBrowserSupport(): void {
    let body: HTMLElement = document.body;
    let div: HTMLElement = document.createElement('div');
    body.appendChild(div);
    BrowserSupport.$selectStart = 'onselectstart' in div;
    body.removeChild(div).style.display = 'none';
    BrowserSupport.$postMessage = 'postMessage' in window;
    BrowserSupport.$historyApi = (typeof (window.history['pushState']) === 'function') && (typeof (window.history['replaceState']) === 'function');
    BrowserSupport.detectDataUriSupport();
    BrowserSupport.detectConsoleLogFormatting();
    BrowserSupport.detectBrowser();
    BrowserSupport.detectTransitionSupport();
    BrowserSupport.detectTransformSupport();
    BrowserSupport.detectDocumentElementFromPoint();
    BrowserSupport.detectDevicePixelRatio();
    BrowserSupport.detectBackingStoragePixelRatio();
    BrowserSupport.detectDateInputSupport();
    BrowserSupport.detectCanvasLinePattern();
    BrowserSupport.detectSetSelectionRangeSupport();
  }

  /**
   * Gets the location.origin property in a browser-independent way.
   * 
   * IE still does not support location.origin. Use a polyfill:
   * {@link http://stackoverflow.com/questions/1420881/javascript-jquery-method-to-find-base-url-from-a-string}
   */
  public static getOrigin(location: Location): string {
    let origin: string = location.origin;
    if (ss.isNullOrUndefined(origin)) {
      origin = location.protocol + '//' + location.host;
    }
    return origin;
  }

  public static doPostMessageWithContext(message: string): void {
    let success: boolean = Utility.doPostMessageWithContext(message);
    if (!success) {
      Logger.lazyGetLogger(BrowserSupport).debug('BrowserSupport::DoPostMessage failed.');
    }
  }

  private static detectDataUriSupport(): void {
    let imgObj: Object = $.$('<img />');
    let img: Element = <Element>imgObj[0];
    imgObj.on('load error', () => {
      BrowserSupport.$dataUri = img.width === 1 && img.height === 1;
    });
    img.src = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw==';
  }

  /**
   * Our default hit-testing uses coordinates which assume we aren't scrolled. This tests for a scroll possibility
   * and then also checks an actual hit test on absolute-positioned temporary DOM element.
   * Call this statically or rarely (once per drag at most).
   */
  private static detectDocumentElementFromPoint(): void {
    let body: HTMLElement = window.document.body;
    if (BrowserSupport.isWebKit && BrowserSupport.isMobile) {
      let target: Object = $.$('<div></div>');
      target.css(new Object('position', 'absolute', 'top', '300px', 'left', '300px', 'width', '25px', 'height', '25px', 'z-index', '10000'));
      let elem: HTMLElement = target.get(0);
      try {
        body.appendChild(elem);
        BrowserSupport.shouldUseAlternateHitStrategy = document.elementFromPoint(310, 310) !== elem;
      } catch { } finally {
        target.remove();
      }
    }
  }

  private static detectConsoleLogFormatting(): void {
    try {
      if (ss.isValue((<any>window).console && ss.isValue((<any>window).console.log))) {
        BrowserSupport.consoleLogFormatting = window.navigator.userAgent.indexOf('iPad') < 0;
      } else {
        BrowserSupport.consoleLogFormatting = false;
      }
    } catch {
      BrowserSupport.consoleLogFormatting = false;
    }
  }

  /**
   * NOTE-jrockwood-2012-11-29: Avoid browser detection like the plague.
   * Instead use feature detection. However, there are some small cases
   * where you have to sniff the browser user agent. Just be careful.
   */
  private static detectBrowser(): void {
    let ua: string = BrowserSupport.getUserAgent();
    BrowserSupport.isKhtml = ua.indexOf('Konqueror') >= 0;
    BrowserSupport.isWebKit = ua.indexOf('WebKit') >= 0;
    BrowserSupport.$isChrome = ua.indexOf('Chrome') >= 0;
    BrowserSupport.$isSafari = ua.indexOf('Safari') >= 0 && !BrowserSupport.$isChrome;
    BrowserSupport.$isOpera = ua.indexOf('Opera') >= 0;
    if (BrowserSupport.$isSafari) {
      let versionMatches: string[] = ua.match(new RegExp('\\bVersion\\/(\\d+\\.\\d+)'));
      if (versionMatches !== null) {
        BrowserSupport.$safariVersion = number.parseFloat(versionMatches[1]);
      }
    }
    BrowserSupport.internetExplorerVersion = 0;
    BrowserSupport.$isIE = false;
    let oldIEVersions: string[] = ua.match(new RegExp('\\bMSIE (\\d+\\.\\d+)'));
    if (oldIEVersions !== null) {
      BrowserSupport.$isIE = true;
      BrowserSupport.internetExplorerVersion = number.parseFloat(oldIEVersions[1]);
    }
    if (!BrowserSupport.$isIE && !BrowserSupport.$isOpera && (ua.indexOf('Trident') >= 0 || ua.indexOf('Edge/') >= 0)) {
      let tridentIEVersions: string[] = ua.match(new RegExp('\\brv:(\\d+\\.\\d+)'));
      let edgeIEVersions: string[] = ua.match(new RegExp('Edge/(\\d+\\.\\d+)'));
      if (tridentIEVersions !== null) {
        BrowserSupport.$isIE = true;
        BrowserSupport.internetExplorerVersion = number.parseFloat(tridentIEVersions[1]);
      } else
        if (edgeIEVersions !== null) {
          BrowserSupport.$isIE = true;
          BrowserSupport.$isChrome = false;
          BrowserSupport.$isSafari = false;
          BrowserSupport.internetExplorerVersion = number.parseFloat(edgeIEVersions[1]);
        }
    }
    BrowserSupport.isMozilla = !BrowserSupport.isKhtml && !BrowserSupport.isWebKit && !BrowserSupport.$isIE && ua.indexOf('Gecko') >= 0;
    BrowserSupport.$isFF = BrowserSupport.isMozilla || ua.indexOf('Firefox') >= 0 || ua.indexOf('Minefield') >= 0;
    let commandRegex: RegExp = new RegExp('iPhone|iPod|iPad');
    BrowserSupport.$isIos = commandRegex.test(ua);
    if (BrowserSupport.$isIos) {
      let iosVersions: string[] = ua.match(new RegExp('\\bOS ([\\d+_?]+) like Mac OS X'));
      if (iosVersions !== null) {
        BrowserSupport.$iosVersion = number.parseFloat(ss.replaceAllString(iosVersions[1].replace('_', '.'), '_', ''));
      }
    }
    BrowserSupport.$isAndroid = ua.indexOf('Android') >= 0 && !BrowserSupport.$isIE;
    BrowserSupport.$isMac = ua.indexOf('Mac') >= 0;
    BrowserSupport.$isWindows = ua.indexOf('Windows') >= 0;
  }

  private static getUserAgent(): string {
    return window.navigator.userAgent;
  }

  private static trySettingCssProperty(styleProp: string, cssProp: string, val: string): boolean {
    let e: HTMLElement = <HTMLElement>document.createElement('div');
    try {
      document.body.insertBefore(e, null);
      if (!stylePropin) {
        return false;
      }
      e.style[styleProp] = val;
      let s: CSSStyleDeclaration = DomUtil.getComputedStyle(e);
      let computedVal: string = s[cssProp];
      return !MiscUtil.isNullOrEmpty$1(computedVal) && computedVal !== 'none';
    } finally {
      (<HTMLElement>document.body.removeChild(e)).style.display = 'none';
    }
  }

  private static detectTransitionSupport(): void {
    let transitions: { [key: string]: string } = new JsDictionary<string, string>('transition', 'transition', 'webkitTransition', '-webkit-transition', 'msTransition', '-ms-transition', 'mozTransition', '-moz-transition', 'oTransition', '-o-transition');
    for (const t of transitions) {
      if (!t.keyin) {
        continue;
      }
      BrowserSupport.$cssTransitionName = t.value;
      break;
    }
  }

  private static detectTransformSupport(): void {
    let transforms: { [key: string]: string } = new JsDictionary<string, string>('transform', 'transform', 'webkitTransform', '-webkit-transform', 'msTransform', '-ms-transform', 'mozTransform', '-moz-transform', 'oTransform', '-o-transform');
    for (const t of transforms) {
      if (!t.keyin) {
        continue;
      }
      BrowserSupport.$cssTransformName = t.value;
      BrowserSupport.cssTranslate2d = BrowserSupport.trySettingCssProperty(t.key, t.value, 'translate(1px,1px)');
      BrowserSupport.cssTranslate3d = BrowserSupport.trySettingCssProperty(t.key, t.value, 'translate3d(1px,1px,1px)');
      break;
    }
  }

  private static detectDevicePixelRatio(): void {
    BrowserSupport.$devicePixelRatio = window.self['devicePixelRatio'] || 1;
  }

  private static detectBackingStoragePixelRatio(): void {
    let canvas: HTMLElement = document.createElement('canvas');
    if (ss.isNullOrUndefined(canvas)) {
      BrowserSupport.$backingStoragePixelRatio = 1;
      return;
    }
    let context: Object = null;
    if ((typeof (ss.getInstanceType(canvas)['getContext']) === 'function')) {
      context = (<Element>canvas).getContext('2d');
    }
    if (ss.isNullOrUndefined(context)) {
      BrowserSupport.$backingStoragePixelRatio = 1;
      return;
    }
    let ctx: any = context;
    BrowserSupport.$backingStoragePixelRatio = ctx.webkitBackingStorePixelRatio || ctx.mozBackingStorePixelRatio || ctx.msBackingStorePixelRatio || ctx.oBackingStorePixelRatio || 1;
  }

  /**
   * Detects if the canvas supports custom line patterns.
   */
  private static detectCanvasLinePattern(): void {
    let canvas: HTMLElement = document.createElement('canvas');
    if (ss.isNullOrUndefined(canvas)) {
      return;
    }
    let context: Object = null;
    if ((typeof (canvas['getContext']) === 'function')) {
      context = (<Element>canvas).getContext('2d');
    }
    if (ss.isNullOrUndefined(context)) {
      return;
    }
    BrowserSupport.$canvasLinePattern = (typeof (context['setLineDash']) === 'function') || 'mozDash' in context || 'webkitLineDash' in context;
  }

  private static detectSetSelectionRangeSupport(): void {
    let inputObject: Object = $.$('<input>');
    BrowserSupport.$setSelectionRange = (typeof (inputObject.get(0)['setSelectionRange']) === 'function');
  }

  private static detectDateInputSupport(): void {
    BrowserSupport.$dateInput = BrowserSupport.detectCustomInputSupport('date');
    BrowserSupport.$dateTimeInput = BrowserSupport.detectCustomInputSupport('datetime');
    BrowserSupport.$dateTimeLocalInput = BrowserSupport.detectCustomInputSupport('datetime-local');
    BrowserSupport.$timeInput = BrowserSupport.detectCustomInputSupport('time');
  }

  private static detectCustomInputSupport(inputType: string): boolean {
    let inputObject: Object = $.$('<input type=\'' + inputType + '\'>').css(new CssDictionary()).appendTo($.$(document.body));
    let input: Element = <Element>inputObject.get(0);
    let reportedInputType: string = <string>input.getAttribute('type');
    const InvalidDataString: string = '@inva/1d:)';
    input.value = InvalidDataString;
    let supportsInput: boolean = (reportedInputType === inputType && input.value !== InvalidDataString);
    inputObject.remove();
    return supportsInput;
  }
}

// Call the static constructor
BrowserSupport.__ctor();
