import { ClientMetric, tsConfig } from 'TypeDefs';

import 'mscorlib';

import { IWebClientMetricsLogger } from './IWebClientMetricsLogger';

import { JsNativeExtensionMethods, TypeUtil } from 'NativeJsTypeDefs';

import { MetricsEvent, MetricsEventParameters } from './MetricsEvent';

import { PerformanceReporting } from './PerformanceReporting';

export enum MetricsSuites {
  None = 0x0,
  Navigation = 0x1,
  Bootstrap = 0x2,
  Commands = 0x4,
  Rendering = 0x10,
  Debug = 0x20,
  Toolbar = 0x40,
  Fonts = 0x80,
  HitTest = 0x100,
  Maps = 0x200,
  Exporting = 0x400,
  Min = 0x3,
  Core = 0xf,
  All = 0xffff,
}

export const enum MetricsEventType {
  Navigation = 'nav',
  ContextEnd = 'wp',
  Generic = 'gen',
  SessionInit = 'init',
}

class LocalWebClientMetricsLogger implements IWebClientMetricsLogger {
  private static readonly appStartMarker: string = 'AppStartEpochMarker';

  public logEvent(evt: MetricsEvent): void {
    let desc: string = LocalWebClientMetricsLogger.buildDescriptionName(evt.parameters.d, evt.parameters.ei);
    let startMarkName: string;
    if (evt.metricSuite === MetricsSuites.Bootstrap && ss.isValue(evt.parameters.e)) {
      startMarkName = LocalWebClientMetricsLogger.appStartMarker;
    } else {
      startMarkName = LocalWebClientMetricsLogger.logLocalMetricStart(desc);
    }
    LocalWebClientMetricsLogger.logLocalMetricEnd(desc, startMarkName);
  }

  public static logLocalMetricStart(metricName: string): string {
    let startMarkName: string = LocalWebClientMetricsLogger.buildStartName(metricName);
    PerformanceReporting.mark(startMarkName);
    return startMarkName;
  }

  public static markAppStart(): void {
    PerformanceReporting.mark(LocalWebClientMetricsLogger.appStartMarker);
  }

  public static logLocalMetricEnd(metricName: string, startMarkName: string = null): void {
    let endMarkName: string = LocalWebClientMetricsLogger.buildEndName(metricName);
    startMarkName = startMarkName || LocalWebClientMetricsLogger.buildStartName(metricName);
    PerformanceReporting.mark(endMarkName);
    PerformanceReporting.measure('✚ ' + metricName, startMarkName, endMarkName);
  }

  public static buildDescriptionName(message: string, extraInfo: string): string {
    message = MetricsController.getFriendlyEventDescription(message);
    if (extraInfo !== null) {
      message += ' ' + extraInfo;
    }
    return message;
  }

  private static buildStartName(desc: string): string {
    return '__start__' + desc;
  }

  private static buildEndName(desc: string): string {
    return '__end__' + desc;
  }
}

/**
 * Singleton class for measuring and logging code performance
 */
export class MetricsController {
  private static readonly fullMetricNameLookup: { [key: string]: string } = {
    [ClientMetric.BTSTRP]: 'Bootstrap Request',
    [ClientMetric.PROPRI]: 'Process Primary Payload',
    [ClientMetric.PROSEC]: 'Process Secondary Payload',
    [ClientMetric.MDLINI]: 'Initialize Models',
    [ClientMetric.MDLEVT]: 'Handle Model Events',
    [ClientMetric.EXELOC]: 'Execute Local Command',
    [ClientMetric.EXEREM]: 'Execute Remote Command',
    [ClientMetric.PROLOC]: 'Process Local Command Response',
    [ClientMetric.PROREM]: 'Process Remote Command Response',
    [ClientMetric.RNDRPT]: 'Render Panetable',
    [ClientMetric.RNDRRG]: 'Render Region',
    [ClientMetric.RTCONV]: 'Runtime model presmodel conversion',
    [ClientMetric.CLNTLD]: 'Client Loaded',
    [ClientMetric.APPSTR]: 'Application Startup',
    [ClientMetric.APPINT]: 'Application Interactive',
    [ClientMetric.ALLZNS]: 'All Zones Loaded',
    [ClientMetric.TBRLAY]: 'Toolbar Layout',
    [ClientMetric.TBRHNT]: 'Toolbar HandleNewToolbar',
    [ClientMetric.TBRADD]: 'Toolbar AddToolbar',
    [ClientMetric.TBRHRE]: 'Toolbar HandleResize',
    [ClientMetric.MDLOAD]: 'Load js async',
    [ClientMetric.EMLOAD]: 'Emscripten load',
    [ClientMetric.RTLOAD]: 'Runtime load',
    [ClientMetric.RTLPRC]: 'Runtime command local processing'
  };

  public static getTiming: () => number;

  public static readonly recordingStart: number;

  public static readonly reportLocalMetrics: boolean;

  private static readonly suiteNameLookup: { [key: string]: MetricsSuites };

  private static readonly appStartEpoch: number;

  private static $instance: MetricsController;

  private nextContextID: number = 0;

  private readonly contextStack: MetricsContext[] = [];

  private eventBuffer: MetricsEvent[] = [];

  private eventLoggers: IWebClientMetricsLogger[] = [];

  private static readonly localEventLogger: IWebClientMetricsLogger = new LocalWebClientMetricsLogger();

  private sessionId: string = '';

  private workbookName: string = '';

  private sheetName: string = '';

  private metricSessionId: string = '';

  private readonly metricsFilter: MetricsSuites = MetricsSuites.None;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    MetricsController.reportLocalMetrics = PerformanceReporting.supportsPerfApi;
    MetricsController.recordingStart = new Date().getTime();
    if (ss.isValue(window) && ss.isValue(window.self.performance) && ss.isValue((<any>window.self.performance)['now'])) {
      let epoch: number;
      if (ss.isValue(window.self.performance.timing)) {
        epoch = <number>(window.self.performance.timing.responseStart - window.self.performance.timing.navigationStart);
      } else {
        epoch = 0;
      }
      MetricsController.getTiming = () => window.self.performance.now() - epoch;
    } else {
      let pageNavigationEpoch: number = new Date().getTime();
      MetricsController.getTiming = () => new Date().getTime() - pageNavigationEpoch;
    }
    MetricsController.appStartEpoch = MetricsController.getTiming();
    if (MetricsController.reportLocalMetrics) {
      LocalWebClientMetricsLogger.markAppStart();
    }
    MetricsController.suiteNameLookup = {};
    MetricsController.suiteNameLookup['none'] = MetricsSuites.None;
    MetricsController.suiteNameLookup['navigation'] = MetricsSuites.Navigation;
    MetricsController.suiteNameLookup['bootstrap'] = MetricsSuites.Bootstrap;
    MetricsController.suiteNameLookup['rendering'] = MetricsSuites.Rendering;
    MetricsController.suiteNameLookup['commands'] = MetricsSuites.Commands;
    MetricsController.suiteNameLookup['toolbar'] = MetricsSuites.Toolbar;
    MetricsController.suiteNameLookup['hittest'] = MetricsSuites.HitTest;
    MetricsController.suiteNameLookup['debug'] = MetricsSuites.Debug;
    MetricsController.suiteNameLookup['fonts'] = MetricsSuites.Fonts;
    MetricsController.suiteNameLookup['maps'] = MetricsSuites.Maps;
    MetricsController.suiteNameLookup['exporting'] = MetricsSuites.Exporting;
    MetricsController.suiteNameLookup['min'] = MetricsSuites.Min;
    MetricsController.suiteNameLookup['core'] = MetricsSuites.Core;
    MetricsController.suiteNameLookup['all'] = MetricsSuites.All;
  }

  private constructor() {
    if (ss.isValue(tsConfig.metricsFilter) && tsConfig.metricsFilter !== '') {
      let filter: MetricsSuites = MetricsSuites.None;
      let filters: string[] = tsConfig.metricsFilter.split(string.fromCharCode('|'));
      for (const suite of filters) {
        let trimmedSuite: string = (typeof (suite['trim']) === 'function') ? suite.trim() : suite;
        trimmedSuite = trimmedSuite.toLowerCase();
        if (ss.keyExists(MetricsController.suiteNameLookup, trimmedSuite)) {
          filter |= MetricsController.suiteNameLookup[trimmedSuite];
        }
      }
      this.metricsFilter = filter;
    }
  }

  /**
   * Gets reference to instance of MetricsController class. Creates instance if it doesn't exist.
   */
  public static get instance(): MetricsController {
    if (MetricsController.$instance === null) {
      MetricsController.$instance = new MetricsController();
    }
    return MetricsController.$instance;
  }

  /**
   * Creates and returns a new timing/measurement context with the provided description.
   * In general, use with a Using() statement, such as:
   * using (var mc = MetricsController.CreateContext("stuff to measure", MetricsSuites.Debug))
   * {
   * ...
   * }
   * Or instead explicitly call the dispose() method on the returned MetricsContext object to complete the timing
   * of this context.
   */
  public static createContext(description: string, suite: MetricsSuites = MetricsSuites.Debug, extraInfo: string = null): MetricsContext {
    if (ss.isNullOrUndefined(suite)) {
      suite = MetricsSuites.Debug;
    }
    let filteredMetric: boolean = (suite === MetricsSuites.None || (suite & MetricsController.instance.metricsFilter) !== suite);
    let newContext: MetricsContext;
    if (filteredMetric) {
      if (MetricsController.reportLocalMetrics) {
        newContext = new LocalMetricsContext(suite, description, extraInfo);
      } else {
        newContext = NullMetricsContext.instance;
      }
    } else {
      newContext = new MetricsContext(MetricsController.instance.getNextContextID(), suite, description, extraInfo);
      MetricsController.instance.contextStack.push(newContext);
    }
    return newContext;
  }

  /**
   * Most consumers who want to log a metrics should use this simplified method.
   * Logs the given event to the chrome dev tools and any reporters wired up
   * @param description 
   * @param metricsSuite 
   */
  public static logMetricsEvent(description: string, metricsSuite: MetricsSuites): void {
    let parameters: MetricsEventParameters = new MetricsEventParameters();
    if (metricsSuite === MetricsSuites.Bootstrap) {
      parameters.e = parameters.t - MetricsController.appStartEpoch;
    }
    MetricsController.logEventInternalUse(new MetricsEvent(MetricsEventType.Generic, metricsSuite, parameters));
  }

  /**
   * This is primarily for inernal and special event logging
   * Logs the given event to the chrome dev tools and any reporters wired up
   * 
   * This method would be private if not for the uses in tests and NavigationMetricsCollector
   */
  public static logEventInternalUse(evt: MetricsEvent): void {
    if (MetricsController.reportLocalMetrics && evt.eventType !== MetricsEventType.SessionInit) {
      MetricsController.localEventLogger.logEvent(evt);
    }
    if (evt.metricSuite === MetricsSuites.None || (evt.metricSuite & MetricsController.instance.metricsFilter) !== evt.metricSuite) {
      return;
    }
    if (MetricsController.instance.eventLoggers.length > 0) {
      MetricsController.sendMetricToAllLoggers(evt);
    } else {
      MetricsController.instance.eventBuffer.push(evt);
    }
  }

  private static sendMetricToAllLoggers(evt: MetricsEvent): void {
    evt.parameters.sid = MetricsController.instance.metricSessionId;
    for (const logger of MetricsController.instance.eventLoggers) {
      logger.logEvent(evt);
    }
  }

  /**
   * Sets the logging interface, and outputs any buffered MetricsEvents
   * that are pending
   */
  public static setEventLoggers(loggers: IWebClientMetricsLogger[]): void {
    ss.Debug.assert(loggers !== null, 'Don\'t pass in a null set of loggers');
    MetricsController.instance.eventLoggers = loggers;
    if (ss.isValue(loggers) && (MetricsController.$instance.eventBuffer.length > 0)) {
      for (const bufferedEvt of MetricsController.$instance.eventBuffer) {
        MetricsController.sendMetricToAllLoggers(bufferedEvt);
      }
      MetricsController.$instance.eventBuffer = [];
    }
  }

  /**
   * (Re-)Initialize the session identifying information so we can associate metrics events
   * with a given session + workbook + dashboard.
   */
  public static initSessionInfo(): void {
    let localInstance: MetricsController = MetricsController.instance;
    let currentSheet: string = ss.isNullOrEmptyString(tsConfig.current_sheet_name) ? tsConfig.sheetId : tsConfig.current_sheet_name;
    if (localInstance.sessionId === tsConfig.sessionid && localInstance.workbookName === tsConfig.workbookName && localInstance.sheetName === currentSheet) {
      return;
    }
    localInstance.sessionId = tsConfig.sessionid;
    localInstance.workbookName = tsConfig.workbookName;
    localInstance.sheetName = currentSheet;
    let now: Date = new Date();
    localInstance.metricSessionId = now.getTime().toString(36);
    localInstance.metricSessionId = localInstance.metricSessionId.substr(localInstance.metricSessionId.length - 6);
    if (localInstance.sessionId.length >= 5) {
      localInstance.metricSessionId = localInstance.metricSessionId + localInstance.sessionId.substr(1, 1);
      localInstance.metricSessionId = localInstance.metricSessionId + localInstance.sessionId.substr(4, 1);
    }
    localInstance.logSessionInfo();
  }

  public static getFriendlyEventDescription(desc: string): string {
    return MetricsController.fullMetricNameLookup[desc] || desc;
  }

  /**
   * Close the provided MetricsContext object and stop the timing.  Removes any orphaned
   * nested contexts in the process.
   */
  public static closeContext(context: MetricsContext): void {
    let id: number = context.id;
    let pos: number = -1;
    for (let i = (MetricsController.instance.contextStack.length - 1); i >= 0; i--) {
      if (MetricsController.$instance.contextStack[i].id === id) {
        pos = i;
        break;
      }
    }
    if (pos !== -1) {
      let cnt: number = MetricsController.$instance.contextStack.length - pos;
      for (let i = 0; i < cnt; i++) {
        MetricsController.$instance.contextStack.pop();
      }
    }
    MetricsController.$instance.logContextEnd(context);
  }

  private static buildMetricsEventCommonParameters(context: MetricsContext): MetricsEventParameters {
    let parameters: MetricsEventParameters = new MetricsEventParameters();
    if (context.extraInfo !== null) {
      parameters.ei = context.extraInfo;
    }
    return parameters;
  }

  /**
   * Returns the next unique ID to be assigned to a new MetricsContext
   */
  public getNextContextID(): number {
    let id: number = this.nextContextID;
    ++this.nextContextID;
    return id;
  }

  /**
   * Log the current session identifying information so we can correlate it with the metrics events
   */
  private logSessionInfo(): void {
    let parameters: MetricsEventParameters = new MetricsEventParameters();
    MetricsController.logEventInternalUse(new MetricsEvent(MetricsEventType.SessionInit, MetricsSuites.Bootstrap, parameters));
  }

  /**
   * Logs an event for the close of a MetricsContext
   */
  private logContextEnd(context: MetricsContext): void {
    let parameters: MetricsEventParameters = MetricsController.buildMetricsEventCommonParameters(context);
    parameters.t = context.endTime;
    parameters.e = context.elapsedMS();
    MetricsController.logEventInternalUse(new MetricsEvent(MetricsEventType.ContextEnd, context.metricSuite, parameters));
  }
}

// Call the static constructor
MetricsController.__ctor();

/**
 * Class representing a single timing context for the MetricsController
 */
export class MetricsContext implements ss.IDisposable {
  public readonly id: number;

  public readonly metricSuite: MetricsSuites;

  public readonly description: string;

  public readonly extraInfo: string;

  protected start: number;

  protected end: number;

  protected open: boolean;

  private readonly localReporter: LocalMetricsContext = null;

  public constructor(contextID: number, suite: MetricsSuites, desc: string, extraInfo: string) {
    this.extraInfo = extraInfo;
    if (MetricsController.reportLocalMetrics) {
      this.localReporter = new LocalMetricsContext(suite, desc, extraInfo);
    }
  }

  protected constructor(contextID: number, suite: MetricsSuites, desc: string) {
    this.id = contextID;
    this.metricSuite = suite;
    this.description = desc;
    this.start = MetricsController.getTiming();
    this.open = true;
  }

  /**
   * Gets the End date/time of this context (if it's been closed)
   */
  public get endTime(): number {
    return this.end;
  }

  /**
   * Closes the current context and stops any existing timing.
   */
  public dispose(): void {
    this.close();
  }

  /**
   * Closes the current context and stops any existing timing.
   */
  protected close(): void {
    if (this.open) {
      this.end = MetricsController.getTiming();
      MetricsController.closeContext(this);
      this.open = false;
      if (this.localReporter !== null) {
        this.localReporter.close();
      }
    }
  }

  /**
   * Returns the number of milliseconds elapsed since this context was opened.
   * If the context has been closed, returns the milliseconds that elapsed between
   * creation and close of the context
   */
  public elapsedMS(): number {
    if (!this.open) {
      return this.end - this.start;
    }
    return MetricsController.getTiming() - this.start;
  }
}

/**
 * Class representing a single timing context for the MetricsController
 */
class LocalMetricsContext extends MetricsContext {
  private readonly metricName: string;

  private static eventCount: number = 0;

  public constructor(suite: MetricsSuites, desc: string, extraInfo: string) {
    desc = LocalWebClientMetricsLogger.buildDescriptionName(desc, extraInfo);
    this.open = true;
    this.metricName = desc + '#' + LocalMetricsContext.eventCount++;
    LocalWebClientMetricsLogger.logLocalMetricStart(this.metricName);
  }

  /**
   * Closes the current context and stops any existing timing.
   */
  protected close(): void {
    if (this.open) {
      this.open = false;
      LocalWebClientMetricsLogger.logLocalMetricEnd(this.metricName);
    }
  }
}

/**
 * A singleton class that 'implements' the MetricsContext interface but doesn't do/store any timing
 * or metrics events in practice
 */
class NullMetricsContext extends MetricsContext {
  private static $instance: NullMetricsContext;

  private constructor() {
    this.open = false;
    this.start = 0;
    this.end = 0;
  }

  public static get instance(): NullMetricsContext {
    if (ss.isNullOrUndefined(NullMetricsContext.$instance)) {
      NullMetricsContext.$instance = new NullMetricsContext();
    }
    return NullMetricsContext.$instance;
  }

  /**
   * Closes the current context and stops any existing timing.
   */
  protected close(): void { }
}
