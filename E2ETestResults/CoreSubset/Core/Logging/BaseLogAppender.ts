import 'mscorlib';

import { ILogAppender } from './ILogAppender';

import { Logger, LoggerLevel } from './Logger';

/**
 * A base class for log appenders.
 */
export abstract class BaseLogAppender implements ILogAppender {
  private readonly filters: Array<(logger: Logger, loggerLevel: LoggerLevel) => boolean>;

  protected constructor() {
    this.filters = [];
  }

  public clearFilters(): void {
    ss.clear(this.filters);
  }

  public addFilter(f: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void {
    this.filters.push(f);
  }

  public removeFilter(f: (logger: Logger, loggerLevel: LoggerLevel) => boolean): void {
    ss.remove(this.filters, f);
  }

  /**
   */
  public log(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    for (const filter of this.filters) {
      if (!filter(source, level)) {
        continue;
      }
      this.logInternal(source, level, message, args);
      return;
    }
  }

  /**
   * Performs the actual logging.  The filters are checked before this method is called.
   * @param source The source Log
   * @param level The message level
   * @param message The message to write
   * @param args Optional message arguments.
   */
  protected abstract logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void;

  protected formatMessage(message: string, args: any[]): string {
    if (ss.isNullOrUndefined(args) || args.length === 0) {
      return message;
    }
    let sb: StringBuilder = new StringBuilder();
    let argNum: number = 0;
    let prevPercent: boolean = false;
    for (let i = 0; i < message.length; i++) {
      let currChar: Int32 = message.charCodeAt(i);
      if (currChar === '%') {
        if (prevPercent) {
          sb.append('%');
          prevPercent = false;
        } else {
          prevPercent = true;
        }
      } else {
        if (prevPercent) {
          switch (currChar) {
            case 'b':
            case 's':
            case 'd':
            case 'n':
            case 'o':
              sb.append(args.length > argNum ? args[argNum] : '');
              argNum++;
              break;
          }
        } else {
          sb.appendChar(currChar);
        }
        prevPercent = false;
      }
    }
    return sb.toString();
  }
}
