import 'mscorlib';

import { BaseLogAppender } from '../../CoreSlim/Logging/BaseLogAppender';

import { JsNativeExtensionMethods } from 'NativeJsTypeDefs';

import { LogAppenderInstance } from '../../CoreSlim/Logging/LogAppenderInstance';

import { Logger, LoggerLevel } from '../../CoreSlim/Logging/Logger';

import { tsConfig } from 'TypeDefs';

/**
 * Method used to collect the stack trace.
 */
export const enum StackTraceMode {
  Stack = 'stack',
  OnError = 'onError',
  Failed = 'failed',
}

/**
 * Saltarelle adaptation/minimization of TraceKit - Cross browser stack traces - http://github.com/occ/TraceKit
 * We have modified it to not synchronously handle subscriptions from an arbitrary number of clients.
 * We want, instead, to enqueue them for retrieval, so that we can process exceptions very early in
 * initialization, without depending on higher level code.
 * Original license MIT Public License
 * 
 * Additional parts are ports from https://github.com/stacktracejs/error-stack-parser
 * Original license "The Unlicense" http://unlicense.org/
 */
export class ErrorTrace {
  private static readonly shouldReThrow: boolean = false;

  private static readonly collectWindowErrors: boolean = true;

  private static getStack: boolean;

  private static queuedTraces: StackTrace[] = [];

  private static onErrorHandlerInstalled: boolean;

  private static oldOnErrorHandler: ErrorEventHandler;

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
        return func.apply(ss.this, [<any[]>Array.prototype.slice.call(arguments)]);
      } catch (e) {
        ErrorTrace.report(e);
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
    let originalFunction: (object: any, object: any) => any = window[functionName];
    let callback: () => any = () => {
      let args: any[] = ss.arrayClone((<any[]>Array.prototype.slice.call(arguments)));
      let originalCallback: any = args[0];
      return originalFunction.apply(ss.this, [args]);
    };
    window[functionName] = callback;
  }

  /**
   * Ensures all global unhandled exceptions are recorded.
   * @param message Error message
   * @param url URL of the script that generated the exception
   * @param lineNo The line number where the error occurred
   * @param column The column number where the error occurred
   * @param error Error from the javascript engine
   */
  public static windowOnError(message: Object<Event, string>, url: string, lineNo: number, column: number, error: any): boolean {
    return ErrorTrace.windowOnError$1(message, url, lineNo, column, error, false);
  }

  /**
   * Overload: Ensures all global unhandled exceptions are recorded.
   * @param message Error message
   * @param url URL of the script that generated the exception
   * @param lineNo The line number where the error occurred
   * @param column The column number where the error occurred
   * @param error Error from the javascript engine
   * @param errorDialogShown Flag indicating if the error resulted in an error dialog being shown
   */
  public static windowOnError$1(message: Object<Event, string>, url: string, lineNo: number, column: number, error: any, errorDialogShown: boolean): boolean {
    let locations: StackLocation[] = StackTraceParser.parseJsErrorForStackLines(<Error>error);
    let stack: StackTrace = new StackTrace(StackTraceMode.OnError, <string>message, 'window.onError', locations, errorDialogShown);
    ErrorTrace.queuedTraces.push(stack);
    if (ss.isValue(ErrorTrace.oldOnErrorHandler)) {
      ErrorTrace.oldOnErrorHandler(message, url, lineNo, column, error);
    }
    return false;
  }

  /**
   * Returns a StackTrace instance created from an exception.
   * @param e The exception to use for creating the StackTrace.
   * @param errorDialogShown Flag indicating if the exception resulted in an error dialog being shown.
   */
  public static getStackTraceFor(e: ss.Exception, errorDialogShown: boolean = false): StackTrace {
    let defaultTrace: StackTrace = new StackTrace(StackTraceMode.Stack, StackTraceParser.getExceptionMessage(e), <string>(<any>e).name, null, errorDialogShown);
    if (ErrorTrace.getStack) {
      let stack: StackTrace = null;
      try {
        stack = StackTraceParser.computeStackTraceFromStackTraceProp(e, errorDialogShown);
      } catch (e) {
        if (ErrorTrace.shouldReThrow) {
          throw e;
        }
      }
      if (ss.isValue(stack)) {
        return stack;
      }
    } else {
      return defaultTrace;
    }
    defaultTrace.traceMode = StackTraceMode.Failed;
    return defaultTrace;
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
   * @param errorDialogShown Flag indicating if the exception resulted in an error dialog being shown.
   */
  public static report(e: ss.Exception, errorDialogShown: boolean = false): void {
    let stack: StackTrace = ErrorTrace.getStackTraceFor(e, errorDialogShown);
    ErrorTrace.queuedTraces.push(stack);
  }
}

export class StackLocation {
  public readonly url: Object;

  /**
   * 1-indexed line number. This is what humans think of as line numbers
   */
  public readonly lineNo: number;

  public readonly columnNo: number;

  public readonly functionName: string;

  public readonly context: string[];

  public constructor(url: string, lineNo: number, colNo: number, defaultContextLine: string = '', functionName: string = null) {
    this.url = url;
    this.lineNo = lineNo;
    this.columnNo = colNo;
    let zeroIndexedLineNumber: number = lineNo - 1;
    this.context = SourceCacheForErrorStacks.gatherContext(this.url, zeroIndexedLineNumber, colNo) || [defaultContextLine];
    this.functionName = functionName || SourceCacheForErrorStacks.guessFunctionName(this.url, zeroIndexedLineNumber);
  }
}

export class StackTrace {
  public readonly userAgent: string = window.navigator.userAgent;

  public readonly message: Object;

  public readonly url: Object;

  public traceMode: StackTraceMode;

  public readonly locations: StackLocation[];

  public isIncomplete: boolean = false;

  public readonly name: string;

  public readonly errorDialogShown: boolean;

  public constructor(traceMode: StackTraceMode, message: Object, name: string, locations: StackLocation[] = null, errorDialogShown: boolean = false) {
    this.traceMode = traceMode;
    this.message = message;
    this.url = document.uRL;
    this.name = name;
    this.locations = locations || [];
    this.errorDialogShown = errorDialogShown;
  }
}

/**
 * A logger that creates a stack trace to aggregate and send to the server.
 */
export class StackTraceAppender extends BaseLogAppender {
  public static readonly globalAppender: LogAppenderInstance<StackTraceAppender> = new LogAppenderInstance<StackTraceAppender>(() => new StackTraceAppender());

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    StackTraceAppender.globalAppender.enableLogging((_, loggerLevel) => loggerLevel > LoggerLevel.Info);
  }

  private constructor() { }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    message = this.formatMessage(ss.replaceAllString(message, '\n', '<br />'), args);
    if (level > LoggerLevel.Info) {
      try {
        throw new ss.Exception('Logged(' + Logger.loggerLevelNames[<number>level] + ', from ' + source.name + '): ' + message);
      } catch (e) {
        ErrorTrace.report(e);
      }
    }
  }
}

// Call the static constructor
StackTraceAppender.__ctor();

export class SourceCacheForErrorStacks {
  private static readonly unknownFunctionName: string = '?';

  private static readonly sourceCache: { [key: string]: string[] } = {};

  private static readonly remoteFetching: boolean = true;

  /**
   * Retrieves source code from the source code cache.
   * @param url URL of source code.
   * @returns Source contents.
   */
  public static getSource(url: Object): string[] {
    if (ss.isNullOrUndefined(url)) {
      return [];
    }
    if (!ss.keyExists(SourceCacheForErrorStacks.sourceCache, url)) {
      let source: string = '';
      if ((<string>url).indexOf(document.domain) > -1) {
        source = SourceCacheForErrorStacks.loadSource(url);
      }
      SourceCacheForErrorStacks.sourceCache[url] = ss.isNullOrEmptyString(source) ? [] : <string[]>source.split('\n');
    }
    return SourceCacheForErrorStacks.sourceCache[url];
  }

  public static guessFunctionName(url: Object, lineNo: number): string {
    let functionArgNames: RegExp = new RegExp('function ([^(]*)\\(([^)]*)\\)');
    let guessFunction: RegExp = new RegExp('[\'\"]?([0-9A-Za-z$_]+)[\'\"]?\\s*[:=]\\s*(function|eval|new Function)');
    let line: string = '';
    const MaxLines: number = 10;
    let source: string[] = SourceCacheForErrorStacks.getSource(url);
    if (source.length === 0) {
      return SourceCacheForErrorStacks.unknownFunctionName;
    }
    for (let i = 0; i < MaxLines; i++) {
      line = source[lineNo] + line;
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
    return SourceCacheForErrorStacks.unknownFunctionName;
  }

  public static gatherContext(url: Object, lineNo: number, colPos: number): string[] {
    let source: string[] = SourceCacheForErrorStacks.getSource(url);
    if (ss.isNullOrUndefined(source) || source.length === 0) {
      return null;
    }
    let context: string[] = [];
    const LinesOfContext: number = 3;
    let linesBefore: number = Math.floor(LinesOfContext / 2);
    let linesAfter: number = linesBefore + (LinesOfContext % 2);
    let start: number = Math.max(0, lineNo - linesBefore);
    let end: number = <number>Math.min(source.length, lineNo + linesAfter);
    for (let i = start; i < end; i++) {
      if (!ss.isNullOrEmptyString(source[i])) {
        context.push(source[i]);
      }
    }
    return context;
  }

  /**
   * Attempts to retrieve source code via XmlHttpRequest, which is used to look up anonymous function names.
   * @param url URL of source code.
   * @returns Source contents.
   */
  private static loadSource(url: Object): string {
    if (!SourceCacheForErrorStacks.remoteFetching) {
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
}

class StackTraceParser {
  private static readonly safariNativeCodeRegexp: RegExp = new RegExp('^(eval@)?(\\[native code\\])?$');

  private static readonly chromeIEStackRegexp: RegExp = new RegExp('^\\s*at .*(\\S+\\:\\d+|\\(native\\))', 'm');

  private static readonly throwAwayEvalRegexp: RegExp = new RegExp('(\\(eval at [^\\()]*)|(\\)\\,.*$)');

  private static readonly extractLocationRegexp: RegExp = new RegExp('(.+?)(?:\\:(\\d+))?(?:\\:(\\d+))?$');

  private static readonly extractLocationUrlLikeRegexp: RegExp = new RegExp('[\\(\\)]', 'g');

  public static computeStackTraceFromStackTraceProp(e: ss.Exception, errorDialogShown: boolean = false): StackTrace {
    let locations: StackLocation[] = StackTraceParser.parseException(e);
    let stack: StackTrace = new StackTrace(StackTraceMode.Stack, StackTraceParser.getExceptionMessage(e), <string>(<any>e).name, locations, errorDialogShown);
    return stack;
  }

  public static getExceptionMessage(e: ss.Exception): string {
    let errorMessage: string = e.message;
    if (e.innerException !== null) {
      errorMessage += ' inner: ' + e.innerException.message;
    }
    return errorMessage;
  }

  /**
   * Get the js error from the exception, delegate it to a browser dependent helper, and
   * parse the error into our stack location data structure.
   * Most of this comes as a simplified port from https://github.com/stacktracejs/error-stack-parser/blob/master/dist/error-stack-parser.js
   */
  private static parseException(e: ss.Exception): StackLocation[] {
    let err: Error;
    if (e.innerException !== null) {
      err = (<any>e.innerException).error;
    } else {
      err = (<any>e)._error;
    }
    return StackTraceParser.parseJsErrorForStackLines(err);
  }

  public static parseJsErrorForStackLines(err: Error): StackLocation[] {
    if (err === null || err.stack === null) {
      return null;
    }
    if (err.stack.match(StackTraceParser.chromeIEStackRegexp) !== null) {
      return StackTraceParser.parseChromeOrIE(err);
    }
    return StackTraceParser.parseFirefoxOrSafari(err);
  }

  private static parseChromeOrIE(error: Error): StackLocation[] {
    let filtered: string[] = ArrayExtensions.filter$1(error.stack.split(string.fromCharCode('
')), (line: string) => line.match(StackTraceParser.chromeIEStackRegexp) !== null);
    return ArrayExtensions.map$1(filtered, (line: string) => {
      if (line.indexOf('(eval ') > -1) {
        line = ss.replaceAllString(line, 'eval code', 'eval').replace(StackTraceParser.throwAwayEvalRegexp, '');
      }
      let tokens: string[] = ArrayExtensions.slice(line.replace(new RegExp('^\\s+'), '').replace(new RegExp('\\(eval code'), '(').split(new RegExp('\\s+')), 1);
      let locationParts: string[] = StackTraceParser.extractLocation(tokens.pop());
      let functionName: string = tokens.join(' ') || 'undefined';
      let fileName: string = ss.indexOf(['eval', '<anonymous>'], locationParts[0]) > -1 ? 'undefined' : locationParts[0];
      let lineNum: number = ss.parseInt(locationParts[1]);
      let colNum: number = ss.parseInt(locationParts[2]);
      return new StackLocation(fileName, lineNum, colNum, line, functionName);
    });
  }

  private static parseFirefoxOrSafari(error: Error): StackLocation[] {
    let filtered: string[] = ArrayExtensions.filter$1(error.stack.split(string.fromCharCode('
')), (line: string) => line.match(StackTraceParser.safariNativeCodeRegexp) === null);
    return ArrayExtensions.map$1(filtered, (line: string) => {
      if (line.indexOf(' > eval') > -1) {
        line = line.replace(new RegExp(' line (\\d+)(?: > eval line \\d+)* > eval\\:\\d+\\:\\d+', 'g'), ':$1');
      }
      if (line.indexOf('@') === -1 && line.indexOf(':') === -1) {
        return new StackLocation(line, 0, 0);
      } else {
        let tokens: string[] = line.split('@');
        let locationParts: string[] = StackTraceParser.extractLocation(tokens.pop());
        let functionName: string = tokens.join('@') || 'undefined';
        let fileName: string = locationParts[0];
        let lineNum: number = ss.parseInt(locationParts[1]);
        let colNum: number = ss.parseInt(locationParts[2]);
        let stackFrame: StackLocation = new StackLocation(fileName, lineNum, colNum, line, functionName);
        return stackFrame;
      }
    });
  }

  private static extractLocation(urlLike: string): string[] {
    if (urlLike.indexOf(string.fromCharCode(':')) === -1) {
      return [urlLike];
    }
    let parts: RegexMatch = StackTraceParser.extractLocationRegexp.exec(urlLike.replace(StackTraceParser.extractLocationUrlLikeRegexp, ''));
    return [parts[1], parts[2] || 'undefined', parts[3] || 'undefined'];
  }
}
