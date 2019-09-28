import { BootstrapResponse, SecondaryBootstrapResponse } from './BootstrapResponse';

import 'mscorlib';

import { MetricsContext, MetricsController, MetricsSuites } from './Performance/MetricsController';

import { tsConfig } from 'TypeDefs';

/**
 * Utility functions for Bootstrap code
 */
export class Utility {
  /**
   * Gets the WindowInstance that corresponds to the viz location. If in WG then it's the WG parent frame.
   * Otherwise it's the viz's frame.
   */
  public static get locationWindow(): Object {
    return window.self;
  }

  public static xhrPostJsonChunked(uri: Object, param: string, firstChunkCallback: (bootstrapResponse: BootstrapResponse) => void, secondaryChunkCallback: (secondaryBootstrapResponse: SecondaryBootstrapResponse) => void, errBack: (object: any) => void, asynchronous: boolean): void {
    let xhr: XMLHttpRequest = new XMLHttpRequest();
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
        let chunkLength: number = ss.parseInt(match[1]).ReinterpretAs();
        if (chunkStart + chunkLength > newData.length) {
          return;
        }
        newData = newData.substr(chunkStart, chunkLength);
        let json: any = null;
        try {
          let contextStr: string = 'Parse ' + ((byteOffset === 0) ? 'Primary' : 'Secondary') + ' JSON';
          {
            const mc: MetricsContext = MetricsController.createContext(contextStr, MetricsSuites.Debug);
            try {
              json = ParseJson<any>(newData);
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
          firstChunkCallback(json.ReinterpretAs());
        } else {
          secondaryChunkCallback(json.ReinterpretAs());
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
    let xhr: XMLHttpRequest = new XMLHttpRequest();
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
    let xhr: XMLHttpRequest = new XMLHttpRequest();
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
          let json: any = ParseJson<any>(xhr.responseText);
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

  public static doPostMessage(message: any): boolean {
    let success: boolean = false;
    if ('postMessage' in window) {
      try {
        window.parent.postMessage(message, '*');
        success = true;
      } catch { }
    }
    return success;
  }

  public static parseJson<T>(data: string): T {
    if (ss.isNullOrUndefined(data) || typeof data !== 'string') {
      return ss.getDefaultValue(T);
    }
    data = data.trim();
    return JSON.parse(data);
  }

  public static getTopmostWindow(): Object {
    let win: Object = window.self;
    while (ss.isValue(win.parent) && win.parent !== win) {
      win = win.parent;
    }
    return win;
  }

  private static isSuccessStatus(xhr: XMLHttpRequest): boolean {
    let status: number = ss.isValue(xhr.status) ? xhr.status : 0;
    if ((status >= 200 && status < 300) || status === 304 || status === 1223 || (status === 0 && (window.location.protocol === 'file:' || window.location.protocol === 'chrome:'))) {
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
      err = err || new any();
      err.status = xhr.status;
      err.responseText = xhr.responseText;
      errBack(err);
    };
  }
}
