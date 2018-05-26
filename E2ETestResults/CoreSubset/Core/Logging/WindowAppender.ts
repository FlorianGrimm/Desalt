import { BaseLogAppender } from './BaseLogAppender';

import { $ } from 'Saltarelle.jQuery';

import { Logger, LoggerLevel } from './Logger';

import 'mscorlib';

import { ScriptEx } from '../../CoreSlim/ScriptEx';

/**
 * A logger that writes to a floating window.  Designed for use by the TiledViewerRegions with mouse.
 * MobileWindowAppender.cs should probably be made a subclass of this one?
 */
export class WindowAppender extends BaseLogAppender {
  private static globalAppender: WindowAppender;

  private logDiv: Object;

  /**
   */
  public static __ctor() {
    WindowAppender.enableLogging((l: Logger, ll: LoggerLevel) => {
      return l.name === 'WindowAppender';
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
    if (ss.isNullOrUndefined(WindowAppender.globalAppender)) {
      WindowAppender.globalAppender = new WindowAppender();
      Logger.addAppender(WindowAppender.globalAppender);
    }
    WindowAppender.globalAppender.addFilter((filter) || (() => {
      return true;
    }));
  }

  /**
   * Disables logging using this appender type.
   */
  public static disableLogging(): void {
    if (ss.isNullOrUndefined(WindowAppender.globalAppender)) {
      return;
    }
    Logger.removeAppender(WindowAppender.globalAppender);
    WindowAppender.globalAppender = null;
  }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    if (ss.isNullOrUndefined(this.logDiv)) {
      this.buildLogDiv();
    }
    message = this.formatMessage(ss.replaceAllString(message, '\n', '<br />'), args);
    this.logDiv.html(message);
  }

  private buildLogDiv(): void {
    this.logDiv = $.$('<div class=\'log-window-appender\'>Debug mode ON</div>');
    this.logDiv.css(new Object('position', 'absolute', 'bottom', '0px', 'right', '0px', 'backgroundColor', 'white', 'opacity', '.8', 'border', '1px solid black', 'minWidth', '5px', 'minHeight', '5px', 'z-index', '100'));
    $.$('body').append(this.logDiv);
  }
}

// Call the static constructor
WindowAppender.__ctor();
