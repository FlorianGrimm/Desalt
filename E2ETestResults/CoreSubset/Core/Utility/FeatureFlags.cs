// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="FeatureFlags.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Tableau.JavaScript.Vql.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Tableau.JavaScript.Vql.TypeDefs;

    public static class FeatureFlags
    {
        private static JsDictionary<FeatureFlagIds, bool> defaultValues;

        public static bool IsEnabled(FeatureFlagIds featureFlagId)
        {
            if (TsConfig.Features != null && TsConfig.Features.ContainsKey(featureFlagId))
            {
                return TsConfig.Features[featureFlagId];
            }

            if (defaultValues != null && defaultValues.ContainsKey(featureFlagId))
            {
                return defaultValues[featureFlagId];
            }

            return false;
        }

        // used in tests (see FeatureFlagDefaults.cs)
        public static void SetDefaults(JsDictionary<FeatureFlagIds, bool> defaults)
        {
            defaultValues = defaults;
        }
    }
}
