/**
 * Extension methods for {@link Script}.
 */
export class ScriptEx {
  /**
   * .  Please use use {@link Script.Coalesce``1} if at all possible possible.
   * Equivalent to Script#'s Script.Value.  Only here for backwards compatibility.
   * typeparam T any type will do
   */
  public static value<T>(a: T, b: T): T {
    return ss.getDefaultValue(T);
  }

  public static arguments(): any {
    return null;
  }
}
