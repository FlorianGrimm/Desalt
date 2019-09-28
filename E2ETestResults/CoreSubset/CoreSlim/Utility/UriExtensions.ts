import 'mscorlib';

/**
 */
export class UriExtensions {
  /**
   * Parses the URI's query parameters and returns a map containing each key/value pair.  Each
   * query parameter and value is also URI decoded.
   * @param uri The URI to decode, must contain a ? in order to indicate the start of params
   * @returns A set of key value pairs
   */
  public static getUriQueryParameters(uri: Object): { [key: string]: string[] } {
    let parameters: { [key: string]: string[] } = {};
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
}
