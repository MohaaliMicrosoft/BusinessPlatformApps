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

    UseImpersonation(): boolean {
        let useImpersonationIfAvailable: boolean = false;
        try {
            //let impersonationUsername: string = this.MS.DataService.GetItemFromDataStore('Credentials', 'ImpersonationUsername');
            useImpersonationIfAvailable = impersonationUsername && impersonationUsername.length > 0;
        } catch (checkImpersonationException) {
            // Impersonation is not being used
        }
        return useImpersonationIfAvailable;
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
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationDomain', domain);
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationUsername', username);
            //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationPassword', password);

            let response = await this.MS.HttpService.Execute('Microsoft-ValidateNTCredential', {});
            isValid = response.isSuccess;

            if (isValid) {
                let responseAdmin = await this.MS.HttpService.Execute('Microsoft-ValidateAdminPrivileges', {});
                isValid = responseAdmin.isSuccess;
                if (isValid) {
                    let responseSecurity = await this.MS.HttpService.Execute('Microsoft-ValidateSecurityOptions', {});
                    isValid = responseSecurity.isSuccess;
                }
            }
        }

        return isValid;
    }

    ValidatePassword(pwd: string, pwd2: string, length: number): string {
        let passwordError: string = '';
        if (pwd !== pwd2) {
            passwordError = 'Passwords do not match.';
        } else if (length && pwd.length < length) {
            passwordError = 'Password must be at least eight characters long.';
        } else if ((/\s/g).test(pwd)) {
            passwordError = 'Password should not contain spaces.';
        } else if (!(/[A-Z]/).test(pwd) || (/^[a-zA-Z0-9]*$/).test(pwd)) {
            passwordError = 'Password must contain at least one uppercase character and at least one special character.';
        }
        return passwordError;
    }

    ValidateUsername(username: string, invalidUsernames: string[], usernameText: string): string {
        let usernameError: string = '';
        if ((/\s/g).test(username)) {
            usernameError = `${usernameText} should not contain spaces.`;
        } else if (username.length > 63) {
            usernameError = `${usernameText} must not be longer than 63 characters.`;
        } else if (invalidUsernames.indexOf(username.toLowerCase()) > -1) {
            usernameError = `${usernameText} cannot be a reserved SQL system name.`;
        } else if (!(/^[a-zA-Z0-9\-]+$/).test(username)) {
            usernameError = `${usernameText} must only contain alphanumeric characters or hyphens.`;
        } else if (username[0] === '-' || username[username.length - 1] === '-') {
            usernameError = `${usernameText} must not start or end with a hyphen.`;
        }
        return usernameError;
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