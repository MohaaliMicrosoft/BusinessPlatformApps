import MainService from './mainservice';
import { ActionResponse } from './actionresponse';
import { ActionStatus } from './actionresponse';
import { HttpClient } from 'aurelia-http-client';

export class HttpService {
    baseUrl: string = 'http://localhost:42387/api/';
    command: any;
    HttpClient: HttpClient;
    isOnPremise: boolean = false;
    isServiceBusy: boolean = false;
    MS: MainService;

    constructor(MainService, HttpClient) {

        if (window.location.href.startsWith('http://localhost') || window.location.href.startsWith('https://localhost')) {
            this.baseUrl = 'http://localhost:42387/api/';
        } else {
            let url = window.location.href;
            if (url.includes('bpsolutiontemplates')) {
                this.baseUrl = 'https://bpstservice.azurewebsites.net/api/';
            } else {
                url = url.replace('bpst', 'bpstservice');
                let splitUrls = url.split('/');
                this.baseUrl = splitUrls[0] + '//' + splitUrls[2] + '/api/';
            }
        }

        this.MS = MainService;
        this.HttpClient = HttpClient;

        let $window: any = window;
        if ($window && $window.command) {
            this.command = $window.command;
            this.isOnPremise = true;
        }
    }

    Close() {
        this.command.close();
    }

    async GetApp(name) {
        var response = null;
        let uniqueId = this.MS.UtilityService.GetUniqueId(20);
        this.MS.LoggerService.TrackStartRequest('GetApp-name', uniqueId);
        if (this.isOnPremise) {
            response = await this.command.gettemplate(this.MS.LoggerService.UserId,
                this.MS.LoggerService.UserGenId, '', this.MS.LoggerService.OperationId,
                uniqueId, name);
        } else {
            response = await this.HttpClient.createRequest(`/App/${name}`)
                .asGet()
                .withBaseUrl(this.baseUrl)
                .withHeader('Content-Type', 'application/json; charset=utf-8')
                .withHeader('UserGeneratedId', this.MS.LoggerService.UserGenId)
                .withHeader('OperationId', this.MS.LoggerService.OperationId)
                .withHeader('SessionId', this.MS.LoggerService.appInsights.context.session.id)
                .withHeader('UserId', this.MS.LoggerService.UserId)
                .withHeader('TemplateName', name)
                .withHeader('UniqueId', uniqueId)
                .send();
            response = response.response;
        }
        if (!response) {
            response = '{}';
        }

        this.MS.LoggerService.TrackEndRequest('GetTemplate-name', uniqueId, true);
        let responseParsed = JSON.parse(response);
        return responseParsed;
    }

    async Execute(method, content) {
        this.isServiceBusy = true;

        this.MS.ErrorService.details = '';
        this.MS.ErrorService.logLocation = '';
        this.MS.ErrorService.message = '';
        this.MS.ErrorService.showContactUs = false;

        let uniqueId = this.MS.UtilityService.GetUniqueId(20);
        let commonRequestBody: any = this.MS.DataService.GetDataStore();

        if (content) {
            for (let prop in content) {
                commonRequestBody[prop] = content[prop];
            }
        }
        commonRequestBody.uniqueId = uniqueId;
        this.MS.LoggerService.TrackStartRequest(method, uniqueId);
        var response = null;
        if (this.isOnPremise) {
            response = await this.command.executeaction(this.MS.LoggerService.UserId, this.MS.LoggerService.UserGenId,
                '', this.MS.LoggerService.OperationId, uniqueId,
                this.MS.NavigationService.appName, method, JSON.stringify(commonRequestBody));
        } else {
            response = await this.HttpClient.createRequest(`${this.baseUrl}/action/${method}`)
                .asPost()
                .withContent(JSON.stringify(commonRequestBody))
                .withHeader('Content-Type', 'application/json; charset=utf-8')
                .withHeader('UserGeneratedId', this.MS.LoggerService.UserGenId)
                .withHeader('OperationId', this.MS.LoggerService.OperationId)
                .withHeader('SessionId', this.MS.LoggerService.appInsights.context.session.id)
                .withHeader('UserId', this.MS.LoggerService.UserId)
                .withHeader('TemplateName', this.MS.NavigationService.appName)
                .withHeader('UniqueId', uniqueId)
                .send();
            response = response.response;
        }

        this.isServiceBusy = false;

        let actionResponse: ActionResponse = new ActionResponse(response);
        this.MS.LoggerService.TrackEndRequest(method, uniqueId, actionResponse.responseStatus === ActionStatus.Failure);

        if (actionResponse.responseStatus === ActionStatus.Failure || actionResponse.responseStatus === ActionStatus.FailureExpected) {
            if (!actionResponse.additionalDetailsErrorMessage) {
                this.MS.ErrorService.details = `Action Failed ${method} --- Error ID:(${this.MS.LoggerService.UserGenId})`;
            } else {
                this.MS.ErrorService.details = `${actionResponse.additionalDetailsErrorMessage} --- Action Failed ${method} --- Error ID:(${this.MS.LoggerService.UserGenId})`;
            }

            this.MS.ErrorService.logLocation = actionResponse.logLocation;
            this.MS.ErrorService.message = actionResponse.friendlyErrorMessage;
            this.MS.ErrorService.showContactUs = actionResponse.responseStatus === ActionStatus.Failure;
        } else {
            this.MS.ErrorService.details = '';
            this.MS.ErrorService.logLocation = '';
            this.MS.ErrorService.message = '';
            this.MS.ErrorService.showContactUs = false;
        }

        return actionResponse;
    }

    async ExecuteWithImpersonation(method, content) {
        let body: any = {};

        if (content) {
            body = content;
        }

        body.ImpersonateAction = true;
        return this.Execute(method, content);
    }
}