import { MetricsEventType, MetricsSuites } from './MetricsController';

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

/**
 * To make the MetricsLogger happy, the ScriptNames here MUST match those of the
 * MetricsParameterName enum.
 */
export class MetricsEventParameters {
  public d: string;

  /**
   * This number represents miliseconds
   */
  public t: number;

  /**
   * This number represents miliseconds
   */
  public e: number;

  public id: string;

  public v: number[];

  public sid: string;

  public ei: string;

  public wb: string;

  public s: string;

  public m: boolean;
}

/**
 * Class representing a single timing event for the MetricsController
 */
export class MetricsEvent {
  public readonly eventType: MetricsEventType;

  public readonly metricSuite: MetricsSuites;

  public readonly parameters: MetricsEventParameters;

  public constructor(evtType: MetricsEventType, suite: MetricsSuites, eventParams: MetricsEventParameters) {
    this.eventType = evtType;
    this.metricSuite = suite;
    this.parameters = eventParams;
  }
}
