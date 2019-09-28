// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="BootstrapResponse.cs" company="Tableau Software">
//   This file is the copyrighted property of Tableau Software and is protected by registered patents and other
//   applicable U.S. and international laws and regulations.
//
//   Unlicensed use of the contents of this file is prohibited. Please refer to the NOTICES.txt file for further details.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------
namespace Tableau.JavaScript.Vql.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Enums;
    using TypeDefs;

    /// <summary>
    /// Corresponds to com.tableausoftware.model.vizql.service.session.BootstrapResponse
    /// </summary>
    [Imported]
    public class BootstrapResponse : Record
    {
        public SheetNameStr SheetName;
        public string LayoutId;
        public bool AllowSubscriptions;
        public bool AllowSubscribeOnDataPresent;
        public string NewClientNum;
        public string NewSessionId;
        public string WorkbookLocale;
        public SessionInitialState SessionState;
        public WorldUpdatePresModel WorldUpdate;
        public ConnectionAttemptInfoPresModel ConnectionAttemptInfo;
    }

    /// <summary>
    /// There is no generated presmodel for this and it's an "ad-hoc" contract that isn't enforced
    /// between the client and the server.
    /// </summary>
    [Imported]
    public class SecondaryBootstrapResponse : Record
    {
        public PresModelMapPresModel SecondaryInfo;
    }

    /// <summary>
    /// Corresponds to vizqlserver-service-protobuf-contract:src/proto/VizqlWorkerBootstrap.proto
    /// SessionInitialState
    /// </summary>
    [Imported]
    public class SessionInitialState : Record
    {
        [ScriptName("stateGuids")]
        public JsReadOnlyArray<string> StateGuids;

        [ScriptName("outcome")]
        public RecoveryOutcome RecoveryOutcome;
    }
}
