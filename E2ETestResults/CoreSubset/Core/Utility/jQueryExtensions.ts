import { TypeUtil } from 'NativeJsTypeDefs';

/**
 * Contains plugins to the native jQuery object. Generally, you should prefer to use helper methods rather than
 * writing a jQuery plugin, but sometimes it's useful to act on a group of jQueryObjects all at once and have the
 * chaining support built-in.
 * 
 * You use this by type-casting a normal {@link jQueryObject} to a {@link jQueryExtensionsObject}.
 */
export class jQueryExtensions {
  public static getTouchEvent(jqueryEvent: Object): TouchEvent {
    return jqueryEvent['originalEvent'];
  }
}

/**
 * This doesn't actually contain any code and is only used to provide the ability to call into the jQuery plugins
 * that we have defined in {@link jQueryExtensions}.
 */
export class jQueryExtensionsObject extends Object {
  public delay(milliseconds: number): jQueryExtensionsObject {
    return null;
  }

  /**
   * Extends the jQuery built-in `focus` function by addint the ability to call focus on a delayed
   * timer. This is extremely useful when inside of a browser event handler so the browser can continue to
   * process the event without waiting for the focus to happen.
   * @param delayMilliseconds The amount of time to wait in milliseconds before focusing.
   */
  public focusDelayed(delayMilliseconds: number): void;

  /**
   * Extends the jQuery built-in `focus` function by addint the ability to call focus on a delayed
   * timer. This is extremely useful when inside of a browser event handler so the browser can continue to
   * process the event without waiting for the focus to happen.
   * 
   * Taken from jquery.ui.core.js
   * @param delayMilliseconds The amount of time to wait in milliseconds before focusing.
   * @param action The code to run after the delay and after the element has received focus.
   */
  public focusDelayed(delayMilliseconds: number, action?: () => void): void { }

  public on<T>(eventName: string, handler: (jQueryEvent: Object, t: T) => void): jQueryExtensionsObject;

  public on<T>(eventName: string, selector: string, handler: (jQueryEvent: Object, t: T) => void): jQueryExtensionsObject;

  public on(eventName: string, handler: (jQueryEvent: Object) => void | (jQueryEvent: Object, t: T) => void | string, handler?: (jQueryEvent: Object, t: T) => void | (jQueryEvent: Object) => void): jQueryExtensionsObject;

  public on(eventName: string, selector: string, handler: (jQueryEvent: Object) => void): jQueryExtensionsObject;

  public outerHeight(height: number): jQueryExtensionsObject;
}
