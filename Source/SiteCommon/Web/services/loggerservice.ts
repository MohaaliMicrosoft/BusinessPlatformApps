import MainService from './mainservice';

export class LoggerService {
    MS: MainService;
    appInsights: Microsoft.ApplicationInsights.AppInsights;
    UserGenId: string;
    OperationId: string;
    UserId: string;
    SessionId: string;

    constructor(MainService) {
        this.MS = MainService;
        var snippet: any = {
            config: {
                instrumentationKey: '74bc59f2-6526-41b1-ab84-370532ec5d42'
            }
        };

        var init = new Microsoft.ApplicationInsights.Initialization(snippet);
        var applicationInsights = init.loadAppInsights();
        this.appInsights = applicationInsights;

        this.UserGenId = this.MS.DataService.GetItem('UserGeneratedId');
        this.SessionId = applicationInsights.context.session.id;
        this.UserId = applicationInsights.context.user.id;
        this.OperationId = applicationInsights.context.operation.id;
    }

    TrackStartRequest(request, uniqueId) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.Request = request;
        properties.UniqueId = uniqueId;
        properties.TemplateName = this.MS.NavigationService.templateName;
        this.appInsights.trackEvent('UI-StartRequest-' + request, properties);
        this.appInsights.flush();
    }

    TrackEndRequest(request, uniqueId, isSucess) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.Request = request;
        properties.UniqueId = uniqueId;
        properties.Sucess = isSucess;
        properties.TemplateName = this.MS.NavigationService.templateName;
        this.appInsights.trackEvent('UI-EndRequest-' + request, properties);
        this.appInsights.flush();
    }

    TrackEvent(requestName) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.TemplateName = this.MS.NavigationService.templateName;
        this.appInsights.trackEvent('UI-' + requestName, properties);
        this.appInsights.flush();
    }

    TrackDeploymentStepStartEvent(deploymentIndex, deploymentName) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.DeploymentIndex = deploymentIndex;
        properties.DeploymentName = deploymentName;
        properties.TemplateName = this.MS.NavigationService.templateName;
        this.appInsights.trackEvent('UI-' + deploymentName + '-Start-' + deploymentIndex, properties);
        this.appInsights.flush();
    }

    TrackDeploymentStepStoptEvent(deploymentIndex, deploymentName, isSucess) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.DeploymentIndex = deploymentIndex;
        properties.DeploymentName = deploymentName;
        properties.TemplateName = this.MS.NavigationService.templateName;
        properties.Sucess = isSucess;
        this.appInsights.trackEvent('UI-' + deploymentName + '-End-' + deploymentIndex, properties);
        this.appInsights.flush();
    }

    TrackDeploymentStart() {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;

        properties.TemplateName = this.MS.NavigationService.templateName;
        this.appInsights.trackEvent('UI-DeploymentStart', properties);
        this.appInsights.flush();
    }

    TrackDeploymentEnd(isSucess) {
        let properties: any = this.GetPropertiesForTelemtry();
        properties.UserGenId = this.UserGenId;
        properties.SessionId = this.SessionId;
        properties.UserId = this.UserId;
        properties.OperationId = this.OperationId;
        properties.TemplateName = this.MS.NavigationService.templateName;
        properties.Sucess = isSucess;
        this.appInsights.trackEvent('UI-DeploymentEnd', properties);
        this.appInsights.flush();
    }

    GetPropertiesForTelemtry(): any {
        let obj: any = {};
        obj.TemplateName = this.MS.NavigationService.templateName;
        obj.FullUrl = window.location.href;
        obj.Origin = window.location.origin;
        obj.Host = window.location.host;
        obj.HostName = window.location.hostname;
        obj.PageNumber = this.MS.NavigationService.index;
        obj.Page = JSON.stringify(this.MS.NavigationService.pages[this.MS.NavigationService.index]);
        obj.RootSource = - this.MS.HttpService.isOnPremise ? 'MSI' : 'WEB';
        return obj;
    }

    TrackPageView(page, url) {
        let properties: any = this.GetPropertiesForTelemtry();
        this.appInsights.trackPageView(page, url, properties);
        this.appInsights.flush();
    }

    TrackTrace(trace: string) {
        this.appInsights.trackTrace('UI-' + trace);
        this.appInsights.flush();
    }
}