import { Logger, LoggerLevel } from './Logger';

/**
 * An interface for all Log appenders.
 */
export interface ILogAppender {
  /**
   * Adds a function used to filter calls to {@link ILogAppender.Log}.  Should return `true`
   * to include a log message or `false` to exclude.
   */
  addFilter(f: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void;
  removeFilter(f: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void;
  clearFilters(): void;
  /**
   * Logs the specified information.
   * @param source The source Log
   * @param level The level of the log message
   * @param message The message to write
   * @param args Optional message arguments.
   */
  log(source: Logger, level: LoggerLevel, message: string, args: any[]): void;
}
