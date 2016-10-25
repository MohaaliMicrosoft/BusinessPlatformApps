import MainService from './mainservice';
import { ActionResponse } from './actionresponse';
import { ActionStatus } from './actionresponse';

export class DeploymentService {
    MS: MainService;
    actions: any[] = [];
    executingIndex: number = -1;
    executingAction: any = {};
    hasError: boolean = false;
    isFinished: boolean = false;
    message: string = '';

    constructor(MainService) {
        this.MS = MainService;
    }

    async ExecuteActions() {
        this.MS.LoggerService.TrackDeploymentStart();
        let lastActionStatus: ActionStatus = ActionStatus.Success;

        for (let i = 0; i < this.actions.length && !this.hasError; i++) {
            this.executingIndex = i;
            this.executingAction = this.actions[i];

            let param: any = {};
            if (lastActionStatus !== ActionStatus.BatchWithState) {
                param = this.actions[i].AdditionalParameters;
            }

            this.MS.LoggerService.TrackDeploymentStepStartEvent(i, this.actions[i].OperationName);
            let response = await this.MS.HttpService.Execute(this.actions[i].OperationName, param);
            this.message = '';

            this.MS.LoggerService.TrackDeploymentStepStoptEvent(i, this.actions[i].OperationName, response.isSuccess);


            if (!(response.isSuccess)) {
                this.hasError = true;
                break;
            }

            //this.MS.DataService.AddObjectToDataStore('Deployment' + i, response.response);
            if (response.responseStatus === ActionStatus.BatchWithState ||
                response.responseStatus === ActionStatus.BatchNoState) {
                i = i - 1; // Loop again but dont add parameter back
            }

            lastActionStatus = response.responseStatus;
        }

        if (!this.hasError) {
            this.executingAction = {};
            this.executingIndex++;
            this.message = 'Success';
        } else {
            this.message = 'Error';
        }

        this.MS.LoggerService.TrackDeploymentEnd(!this.hasError);
        this.isFinished = true;
    }
}