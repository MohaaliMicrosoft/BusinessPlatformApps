import MainService from './mainservice';

export class UtilityService {
    MS: MainService;

    constructor(mainservice) {
        this.MS = mainservice;
    }
    
    GenerateDailyTriggers(): string[] {
        let dailyTriggers: string [] = [];
        for (let i = 0; i < 24; i++) {
            dailyTriggers.push(`${i}:00`);
            dailyTriggers.push(`${i}:30`);
        }
        return dailyTriggers;
    }

    GetQueryParameter(id) {
        var regex = new RegExp('[?&]' + id.replace(/[\[\]]/g, '\\$&') + '(=([^&#]*)|&|#|$)');
        var results = regex.exec(window.location.href);
        return (!results || !results[2])
            ? ''
            : decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    GetQueryParameterFromUrl(name, url) {
        var regex = new RegExp('[?&]' + name.replace(/[\[\]]/g, '\\$&') + '(=([^&#]*)|&|#|$)');
        var results = regex.exec(url);
        return (!results || !results[2])
            ? ''
            : decodeURIComponent(results[2].replace(/\+/g, ' '));
    }

    GetRouteFromUrl() {
        let route = '';
        if (window.location.hash) {
            route = window.location.hash.substring(1);
        }
        return route;
    }

    GetUniqueId(characters:number) :string {
        return Math.random().toString(36).substr(2, characters +2);
    }

    GetPropertiesForTelemtry(): any {
        let obj: any = {};
        obj.TemplateName = this.MS.NavigationService.appName;
        obj.FullUrl = window.location.href;
        obj.Origin = window.location.origin;
        obj.Host = window.location.host;
        obj.HostName = window.location.hostname;
        obj.PageNumber = this.MS.NavigationService.index;
        obj.Page = JSON.stringify(this.MS.NavigationService.pages[this.MS.NavigationService.index]);
        obj.RootSource = - this.MS.HttpService.isOnPremise ? 'MSI' : 'WEB';
        return obj;
    }

    HasInternetAccess() {
        let response = true;
        if (window.navigator && window.navigator.onLine !== null && window.navigator.onLine !== undefined) {
            response = window.navigator.onLine;
        }
        return response;
    }

    async ValidateImpersonation(username: string, password: string, useCurrentUser: boolean): Promise<boolean> {
        let isValid: boolean = true;

        let domain: string = '';

        if (!useCurrentUser && username.includes('\\')) {
            let usernameSplit: string[] = username.split('\\');
            domain = usernameSplit[0];
            username = usernameSplit[1];
        } else if (!useCurrentUser && username.length > 0) {
            this.MS.ErrorService.message = 'Username must be in <domain>\\<username> or <machinename>\\<username> format.';
            isValid = false;
        }

        if (isValid) {
            //TODO
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationDomain', domain);
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationUsername', username);
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationPassword', password);

            let response = await this.MS.HttpService.executeAsync('Microsoft-ValidateNTCredential', {});
            isValid = response.IsSuccess;

            if (isValid) {
                let responseAdmin = await this.MS.HttpService.executeAsync('Microsoft-ValidateAdminPrivileges', {});
                isValid = responseAdmin.IsSuccess;
                if (isValid) {
                    let responseSecurity = await this.MS.HttpService.executeAsync('Microsoft-ValidateSecurityOptions', {});
                    isValid = responseSecurity.IsSuccess;
                }
            }
        }

        return isValid;
    }

   

    // Add items to the session storage - should use DataStore where possible
    SaveItem(key, value) {
        let val = JSON.stringify(value);
        if (window.sessionStorage.getItem(key)) {
            window.sessionStorage.removeItem(key);
        }
        window.sessionStorage.setItem(key, val);
    }

    ClearSessionStorage() {
        window.sessionStorage.clear();
    }

    GetItem(key) {
        let item = JSON.parse(window.sessionStorage.getItem(key));
        return item;
    }

    RemoveItem(key) {
        window.sessionStorage.removeItem(key);
    }
}