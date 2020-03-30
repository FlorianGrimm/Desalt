import { jQueryExtensions } from './jQueryExtensions';

import { Logger } from '../../CoreSlim/Logging/Logger';

import 'mscorlib';

/**
 * Contains utility methods for floating point values.
 */
export class DoubleUtil {
  public static readonly epsilon: number = Math.pow(2, -52);

  private static readonly onePlusEpsilon: number = 1 + DoubleUtil.epsilon;

  private static readonly upperBound: number = DoubleUtil.onePlusEpsilon;

  private static readonly lowerBound: number = 1 / DoubleUtil.onePlusEpsilon;

  private static readonly longMaxValue: number = 9.22337203685477E+18;

  private static readonly longMinValue: number = -DoubleUtil.longMaxValue;

  private static get log(): Logger {
    return Logger.lazyGetLogger(DoubleUtil);
  }

  /**
   * Returns whether d1 equals, or is within rounding error of equal, to d2
   * (For why this is needed, see https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/ )
   */
  public static isApproximatelyEqual(d1: number, d2: number): boolean {
    if (Math.abs(d1 - d2) < DoubleUtil.epsilon) {
      return true;
    }
    if (d1 === 0) {
      return false;
    }
    let intermediate: number = d2 / d1;
    if (DoubleUtil.lowerBound <= intermediate && intermediate <= DoubleUtil.upperBound) {
      return true;
    }
    return false;
  }

  /**
   * Returns whether d is approximately equal to 0
   */
  public static isApproximatelyZero(d: number): boolean {
    return DoubleUtil.isApproximatelyEqual(0, d);
  }

  /**
   * Returns whether d1 is less than d2, but not (approximately) equal
   * (i.e., the inverse of IsGreaterThanOrApproximatelyEqual).
   */
  public static isLessThanAndNotApproximatelyEqual(d1: number, d2: number): boolean {
    return d1 < d2 && !DoubleUtil.isApproximatelyEqual(d1, d2);
  }

  /**
   * Returns whether d1 is less than or approximately equal to d2
   * (i.e., the inverse of IsGreaterThanAndNotApproximatelyEqual).
   */
  public static isLessThanOrApproximatelyEqual(d1: number, d2: number): boolean {
    return d1 < d2 || DoubleUtil.isApproximatelyEqual(d1, d2);
  }

  /**
   * Returns whether d1 is greater than d2, but not (approximately) equal
   * (i.e., the inverse of IsLessThanOrApproximatelyEqual).
   */
  public static isGreaterThanAndNotApproximatelyEqual(d1: number, d2: number): boolean {
    return d1 > d2 && !DoubleUtil.isApproximatelyEqual(d1, d2);
  }

  /**
   * Returns whether d1 is greater than or approximately equal to d2
   * (i.e., the inverse of IsLessThanAndNotApproximatelyEqual).
   */
  public static isGreaterThanOrApproximatelyEqual(d1: number, d2: number): boolean {
    return d1 > d2 || DoubleUtil.isApproximatelyEqual(d1, d2);
  }

  /**
   * Returns n rounded to the number of significant figures indicated by numSigFigs
   */
  public static sigFigs(n: number, numSigFigs: number): number {
    if (n === 0 || numSigFigs === 0) {
      DoubleUtil.log.warn('Neither the input nor the number of significant figures can be 0');
      return n;
    }
    let mult: number = Math.pow(10, <number>numSigFigs - Math.floor(Math.log(Math.abs(n)) / Math.LN10) - 1);
    return ss.round(n * mult) / mult;
  }

  /**
   * Extension method on double to convert to a rounded int.
   */
  public static roundToInt(value: number): number {
    return ss.round(value);
  }

  /**
   * Returns double.Parse(s) if it yields a finite value, null otherwise
   */
  public static parseDouble(s: string): number | null {
    let val: number = number.parseFloat(s);
    return number.isFinite(val) ? val : <number | null>null;
  }

  /**
   * Returns double.Parse(s) if it yields a value, or the default otherwise
   */
  public static tryParseDouble(s: string, defaultValue: number): number {
    let val: number = number.parseFloat(s);
    if (number.isNaN(val) || !number.isFinite(val)) {
      return defaultValue;
    }
    return val;
  }

  /**
   * Returns true if string can be parsed into a double
   */
  public static isValidDouble(s: string): boolean {
    if (ss.isNullOrEmptyString(s)) {
      return false;
    }
    return $.isNumeric(s);
  }

  /**
   * Uses parseInt in javascript to parse string
   * This means that accuracy is not guaranteed to 64-bits
   * See:http://stackoverflow.com/questions/9297434/parseint-rounds-incorrectly
   */
  public static parse64BitInteger(s: string): number | null {
    let parsed: number;
    if (!number.tryParse(s, parsed)) {
      return <number | null>null;
    }
    let validDouble: number | null = DoubleUtil.parseValidDouble(s);
    if (validDouble > DoubleUtil.longMaxValue || validDouble < DoubleUtil.longMinValue) {
      return <number | null>null;
    }
    return parsed;
  }

  /**
   * Returns true if string can be parsed into a 32-bit integer
   * Returns false if string can be parsed, but overflows or underflows
   */
  public static isValid32BitInteger(s: string): boolean {
    if (ss.isNullOrEmptyString(s)) {
      return false;
    }
    let parsed: number | null = DoubleUtil.parse64BitInteger(s);
    if (parsed === null) {
      return false;
    }
    return (parsed <= number.maxValue) && (parsed >= number.minValue);
  }

  /**
   * Returns true if string can be parsed into a 64-bit integer
   */
  public static isValid64BitInteger(s: string): boolean {
    if (ss.isNullOrEmptyString(s)) {
      return false;
    }
    let parsed: number | null = DoubleUtil.parse64BitInteger(s);
    if (parsed === null) {
      return false;
    }
    return true;
  }

  /**
   * Returns a double if the string represents a double, NaN otherwise
   * Protects against double.Parse parsing of "1a" into "1"
   */
  public static parseValidDouble(s: string): number {
    if (ss.isNullOrEmptyString(s)) {
      return number.NaN;
    }
    if (!DoubleUtil.isValidDouble(s)) {
      return number.NaN;
    }
    return number.parseFloat(s);
  }

  public static multiplyBy100(num: number): number {
    return (num * 1000) / 10;
  }

  public static truncateTwoDecimalPlaces(num: number): number {
    return <number>(DoubleUtil.multiplyBy100(num)) / 100;
  }

  public static roundTwoDecimalPlaces(num: number): number {
    return ss.round(DoubleUtil.multiplyBy100(num)) / 100;
  }
}
