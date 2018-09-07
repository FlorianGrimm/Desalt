import 'mscorlib';

import { tsConfig } from 'TypeDefs';

import { TypeUtil } from 'NativeJsTypeDefs';

export enum MetricsSuites {
  None = 0x0,
  Navigation = 0x1,
  Bootstrap = 0x2,
  Rendering = 0x4,
  Commands = 0x8,
  HitTest = 0x10,
  Debug = 0x20,
  Toolbar = 0x40,
  Fonts = 0x80,
  Min = 0x3,
  Core = 0xf,
  All = 0xff,
}

export const enum MetricsParameterName {
  description = 'd',
  time = 't',
  id = 'id',
  elapsed = 'e',
  values = 'v',
  sessionId = 'sid',
  workbook = 'wb',
  sheet = 's',
  isMobile = 'm',
  properties = 'p',
}

export const enum MetricsEventType {
  Navigation = 'nav',
  ContextStart = 'wps',
  ContextEnd = 'wp',
  Generic = 'gen',
  SessionInit = 'init',
}

/**
 * Singleton class for measuring and logging code performance
 */
export class MetricsController {
  public static getTiming: () => number;

  private static epoch: number;

  private static $instance: MetricsController;

  private static suiteNameLookup: Object<string, MetricsSuites>;

  private nextContextID: number = 0;

  private contextStack: MetricsContext[] = [];

  private eventBuffer: MetricsEvent[] = [];

  private eventLogger: (metricsEvent: MetricsEvent) => void = null;

  private sessionId: string = '';

  private workbookName: string = '';

  private sheetName: string = '';

  private metricSessionId: string = '';

  private metricsFilter: MetricsSuites = MetricsSuites.None;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    if (ss.isValue(window) && ss.isValue(window.self.performance) && ss.isValue((<any>window.self.performance)['now'])) {
      if (ss.isValue(window.self.performance.timing)) {
        MetricsController.epoch = <number>(window.self.performance.timing.responseStart - window.self.performance.timing.navigationStart);
      } else {
        MetricsController.epoch = 0;
      }
      MetricsController.getTiming = () => window.self.performance.now() - MetricsController.epoch;
    } else {
      MetricsController.epoch = new Date().getTime();
      MetricsController.getTiming = () => new Date().getTime() - MetricsController.epoch;
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
  private static get instance(): MetricsController {
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
  public static createContext(description: string, suite: MetricsSuites = MetricsSuites.Debug, props: Object<string, any> = null): MetricsContext {
    if (ss.isNullOrUndefined(suite)) {
      suite = MetricsSuites.Debug;
    }
    let filteredMetric: boolean = (suite === MetricsSuites.None || (suite & MetricsController.instance.metricsFilter) !== suite);
    if (ss.isNullOrUndefined(props)) {
      props = {};
    }
    let newContext: MetricsContext;
    if (filteredMetric) {
      newContext = NullMetricsContext.instance;
    } else {
      newContext = new MetricsContext(MetricsController.instance.getNextContextID(), suite, description, props);
      MetricsController.instance.contextStack.push(newContext);
    }
    return newContext;
  }

  /**
   * Logs the given event by outputting it to the console and/or sending it to
   * the server for recording in the log
   */
  public static logEvent(evt: MetricsEvent): void {
    if (evt.metricSuite === MetricsSuites.None || (evt.metricSuite & MetricsController.instance.metricsFilter) !== evt.metricSuite) {
      return;
    }
    if (ss.isValue(MetricsController.instance.eventLogger)) {
      evt.parameters[MetricsParameterName.sessionId] = MetricsController.instance.metricSessionId;
      MetricsController.instance.eventLogger(evt);
    } else {
      MetricsController.instance.eventBuffer.push(evt);
    }
  }

  /**
   * Sets the logging interface, and outputs any buffered MetricsEvents
   * that are pending
   */
  public static setEventLogger(logger: (metricsEvent: MetricsEvent) => void): void {
    MetricsController.instance.eventLogger = logger;
    if (ss.isValue(logger) && (MetricsController.$instance.eventBuffer.length > 0)) {
      for (const bufferedEvt of MetricsController.$instance.eventBuffer) {
        bufferedEvt.parameters[MetricsParameterName.sessionId] = MetricsController.$instance.metricSessionId;
        MetricsController.$instance.eventLogger(bufferedEvt);
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

  private static buildMetricsEventCommonParameters(context: MetricsContext): Object<MetricsParameterName, any> {
    let parameters: Object<MetricsParameterName, any> = {};
    parameters[MetricsParameterName.id] = context.id;
    parameters[MetricsParameterName.description] = context.description;
    if (context.properties.count > 0) {
      parameters[MetricsParameterName.properties] = context.properties;
    }
    return parameters;
  }

  /**
   * Returns the next unique ID to be assigned to a new MetricsContext
   */
  private getNextContextID(): number {
    let id: number = this.nextContextID;
    ++this.nextContextID;
    return id;
  }

  /**
   * Log the current session identifying information so we can correlate it with the metrics events
   */
  private logSessionInfo(): void {
    let parameters: Object<MetricsParameterName, any> = {};
    parameters[MetricsParameterName.id] = this.sessionId;
    parameters[MetricsParameterName.sessionId] = this.metricSessionId;
    parameters[MetricsParameterName.workbook] = this.workbookName;
    parameters[MetricsParameterName.sheet] = this.sheetName;
    parameters[MetricsParameterName.isMobile] = tsConfig.is_mobile;
    MetricsController.logEvent(new MetricsEvent(MetricsEventType.SessionInit, MetricsSuites.Bootstrap, parameters));
  }

  /**
   * Logs an event for the close of a MetricsContext
   */
  private logContextEnd(context: MetricsContext): void {
    let parameters: Object<MetricsParameterName, any> = MetricsController.buildMetricsEventCommonParameters(context);
    parameters[MetricsParameterName.time] = context.endTime;
    parameters[MetricsParameterName.elapsed] = context.elapsedMS();
    MetricsController.logEvent(new MetricsEvent(MetricsEventType.ContextEnd, context.metricSuite, parameters));
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

  private readonly propBag: Object<string, any>;

  protected start: number;

  protected end: number;

  protected open: boolean;

  public constructor(contextID: number, suite: MetricsSuites, desc: string, props: Object<string, any>) {
    this.id = contextID;
    this.metricSuite = suite;
    this.description = desc;
    this.start = MetricsController.getTiming();
    this.open = true;
    this.propBag = props;
  }

  /**
   * Gets the End date/time of this context (if it's been closed)
   */
  public get endTime(): number {
    return this.end;
  }

  public get properties(): Object<string, any> {
    return this.propBag;
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

/**
 * Class representing a single timing event for the MetricsController
 */
export class MetricsEvent {
  public readonly eventType: MetricsEventType;

  public readonly metricSuite: MetricsSuites;

  public readonly parameters: Object<MetricsParameterName, any>;

  public constructor(evtType: MetricsEventType, suite: MetricsSuites, eventParams: Object<MetricsParameterName, any>) {
    this.eventType = evtType;
    this.metricSuite = suite;
    this.parameters = eventParams;
  }
}
