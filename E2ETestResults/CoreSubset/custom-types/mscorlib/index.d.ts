// Type definitions for @saltarelle-mscorlib
// Project: [LIBRARY URL]
// Definitions by: Justin Rockwood <jrockwood@github.com>
// Definitions: https://github.com/DefinitelyTyped/DefinitelyTyped

declare namespace ss {
  function isValue(o: any): boolean;

  ///////////////////////////////////////////////////////////////////////////////
  // Object Extensions

  function keyExists(d: { [key: string]: any }, key: string): boolean;

  ///////////////////////////////////////////////////////////////////////////////
  // Type System Implementation

  function getInstanceType(instance: any): Function;

  ///////////////////////////////////////////////////////////////////////////////
  // ICollection

  function clear<T>(obj: T[]): void;
  function remove<T>(obj: T[], item: T): boolean;

  ///////////////////////////////////////////////////////////////////////////////
  // IList

  function indexOf<T>(obj: T[], item: T): number;
}
