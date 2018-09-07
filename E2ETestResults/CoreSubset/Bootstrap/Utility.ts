import 'mscorlib';

import { Metric } from './LayoutMetrics';

import { MetricsContext, MetricsController, MetricsSuites } from './MetricsController';

import { tsConfig } from 'TypeDefs';

import { TypeUtil, WindowOrientation } from 'NativeJsTypeDefs';

/**
 * The different scenarios in which the current viz can be embedded.
 */
export const enum EmbedMode {
  NotEmbedded = 'notEmbedded',
  EmbeddedInWg = 'embeddedInWg',
  EmbeddedNotInWg = 'embeddedNotInWg',
}

/**
 * Utility functions for Bootstrap code
 */
export class Utility {
  private static readonly safari7ClientHeightErrorPixels: number = 20;

  private static readonly regexNotwhite: RegExp = new RegExp('\\s');

  private static readonly regexTrimLeft: RegExp = new RegExp('^\\s+');

  private static readonly regexTrimRight: RegExp = new RegExp('\\s+$');

  private static readonly embedModeVar: EmbedMode;

  public static cLIENTNO: string = 'cn';

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    if (Utility.regexNotwhite.test('\\xA0')) {
      Utility.regexTrimLeft = new RegExp('^[\\s\\xA0]+');
      Utility.regexTrimRight = new RegExp('[\\s\\xA0]+$');
    }
    Utility.embedModeVar = Utility.calculateEmbedMode();
  }

  public static get needsSafari7HackFix(): boolean {
    if (!tsConfig.is_mobile_device || tsConfig.embedded) {
      return false;
    }
    let isAndroid: boolean = (window.navigator.userAgent.indexOf('Android') !== -1);
    if (isAndroid) {
      return false;
    }
    let isSafari7: boolean = (window.navigator.userAgent.indexOf('Safari') !== -1) && (window.navigator.userAgent.indexOf('OS 7') !== -1);
    return isSafari7;
  }

  /**
   * This method should only be used within Utility, and only to find out the orientation for Safari Mobile.
   * In general, please use Spiff.OrientationHandler for all things orientation.
   * 
   * This method lives here so it can be in the Bootstrap code that's loaded in the page
   */
  private static get inLandscapeMode(): boolean {
    try {
      let win: Object = Utility.getTopmostWindow();
      let orientation: WindowOrientation = win.GetOrientation();
      return ss.isValue(orientation) && (orientation === WindowOrientation.LeftLandscape || orientation === WindowOrientation.RightLandscape);
    } catch { }
    return false;
  }

  public static get urlLocationSearchParams(): Object<string, string> {
    return Utility.parseQueryParamString(Utility.urlLocationSearch.substring(1));
  }

  public static get urlLocationHashData(): Object<string, string> {
    let urlHashData: Object<string, string> = {};
    let fragmentId: string = Utility.urlLocationHash;
    if (fragmentId.length < 2) {
      return {};
    }
    fragmentId = fragmentId.substr(1);
    let pairs: string[] = fragmentId.split('&');
    for (const pair of pairs) {
      let keyVal: string[] = pair.split('=');
      if (keyVal.length === 1) {
        urlHashData[Utility.cLIENTNO] = keyVal[0];
      } else
        if (keyVal.length === 2) {
          let key: string = string.decodeURIComponent(keyVal[0]);
          let value: string = string.decodeURIComponent(keyVal[1]);
          urlHashData[key] = value;
        }
    }
    return urlHashData;
  }

  public static set urlLocationHashData(value: Object<string, string>) {
    let newFragmentId: ss.StringBuilder = new ss.StringBuilder();
    let first: boolean = true;
    let appendSeparator: () => void = () => {
      newFragmentId.append(first ? '#' : '&');
      first = false;
    };
    for (const pairs of value) {
      let keyEncoded: string = string.encodeURIComponent(pairs.key);
      appendSeparator();
      if (keyEncoded === Utility.cLIENTNO) {
        newFragmentId.append(pairs.value);
      } else
        if (ss.isNullOrUndefined(pairs.value)) {
          newFragmentId.append(keyEncoded);
        } else {
          newFragmentId.append(keyEncoded).append('=').append(string.encodeURIComponent(pairs.value));
        }
    }
    if (ss.isValue(newFragmentId)) {
      let window: Object = Utility.locationWindow;
      if (Utility.historyApiSupported()) {
        Utility.replaceState(window, null, null, newFragmentId.toString());
      } else {
        window.location.hash = newFragmentId.toString();
      }
    }
  }

  public static get urlLocationHash(): string {
    let window: Object = Utility.locationWindow;
    return window.location.hash;
  }

  public static get urlLocationSearch(): string {
    let window: Object = Utility.locationWindow;
    return window.location.search;
  }

  public static get embedMode(): EmbedMode {
    return Utility.embedModeVar;
  }

  /**
   * Gets the WindowInstance that corresponds to the viz location. If in WG then it's the WG parent frame.
   * Otherwise it's the viz's frame.
   */
  public static get locationWindow(): Object {
    return Utility.embedMode === EmbedMode.EmbeddedInWg ? window.parent : window.self;
  }

  private static parseQueryParamString(urlStr: string): Object<string, string> {
    let urlData: Object<string, string> = {};
    let pairs: string[] = urlStr.split('&');
    for (const pair of pairs) {
      let keyVal: string[] = pair.split('=');
      if (keyVal.length === 2) {
        let key: string = string.decodeURIComponent(keyVal[0]);
        let value: string = string.decodeURIComponent(keyVal[1]);
        urlData[key] = value;
      }
    }
    return urlData;
  }

  public static xhrPostJsonChunked(uri: Object, param: string, firstChunkCallback: (object: any) => void, secondaryChunkCallback: (object: any) => void, errBack: (object: any) => void, asynchronous: boolean): void {
    let xhr: XMLHttpRequest = <XMLHttpRequest>Utility.createXhr();
    xhr.open('POST', uri, asynchronous);
    xhr.setRequestHeader('Accept', 'text/javascript');
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    if (!ss.isNullOrUndefined(tsConfig.sheetId)) {
      xhr.setRequestHeader('X-Tsi-Active-Tab', tsConfig.sheetId);
    }
    let invokeError: (object: any) => void = Utility.getInvokeErrorDelegate(xhr, errBack);
    let byteOffset: number = 0;
    let consumeJSONChunks: () => void = () => {
      let buffer: string = '';
      try {
        buffer = xhr.responseText;
      } catch (e) { }
      let bufferLength: number = buffer.length;
      while (byteOffset < bufferLength) {
        let newData: string = buffer.substr(byteOffset);
        let regex: RegExp = new RegExp('^(\\d+);');
        let match: string[] = newData.match(regex);
        if (!ss.isValue(match)) {
          return;
        }
        let chunkStart: number = match[0].length;
        let chunkLength: number = number.parseInt(match[1]);
        if (chunkStart + chunkLength > newData.length) {
          return;
        }
        newData = newData.substr(chunkStart, chunkLength);
        let json: string = null;
        try {
          let contextStr: string = 'Parse ' + ((byteOffset === 0) ? 'Primary' : 'Secondary') + ' JSON';
          {
            const mc: MetricsContext = MetricsController.createContext(contextStr, MetricsSuites.Debug);
            try {
              json = Utility.parseJson(newData);
            } finally {
              if (mc) {
                mc.dispose();
              }
            }
          }
        } catch (e) {
          invokeError(new ss.Exception('Invalid JSON'));
        }
        if (byteOffset === 0) {
          firstChunkCallback(json);
        } else {
          secondaryChunkCallback(json);
        }
        byteOffset += chunkStart + chunkLength;
      }
    };
    let intervalID: number = -1;
    let isReceiving: boolean = false;
    let cannotTouchXhrWhileDownloading: boolean = (window.navigator.userAgent.indexOf('MSIE') >= 0 && number.parseFloat(window.navigator.appVersion.split('MSIE ')[1]) < 10);
    xhr.onreadystatechange = () => {
      try {
        if (!cannotTouchXhrWhileDownloading && xhr.readyState === ReadyState.Loading && xhr.status === 200 && !isReceiving) {
          consumeJSONChunks();
          if (intervalID === -1) {
            intervalID = window.setInterval(consumeJSONChunks, 10);
          }
          isReceiving = true;
          return;
        }
        if (xhr.readyState !== ReadyState.Done) {
          return;
        }
        if (intervalID !== -1) {
          window.clearInterval(intervalID);
          intervalID = -1;
        }
        if (Utility.isSuccessStatus(xhr)) {
          consumeJSONChunks();
        } else {
          invokeError(new ss.Exception('Unable to load ' + uri + '; status: ' + xhr.status));
        }
      } catch (ex) {
        if (typeof ss.getType('ss') === 'undefined') {
          xhr.abort();
        } else {
          throw ex;
        }
      }
    };
    try {
      xhr.send(param);
    } catch (e) {
      invokeError(e);
    }
  }

  /**
   * A Simple synchronous GET to an XML-returning URI
   */
  public static xhrGetXmlSynchronous(uri: Object, errBack: (object: any) => void): string {
    let xhr: XMLHttpRequest = <XMLHttpRequest>Utility.createXhr();
    xhr.open('GET', uri, false);
    xhr.setRequestHeader('Accept', 'text/xml');
    try {
      xhr.send();
    } catch (e) {
      Utility.invokeErrorDelegate(xhr, errBack, e);
      return null;
    }
    return xhr.responseText;
  }

  public static xhrPostJson(uri: Object, param: string, callback: (object: any) => void, errBack: (object: any) => void, asynchronous: boolean): void {
    let xhr: XMLHttpRequest = <XMLHttpRequest>Utility.createXhr();
    xhr.open('POST', uri, asynchronous);
    xhr.setRequestHeader('Accept', 'text/javascript');
    xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
    if (ss.isValue(tsConfig.sheetId)) {
      xhr.setRequestHeader('X-Tsi-Active-Tab', tsConfig.sheetId);
    }
    let invokeError: (object: any) => void = Utility.getInvokeErrorDelegate(xhr, errBack);
    xhr.onreadystatechange = () => {
      if (xhr.readyState !== ReadyState.Done) {
        return;
      }
      if (Utility.isSuccessStatus(xhr)) {
        try {
          let json: string = Utility.parseJson(xhr.responseText);
          callback(json);
        } catch (x) {
          invokeError(x);
        }
      } else {
        invokeError(new ss.Exception('Unable to load ' + uri + '; status: ' + xhr.status));
      }
    };
    try {
      xhr.send(param);
    } catch (e) {
      invokeError(e);
    }
  }

  public static applySafari7CSSHackFix(): void {
    if (Utility.needsSafari7HackFix) {
      if (Utility.inLandscapeMode) {
        window.document.body.style.height = (window.outerHeight - Utility.safari7ClientHeightErrorPixels) + 'px';
      } else {
        window.document.body.style.height = '';
      }
    }
  }

  /**
   * Attaches an event handler for 'message' event, and calls the specified handler
   * function when event is received. If the passed handler handles the event (returns true) the
   * event handler will be removed
   * @param eventHandler 
   */
  public static attachOneTimeMessageHandler(eventHandler: (messageEvent: MessageEvent) => boolean): void {
    let messageListener: HtmlEventHandler = null;
    messageListener = (ev: Event) => {
      let e: MessageEvent = <MessageEvent>ev;
      if (eventHandler(e)) {
        if (ss.isValue((<any>window.self).removeEventListener)) {
          window.removeEventListener('message', messageListener, false);
        } else {
          (<any>window.self).detachEvent('onmessage', messageListener);
        }
      }
    };
    if (ss.isValue((<any>window.self).addEventListener)) {
      window.addEventListener('message', messageListener, false);
    } else {
      (<any>window.self).attachEvent('onmessage', messageListener);
    }
  }

  public static doPostMessageWithContext(message: string): boolean {
    let success: boolean = false;
    if (tsConfig.loadOrderID >= 0) {
      message += ',' + tsConfig.loadOrderID.As();
    }
    if (!ss.isNullOrEmptyString(tsConfig.apiID)) {
      if (tsConfig.loadOrderID < 0) {
        message += ',' + tsConfig.loadOrderID.As();
      }
      message += ',' + tsConfig.apiID.As();
    }
    success = Utility.doPostMessage(message);
    return success;
  }

  public static doPostMessage(message: string): boolean {
    let success: boolean = false;
    if ('postMessage' in window) {
      try {
        window.parent.postMessage(message, '*');
        success = true;
      } catch { }
    }
    return success;
  }

  public static calculateEmbedMode(): EmbedMode {
    let parentIsSelf: boolean = false;
    try {
      parentIsSelf = window.self === window.parent;
    } catch { }
    if (parentIsSelf) {
      return EmbedMode.NotEmbedded;
    }
    if (tsConfig.single_frame) {
      return EmbedMode.EmbeddedNotInWg;
    }
    return EmbedMode.EmbeddedInWg;
  }

  public static trim(text: string): string {
    if (ss.isNullOrUndefined(text)) {
      return '';
    }
    let nativeTrimFunction: Function = string.prototype['trim'].As();
    if (nativeTrimFunction !== null) {
      let result: string = nativeTrimFunction.call(text).As();
      return result;
    } else {
      return text.replace(Utility.regexTrimLeft, '').replace(Utility.regexTrimRight, '');
    }
  }

  public static parseJson(data: Object): string {
    if (ss.isNullOrUndefined(data) || typeof data !== 'string') {
      return null;
    }
    data = Utility.trim(data);
    let result: string = window.JSON && window.JSON.parse ? window.JSON.parse(data) : (new Function('return ' + data))();
    return result;
  }

  private static nativeParseJson(data: Object): string {
    return null;
  }

  public static createXhr(): any {
    try {
      return new XMLHttpRequest();
    } catch (e) { }
    try {
      return new ActiveXObject('Microsoft.XMLHTTP');
    } catch (e) { }
    throw new ss.Exception('XMLHttpRequest not supported');
  }

  public static getViewport(): Metric {
    let docElem: HTMLElement = <HTMLElement>window.document.documentElement;
    return new Metric(docElem.clientWidth, docElem.clientHeight);
  }

  public static getTopmostWindow(): Object {
    let win: Object = window.self;
    while (ss.isValue(win.parent) && win.parent !== win) {
      win = win.parent;
    }
    return win;
  }

  public static getNonEmbeddedMobileViewport(): Metric {
    let temp: number, chromeSpace: number;
    let w: number = window.document.documentElement.clientWidth;
    let h: number = window.document.documentElement.clientHeight;
    let isAndroid: boolean = (window.navigator.userAgent.indexOf('Android') !== -1);
    if (isAndroid) {
      if (w === window.screen.height) {
        chromeSpace = window.screen.width - h;
        temp = w - chromeSpace;
        w = h + chromeSpace;
        h = temp;
      }
    } else
      if (Utility.inLandscapeMode) {
        if ((window.innerHeight < h) && Utility.needsSafari7HackFix) {
          h -= Utility.safari7ClientHeightErrorPixels;
        }
        if (w === window.screen.width) {
          chromeSpace = window.screen.height - h;
          temp = w - chromeSpace;
          w = h + chromeSpace;
          h = temp;
        }
      } else {
        if (w === window.screen.height) {
          chromeSpace = window.screen.width - h;
          temp = w - chromeSpace;
          w = h + chromeSpace;
          h = temp;
        }
      }
    return new Metric(w, h);
  }

  public static isCanvasSupported(): boolean {
    let canvas: HTMLElement = document.createElement('canvas');
    if (ss.isNullOrUndefined(canvas) || ss.isNullOrUndefined((<any>canvas)['getContext'])) {
      return false;
    }
    let context: Object = (<Element>canvas).getContext(CanvasContextId.Render2D);
    return (typeof (context['fillText']) === 'function') && ss.isValue(context['measureText']('foo'));
  }

  public static get hashClientNumber(): string {
    let info: Object<string, string> = Utility.urlLocationHashData;
    return ss.isValue(info) && ss.isValue(info[Utility.cLIENTNO]) ? info[Utility.cLIENTNO] : '';
  }

  public static addToUrlHash(key: string, value: string): void {
    let urlHash: Object<string, string> = Utility.urlLocationHashData;
    urlHash[key] = value;
    Utility.urlLocationHashData = urlHash;
  }

  public static historyApiSupported(): boolean {
    return (typeof (window.history['pushState']) === 'function') && (typeof (window.history['replaceState']) === 'function');
  }

  public static replaceState(window: Object, state: any, title: string, url: Object): void {
    try {
      window.history.replaceState(state, title, url);
    } catch (e) { }
  }

  public static getValueFromUrlHash(key: string): string {
    let urlHash: Object<string, string> = Utility.urlLocationHashData;
    return ss.keyExists(urlHash, key) ? urlHash[key] : '';
  }

  public static removeEntryFromUrlHash(key: string): void {
    let fragInfo: Object<string, string> = Utility.urlLocationHashData;
    delete fragInfo[key];
    Utility.urlLocationHashData = fragInfo;
  }

  /**
   * Gets the pixel ratio of the device. We will fall back to 1.0f if it's not supported on
   * the browser. This should be safe because at worse, we will not render at high resolution.
   */
  public static getDevicePixelRatio(): number {
    let devicePixelRatio: number = 1;
    if (ss.isValue(tsConfig.highDpi) && tsConfig.highDpi) {
      if (ss.isValue(tsConfig.pixelRatio)) {
        devicePixelRatio = tsConfig.pixelRatio;
      } else {
        devicePixelRatio = window.self['devicePixelRatio'] || 1;
      }
    }
    return devicePixelRatio;
  }

  private static isSuccessStatus(xhr: XMLHttpRequest): boolean {
    let status: number = ss.isValue(xhr.status) ? xhr.status : 0;
    if ((status >= 200 && status < 300) || status === 304 || status === 1223 || (0 === status && (window.location.protocol === 'file:' || window.location.protocol === 'chrome:'))) {
      return true;
    }
    return false;
  }

  private static invokeErrorDelegate(xhr: XMLHttpRequest, errBack: (object: any) => void, e: ss.Exception): void {
    if (errBack === null) {
      return;
    }
    let invokeError: (object: any) => void = Utility.getInvokeErrorDelegate(xhr, errBack);
    invokeError(e);
  }

  private static getInvokeErrorDelegate(xhr: XMLHttpRequest, errBack: (object: any) => void): (object: any) => void {
    return (err: any) => {
      err.status = xhr.status;
      err.responseText = xhr.responseText;
      errBack(err);
    };
  }
}

// Call the static constructor
Utility.__ctor();
