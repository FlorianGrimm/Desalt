import { MetricsEvent } from './MetricsEvent';

export interface IWebClientMetricsLogger {
  logEvent(evt: MetricsEvent): void;
}
