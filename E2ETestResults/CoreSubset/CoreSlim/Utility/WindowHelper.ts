import 'mscorlib';

import { TypeUtil } from 'NativeJsTypeDefs';

/**
 * Contains a set of helper functions for a {@link WindowInstance}.
 */
export class WindowHelper {
  private static readonly innerWidthFunc: (windowInstance: Object) => number;

  private static readonly innerHeightFunc: (windowInstance: Object) => number;

  private static readonly clientWidthFunc: (windowInstance: Object) => number;

  private static readonly clientHeightFunc: (windowInstance: Object) => number;

  private static readonly pageXOffsetFunc: (windowInstance: Object) => number;

  private static readonly pageYOffsetFunc: (windowInstance: Object) => number;

  private static readonly screenLeftFunc: (windowInstance: Object) => number;

  private static readonly screenTopFunc: (windowInstance: Object) => number;

  private static readonly outerWidthFunc: (windowInstance: Object) => number;

  private static readonly outerHeightFunc: (windowInstance: Object) => number;

  private static requestAnimationFrameFunc: (action: () => void) => number;

  private static cancelAnimationFrameFunc: (int32: number) => void;

  private readonly window: Object;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    if ('innerWidth' in window) {
      WindowHelper.innerWidthFunc = (w: Object) => {
        return w.innerWidth;
      };
    } else {
      WindowHelper.innerWidthFunc = (w: Object) => {
        return w.document.documentElement.offsetWidth;
      };
    }
    if ('outerWidth' in window) {
      WindowHelper.outerWidthFunc = (w: Object) => {
        return w.outerWidth;
      };
    } else {
      WindowHelper.outerWidthFunc = WindowHelper.innerWidthFunc;
    }
    if ('innerHeight' in window) {
      WindowHelper.innerHeightFunc = (w: Object) => {
        return w.innerHeight;
      };
    } else {
      WindowHelper.innerHeightFunc = (w: Object) => {
        return w.document.documentElement.offsetHeight;
      };
    }
    if ('outerHeight' in window) {
      WindowHelper.outerHeightFunc = (w: Object) => {
        return w.outerHeight;
      };
    } else {
      WindowHelper.outerHeightFunc = WindowHelper.innerHeightFunc;
    }
    if ('clientWidth' in window) {
      WindowHelper.clientWidthFunc = (w: Object) => {
        return w['clientWidth'];
      };
    } else {
      WindowHelper.clientWidthFunc = (w: Object) => {
        return w.document.documentElement.clientWidth;
      };
    }
    if ('clientHeight' in window) {
      WindowHelper.clientHeightFunc = (w: Object) => {
        return w['clientHeight'];
      };
    } else {
      WindowHelper.clientHeightFunc = (w: Object) => {
        return w.document.documentElement.clientHeight;
      };
    }
    if (ss.isValue(window.self.pageXOffset)) {
      WindowHelper.pageXOffsetFunc = (w: Object) => {
        return w.pageXOffset;
      };
    } else {
      WindowHelper.pageXOffsetFunc = (w: Object) => {
        return w.document.documentElement.scrollLeft;
      };
    }
    if (ss.isValue(window.self.pageYOffset)) {
      WindowHelper.pageYOffsetFunc = (w: Object) => {
        return w.pageYOffset;
      };
    } else {
      WindowHelper.pageYOffsetFunc = (w: Object) => {
        return w.document.documentElement.scrollTop;
      };
    }
    if ('screenLeft' in window) {
      WindowHelper.screenLeftFunc = (w: Object) => {
        return (<any>w).screenLeft;
      };
    } else {
      WindowHelper.screenLeftFunc = (w: Object) => {
        return w.screenX;
      };
    }
    if ('screenTop' in window) {
      WindowHelper.screenTopFunc = (w: Object) => {
        return (<any>w).screenTop;
      };
    } else {
      WindowHelper.screenTopFunc = (w: Object) => {
        return w.screenY;
      };
    }
    {
      const DefaultRequestName: string = 'requestAnimationFrame';
      const DefaultCancelName: string = 'cancelAnimationFrame';
      let vendors: string[] = ['ms', 'moz', 'webkit', 'o'];
      let requestFuncName: string = null;
      let cancelFuncName: string = null;
      if (DefaultRequestName in window) {
        requestFuncName = DefaultRequestName;
      }
      if (DefaultCancelName in window) {
        cancelFuncName = DefaultCancelName;
      }
      for (let ii = 0; ii < vendors.length && (requestFuncName === null || cancelFuncName === null); ++ii) {
        let vendor: string = vendors[ii];
        let funcName: string = vendor + 'RequestAnimationFrame';
        if (requestFuncName === null && funcName in window) {
          requestFuncName = funcName;
        }
        if (cancelFuncName === null) {
          funcName = vendor + 'CancelAnimationFrame';
          if (funcName in window) {
            cancelFuncName = funcName;
          }
          funcName = vendor + 'CancelRequestAnimationFrame';
          if (funcName in window) {
            cancelFuncName = funcName;
          }
        }
      }
      if (requestFuncName !== null) {
        WindowHelper.requestAnimationFrameFunc = (callback: () => void) => {
          return window[requestFuncName](callback);
        };
      } else {
        WindowHelper.setDefaultRequestAnimationFrameImpl();
      }
      if (cancelFuncName !== null) {
        WindowHelper.cancelAnimationFrameFunc = (animationId: number) => {
          window[cancelFuncName](animationId);
        };
      } else {
        WindowHelper.cancelAnimationFrameFunc = (id: number) => {
          window.clearTimeout(id);
        };
      }
    }}

  public constructor(window: Object) {
    this.window = window;
  }

  /**
   * Gets the window pageXOffset, or equivalent.
   */
  public get pageXOffset(): number {
    return WindowHelper.pageXOffsetFunc(this.window);
  }

  /**
   * Gets the window pageYOffset, or equivalent.
   */
  public get pageYOffset(): number {
    return WindowHelper.pageYOffsetFunc(this.window);
  }

  /**
   * Gets the window pageXOffset, or equivalent.
   */
  public get clientWidth(): number {
    return WindowHelper.clientWidthFunc(this.window);
  }

  /**
   * Gets the window pageYOffset, or equivalent.
   */
  public get clientHeight(): number {
    return WindowHelper.clientHeightFunc(this.window);
  }

  /**
   * Gets the window innerWidth, or equivalent.
   */
  public get innerWidth(): number {
    return WindowHelper.innerWidthFunc(this.window);
  }

  /**
   * Gets the window outerWidth, or innerWidth if unavailable
   */
  public get outerWidth(): number {
    return WindowHelper.outerWidthFunc(this.window);
  }

  /**
   * Gets the window innerHeight, or equivalent.
   */
  public get innerHeight(): number {
    return WindowHelper.innerHeightFunc(this.window);
  }

  /**
   * Gets the window outerHeight, or innerHeight if unavailable.
   */
  public get outerHeight(): number {
    return WindowHelper.outerHeightFunc(this.window);
  }

  /**
   * Gets the window screenLeft, or equivalent.
   */
  public get screenLeft(): number {
    return WindowHelper.screenLeftFunc(this.window);
  }

  /**
   * Gets the window screenTop, or equivalent
   */
  public get screenTop(): number {
    return WindowHelper.screenTopFunc(this.window);
  }

  /**
   * Gets the window's self.
   * This method only exists to support testing.
   */
  public static get windowSelf(): Object {
    return window.self;
  }

  /**
   * Get the current window or document Selection object
   * @returns The selection object, or null if none
   */
  public static get selection(): Selection {
    if ((typeof (window['getSelection']) === 'function')) {
      return window.getSelection();
    } else
      if ((typeof (document['getSelection']) === 'function')) {
        return document.getSelection();
      }
    return null;
  }

  /**
   * Calls {@link Window.Close} on the given window.
   * This method only exists to support testing.
   */
  public static close(window: Object): void {
    window.close();
  }

  /**
   * Gets the window opener.
   * This method only exists to support testing.
   */
  public static getOpener(window: Object): Object {
    return window.opener;
  }

  /**
   * Gets the current window location.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   */
  public static getLocation(window: Object): Location {
    return window.location;
  }

  /**
   * Gets the current window location.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   */
  public static getPathAndSearch(window: Object): string {
    return window.location.pathname + window.location.search;
  }

  /**
   * Sets the href property on the window's location.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   * @param window The window containing the location
   * @param href The href to set
   */
  public static setLocationHref(window: Object, href: string): void {
    window.location.href = href;
  }

  /**
   * Calls {@link Location.Replace} on the given window.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   */
  public static locationReplace(window: Object, url: string): void {
    window.location.replace(url);
  }

  public static open(href: string, target: string): Object;

  /**
   * Calls {@link Window.Open}.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   * @param href 
   * @param target 
   * @param options 
   */
  public static open(href: string, target: string, options?: string): Object {
    return window.open(href, target, options);
  }

  /**
   * Calls window.location.reload.
   * This method only exists to support testing as the standard window.location cannot be mocked safely.
   */
  public static reload(w: Object, forceGet: boolean = false): void {
    w.location.reload(forceGet);
  }

  /**
   * Requests an animation frame
   * Falls back to using Window.SetTimeout if the browser does not support RequestAnimationFrame
   * @param action Action to execute
   * @returns animation id used to cancel the animation
   */
  public static requestAnimationFrame(action: () => void): number {
    return WindowHelper.requestAnimationFrameFunc(action);
  }

  /**
   * Cancels an animation
   * @param animationId id of animation to cancel
   */
  public static cancelAnimationFrame(animationId: number): void {
    if (ss.isValue(animationId)) {
      WindowHelper.cancelAnimationFrameFunc(animationId);
    }
  }

  /**
   * This method exists so that Window.SetTimeout can be mocked
   */
  public static setTimeout(callback: () => void, milliseconds: number): void {
    window.setTimeout(callback, milliseconds);
  }

  public static addListener(windowParam: EventTarget, eventName: string, messageListener: HtmlEventHandler): void {
    if ('addEventListener' in windowParam) {
      windowParam.addEventListener(eventName, messageListener, false);
    } else {
      (<any>windowParam).attachEvent('on' + eventName, messageListener);
    }
  }

  public static removeListener(window: EventTarget, eventName: string, messageListener: HtmlEventHandler): void {
    if ('removeEventListener' in window) {
      window.removeEventListener(eventName, messageListener, false);
    } else {
      (<any>window).detachEvent('on' + eventName, messageListener);
    }
  }

  /**
   * Sets the RequestAnimationFrame implementation to the fallback if there is no browser support.
   * This is in its own method so the closure over 'lastTime' is smaller.
   */
  private static setDefaultRequestAnimationFrameImpl(): void {
    let lastTime: number = 0;
    WindowHelper.requestAnimationFrameFunc = (callback: () => void) => {
      let curTime: number = <number>(new Date().getTime());
      let timeToCall: number = Math.max(0, 16 - (curTime - lastTime));
      lastTime = curTime + timeToCall;
      let id: number = window.setTimeout(callback, timeToCall);
      return id;
    };
  }

  /**
   * Clears the selection object at the window/document level
   */
  public static clearSelection(): void {
    let selection: Selection = WindowHelper.selection;
    if (selection !== null) {
      if ((typeof (selection['removeAllRanges']) === 'function')) {
        selection.removeAllRanges();
      } else
        if ((typeof (selection['empty']) === 'function')) {
          selection['empty']();
        }
    }
  }
}

// Call the static constructor
WindowHelper.__ctor();
