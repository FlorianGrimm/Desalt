import { MetricsController, MetricsEventType, MetricsSuites } from '../../Bootstrap/Performance/MetricsController';

import { MetricsEvent, MetricsEventParameters } from '../../Bootstrap/Performance/MetricsEvent';

import 'mscorlib';

import { _ } from 'Underscore';

export const enum NavigationMetricsName {
  navigationStart = 'navigationStart',
  unloadEventStart = 'unloadEventStart',
  unloadEventEnd = 'unloadEventEnd',
  redirectStart = 'redirectStart',
  redirectEnd = 'redirectEnd',
  fetchStart = 'fetchStart',
  domainLookupStart = 'domainLookupStart',
  domainLookupEnd = 'domainLookupEnd',
  connectStart = 'connectStart',
  connectEnd = 'connectEnd',
  secureConnectionStart = 'secureConnectionStart',
  requestStart = 'requestStart',
  responseStart = 'responseStart',
  responseEnd = 'responseEnd',
  domLoading = 'domLoading',
  domInteractive = 'domInteractive',
  domContentLoadedEventStart = 'domContentLoadedEventStart',
  domContentLoadedEventEnd = 'domContentLoadedEventEnd',
  domComplete = 'domComplete',
  loadEventStart = 'loadEventStart',
  loadEventEnd = 'loadEventEnd',
}

/**
 * Static class for collecting navigation/performance timings and logging them
 */
export class NavigationMetricsCollector {
  /**
   * Array of names of navigation events, ordered by their occurrence in time
   */
  private static navigationMetricsOrder: NavigationMetricsName[] = NavigationMetricsName.navigationStart;

  private static navMetrics: { [key: string]: number } = null;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    let w: any = window;
    let loadHandler: HtmlEventHandler = ev => _.defer(NavigationMetricsCollector.collectMetrics);
    if (ss.isValue(w.addEventListener)) {
      window.addEventListener('load', loadHandler, false);
    } else
      if (ss.isValue(w.attachEvent)) {
        w.attachEvent('load', loadHandler);
      }
  }

  /**
   * Gets the timing metrics from the browser and logs them using the MetricsController
   */
  public static collectMetrics(): void {
    if (typeof window !== 'undefined' && typeof window.performance !== 'undefined' && typeof window.performance.timing !== 'undefined' && typeof MetricsEvent !== 'undefined') {
      NavigationMetricsCollector.navMetrics = ss.reinterpret(window.performance.timing);
      if (ss.reinterpret(NavigationMetricsName.navigationStart) in NavigationMetricsCollector.navMetrics) {
        let start: number = NavigationMetricsCollector.navMetrics[NavigationMetricsCollector.navigationMetricsOrder[0]];
        let metricArray: number[] = [];
        for (const name of NavigationMetricsCollector.navigationMetricsOrder) {
          let metric: number = NavigationMetricsCollector.navMetrics[name];
          metric = (metric === 0 ? 0 : metric - start);
          metricArray.push(metric);
        }
        let parameters: MetricsEventParameters = new MetricsEventParameters();
        let evt: MetricsEvent = new MetricsEvent(MetricsEventType.Navigation, MetricsSuites.Navigation, parameters);
        MetricsController.logEventInternalUse(evt);
      }
    }
  }
}

// Call the static constructor
NavigationMetricsCollector.__ctor();
