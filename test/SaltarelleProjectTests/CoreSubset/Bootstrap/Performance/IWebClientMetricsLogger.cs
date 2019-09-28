// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="IWebClientMetricsLogger.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Tableau.JavaScript.Vql.Bootstrap
{
    using System.Runtime.CompilerServices;

    [Imported]
    public interface IWebClientMetricsLogger
    {
        void LogEvent(MetricsEvent evt);
    }
}
