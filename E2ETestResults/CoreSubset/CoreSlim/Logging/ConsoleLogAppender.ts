import { BaseLogAppender } from './BaseLogAppender';

import { LogAppenderInstance } from './LogAppenderInstance';

import { Logger, LoggerLevel } from './Logger';

import 'mscorlib';

import { TypeUtil } from 'NativeJsTypeDefs';

/**
 * An appender that writes to console.log.
 */
export class ConsoleLogAppender extends BaseLogAppender {
  public static readonly globalAppender: LogAppenderInstance<ConsoleLogAppender> = new LogAppenderInstance<ConsoleLogAppender>(() => new ConsoleLogAppender());

  private levelMethods: { [key: string]: any };

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    ConsoleLogAppender.globalAppender.enableLogging((_, loggerLevel) => loggerLevel >= LoggerLevel.Info);
  }

  private constructor() { }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    if (typeof (<any>window).console !== 'object') {
      return;
    }
    message = source.name + ': ' + message;
    let consoleArgs: any[] = [];
    consoleArgs = consoleArgs.concat([message]).concat([args]);
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
