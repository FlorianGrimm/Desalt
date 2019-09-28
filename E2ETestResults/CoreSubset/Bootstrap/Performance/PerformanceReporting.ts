import { console } from 'NativeJsTypeDefs';

/**
 * Documentation here https://developer.mozilla.org/en-US/docs/Web/API/User_Timing_API
 */
export class performance {
  public static mark(markName: string): void { }

  public static measure(measureName: string, startMarkName: string, endMarkName: string): void { }
}

/**
 * Safe wrapper for the UserTiming API, performance. If not supported, commands become no-ops.
 */
export class PerformanceReporting {
  public static readonly supportsPerfApi: boolean;

  // Converted from the C# static constructor - it would be good to convert this
  // block to inline initializations.
  public static __ctor() {
    let windowAsDynamic: any = window.window;
    PerformanceReporting.supportsPerfApi = (windowAsDynamic['performance'] !== null && windowAsDynamic['performance']['mark'] !== null);
  }

  public static mark(markName: string): void {
    if (PerformanceReporting.supportsPerfApi) {
      performance.mark(markName);
    }
  }

  /**
   */
  public static measure(measureName: string, startMarkName: string, endMarkName: string): void {
    if (PerformanceReporting.supportsPerfApi) {
      try {
        performance.measure(measureName, startMarkName, endMarkName);
      } catch (e) {
        console.error('Missing a performance mark', e);
      }
    }
  }
}

// Call the static constructor
PerformanceReporting.__ctor();
