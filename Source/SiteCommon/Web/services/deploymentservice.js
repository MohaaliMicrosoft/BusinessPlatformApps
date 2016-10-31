"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator.throw(value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var actionresponse_1 = require('./actionresponse');
var DeploymentService = (function () {
    function DeploymentService(MainService) {
        this.actions = [];
        this.executingIndex = -1;
        this.executingAction = {};
        this.hasError = false;
        this.isFinished = false;
        this.message = '';
        this.MS = MainService;
    }
    DeploymentService.prototype.ExecuteActions = function () {
        return __awaiter(this, void 0, void 0, function* () {
            this.MS.LoggerService.TrackDeploymentStart();
            var lastActionStatus = actionresponse_1.ActionStatus.Success;
            for (var i = 0; i < this.actions.length && !this.hasError; i++) {
                this.executingIndex = i;
                this.executingAction = this.actions[i];
                var param = {};
                if (lastActionStatus !== actionresponse_1.ActionStatus.BatchWithState) {
                    param = this.actions[i].AdditionalParameters;
                }
                this.MS.LoggerService.TrackDeploymentStepStartEvent(i, this.actions[i].OperationName);
                var response = yield this.MS.HttpService.executeAsync(this.actions[i].OperationName, param);
                this.message = '';
                this.MS.LoggerService.TrackDeploymentStepStoptEvent(i, this.actions[i].OperationName, response.IsSuccess);
                if (!(response.IsSuccess)) {
                    this.hasError = true;
                    break;
                }
                //this.MS.DataService.AddObjectToDataStore('Deployment' + i, response.response);
                if (response.Status === actionresponse_1.ActionStatus.BatchWithState ||
                    response.Status === actionresponse_1.ActionStatus.BatchNoState) {
                    i = i - 1; // Loop again but dont add parameter back
                }
                lastActionStatus = response.Status;
            }
            if (!this.hasError) {
                this.executingAction = {};
                this.executingIndex++;
                this.message = 'Success';
            }
            else {
                this.message = 'Error';
            }
            this.MS.LoggerService.TrackDeploymentEnd(!this.hasError);
            this.isFinished = true;
        });
    };
    return DeploymentService;
}());
exports.DeploymentService = DeploymentService;
