import { Logger, LoggerLevel } from './Logger';

import 'mscorlib';

/**
 * Used to hold an instance of ILogAppender, ensures that when enabled/disabled the appender is
 * removed/added to the global Logger instance.
 * This has the IncludeGenericArguments(false) attribute because it is part of CoreSlim which
 * does not include the full mscorlib so does not have the generic class initializer.
 * This attribute means that this class is treated like a normal class (generic is checked only
 * at compile time)
 * typeparam T The concrete implementation of ILogAppender
 */
export class LogAppenderInstance<T> {
  private _$instanceField: T;

  private readonly appenderFactoryFunc: () => T;

  public constructor(appenderFactoryFunc: () => T) {
    this.appenderFactoryFunc = appenderFactoryFunc;
  }

  public get instance(): T {
    return this._$instanceField;
  }

  public set instance(value: T) {
    this._$instanceField = value;
  }

  /**
   * Enables logging using this appender type.
   * @param filter The filter to apply to this appender or `null` to enable for all loggers
   */
  public enableLogging(filter: (logger: Logger, loggerLevel: LoggerLevel) => boolean = null): void {
    if (this.instance === null) {
      this.instance = this.appenderFactoryFunc();
      Logger.addAppender(this.instance);
    } else
      if (!Logger.hasAppender(this.instance)) {
        Logger.addAppender(this.instance);
      }
    this.instance.addFilter(ss.coalesce(filter, (_, __) => true));
  }

  /**
   * Disables logging using this appender type.
   */
  public disableLogging(): void {
    if (this.instance === null) {
      return;
    }
    Logger.removeAppender(this.instance);
    this.instance = null;
  }
}
