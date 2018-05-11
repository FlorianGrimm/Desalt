import { ILogAppender } from './ILogAppender';

import { MiscUtil } from '../Utility/MiscUtil';

import 'mscorlib';

import { ScriptEx } from '../../CoreSlim/ScriptEx';

/**
 * The various levels of logging priority.
 * {@link Logger.LoggerLevelNames}
 */
export enum LoggerLevel {
  all = 0,
  debug = 1,
  info = 2,
  warn = 3,
  error = 4,
  off = 5,
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

  private readonly $name: string;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    Logger.setupUrlFilters();
    Logger.loggerLevelNames[<number>LoggerLevel.all] = 'all';
    Logger.loggerLevelNames[<number>LoggerLevel.debug] = 'debug';
    Logger.loggerLevelNames[<number>LoggerLevel.info] = 'info';
    Logger.loggerLevelNames[<number>LoggerLevel.warn] = 'warn';
    Logger.loggerLevelNames[<number>LoggerLevel.error] = 'error';
    Logger.loggerLevelNames[<number>LoggerLevel.off] = 'off';
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
   * Gets the list of static appenders.  You might ask yourself, why do I need to do some crazy initialization like this,
   * can't I just use a static field? Sadly the answer is no.  This is because of ordering dependencies in the way
   * Script# initializes static variables.  Because we want to be able to register appenders inside of other static
   * initializers we need to make sure that Appenders don't depend on the order of static init.
   */
  private static get appenders(): ILogAppender[] {
    return <ILogAppender[]>MiscUtil.lazyInitStaticField(Logger, 'appenders', () => {
      return [];
    });
  }

  /**
   * Gets the list of static filters.  You might ask yourself, why do I need to do some crazy initialization like this,
   * can't I just use a static field? Sadly the answer is no.  This is because of ordering dependencies in the way
   * Script# initializes static variables.
   */
  private static get filters(): Array<(logger: Logger, loggerLevel: LoggerLevel) => boolean> {
    return <Array<(logger: Logger, loggerLevel: LoggerLevel) => boolean>>MiscUtil.lazyInitStaticField(Logger, 'filters', () => {
      return [];
    });
  }

  /**
   * Gets the Null logger.  Again, have to do crazy static lazy init here to avoid issues with Script#
   * compilation/static init.
   */
  private static get nullLog(): Logger {
    return <Logger>MiscUtil.lazyInitStaticField(Logger, 'nullLog', () => {
      return new Logger('');
    });
  }

  /**
   * Removes all existing filters.
   */
  public static clearFilters(): void {
    for (const logAppender of Logger.appenders) {
      logAppender.clearFilters();
    }
    ss.clear(Logger.filters);
  }

  /**
   * Adds a filter to allow logging from the given logger at any level.
   * @param l The logger to accept
   */
  public static filterByLogger(l: Logger): void;

  /**
   * Adds a filter to allow logging from the given logger at the specified level.
   * @param validLogger The logger to accept
   * @param minLogLevel The minimum level to accept
   */
  public static filterByLogger(validLogger: Logger, minLogLevel?: LoggerLevel): void {
    minLogLevel = minLogLevel || LoggerLevel.all;
    Logger.addFilter((l: Logger, ll: LoggerLevel) => {
      return l === validLogger && ll >= minLogLevel;
    });
  }

  /**
   * Adds a filter to allow logging from the given type at any level.  Assumes
   * that the type contains a static logger generated using {@link Logger.GetLogger}.
   * @param t The type used for creating the logger
   */
  public static filterByType(t: Function): void;

  /**
   * Adds a filter to allow logging from the given type at the specified level.  Assumes
   * that the type contains a static logger generated using {@link Logger.GetLogger}.
   * @param t The type used for creating the logger
   * @param minLogLevel The minimum level to accept
   */
  public static filterByType(t: Function, minLogLevel?: LoggerLevel): void {
    minLogLevel = minLogLevel || LoggerLevel.all;
    Logger.addFilter((l: Logger, ll: LoggerLevel) => {
      return ll >= minLogLevel && l.name === t.name;
    });
  }

  /**
   * Adds a filter to allow logging from a logger that matches the given pattern at any level.
   * @param namePattern A regular expression to match against the logger name
   */
  public static filterByName(namePattern: string): void;

  /**
   * Adds a filter to allow logging from a logger that matches the given pattern at the specified
   * level.
   * @param namePattern A regular expression to match against the logger name
   * @param minLogLevel The minimum level to accept
   */
  public static filterByName(namePattern: string, minLogLevel?: LoggerLevel): void {
    minLogLevel = minLogLevel || LoggerLevel.all;
    let regex: RegExp = new RegExp(namePattern, 'i');
    Logger.addFilter((l: Logger, ll: LoggerLevel) => {
      return ll >= minLogLevel && ss.isValue(l.name.match(regex));
    });
  }

  /**
   * Clears all appenders.
   */
  public static clearAppenders(): void {
    ss.clear(Logger.appenders);
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
    ss.remove(Logger.appenders, appender);
  }

  /**
   * Convenience method for lazily getting a logger for a class.  A static instance of a logger
   * will be created the first time this method is called.  All subsequent calls will use the
   * existing value.
   * @param t The type to assign the logger to.
   * @returns The type's logger
   */
  public static lazyGetLogger(t: Function): Logger {
    return ss.reinterpret(MiscUtil.lazyInitStaticField(t, '_logger', () => {
      return Logger.getLogger(t);
    }));
  }

  /**
   * Creates a new instance of a log for the given type.
   * @param t The type to create a log for
   * @returns A new Log instance
   */
  public static getLogger(t: Function): Logger;

  /**
   * Creates a new instance of a log for the given type and includes a filter for the
   * created logger at the given level.  This method should be used sparingly when
   * debugging.
   * @param t The type to create a log for
   * @param ll The min
   * @returns A new Log instance
   */
  public static getLogger(t: Function, ll?: LoggerLevel): Logger {
    let l: Logger = Logger.getLoggerWithName(t.name);
    if (ss.isValue(ll)) {
      Logger.filterByLogger(l, ll);
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
    this.logInternal(LoggerLevel.debug, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Info}.
   * @param message The message
   * @param args The format arguments.
   */
  public info(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.info, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Warn}.
   * @param message The message
   * @param args The format arguments.
   */
  public warn(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.warn, message, args);
  }

  /**
   * Logs the given message with {@link LoggerLevel.Error}.
   * @param message The message
   * @param args The format arguments.
   */
  public error(message: string, args: any[]): void {
    this.logInternal(LoggerLevel.error, message, args);
  }

  /**
   * Logs the given message.
   */
  public log(level: LoggerLevel, message: string, args: any[]): void {
    this.logInternal(level, message, args);
  }

  private static setupUrlFilters(): void {
    let queryParams: Object<string, string[]> = MiscUtil.getUriQueryParameters(window.self.location.search);
    if (!ss.keyExists(queryParams, Logger.logQueryParam)) {
      return;
    }
    Logger.clearFilters();
    let logParams: string[] = queryParams[Logger.logQueryParam];
    if (logParams.length === 0) {
      Logger.filterByName('.*', LoggerLevel.all);
    }
    for (const logParam of logParams) {
      let logVals: string[] = logParam.split(string.fromCharCode(':'));
      let level: LoggerLevel = LoggerLevel.debug;
      if (logVals.length > 0 && ss.isValue(logVals[1])) {
        let key: string = logVals[1].toLowerCase();
        let index: number = ss.indexOf(Logger.loggerLevelNames, key);
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
}
