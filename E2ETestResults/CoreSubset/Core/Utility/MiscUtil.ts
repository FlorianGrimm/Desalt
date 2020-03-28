import { FeatureFlagIds, tsConfig } from 'TypeDefs';

import { FeatureFlags } from './FeatureFlags';

import { JsNativeExtensionMethods, TypeUtil } from 'NativeJsTypeDefs';

import 'mscorlib';

import { _ } from 'Underscore';

import { Utility } from '../../Bootstrap/Utility';

import { WindowHelper } from '../../CoreSlim/Utility/WindowHelper';

export enum PathnameKey {
  WorkbookName = 2,
  SheetId = 3,
  AuthoringSheet = 4,
}

/**
 * Contains utility methods for etc.
 */
export class MiscUtil {
  public static get_pathName(): string {
    let window: Object = Utility.locationWindow;
    return WindowHelper.getLocation(window).pathname;
  }

  public static get_urlPathnameParts(): { [key: number]: string } {
    let pathname: string = MiscUtil.pathName;
    let siteRoot: string = tsConfig.site_root;
    let index: number = pathname.indexOf(siteRoot, 0);
    let actualPath: string = pathname.substr(index + siteRoot.length);
    let pathnameParts: string[] = actualPath.split('/');
    let pathnameProps: { [key: number]: string } = {};
    pathnameProps[PathnameKey.WorkbookName] = pathnameParts[<number>PathnameKey.WorkbookName];
    pathnameProps[PathnameKey.SheetId] = pathnameParts[<number>PathnameKey.SheetId];
    pathnameProps[PathnameKey.AuthoringSheet] = pathnameParts[<number>PathnameKey.AuthoringSheet];
    return pathnameProps;
  }

  /**
   * Performs a shallow equals of two given objects.
   * @param valueA An object or `null`
   * @param valueB Another object or `null`
   * @returns `true` if all keys are `===` between the two objects.
   */
  public static shallowEquals(valueA: any, valueB: any): boolean {
    if (valueA === valueB) {
      return true;
    }
    if (valueA === null || valueB === null) {
      return false;
    }
    let keysA: string[] = any.keys(valueA);
    let keysB: string[] = any.keys(valueB);
    if (keysA.length !== keysB.length) {
      return false;
    }
    for (let i = 0; i < keysA.length; i++) {
      let key: string = keysA[i];
      if (!valueB.hasOwnProperty(key) || valueA[key] !== valueB[key]) {
        return false;
      }
    }
    return true;
  }

  /**
   * Percent encodes (also called URL encoding) the given string.  First the browser's encodeUriComponent()
   * is called on the string and then any characters not included in `unreservedChars` are
   * then encoded.  If you only want the browser standard behavior then use {@link String.EncodeUriComponent},
   * this method is only useful for encoding extra characters that the browser might not encode.
   * @param valueToEncode The string to be encoded
   * @param unreservedChars A dictionary of unreserved string keys (single characters).  Any character
   * not included as a key will be escaped.
   * @returns The encoded string
   */
  public static percentEncode(valueToEncode: string, unreservedChars: { [key: string]: string }): string {
    valueToEncode = string.encodeURIComponent(valueToEncode);
    if (ss.isNullOrUndefined(unreservedChars)) {
      return valueToEncode;
    }
    let sb: ss.StringBuilder = new ss.StringBuilder();
    let i: number = 0;
    while (i < valueToEncode.length) {
      let s: string = valueToEncode.substr(i, 1);
      if (s === '%') {
        sb.append(valueToEncode.substr(i, 3));
        i += 2;
      } else
        if (!ss.keyExists(unreservedChars, s)) {
          sb.append('%').append((<number>s.charCodeAt(0)).toString(16).toUpperCase());
        } else {
          sb.append(s);
        }
      i++;
    }
    return sb.toString();
  }

  /**
   * Encodes string based on string encoding defined in wgapp\app\helpers\link_help.rb:encode_id(name)
   * First, specific characters are url-encoded, and the resulting string is then completely url-encoded.
   * @param valueToEncode The string to be encoded
   * @returns The encoded string
   */
  public static encodeForWG(valueToEncode: string): string {
    let usernameValidChars: { [key: string]: string } = {};
    let addCodes: (char: number, char: number) => void = (from: number, to: number) => {
      for (let i = from; i <= to; i++) {
        let s: string = string.fromCharCode(i);
        usernameValidChars[s] = s;
      }
    };
    addCodes('a', 'z');
    addCodes('A', 'Z');
    addCodes('0', '9');
    addCodes('_', '_');
    addCodes('-', '-');
    valueToEncode = MiscUtil.percentEncode(valueToEncode, usernameValidChars);
    valueToEncode = MiscUtil.percentEncode(valueToEncode, null);
    return valueToEncode;
  }

  /**
   * Tests whether a single argument or at least one of a list of arguments is null OR
   * undefined.
   * @param args argument to be checked
   * @returns true or false
   */
  public static isNullOrUndefined(args: any[]): boolean {
    if (ss.isNullOrUndefined(args)) {
      return true;
    }
    for (let i = 0; i < args.length; i++) {
      if (ss.isNullOrUndefined(args[i])) {
        return true;
      }
    }
    return false;
  }

  /**
   * Tests whether an array or a string is either null, undefined, or empty
   * (length is 0). You can pass one or more arguments to the function and it will
   * return true if at least one of the arguments is null or empty.
   * @param args argument to be checked
   * @returns true or false
   */
  public static isNullOrEmpty(args: any): boolean {
    if (ss.isNullOrUndefined(args)) {
      return true;
    }
    let dict: { [key: string]: number } = args;
    if (ss.isValue(dict['length']) && dict['length'] === 0) {
      return true;
    }
    return false;
  }

  /**
   * Calling this on a null string would be either an error or vacuously impossible in real C#, but since this
   * gets compiled to a static utility method in Saltarelle, it gives us a one-line way to check this condition
   * rather than three-line 'if (string.isNullOrEmpty()) {}' conditionals everywhere.
   */
  public static isNullOrEmpty$1(s: string): boolean {
    return ss.isNullOrEmptyString(s);
  }

  /**
   * Overload of same IsNullOrEmpty for generic List objects
   * typeparam T The object type contained by the List
   */
  public static isNullOrEmpty$2<T>(list: T[]): boolean {
    return list === null || list.length === 0;
  }

  /**
   * Overload of same IsNullOrEmpty for generic JsArray objects
   * typeparam T The object type contained by the List
   */
  public static isNullOrEmpty$3<T>(array: T[]): boolean {
    return array === null || array.length === 0;
  }

  /**
   * check if the given index is valid in the given array
   * @param index index of the array
   * @param arr the array to check
   * @returns true of false
   */
  public static isValidIndex(index: number, arr: any[]): boolean {
    return index >= 0 && index < arr.length;
  }

  /**
   * Is given string a truthy value as defined by Tableau's query param truthy values?
   * @param value 
   * @param defaultIfMissing 
   * @returns truthiness, or defaultIfMissing if the string is null or empty
   */
  public static toBoolean(value: string, defaultIfMissing: boolean): boolean {
    let positiveRegex: RegExp = new RegExp('^(yes|y|true|t|1)$', 'i');
    if (MiscUtil.isNullOrEmpty$1(value)) {
      return defaultIfMissing;
    }
    let match: string[] = value.match(positiveRegex);
    return !MiscUtil.isNullOrEmpty(match);
  }

  /**
   * Determines whether or not the user has the ability to view the data tab
   * @returns True if the data tab should be enabled, false if not
   */
  public static shouldShowDataTab(): boolean {
    return (tsConfig.allow_add_new_datasource && FeatureFlags.isEnabled(FeatureFlagIds.DataToTheWeb) && (!tsConfig.is_mobile || (tsConfig.is_mobile && FeatureFlags.isEnabled(FeatureFlagIds.DataToTheWebMobile))));
  }

  /**
   * Replaces the query in the given URI with a new set of key/value pairs.
   * @param uri A uri
   * @param parameters The set of query paramters to use
   * @returns A uri with the query replaced(or appended)
   */
  public static replaceUriQueryParameters(uri: Object, parameters: { [key: string]: string[] }): Object {
    if (parameters.count === 0) {
      return uri;
    }
    let newQueryString: ss.StringBuilder = new ss.StringBuilder();
    let first: boolean = true;
    let appendSeparator: () => void = () => {
      newQueryString.append(first ? '?' : '&');
      first = false;
    };
    for (const key of _.keys(parameters)) {
      let vals: string[] = parameters[key];
      let keyEncoded: string = string.encodeURIComponent(key);
      if (ss.isNullOrUndefined(vals) || vals.length === 0) {
        appendSeparator();
        newQueryString.append(keyEncoded);
      } else {
        for (const value of vals) {
          appendSeparator();
          newQueryString.append(keyEncoded).append('=').append(string.encodeURIComponent(value));
        }
      }
    }
    let hash: string = '';
    let baseUri: string = '';
    if (uri.length > 0) {
      let indexOfQuery: number = (<string>uri).indexOf('?');
      let indexOfHash: number = (<string>uri).indexOf('#');
      let indexOfEnd: number = Math.min(indexOfQuery < 0 ? (<string>uri).length : indexOfQuery, indexOfHash < 0 ? (<string>uri).length : indexOfHash);
      baseUri = (<string>uri).substr(0, indexOfEnd);
      hash = indexOfHash < 0 ? '' : (<string>uri).substr(indexOfHash);
    }
    return baseUri + newQueryString + hash;
  }

  /**
   * We can find that some of our bools are actually strings, like "false" or "true". c# won't catch this
   * and makes us write a silly method to convert our bool to another bool.
   * @param value The bool that needs sanatized
   * @returns Null/undefined if the value passed is null. Otherse it will pass back a parsed bool
   */
  public static sanatizeBoolean(value: boolean): boolean {
    if (ss.isNullOrUndefined(value)) {
      return value;
    }
    return value.toString().toLowerCase() === 'true';
  }

  /**
   * Disposes the given object if non-null.
   * typeparam T The type of the class to dispose
   * @returns returns null
   */
  public static dispose<T>(d: T): T {
    if (ss.isValue(d)) {
      d.dispose();
    }
    return null;
  }

  /**
   * Disposes a list of disposables.
   * typeparam T The type of the class to dispose
   * @returns returns null
   */
  public static dispose$1<T>(d: T[]): T[] {
    if (ss.isValue(d)) {
      for (const v of d) {
        if (ss.isValue(v)) {
          v.dispose();
        }
      }
      ss.clear(d);
    }
    return null;
  }

  /**
   * Safely calls Window.ClearTimeout for the provided handle, which was previously created using Window.SetTimeout.
   * Returns null so that you can clear the timeout and assign in one statement, e.g.:
   * myTimeout = MiscUtil.ClearTimeout(myTimeout);
   */
  public static clearTimeout(handle: number | null): number | null {
    if (handle.hasValue) {
      window.clearTimeout(handle.value);
    }
    return null;
  }

  public static clearInterval(handle: number | null): number | null {
    if (handle.hasValue) {
      window.clearInterval(handle.value);
    }
    return null;
  }

  /**
   * Returns a deep clone of the given object
   */
  public static cloneObject(src: any): any {
    let objStr: string = JSON.stringify(src, (k, v) => {
      return v;
    });
    return JSON.parse(objStr);
  }
}
