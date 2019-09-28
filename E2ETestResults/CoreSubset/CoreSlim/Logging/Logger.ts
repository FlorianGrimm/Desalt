import { ILogAppender } from './ILogAppender';

import 'mscorlib';

import { ScriptEx } from '../Utility/ScriptEx';

import { UriExtensions } from '../Utility/UriExtensions';

/**
 * The various levels of logging priority.
 * {@link Logger.LoggerLevelNames}
 */
export enum LoggerLevel {
  All = 0,
  Debug = 1,
  Info = 2,
  Warn = 3,
  Error = 4,
  Off = 5,
}

/**
 * Supports javascript logging.  See [the wiki]{@link http://mytableau/display/DevServFront/JavaScript+Logging}
 * for details.
 */
export class Logger {
  /**
   * A global logger.  To be used by code that doesn't want to create an local instance of
   * a logger.
   */
  public static readonly global: Logger = Logger.getLoggerWithName('global');

  /**
   * The translation of logging priority into string names, indexed by level value.
   * {@link LoggerLevel}
   */
  public static readonly loggerLevelNames: string[] = [];

  private static readonly logQueryParam: string = ':log';

  private static readonly appenders: ILogAppender[] = [];

  private static readonly filters: Array<(logger: Logger, loggerLevel: LoggerLevel) => boolean> = [];

  private static readonly nullLog: Logger = new Logger('');

  private readonly $name: string;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    Logger.setupUrlFilters();
    Logger.loggerLevelNames[<number>LoggerLevel.All] = 'all';
    Logger.loggerLevelNames[<number>LoggerLevel.Debug] = 'debug';
    Logger.loggerLevelNames[<number>LoggerLevel.Info] = 'info';
    Logger.loggerLevelNames[<number>LoggerLevel.Warn] = 'warn';
    Logger.loggerLevelNames[<number>LoggerLevel.Error] = 'error';
    Logger.loggerLevelNames[<number>LoggerLevel.Off] = 'off';
  }

  private constructor(name: string) {
    this.$name = name;
  }

  public static get globalLog(): Logger {
    return Logger.global;
  }

  /**
   * Gets the name of this log.
   */
  public get name(): string {
    return this.$name;
  }

  /**
   * Removes all existing filters.
   */
  public static clearFilters(): void {
    for (const logAppender of Logger.appenders) {
      logAppender.clearFilters();
    }
    Logger.filters.splice(0, Logger.filters.length);
  }

  /**
   * Adds a filter to allow logging from the given logger at the specified level.
   * @param validLogger The logger to accept
   * @param minLogLevel The minimum level to accept
   */
  public static filterByLogger(validLogger: Logger, minLogLevel: LoggerLevel): void {
    minLogLevel = (minLogLevel) || (LoggerLevel.All);
    Logger.addFilter((l: Logger, ll: LoggerLevel) => l === validLogger && ll >= minLogLevel);
  }

  /**
   * Adds a filter to allow logging from the given type at the specified level.  Assumes
   * that the type contains a static logger generated using {@link Logger.GetLogger}.
   * @param t The type used for creating the logger
   * @param minLogLevel The minimum level to accept
   */
  public static filterByType(t: Function, minLogLevel: LoggerLevel): void {
    minLogLevel = (minLogLevel) || (LoggerLevel.All);
    Logger.addFilter((l: Logger, ll: LoggerLevel) => ll >= minLogLevel && l.name === t.name);
  }

  /**
   * Adds a filter to allow logging from a logger that matches the given pattern at the specified
   * level.
   * @param namePattern A regular expression to match against the logger name
   * @param minLogLevel The minimum level to accept
   */
  public static filterByName(namePattern: string, minLogLevel: LoggerLevel): void {
    minLogLevel = (minLogLevel) || (LoggerLevel.All);
    let regex: RegExp = new RegExp(namePattern, 'i');
    Logger.addFilter((l: Logger, ll: LoggerLevel) => ll >= minLogLevel && ss.isValue(l.name.match(regex)));
  }

  /**
   * Clears all appenders.
   */
  public static clearAppenders(): void {
    Logger.appenders.splice(0, Logger.filters.length);
  }

  /**
   * Is the given appender already added
   * @param appender The appender to check for
   * @returns If the appender has been added
   */
  public static hasAppender(appender: ILogAppender): boolean {
    return Logger.appenders.indexOf(appender) > -1;
  }

  /**
   * Adds a logging appender.
   * @param appender The appender to be added
   */
  public static addAppender(appender: ILogAppender): void {
    for (const filter of Logger.filters) {
      appender.addFilter(filter);
    }
    Logger.appenders.push(appender);
  }

  /**
   * Removes a logging appender.
   * @param appender The appender to be removed
   */
  public static removeAppender(appender: ILogAppender): void {
    let indexOfAppender: number = Logger.appenders.indexOf(appender);
    if (indexOfAppender > -1) {
      Logger.appenders.splice(indexOfAppender, 1);
    }
  }

  /**
   * Convenience method for lazily getting a logger for a class.  A static instance of a logger
   * will be created the first time this method is called.  All subsequent calls will use the
   * existing value.
   * @param t The type to assign the logger to.
   * @returns The type's logger
   */
  public static lazyGetLogger(t: Function): Logger {
    const FieldName: string = '_logger';
    let logger: Logger = Object.getDictionary(t)[FieldName].ReinterpretAs();
    if (ss.isNullOrUndefined(logger)) {
      logger = Logger.getLogger(t);
      Object.getDictionary(t)[FieldName] = logger;
    }
    return logger;
  }

  /**
   * Creates a new instance of a log for the given type and includes a filter for the
   * created logger at the given level.  This method should be used sparingly when
   * debugging.
   * @param t The type to create a log for
   * @param ll The min
   * @returns A new Log instance
   */
  public static getLogger(t: Function, ll: LoggerLevel | null = null): Logger {
    let l: Logger = Logger.getLoggerWithName(t.name);
    if (ll !== null) {
      Logger.filterByLogger(l, ll.value);
    }
    return l;
  }

  /**
   * Creates a new instance of a log with the given name.
   * @param name The log name
   * @returns A new Log instance
   */
  public static getLoggerWithName(name: string): Logger {
    return new Logger(name);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Debug}. By default Debug level output
   * is not shown in the Console. See [the wiki]{@link http://mytableau/display/DevServFront/JavaScript+Logging}
   * for details on how to control the logging output.
   * @param message The message
   * @param args The format arguments.
   */
  public debug(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.Debug, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Info}.
   * @param message The message
   * @param args The format arguments.
   */
  public info(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.Info, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Warn}.
   * @param message The message
   * @param args The format arguments.
   */
  public warn(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.Warn, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Error}.
   * @param message The message
   * @param args The format arguments.
   */
  public error(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.Error, message, args);
  }

  /**
   * Logs the given message.
   */
  public log(level: LoggerLevel, message: string, args: any[]): void {
    this.logInternal(level, message, args);
  }

  private static setupUrlFilters(): void {
    let queryParams: { [key: string]: string[] } = UriExtensions.getUriQueryParameters(window.self.location.search);
    if (!ss.keyExists(queryParams, Logger.logQueryParam)) {
      return;
    }
    Logger.clearFilters();
    let logParams: string[] = queryParams[Logger.logQueryParam];
    if (logParams.length === 0) {
      Logger.filterByName('.*', LoggerLevel.All);
    }
    for (const logParam of logParams) {
      let logVals: string[] = logParam.split(string.fromCharCode(':'));
      let level: LoggerLevel = LoggerLevel.Debug;
      if (logVals.length > 0 && ss.isValue(logVals[1])) {
        let key: string = logVals[1].toLowerCase();
        let index: number = Logger.loggerLevelNames.indexOf(key);
        if (index >= 0) {
          level = <LoggerLevel>index;
        }
      }
      Logger.filterByName(logVals[0], level);
    }
  }

  private static addFilter(filterFunc: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void {
    Logger.filters.push(filterFunc);
    for (const logAppender of Logger.appenders) {
      logAppender.addFilter(filterFunc);
    }
  }

  private logInternal(level: LoggerLevel, message: string, args: any[]): void {
    try {
      for (const logAppender of Logger.appenders) {
        logAppender.log(this, level, message, args);
      }
    } catch { }
  }
}

// Call the static constructor
Logger.__ctor();

/**
 * Utility class to make creating loggers a little less verbose.
 */
export class Log {
  private constructor() { }

  public static get(o: any): Logger {
    return Logger.lazyGetLogger(ss.getInstanceType(o));
  }

  public static get$1(t: Function): Logger {
    return Logger.lazyGetLogger(t);
  }
}
