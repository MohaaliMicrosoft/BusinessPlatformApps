"use strict";
var LoggerService = (function () {
    function LoggerService(MainService) {
        this.MS = MainService;
        var snippet = {
            config: {
                instrumentationKey: '74bc59f2-6526-41b1-ab84-370532ec5d42'
            }
        };
        var init = new Microsoft.ApplicationInsights.Initialization(snippet);
        var applicationInsights = init.loadAppInsights();
        this.appInsights = applicationInsights;
        this.UserGenId = this.MS.UtilityService.GetItem('UserGeneratedId');
        this.SessionId = applicationInsights.context.session.id;
        this.UserId = applicationInsights.context.user.id;
        this.OperationId = applicationInsights.context.operation.id;
    }
    LoggerService.prototype.TrackStartRequest = function (request, uniqueId) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.Request = request;
        properties.UniqueId = uniqueId;
        properties.TemplateName = this.MS.NavigationService.appName;
        this.appInsights.trackEvent('UI-StartRequest-' + request, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackEndRequest = function (request, uniqueId, isSucess) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.Request = request;
        properties.UniqueId = uniqueId;
        properties.Sucess = isSucess;
        properties.TemplateName = this.MS.NavigationService.appName;
        this.appInsights.trackEvent('UI-EndRequest-' + request, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackEvent = function (requestName) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.TemplateName = this.MS.NavigationService.appName;
        this.appInsights.trackEvent('UI-' + requestName, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackDeploymentStepStartEvent = function (deploymentIndex, deploymentName) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.DeploymentIndex = deploymentIndex;
        properties.DeploymentName = deploymentName;
        properties.TemplateName = this.MS.NavigationService.appName;
        this.appInsights.trackEvent('UI-' + deploymentName + '-Start-' + deploymentIndex, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackDeploymentStepStoptEvent = function (deploymentIndex, deploymentName, isSucess) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.DeploymentIndex = deploymentIndex;
        properties.DeploymentName = deploymentName;
        properties.TemplateName = this.MS.NavigationService.appName;
        properties.Sucess = isSucess;
        this.appInsights.trackEvent('UI-' + deploymentName + '-End-' + deploymentIndex, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackDeploymentStart = function () {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.TemplateName = this.MS.NavigationService.appName;
        this.appInsights.trackEvent('UI-DeploymentStart', properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackDeploymentEnd = function (isSucess) {
        var properties = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.TemplateName = this.MS.NavigationService.appName;
        properties.Sucess = isSucess;
        this.appInsights.trackEvent('UI-DeploymentEnd', properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.GetPropertiesForTelemtry = function () {
        var obj = {};
        obj.AppName = this.MS.NavigationService.appName;
        obj.FullUrl = window.location.href;
        obj.Origin = window.location.origin;
        obj.Host = window.location.host;
        obj.HostName = window.location.hostname;
        obj.PageNumber = this.MS.NavigationService.index;
        if (this.MS.NavigationService.getCurrentSelectedPage()) {
            obj.Route = this.MS.NavigationService.getCurrentSelectedPage().RoutePageName;
            obj.PageName = this.MS.NavigationService.getCurrentSelectedPage().PageName;
            obj.PageModuleId = this.MS.NavigationService.getCurrentSelectedPage().Path.replace(/\\/g, "/");
            obj.PageDisplayName = this.MS.NavigationService.getCurrentSelectedPage().DisplayName;
        }
        obj.RootSource = -this.MS.HttpService.isOnPremise ? 'MSI' : 'WEB';
        return obj;
    };
    LoggerService.prototype.TrackPageView = function (page, url) {
        var properties = this.GetPropertiesForTelemtry();
        this.appInsights.trackPageView(page, url, properties);
        this.appInsights.flush();
    };
    LoggerService.prototype.TrackTrace = function (trace) {
        this.appInsights.trackTrace('UI-' + trace);
        this.appInsights.flush();
    };
    return LoggerService;
}());
exports.LoggerService = LoggerService;
