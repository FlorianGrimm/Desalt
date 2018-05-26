import 'mscorlib';

import { BaseLogAppender } from './BaseLogAppender';

import { BrowserSupport } from '../Utility/BrowserSupport';

import { Logger, LoggerLevel } from './Logger';

import { ScriptEx } from '../../CoreSlim/ScriptEx';

import { tsConfig } from 'TypeDefs';

import { TypeUtil } from 'NativeJsTypeDefs';

/**
 * Method used to collect the stack trace.
 */
export enum StackTraceMode {
  stack,
  stackTrace,
  multiLine,
  callers,
  onError,
  failed,
}

/**
 * Saltarelle adaptation/minimization of TraceKit - Cross browser stack traces - http://github.com/occ/TraceKit
 * We have modified it to not synchronously handle subscriptions from an arbitrary number of clients.
 * We want, instead, to enqueue them for retrieval, so that we can process exceptions very early in
 * initialization, without depending on higher level code.
 * Original license MIT Public License
 */
export class ErrorTrace {
  public static readonly unknownFunctionName: string = '?';

  public static shouldReThrow: boolean = false;

  public static remoteFetching: boolean = true;

  public static collectWindowErrors: boolean = true;

  public static linesOfContext: number = 3;

  public static getStack: boolean = false;

  public static lastExceptionStack: StackTrace;

  public static lastException: Exception;

  private static readonly sourceCache: Object<Object, string[]> = {};

  private static queuedTraces: StackTrace[] = [];

  private static onErrorHandlerInstalled: boolean = false;

  private static oldOnErrorHandler: ErrorEventHandler = null;

  private constructor() { }

  /**
   * Install all handlers for unhandled exceptions.
   */
  public static install(): void {
    let enabled: string = tsConfig.clientErrorReportingLevel;
    if (!ss.isNullOrEmptyString(enabled)) {
      if (enabled === 'debug') {
        ErrorTrace.getStack = true;
      }
    }
    ErrorTrace.extendToAsynchronousCallback('setTimeout');
    ErrorTrace.extendToAsynchronousCallback('setInterval');
    ErrorTrace.installGlobalHandler();
  }

  /**
   * Wrap any function in a reporter.
   * Example: `func = ErrorTrace.Wrap(func);`
   * @param func The function to be wrapped.
   * @returns The wrapped function.
   */
  public static wrap(func: () => void): () => any {
    return () => {
      try {
        return func.As().apply(ss.this, Array.prototype.slice.call(arguments));
      } catch (e) {
        ErrorTrace.report(e, false);
        throw e;
      }
    };
  }

  /**
   * Extends global error handling support for asynchronous browser operations.
   * Derives from Closure's errorhandler.js.
   * Helper function for protecting setTimeout/setInterval.
   * functionName is the name of the function we're protecting. Must be setTimeout or setInterval.
   * @param functionName 
   */
  private static extendToAsynchronousCallback(functionName: string): void {
    let originalFunction: (object: any, object: any) => any = Object.getDictionary(window)[functionName].As();
    let callback: () => any = () => {
      let args: any[] = (<any[]>Array.prototype.slice.call(arguments)).Clone();
      let originalCallback: any = args[0];
      if (ss.getInstanceType(originalCallback) === Function) {
        args[0] = ErrorTrace.wrap(<() => void>originalCallback);
      }
      if ('apply' in originalFunction) {
        return ss.reinterpret(originalFunction).apply(ss.this, args);
      } else {
        return originalFunction(args[0], args[1]);
      }
    };
    Object.getDictionary(window)[functionName] = callback;
  }

  /**
   * Ensures all global unhandled exceptions are recorded.
   * @param message Error message
   * @param url URL of the script that generated the exception
   * @param lineNo The line number where the error occurred
   * @param column The column number where the error occurred
   * @param error Not used currently.
   */
  public static windowOnError(message: Object<Event, string>, url: string, lineNo: number, column: number, error: any): boolean {
    let stack: StackTrace;
    if (ss.isValue(ErrorTrace.lastExceptionStack)) {
      ErrorTrace.augmentStackTraceWithInitialElement(ErrorTrace.lastExceptionStack, url, lineNo, <string>message);
      stack = ErrorTrace.lastExceptionStack;
      ErrorTrace.lastExceptionStack = null;
      ErrorTrace.lastException = null;
    } else {
      let location: StackLocation = new StackLocation(url, lineNo);
      location.functionName = ErrorTrace.guessFunctionName(location);
      location.context = ErrorTrace.gatherContext(location);
      stack = new StackTrace(StackTraceMode.onError, <string>message);
      stack.name = 'window.onError';
      stack.locations = [location];
    }
    ErrorTrace.queuedTraces.push(stack);
    if (ss.isValue(ErrorTrace.oldOnErrorHandler)) {
      ss.reinterpret(ErrorTrace.oldOnErrorHandler).apply(ss.this, <any[]>Array.prototype.slice.call(arguments));
    }
    return false;
  }

  /**
   * Adds information about the first frame to incomplete stack traces.
   * Safari and IE require this to get complete data on the first frame
   * @returns Whether the stack trace was successfully augmented.
   */
  public static augmentStackTraceWithInitialElement(stack: StackTrace, url: Object, lineNo: number, message: string): boolean {
    let initial: StackLocation = new StackLocation(url, lineNo);
    if (ss.isValue(initial.url) && ss.isValue(initial.lineNo)) {
      stack.isIncomplete = false;
      if (ss.isNullOrUndefined(initial.functionName)) {
        initial.functionName = ErrorTrace.guessFunctionName(initial);
      }
      if (ss.isNullOrUndefined(initial.context)) {
        initial.context = ErrorTrace.gatherContext(initial);
      }
      let reference: string[] = message.match(new RegExp(' \'([^\']+)\' '));
      if (ss.isValue(reference) && reference.length > 1) {
        initial.columnNo = ErrorTrace.findSourceInLine(reference[1], initial);
      }
      if (ss.isValue(stack.locations) && stack.locations.length > 0) {
        let top: StackLocation = stack.locations[0];
        if (top.url === initial.url) {
          if (top.lineNo === initial.lineNo) {
            return false;
          } else
            if (ss.isNullOrUndefined(top.lineNo) && top.functionName === initial.functionName) {
              top.lineNo = initial.lineNo;
              top.context = initial.context;
              return false;
            }
        }
      }
      stack.locations.unshift(initial);
      stack.isPartial = true;
      return true;
    } else {
      stack.isIncomplete = true;
    }
    return false;
  }

  /**
   * Attempts to retrieve source code via XmlHttpRequest, which is used to look up anonymous function names.
   * @param url URL of source code.
   * @returns Source contents.
   */
  private static loadSource(url: Object): string {
    if (!ErrorTrace.remoteFetching) {
      return '';
    }
    try {
      let srcRequest: XMLHttpRequest = new XMLHttpRequest();
      srcRequest.open('GET', url, false);
      srcRequest.send('');
      return srcRequest.responseText;
    } catch {
      return '';
    }
  }

  /**
   * Retrieves source code from the source code cache.
   * @param url URL of source code.
   * @returns Source contents.
   */
  private static getSource(url: Object): string[] {
    if (ss.isNullOrUndefined(url)) {
      return [];
    }
    if (!ss.keyExists(ErrorTrace.sourceCache, url)) {
      let source: string = '';
      if ((<string>url).indexOf(document.domain) > -1) {
        source = ErrorTrace.loadSource(url);
      }
      ErrorTrace.sourceCache[url] = ss.isNullOrEmptyString(source) ? [] : <string[]>source.split('\n');
    }
    return ErrorTrace.sourceCache[url];
  }

  /**
   * Determines at which column a code fragment occurs in a line of source code.
   */
  private static findSourceInLine(fragment: string, location: StackLocation): number {
    let source: string[] = ErrorTrace.getSource(location.url);
    let re: RegExp = new RegExp('\\b' + ErrorTrace.escapeRegexp(fragment) + '\\b');
    if (ss.isValue(source) && source.length > location.lineNo) {
      let matches: RegexMatch = re.exec(source[location.lineNo]);
      if (ss.isValue(matches)) {
        return matches.index;
      }
    }
    return -1;
  }

  public static guessFunctionName(location: StackLocation): string {
    let functionArgNames: RegExp = new RegExp('function ([^(]*)\\(([^)]*)\\)');
    let guessFunction: RegExp = new RegExp('[\'\"]?([0-9A-Za-z$_]+)[\'\"]?\\s*[:=]\\s*(function|eval|new Function)');
    let line: string = '';
    let maxLines: number = 10;
    let source: string[] = ErrorTrace.getSource(location.url);
    if (source.length === 0) {
      return ErrorTrace.unknownFunctionName;
    }
    for (let i = 0; i < maxLines; i++) {
      line = source[location.lineNo - 1] + line;
      if (!ss.isNullOrEmptyString(line)) {
        let matches: string[] = guessFunction.exec(line);
        if (ss.isValue(matches) && matches.length > 0) {
          return matches[1];
        }
        matches = functionArgNames.exec(line);
        if (ss.isValue(matches) && matches.length > 0) {
          return matches[1];
        }
      }
    }
    return ErrorTrace.unknownFunctionName;
  }

  public static gatherContext(location: StackLocation): string[] {
    let source: string[] = ErrorTrace.getSource(location.url);
    if (ss.isNullOrUndefined(source) || source.length === 0) {
      return null;
    }
    let context: string[] = [];
    let linesBefore: number = Math.floor(ErrorTrace.linesOfContext / 2);
    let linesAfter: number = linesBefore + (ErrorTrace.linesOfContext % 2);
    let start: number = Math.max(0, location.lineNo - linesBefore - 1);
    let end: number = <number>Math.min(source.length, location.lineNo + linesAfter - 1);
    location.lineNo -= 1;
    for (let i = start; i < end; i++) {
      if (!ss.isNullOrEmptyString(source[i])) {
        context.push(source[i]);
      }
    }
    return context;
  }

  public static escapeRegexp(input: string): string {
    return input.replace(new RegExp('[\\-\\[\\]{}()*+?.,\\\\\\^$|#]', 'g'), '\\\\$&');
  }

  public static escapeCodeAsRegexpForMatchingInsideHTML(body: string): string {
    return ss.replaceAllString(ss.replaceAllString(ss.replaceAllString(ss.replaceAllString(ErrorTrace.escapeRegexp(body), '<', '(?:<|&lt;)'), '>', '(?:>|&gt;)'), '&', '(?:&|&amp;)'), '"', '(?:"|&quot;)').replace(new RegExp('\\\\s+', 'g'), '\\\\s+');
  }

  public static findSourceInUrls(re: RegExp, urls: Object[]): StackLocation {
    for (const url of urls) {
      let source: string[] = ErrorTrace.getSource(url);
      if (ss.isValue(source) && source.length > 0) {
        for (let lineNo = 0; lineNo < source.length; lineNo++) {
          let matches: RegexMatch = re.exec(source[lineNo]);
          if (ss.isValue(matches) && matches.length > 0) {
            let location: StackLocation = new StackLocation(url, lineNo);
            location.columnNo = matches.index;
            return location;
          }
        }
      }
    }
    return null;
  }

  public static getStackTraceFor(e: Exception): StackTrace {
    let defaultTrace: StackTrace = new StackTrace(StackTraceMode.stack, e.message);
    defaultTrace.name = <string>(<any>e).name;
    if (ErrorTrace.getStack) {
      let stackTraceComputers: Array<(exception: Exception) => StackTrace> = [];
      stackTraceComputers.push(ErrorTrace.computeStackTraceFromStackTraceProp);
      stackTraceComputers.push(ErrorTrace.computeStackTraceByWalkingCallerChain);
      for (const stackTraceComputer of stackTraceComputers) {
        let stack: StackTrace = null;
        try {
          stack = stackTraceComputer(e);
        } catch (inner) {
          if (ErrorTrace.shouldReThrow) {
            throw inner;
          }
        }
        if (ss.isValue(stack)) {
          return stack;
        }
      }
    } else {
      return defaultTrace;
    }
    defaultTrace.traceMode = StackTraceMode.failed;
    return defaultTrace;
  }

  public static computeStackTraceByWalkingCallerChain(e: Exception): StackTrace {
    let err: Error = (<any>e)._error;
    let functionName: RegExp = new RegExp('function\\s+([_$a-zA-Z\x00a0-\xFFFF][_$a-zA-Z0-9\x00a0-\xFFFF]*)?\\s*\\(', 'i');
    let locations: StackLocation[] = [];
    let funcs: Object<string, boolean> = {};
    let recursion: boolean = false;
    let curr: any = null;
    for (curr = (<any>ErrorTrace).computeStackTraceByWalkingCallerChain.caller; ss.isValue(curr) && !recursion; curr = curr.caller) {
      if (curr === ErrorTrace) {
        continue;
      }
      let functionText: string = curr.toString();
      let item: StackLocation = new StackLocation(null, 0);
      if (ss.isValue(curr.name)) {
        item.functionName = <string>curr.name;
      } else {
        let parts: string[] = functionName.exec(functionText);
        if (ss.isValue(parts) && parts.length > 1) {
          item.functionName = parts[1];
        }
      }
      let source: StackLocation = ErrorTrace.findSourceByFunctionBody(curr);
      if (ss.isValue(source)) {
        item.url = source.url;
        item.lineNo = source.lineNo;
        if (item.functionName === ErrorTrace.unknownFunctionName) {
          item.functionName = ErrorTrace.guessFunctionName(item);
        }
        let reference: RegexMatch = new RegExp(' \'([^\']+)\' ').exec((e.message) || (e['description']));
        if (ss.isValue(reference) && reference.length > 1) {
          item.columnNo = ErrorTrace.findSourceInLine(reference[1], source);
        }
      }
      if (ss.keyExists(funcs, functionText)) {
        recursion = true;
      } else {
        funcs[functionText] = true;
      }
      locations.push(item);
    }
    let result: StackTrace = new StackTrace(StackTraceMode.callers, e.message);
    result.name = <string>err['name'];
    result.locations = locations;
    ErrorTrace.augmentStackTraceWithInitialElement(result, <string>(err['sourceURL']) || (err['fileName']), <number>(err['line']) || (err['lineNumber']), <string>(e.message) || (err['description']));
    return result;
  }

  private static findSourceByFunctionBody(func: any): StackLocation {
    let urls: Object[] = [window.location.href];
    let scripts: HTMLCollection = document.getElementsByTagName('script');
    let code: string = func.toString();
    let codeMatcher: RegExp = new RegExp('');
    let matcher: RegExp;
    for (let i = 0; i < scripts.length; i++) {
      let script: HTMLElement = <HTMLElement>scripts[i];
      if (script.hasAttribute('src') && ss.isValue(script.getAttribute('src'))) {
        urls.push(<string>script.getAttribute('src'));
      }
    }
    let parts: RegexMatch = codeMatcher.exec(code);
    if (ss.isValue(parts) && parts.length > 0) {
      matcher = new RegExp(ErrorTrace.escapeRegexp(code).replace(new RegExp('\\s+', 'g'), '\\\\s+'));
    } else {
      let name: string = parts.length > 1 ? '\\\\s+' + parts[1] : '';
      let args: string = parts[2].split(',').join('\\\\s*,\\\\s*');
      let body: string = ErrorTrace.escapeRegexp(parts[3]).replace(new RegExp(';$'), ';?');
      matcher = new RegExp('function' + name + '\\\\s*\\\\(\\\\s*' + args + '\\\\s*\\\\)\\\\s*{\\\\s*' + body + '\\\\s*}');
    }
    let result: StackLocation = ErrorTrace.findSourceInUrls(matcher, urls);
    if (ss.isValue(result)) {
      return result;
    }
    return null;
  }

  private static computeStackTraceFromStackTraceProp(e: Exception): StackTrace {
    let err: Error = (<any>e)._error;
    if (ss.isNullOrUndefined(err) || ss.isNullOrUndefined(err.stack)) {
      return null;
    }
    let chromeMatcher: RegExp = new RegExp('^\\s*at (?:((?:\\[object object\\])?\\S+(?: \\[as \\S+\\])?) )?\\(?((?:file|http|https):.*?):(\\d+)(?::(\\d+))?\\)?\\s*$', 'i');
    let geckoMatcher: RegExp = new RegExp('^\\s*(\\S*)(?:\\((.*?)\\))?@((?:file|http|https).*?):(\\d+)(?::(\\d+))?\\s*$', 'i');
    let matcher: RegExp = BrowserSupport.isFF ? geckoMatcher : chromeMatcher;
    let lines: string[] = (<string>err['stack']).split('\n');
    let locations: StackLocation[] = [];
    let reference: RegexMatch = new RegExp('^(.*) is undefined').exec(e.message);
    for (const line of lines) {
      let parts: RegexMatch = matcher.exec(line);
      if (ss.isValue(parts) && parts.length >= 5) {
        let functionName: string = parts[1];
        let url: string = parts[2];
        let lineNumStr: string = parts[3];
        let colNumStr: string = parts[4];
        let element: StackLocation = new StackLocation(url, number.parseInt(lineNumStr));
        if (ss.isValue(functionName)) {
          element.functionName = functionName;
        }
        if (ss.isValue(colNumStr)) {
          element.columnNo = number.parseInt(colNumStr);
        }
        if (ss.isValue(element.lineNo)) {
          if (ss.isNullOrUndefined(element.functionName)) {
            element.functionName = ErrorTrace.guessFunctionName(element);
          }
          element.context = ErrorTrace.gatherContext(element);
        }
        locations.push(element);
      }
    }
    if (locations.length > 0 && ss.isValue(locations[0].lineNo) && ss.isNullOrUndefined(locations[0].columnNo) && ss.isValue(reference) && reference.length > 1) {
      locations[0].columnNo = ErrorTrace.findSourceInLine(reference[1], locations[0]);
    }
    if (locations.length === 0) {
      return null;
    }
    let stack: StackTrace = new StackTrace(StackTraceMode.stack, e.message);
    stack.name = <string>(<any>e).name;
    stack.locations = locations;
    return stack;
  }

  /**
   * Returns whether any stack traces have been reported and enqueued.
   */
  public static hasTraces(): boolean {
    return ErrorTrace.queuedTraces.length > 0;
  }

  /**
   * Retrieves and clears all enqueued stack trace objects in one step.
   * This assumes that exactly one listener is handling the policy for these unhandled exceptions.
   * @returns All pending stack trace objects.
   */
  public static dequeueTraces(): StackTrace[] {
    let traces: StackTrace[] = ErrorTrace.queuedTraces;
    ErrorTrace.queuedTraces = [];
    return traces;
  }

  /**
   * Replaces and wraps the existing `window.onerror` handler.
   * This will avoid wrapping itself, so it can be called multiple times.
   */
  public static installGlobalHandler(): void {
    if (ErrorTrace.onErrorHandlerInstalled || !ErrorTrace.collectWindowErrors) {
      return;
    }
    ErrorTrace.oldOnErrorHandler = window.onerror;
    window.onerror = ErrorTrace.windowOnError;
    ErrorTrace.onErrorHandlerInstalled = true;
  }

  /**
   * Cross-browser processing of unhandled exceptions.
   * Enqueues them for later retrieval by a higher-level processor (like FailureHandler).
   * Example: `try { } catch (Exception e) { ErrorTrace.Report(e); }`
   * Retrieval: `JsArray&gt;StackTrace&lt; traces = ErrorTrace.DequeueTraces();`
   * @param e The exception to report.
   * @param rethrow Whether to throw the exception again when done processing it (originally a default of true).
   */
  public static report(e: Exception, rethrow: boolean): void {
    if (ss.isNullOrUndefined(rethrow)) {
      rethrow = true;
    }
    if (ss.isValue(ErrorTrace.lastExceptionStack)) {
      if (ErrorTrace.lastException === e) {
        return;
      } else {
        let s: StackTrace = ErrorTrace.lastExceptionStack;
        ErrorTrace.lastExceptionStack = null;
        ErrorTrace.lastException = null;
        ErrorTrace.queuedTraces.push(s);
      }
    }
    let stack: StackTrace = ErrorTrace.getStackTraceFor(e);
    ErrorTrace.lastExceptionStack = stack;
    ErrorTrace.lastException = e;
    window.setTimeout(() => {
      if (ErrorTrace.lastException === e) {
        ErrorTrace.lastExceptionStack = null;
        ErrorTrace.lastException = null;
        ErrorTrace.queuedTraces.push(stack);
      }
    }, (stack.isIncomplete ? 2000 : 0));
    if (rethrow) {
      throw e;
    }
  }
}

export class StackLocation {
  public url: Object;

  public lineNo: number = 0;

  public columnNo: number = 0;

  public functionName: string = ErrorTrace.unknownFunctionName;

  public context: string[];

  public constructor(url: string, lineNo: number) {
    this.url = url;
    this.lineNo = lineNo;
  }
}

export class StackTrace {
  public readonly userAgent: string = window.navigator.userAgent;

  public traceMode: StackTraceMode = StackTraceMode.onError;

  public message: Object;

  public url: Object;

  public locations: StackLocation[];

  public isIncomplete: boolean = false;

  public isPartial: boolean = false;

  public name: string;

  public constructor(traceMode: StackTraceMode, message: Object) {
    this.traceMode = traceMode;
    this.message = message;
    this.url = document.uRL;
    this.locations = [];
  }
}

/**
 * A logger that creates a stack trace to aggregate and send to the server.
 */
export class StackTraceAppender extends BaseLogAppender {
  private static globalAppender: StackTraceAppender;

  /**
   */
  public static __ctor() {
    StackTraceAppender.enableLogging((l: Logger, ll: LoggerLevel) => {
      return ll > LoggerLevel.info;
    });
  }

  /**
   */
  private constructor() { }

  /**
   * Enables logging using this appender type.
   * @param filter The filter to apply to this appender or `null` to enable for all loggers
   */
  public static enableLogging(filter: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void {
    if (ss.isNullOrUndefined(StackTraceAppender.globalAppender)) {
      StackTraceAppender.globalAppender = new StackTraceAppender();
      Logger.addAppender(StackTraceAppender.globalAppender);
    }
    StackTraceAppender.globalAppender.addFilter((filter) || (() => {
      return true;
    }));
  }

  /**
   * Disables logging using this appender type.
   */
  public static disableLogging(): void {
    if (ss.isValue(StackTraceAppender.globalAppender)) {
      Logger.removeAppender(StackTraceAppender.globalAppender);
      StackTraceAppender.globalAppender = null;
    }
  }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    message = this.formatMessage(ss.replaceAllString(message, '\n', '<br />'), args);
    if (level > LoggerLevel.info) {
      try {
        throw new Exception('Logged(' + Logger.loggerLevelNames[<number>level] + ', from ' + source.name + '): ' + message);
      } catch (e) {
        ErrorTrace.report(e, false);
      }
    }
  }
}

// Call the static constructor
StackTraceAppender.__ctor();
