import 'mscorlib';

/**
 * Contains utility methods for checking method parameters.
 */
export class Param {
  /**
   * Indicates whether to suppress browser alerts when an excpetion is thrown.
   */
  public static suppressAlerts: boolean = false;

  /**
   * Creates a standard {@link Exception} for reporting null or undefined argument values. Useful for
   * unit testing to verify that an exception gets thrown for an invalid parameter.
   * @param paramName The name of the argument that is null or undefined.
   * @returns An {@link Exception} (Error in JavaScript) containing a helpful message and errorInfo of
   * the following form:
   * {
   * paramName: paramName
   * }
   */
  public static createArgumentNullOrUndefinedException(paramName: string): ss.Exception {
    let ex: ss.Exception = new ss.Exception(paramName + ' is null or undefined.');
    (<any>ex).paramName = paramName;
    return ex;
  }

  /**
   * Verifies that the string parameter is not null, undefined, or whitespace. Throws an exception if the
   * parameter is not valid.
   * @param param The parameter to check.
   * @param paramName The name of the parameter.
   */
  public static verifyString(param: string, paramName: string): void {
    Param.verifyValue(param, paramName);
    if (param.trim().length === 0) {
      let ex: ss.Exception = new ss.Exception(paramName + ' contains only white space');
      (<any>ex).paramName = paramName;
      Param.showParameterAlert(ex);
      throw ex;
    }
  }

  /**
   * Verifies that the parameter is not null, undefined, or whitespace. Throws an exception if the parameter is
   * not valid.
   * @param param The parameter to check.
   * @param paramName The name of the parameter.
   */
  public static verifyValue(param: any, paramName: string): void {
    if (ss.isNullOrUndefined(param)) {
      let ex: ss.Exception = Param.createArgumentNullOrUndefinedException(paramName);
      Param.showParameterAlert(ex);
      throw ex;
    }
  }

  private static showParameterAlert(ex: ss.Exception): void {
    if (Param.suppressAlerts) {
      return;
    }
    try {
      throw ex;
    } catch (exceptionWithStack) {
      window.alert(Param.formatExceptionMessage(exceptionWithStack));
    }
  }

  /**
   * Adds stack frame information to the error message if it's present.
   * @param ex The exception to format.
   * @returns A formatted string useful for logging or displaying in an alert dialog.
   */
  private static formatExceptionMessage(ex: ss.Exception): string {
    let message: string;
    if (ss.isValue((<any>ex).stack)) {
      message = (<any>ex).stack;
    } else {
      message = ex.message;
    }
    return message;
  }
}
