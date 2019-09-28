import { BaseLogAppender } from '../../CoreSlim/Logging/BaseLogAppender';

import { $ } from 'jQuery';

import { LogAppenderInstance } from '../../CoreSlim/Logging/LogAppenderInstance';

import { Logger, LoggerLevel } from '../../CoreSlim/Logging/Logger';

import 'mscorlib';

/**
 * A logger that writes to a floating window.  Designed for use by the TiledViewerRegions with mouse.
 * MobileWindowAppender.cs should probably be made a subclass of this one?
 */
export class WindowAppender extends BaseLogAppender {
  public static readonly globalAppender: LogAppenderInstance<WindowAppender> = new LogAppenderInstance<WindowAppender>(() => new WindowAppender());

  private logDiv: Object;

  /**
   */
  public static __ctor() {
    WindowAppender.globalAppender.enableLogging((logger, _) => logger.name === 'WindowAppender');
  }

  /**
   */
  private constructor() { }

  protected logInternal(source: Logger, level: LoggerLevel, message: string, args: any[]): void {
    if (ss.isNullOrUndefined(this.logDiv)) {
      this.buildLogDiv();
    }
    message = this.formatMessage(ss.replaceAllString(message, '\n', '<br />'), args);
    this.logDiv.html(message);
  }

  private buildLogDiv(): void {
    this.logDiv = $.$('<div class=\'log-window-appender\'>Debug mode ON</div>');
    this.logDiv.css({
      'position': 'absolute',
      'bottom': '0px',
      'right': '0px',
      'backgroundColor': 'white',
      'opacity': '.8',
      'border': '1px solid black',
      'minWidth': '5px',
      'minHeight': '5px',
      'z-index': '100'
    });
    $.$('body').append(this.logDiv);
  }
}

// Call the static constructor
WindowAppender.__ctor();
