import { FeatureFlagIds, tsConfig } from 'TypeDefs';

export class FeatureFlags {
  private static defaultValues: { [key: string]: boolean };

  public static isEnabled(featureFlagId: FeatureFlagIds): boolean {
    if (tsConfig.features !== null && ss.keyExists(tsConfig.features, featureFlagId)) {
      return tsConfig.features[featureFlagId];
    }
    if (FeatureFlags.defaultValues !== null && ss.keyExists(FeatureFlags.defaultValues, featureFlagId)) {
      return FeatureFlags.defaultValues[featureFlagId];
    }
    return false;
  }

  public static setDefaults(defaults: { [key: string]: boolean }): void {
    FeatureFlags.defaultValues = defaults;
  }
}
