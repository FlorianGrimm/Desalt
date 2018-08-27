import 'mscorlib';

import { tsConfig } from 'TypeDefs';

import { _ } from 'Underscore';

import { Utility } from '../../Bootstrap/Utility';

import { WindowHelper } from '../../CoreSlim/WindowHelper';

export enum PathnameKey {
  workbookName = 2,
  sheetId = 3,
  authoringSheet = 4,
}

/**
 * Contains utility methods for etc.
 */
export class MiscUtil {
  public static get pathName(): string {
    let window: Object = Utility.locationWindow;
    return WindowHelper.getLocation(window).pathname;
  }

  public static get urlPathnameParts(): Object<PathnameKey, string> {
    let pathname: string = MiscUtil.pathName;
    let siteRoot: string = tsConfig.site_root;
    let index: number = pathname.indexOf(siteRoot, 0);
    let actualPath: string = pathname.substr(index + siteRoot.length);
    let pathnameParts: string[] = actualPath.split('/');
    let pathnameProps: Object<PathnameKey, string> = {};
    pathnameProps[PathnameKey.workbookName] = pathnameParts[<number>PathnameKey.workbookName];
    pathnameProps[PathnameKey.sheetId] = pathnameParts[<number>PathnameKey.sheetId];
    pathnameProps[PathnameKey.authoringSheet] = pathnameParts[<number>PathnameKey.authoringSheet];
    return pathnameProps;
  }

  /**
   * Lazily initializes a static field (field on a type).  This is necessary at times to workaround the
   * Script# initialization process for static blocks as the ordering there is beyond our control.
   * 
   * When using this method for initialization the field should not be declared by the type.
   * @param t The type to contain the field
   * @param fieldName The field name
   * @param initializer A function for providing the default field value
   * @returns The field's value, initialized the first time this method is called
   */
  public static lazyInitStaticField(t: Function, fieldName: string, initializer: () => any): any {
    let value: any = (<any>t)[fieldName];
    if (ss.isNullOrUndefined(value)) {
      value = initializer();
      (<any>t)[fieldName] = value;
    }
    return value;
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
  public static percentEncode(valueToEncode: string, unreservedChars: Object<string, string>): string {
    valueToEncode = string.encodeURIComponent(valueToEncode);
    if (ss.isNullOrUndefined(unreservedChars)) {
      return valueToEncode;
    }
    let sb: StringBuilder = new StringBuilder();
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
    let usernameValidChars: Object<string, string> = {};
    let addCodes: (char: Int32, char: Int32) => void = (from: Int32, to: Int32) => {
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
    let dict: Object<string, number> = JsDictionary<string, number>.getDictionary(args);
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
   * check if the given index is valid in the given array
   * @param index index of the array
   * @param arr the array to check
   * @returns true of false
   */
  public static isValidIndex(index: number, arr: any[]): boolean {
    return index >= 0 && index < arr.length;
  }

  /**
   * Checks if the given value is a non-null object.  Implementation cribbed from Underscore.
   * @param o 
   * @returns True if an object value
   */
  public static isObject(o: any): boolean {
    return false;
  }

  public static hasOwnProperty(owner: any, field: string): boolean {
    return false;
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
   * Parses the URI's query parameters and returns a map containing each key/value pair.  Each
   * query parameter and value is also URI decoded.
   * @param uri The URI to decode, must contain a ? in order to indicate the start of params
   * @returns A set of key value pairs
   */
  public static getUriQueryParameters(uri: Object): Object<string, string[]> {
    let parameters: Object<string, string[]> = {};
    if (ss.isNullOrUndefined(uri)) {
      return parameters;
    }
    let indexOfQuery: number = (<string>uri).indexOf('?');
    if (indexOfQuery < 0) {
      return parameters;
    }
    let query: string = (<string>uri).substr(indexOfQuery + 1);
    let indexOfHash: number = query.indexOf('#');
    if (indexOfHash >= 0) {
      query = query.substr(0, indexOfHash);
    }
    if (ss.isNullOrEmptyString(query)) {
      return parameters;
    }
    let paramPairs: string[] = query.split('&');
    for (const pair of paramPairs) {
      let keyValue: string[] = pair.split('=');
      let key: string = string.decodeURIComponent(keyValue[0]);
      let values: string[];
      if (ss.keyExists(parameters, key)) {
        values = parameters[key];
      } else {
        values = [];
        parameters[key] = values;
      }
      if (keyValue.length > 1) {
        values.push(string.decodeURIComponent(keyValue[1]));
      }
    }
    return parameters;
  }

  /**
   * Replaces the query in the given URI with a new set of key/value pairs.
   * @param uri A uri
   * @param parameters The set of query paramters to use
   * @returns A uri with the query replaced(or appended)
   */
  public static replaceUriQueryParameters(uri: Object, parameters: Object<string, string[]>): Object {
    if (parameters.count === 0) {
      return uri;
    }
    let newQueryString: StringBuilder = new StringBuilder();
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
    if (uri.As().length > 0) {
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
        v.dispose();
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
  public static clearTimeout(handle: Nullable<number>): Nullable<number> {
    if (handle.hasValue) {
      window.clearTimeout(handle.value);
    }
    return null;
  }

  /**
   * Returns a deep clone of the given object
   */
  public static cloneObject(src: any): any {
    let objStr: string = JSON.stringify(src);
    return JSON.parse(objStr);
  }
}
