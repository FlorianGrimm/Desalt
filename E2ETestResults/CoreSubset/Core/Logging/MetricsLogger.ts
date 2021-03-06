import { IWebClientMetricsLogger } from '../../Bootstrap/Performance/IWebClientMetricsLogger';

import { $ } from 'jQuery';

import { JsNativeExtensionMethods } from 'NativeJsTypeDefs';

import { Logger } from '../../CoreSlim/Logging/Logger';

import 'mscorlib';

import { MetricsEvent, MetricsEventParameters, MetricsParameterName } from '../../Bootstrap/Performance/MetricsEvent';

import { MetricsEventType } from '../../Bootstrap/Performance/MetricsController';

import { MiscUtil } from '../Utility/MiscUtil';

import { ScriptEx } from '../../CoreSlim/Utility/ScriptEx';

import { tsConfig } from 'TypeDefs';

/**
 * class for logging metrics events
 */
export class MetricsLogger implements IWebClientMetricsLogger {
  /**
   * Cap on # of image elements we keep around for beacon messages
   * simply to avoid out of memory issues.
   */
  private static readonly maxBeaconElementArraySize: number = 100;

  /**
   * Cap on # of metrics events sitting in buffer/waiting to be reported
   */
  private static readonly maxEventBufferSize: number = 400;

  /**
   * Cap on # of metrics events to report/log in a given time slice
   */
  private static readonly maxEventsToProcess: number = 20;

  /**
   * Default delay to wait between event being logged and reporting the metrics
   * (to allow for a few to collect so they can be batched)
   */
  private static readonly defaultProcessingDelay: number = 250;

  /**
   * Amount of time to delay between processing batches of events
   * (eg if we've processed the first batch but still have more queued)
   */
  private static readonly overflowProcessingDelay: number = 5;

  /**
   * Interval(ms) at which to attempt to clean up image elements
   * created for sending web beacon
   */
  private static readonly beaconCleanupDelay: number = 250;

  /**
   * Mapping of metrics parameter names to verbose/debugging names
   */
  private static readonly debugParamNames: { [key: string]: string };

  /**
   * Mapping of metrics event types to verbose/debugging type names
   */
  private static readonly debugEventNames: { [key: string]: string };

  /**
   * Logged events waiting to be transmitted to the server
   */
  private eventBuffer: MetricsEvent[] = [];

  /**
   * Logger object for outputting metrics to the console
   */
  private logger: Logger;

  /**
   * Collection of image elements used to report metrics events back to the server
   * We must hold on to these until they are "complete", ie the src attribute is updated
   * and the browser has made the request/gotten the response
   */
  private beaconImages: Element[] = [];

  /**
   * Cached id of timer (via window.setTimeout) for callback to clean up beacon
   * images that are no longer needed
   */
  private beaconCleanupTimerId: number | null = null;

  /**
   * Cached timerID for buffer processing timer
   */
  private bufferProcessTimerId: number | null = null;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    MetricsLogger.debugParamNames = {};
    MetricsLogger.debugParamNames[MetricsParameterName.description] = 'DESC';
    MetricsLogger.debugParamNames[MetricsParameterName.time] = 'TIME';
    MetricsLogger.debugParamNames[MetricsParameterName.id] = 'ID';
    MetricsLogger.debugParamNames[MetricsParameterName.sessionId] = 'SESSION_ID';
    MetricsLogger.debugParamNames[MetricsParameterName.elapsed] = 'ELAPSED';
    MetricsLogger.debugParamNames[MetricsParameterName.values] = 'VALS';
    MetricsLogger.debugParamNames[MetricsParameterName.workbook] = 'WORKBOOK';
    MetricsLogger.debugParamNames[MetricsParameterName.sheet] = 'SHEET_NAME';
    MetricsLogger.debugParamNames[MetricsParameterName.properties] = 'PROPS';
    MetricsLogger.debugParamNames[MetricsParameterName.isMobile] = 'MOBILE';
    MetricsLogger.debugEventNames = {};
    MetricsLogger.debugEventNames[MetricsEventType.Navigation] = 'Navigation';
    MetricsLogger.debugEventNames[MetricsEventType.ContextEnd] = 'ProfileEnd';
    MetricsLogger.debugEventNames[MetricsEventType.Generic] = 'Generic';
    MetricsLogger.debugEventNames[MetricsEventType.SessionInit] = 'SessionInit';
  }

  /**
   * Logs the specified metric event.
   */
  public logEvent(evt: MetricsEvent): void {
    if (this.eventBuffer.length >= MetricsLogger.maxEventBufferSize) {
      this.eventBuffer.shift();
    }
    this.eventBuffer.push(evt);
    this.startProcessingTimer();
  }

  private startProcessingTimer(delay: number = MetricsLogger.defaultProcessingDelay): void {
    if (this.bufferProcessTimerId.hasValue) {
      return;
    }
    this.bufferProcessTimerId = window.setTimeout(this.processBufferedEvents, delay);
  }

  /**
   * Process our eventBuffer and output each event
   */
  private processBufferedEvents(): void {
    this.bufferProcessTimerId = null;
    let metricsToProcess: MetricsEvent[];
    if (this.eventBuffer.length > MetricsLogger.maxEventsToProcess) {
      metricsToProcess = this.eventBuffer.slice(0, MetricsLogger.maxEventsToProcess);
      this.eventBuffer = this.eventBuffer.slice(MetricsLogger.maxEventsToProcess);
      this.startProcessingTimer(MetricsLogger.overflowProcessingDelay);
    } else {
      metricsToProcess = this.eventBuffer;
      this.eventBuffer = [];
    }
    this.outputEventsToConsole(metricsToProcess);
    if (MetricsLogger.isLoggerEnabled()) {
      try {
        this.outputEventsToServer(metricsToProcess);
      } catch { }
    }
  }

  public static isLoggerEnabled(): boolean {
    return tsConfig.metricsReportingEnabled;
  }

  /**
   * Output the given event to the console, if appropriate
   */
  private outputEventsToConsole(evts: MetricsEvent[]): void {
    this.logger = (this.logger) || (Logger.lazyGetLogger(MetricsLogger));
    for (const evt of evts) {
      this.logger.debug(MetricsLogger.formatEvent(evt, true), []);
    }
  }

  /**
   * Output the given event to the server, if appropriate
   */
  private outputEventsToServer(evts: MetricsEvent[]): void {
    const MaxPayloadLength: number = 1500;
    let numEvents: number = evts.length;
    let payload: string = '';
    if (numEvents === 0) {
      return;
    }
    for (let i = 0; i < numEvents; i++) {
      let evt: MetricsEvent = evts[i];
      let formattedEvent: string = MetricsLogger.formatEvent(evt, false);
      if (payload.length > 0 && payload.length + formattedEvent.length > MaxPayloadLength) {
        this.sendBeacon(tsConfig.metricsServerHostname, payload);
        payload = '';
      } else
        if (payload.length > 0) {
          payload += '&';
        }
      payload += formattedEvent;
    }
    if (payload.length > 0) {
      this.sendBeacon(tsConfig.metricsServerHostname, payload);
    }
    if (this.beaconCleanupTimerId === null) {
      this.beaconCleanupTimerId = window.setTimeout(this.cleanupBeaconImages, MetricsLogger.beaconCleanupDelay);
    }
  }

  /**
   * Formats the specified MetricsEvent to a string.  If 'verbose' is true then
   * uses more user-readable formatting
   */
  private static formatEvent(evt: MetricsEvent, verbose: boolean): string {
    let delimiter: string = verbose ? ', ' : ',';
    let strBuilder: ss.StringBuilder = new ss.StringBuilder();
    strBuilder.append(verbose ? MetricsLogger.debugEventNames[evt.eventType] : evt.eventType.toString());
    let parameters: MetricsEventParameters = evt.parameters;
    let eventDict: { [key: string]: any } = parameters;
    if (parameters.ei !== null) {
      eventDict = <{ [key: string]: any }>MiscUtil.cloneObject(eventDict);
      let extraInfoParts: string[] = parameters.ei.split(': ');
      if (extraInfoParts.length > 1) {
        let fakeProps: { [key: string]: string } = new JsDictionary<string, string>([extraInfoParts]);
        eventDict[MetricsParameterName.properties] = fakeProps;
        delete eventDict['ei'];
      }
    }
    let count: number = eventDict.count;
    if (count > 0) {
      strBuilder.append('=');
      strBuilder.append('{');
      let i: number = 0;
      let propSeparator: string = verbose ? ': ' : ':';
      for (const key of eventDict.keys) {
        if (key === MetricsParameterName.id && evt.eventType !== MetricsEventType.SessionInit) {
          continue;
        }
        if (i++ > 0) {
          strBuilder.append(delimiter);
        }
        strBuilder.append(verbose ? MetricsLogger.debugParamNames[key] : key.toString());
        strBuilder.append(propSeparator);
        let val: any = eventDict[key];
        MetricsLogger.formatValue(strBuilder, val, verbose);
      }
      strBuilder.append('}');
    }
    return strBuilder.toString();
  }

  /**
   * Given a dictionary of values, output the list of name+value pairs to the string builder
   */
  private static formatDictionaryValues(strBuilder: ss.StringBuilder, dict: { [key: string]: any }, verbose: boolean): void {
    let delimiter: string = verbose ? ', ' : ',';
    let propSeparator: string = verbose ? ': ' : ':';
    let propCount: number = dict.count;
    let j: number = 0;
    for (const propertyName of dict.keys) {
      if (dict.hasOwnProperty(propertyName)) {
        let propertyVal: any = dict[propertyName];
        strBuilder.append(propertyName);
        strBuilder.append(propSeparator);
        MetricsLogger.formatValue(strBuilder, propertyVal, verbose);
        if (++j < propCount) {
          strBuilder.append(delimiter);
        }
      }
    }
  }

  /**
   * Given a specific object, output its value to the stringbuilder with the appropriate format
   */
  private static formatValue(strBuilder: ss.StringBuilder, value: any, verbose: boolean): void {
    let type: string = typeof value;
    if (type === 'number' && Math.floor(<number>value) !== <number>value) {
      strBuilder.append((<number>value).toFixed(1));
    } else
      if (type === 'string') {
        if (verbose || (value !== null && ((<string>value).indexOf('/') !== -1))) {
          let tempBuffer: ss.StringBuilder = new ss.StringBuilder();
          tempBuffer.append('\"');
          tempBuffer.append(value);
          tempBuffer.append('\"');
          if (verbose) {
            strBuilder.append(tempBuffer);
          } else {
            strBuilder.append(string.encodeURIComponent(tempBuffer.toString()));
          }
        } else {
          strBuilder.append(string.encodeURIComponent(<string>value));
        }
      } else
        if ($.isArray(value)) {
          strBuilder.append('[');
          strBuilder.append(value);
          strBuilder.append(']');
        } else
          if (type === 'object') {
            strBuilder.append('{');
            let dict: { [key: string]: any } = value;
            MetricsLogger.formatDictionaryValues(strBuilder, dict, verbose);
            strBuilder.append('}');
          } else {
            strBuilder.append(value);
          }
  }

  /**
   * A method of sending data back to the server for logging.
   * With the right apache configuration, the image request simply gets logged,
   * and we get a 204 in response.
   */
  private sendBeacon(hostname: string, payload: string): void {
    const Version: number = 1;
    if (MiscUtil.isNullOrEmpty$1(hostname) || MiscUtil.isNullOrEmpty$1(payload)) {
      return;
    }
    let beaconImg: Element = <Element>document.createElement('img');
    let versionStr: string = 'v=' + Version.toString();
    let beaconStr: string = hostname;
    beaconStr += '?' + versionStr + '&' + payload;
    beaconImg.src = beaconStr;
    this.beaconImages.push(beaconImg);
    if (this.beaconImages.length > MetricsLogger.maxBeaconElementArraySize) {
      this.beaconImages.shift();
    }
  }

  /**
   * Cleans up dynamically created beacon images that have served their purpose
   * ie, request has been sent and response received
   */
  private cleanupBeaconImages(): void {
    try {
      this.beaconCleanupTimerId = null;
      let index: number = 0;
      while (index < this.beaconImages.length) {
        if (this.beaconImages[index].complete) {
          this.beaconImages.splice(index, 1);
        } else {
          index++;
        }
      }
      if (this.beaconImages.length > 0) {
        this.beaconCleanupTimerId = window.setTimeout(this.cleanupBeaconImages, MetricsLogger.beaconCleanupDelay);
      }
    } catch { }
  }
}

// Call the static constructor
MetricsLogger.__ctor();
