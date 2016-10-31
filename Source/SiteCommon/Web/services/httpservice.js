"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator.throw(value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var actionrequest_1 = require('./actionrequest');
var actionresponse_1 = require('./actionresponse');
var HttpService = (function () {
    function HttpService(MainService, HttpClient) {
        this.baseUrl = 'http://localhost:42387/api/';
        this.isOnPremise = false;
        this.isServiceBusy = false;
        if (window.location.href.startsWith('http://localhost') || window.location.href.startsWith('https://localhost')) {
            this.baseUrl = 'http://localhost:2305/api/';
        }
        else {
            var url = window.location.href;
            if (url.includes('bpsolutiontemplates')) {
                this.baseUrl = 'https://bpstservice.azurewebsites.net/api/';
            }
            else {
                url = url.replace('bpst', 'bpstservice');
                var splitUrls = url.split('/');
                this.baseUrl = splitUrls[0] + '//' + splitUrls[2] + '/api/';
            }
        }
        this.MS = MainService;
        this.HttpClient = HttpClient;
        var $window = window;
        if ($window && $window.command) {
            this.command = $window.command;
            this.isOnPremise = true;
        }
    }
    HttpService.prototype.Close = function () {
        this.command.close();
    };
    HttpService.prototype.getApp = function (name) {
        return __awaiter(this, void 0, void 0, function* () {
            var response = null;
            var uniqueId = this.MS.UtilityService.GetUniqueId(20);
            this.MS.LoggerService.TrackStartRequest('GetApp-name', uniqueId);
            if (this.isOnPremise) {
                response = yield this.command.gettemplate(this.MS.LoggerService.UserId, this.MS.LoggerService.UserGenId, '', this.MS.LoggerService.OperationId, uniqueId, name);
            }
            else {
                response = yield this.getRequestObject('get', "/App/" + name).send();
                response = response.response;
            }
            if (!response) {
                response = '{}';
            }
            this.MS.LoggerService.TrackEndRequest('GetTemplate-name', uniqueId, true);
            var responseParsed = JSON.parse(response);
            return responseParsed;
        });
    };
    HttpService.prototype.executeAsync = function (method, content) {
        return __awaiter(this, void 0, Promise, function* () {
            this.isServiceBusy = true;
            var actionResponse = null;
            this.MS.ErrorService.Clear();
            var uniqueId = this.MS.UtilityService.GetUniqueId(20);
            try {
                var actionRequest = new actionrequest_1.ActionRequest(content, this.MS.DataStore);
                this.MS.LoggerService.TrackStartRequest(method, uniqueId);
                var response = null;
                if (this.isOnPremise) {
                    response = yield this.command.executeaction(this.MS.LoggerService.UserId, this.MS.LoggerService.UserGenId, '', this.MS.LoggerService.OperationId, uniqueId, this.MS.NavigationService.appName, method, JSON.stringify(actionRequest));
                }
                else {
                    response = yield this.getRequestObject('post', "/action/" + method, actionRequest).send();
                    response = response.response;
                }
                var responseParsed = JSON.parse(response);
                actionResponse = responseParsed;
                actionResponse.Status = actionresponse_1.ActionStatus[responseParsed.Status];
                this.MS.LoggerService.TrackEndRequest(method, uniqueId, !actionResponse.IsSuccess);
                this.MS.DataStore.loadDataStoreFromJson(actionResponse.DataStore);
                // Handle any errors here
                if (actionResponse.Status === actionresponse_1.ActionStatus.Failure || actionResponse.Status === actionresponse_1.ActionStatus.FailureExpected) {
                    this.MS.ErrorService.details = actionResponse.ExceptionDetail.AdditionalDetailsErrorMessage + " --- Action Failed " + method + " --- Error ID:(" + this.MS.LoggerService.UserGenId + ")";
                    this.MS.ErrorService.logLocation = actionResponse.ExceptionDetail.LogLocation;
                    this.MS.ErrorService.message = actionResponse.ExceptionDetail.FriendlyErrorMessage;
                    this.MS.ErrorService.showContactUs = actionResponse.Status === actionresponse_1.ActionStatus.Failure;
                }
                else {
                    this.MS.ErrorService.Clear();
                }
            }
            catch (e) {
                this.MS.ErrorService.message = 'Unknown Error has occured';
                this.MS.ErrorService.showContactUs = true;
                throw e;
            }
            finally {
                this.isServiceBusy = false;
            }
            return actionResponse;
        });
    };
    HttpService.prototype.executeAsyncWithImpersonation = function (method, content) {
        return __awaiter(this, void 0, Promise, function* () {
            var body = {};
            if (content) {
                body = content;
            }
            body.ImpersonateAction = true;
            return this.executeAsync(method, content);
        });
    };
    HttpService.prototype.getRequestObject = function (method, relatgiveUrl, body) {
        if (body === void 0) { body = {}; }
        var uniqueId = this.MS.UtilityService.GetUniqueId(20);
        var request = this.HttpClient.createRequest(relatgiveUrl);
        request = request
            .withBaseUrl(this.baseUrl)
            .withHeader('Content-Type', 'application/json; charset=utf-8')
            .withHeader('UserGeneratedId', this.MS.LoggerService.UserGenId)
            .withHeader('OperationId', this.MS.LoggerService.OperationId)
            .withHeader('SessionId', this.MS.LoggerService.appInsights.context.session.id)
            .withHeader('UserId', this.MS.LoggerService.UserId)
            .withHeader('TemplateName', this.MS.NavigationService.appName)
            .withHeader('UniqueId', uniqueId);
        if (method === 'get') {
            request = request.asGet();
        }
        else {
            request = request
                .asPost()
                .withContent(JSON.stringify(body));
        }
        return request;
    };
    return HttpService;
}());
exports.HttpService = HttpService;
