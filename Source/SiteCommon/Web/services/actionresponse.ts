export class ActionResponse {
    response: any;
    isSuccess: boolean;
    errorMessage: any;
    logLocation: string;
    responseStatus: ActionStatus;
    friendlyErrorMessage: string;
    friendlyMessageCode: string;
    additionalDetailsErrorMessage: string;

    constructor(response) {
        let responseParsed: any = JSON.parse(response);

        this.isSuccess = !responseParsed.Status.startsWith('failure');

        if (responseParsed.Status === 'failure') {
            this.responseStatus = ActionStatus.Failure;
        }

        if (responseParsed.Status === 'failureexpected') {
            this.responseStatus = ActionStatus.FailureExpected;
        }

        if (responseParsed.Status === 'batchnostate') {
            this.responseStatus = ActionStatus.BatchNoState;
        }

        if (responseParsed.Status === 'batchwithstate') {
            this.responseStatus = ActionStatus.BatchWithState;
        }

        if (responseParsed.Status === 'userinteractionrequired') {
            this.responseStatus = ActionStatus.UserInteractionRequired;
        }

        if (responseParsed.Status === 'success') {
            this.responseStatus = ActionStatus.Success;
        }

        this.response = responseParsed.Response;
        this.friendlyErrorMessage = responseParsed.FriendlyErrorMessage;
        this.friendlyMessageCode = responseParsed.FriendlyMessageCode;
        this.additionalDetailsErrorMessage = responseParsed.AdditionalDetailsErrorMessage;
        this.logLocation = responseParsed.LogLocation;
    }
}

export enum ActionStatus {
    Failure,
    FailureExpected,
    BatchNoState,
    BatchWithState,
    UserInteractionRequired,
    Success,
}