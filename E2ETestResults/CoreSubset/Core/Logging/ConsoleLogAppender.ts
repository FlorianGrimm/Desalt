import { BaseLogAppender } from './BaseLogAppender';

import { BrowserSupport } from '../Utility/BrowserSupport';

import { Logger, LoggerLevel } from './Logger';

import 'mscorlib';

import { ScriptEx } from '../../CoreSlim/ScriptEx';

import { TypeUtil } from 'NativeJsTypeDefs';

/**
 * An appender that writes to console.log.
 */
export class ConsoleLogAppender extends BaseLogAppender {
  private static globalAppender: ConsoleLogAppender;

  private levelMethods: { [key: string]: any };

  /**
   */
  public static __ctor() {
    ConsoleLogAppender.enableLogging((l: Logger, ll: LoggerLevel) => {
      return ll >= LoggerLevel.Info;
    });
  }

  /**
   */
  private constructor() { }

  public static enableLogging(): void;

  /**
   * Enables logging using this appender type.
   * @param filter The filter to apply to this appender or `null` to enable for all loggers
   */
  public static enableLogging(filter?: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void {
    if (ss.isNullOrUndefined(ConsoleLogAppender.globalAppender)) {
      ConsoleLogAppender.globalAppender = new ConsoleLogAppender();
      Logger.addAppender(ConsoleLogAppender.globalAppender);
    }
    ConsoleLogAppender.globalAppender.addFilter((filter) || (() => {
      return true;
    }));
  }

  /**
   * Disables logging using this appender type.
   */
  public static disableLogging(): void {
    if (ss.isNullOrUndefined(ConsoleLogAppender.globalAppender)) {
      return;
    }
    Logger.removeAppender(ConsoleLogAppender.globalAppender);
    ConsoleLogAppender.globalAppender = null;
  }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    if (typeof (<any>window).console !== 'object') {
      return;
    }
    message = source.name + ': ' + message;
    let consoleArgs: any[] = [];
    if (BrowserSupport.consoleLogFormating) {
      consoleArgs = consoleArgs.concat(message).concat(args);
    } else {
      consoleArgs = consoleArgs.concat(this.formatMessage(message, args));
    }
    try {
      (<any>Function.prototype).apply.call(this.getConsoleMethod(level), (<any>window).console, consoleArgs);
    } catch { }
  }

  private getConsoleMethod(level: LoggerLevel): any {
    let console: any = window.self['console'];
    if (this.levelMethods === null) {
      this.levelMethods = {};
      this.levelMethods[LoggerLevel.Debug.toString()] = console.log;
      this.levelMethods[LoggerLevel.Error.toString()] = console.error;
      this.levelMethods[LoggerLevel.Info.toString()] = console.info;
      this.levelMethods[LoggerLevel.Warn.toString()] = console.warn;
    }
    return this.levelMethods[level.toString()] || console.log;
  }
}

// Call the static constructor
ConsoleLogAppender.__ctor();
