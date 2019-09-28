import { ConnectionAttemptInfoPresModel, PresModelMapPresModel, RecoveryOutcome, WorldUpdatePresModel } from 'TypeDefs';

/**
 * Corresponds to com.tableausoftware.model.vizql.service.session.BootstrapResponse
 */
export class BootstrapResponse extends Object {
  public sheetName: Object;

  public layoutId: string;

  public allowSubscriptions: boolean;

  public allowSubscribeOnDataPresent: boolean;

  public newClientNum: string;

  public newSessionId: string;

  public workbookLocale: string;

  public sessionState: SessionInitialState;

  public worldUpdate: WorldUpdatePresModel;

  public connectionAttemptInfo: ConnectionAttemptInfoPresModel;
}

/**
 * There is no generated presmodel for this and it's an "ad-hoc" contract that isn't enforced
 * between the client and the server.
 */
export class SecondaryBootstrapResponse extends Object {
  public secondaryInfo: PresModelMapPresModel;
}

/**
 * Corresponds to vizqlserver-service-protobuf-contract:src/proto/VizqlWorkerBootstrap.proto
 * SessionInitialState
 */
export class SessionInitialState extends Object {
  public stateGuids: string[];

  public outcome: RecoveryOutcome;
}
