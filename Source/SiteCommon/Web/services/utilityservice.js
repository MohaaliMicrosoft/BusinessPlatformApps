"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator.throw(value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var UtilityService = (function () {
    function UtilityService(mainservice) {
        this.MS = mainservice;
    }
    UtilityService.prototype.GenerateDailyTriggers = function () {
        var dailyTriggers = [];
        for (var i = 0; i < 24; i++) {
            dailyTriggers.push(i + ":00");
            dailyTriggers.push(i + ":30");
        }
        return dailyTriggers;
    };
    UtilityService.prototype.GetQueryParameter = function (id) {
        var regex = new RegExp('[?&]' + id.replace(/[\[\]]/g, '\\$&') + '(=([^&#]*)|&|#|$)');
        var results = regex.exec(window.location.href);
        return (!results || !results[2])
            ? ''
            : decodeURIComponent(results[2].replace(/\+/g, ' '));
    };
    UtilityService.prototype.GetQueryParameterFromUrl = function (name, url) {
        var regex = new RegExp('[?&]' + name.replace(/[\[\]]/g, '\\$&') + '(=([^&#]*)|&|#|$)');
        var results = regex.exec(url);
        return (!results || !results[2])
            ? ''
            : decodeURIComponent(results[2].replace(/\+/g, ' '));
    };
    UtilityService.prototype.GetRouteFromUrl = function () {
        var route = '';
        if (window.location.hash) {
            route = window.location.hash.substring(1);
        }
        return route;
    };
    UtilityService.prototype.GetUniqueId = function (characters) {
        return Math.random().toString(36).substr(2, characters + 2);
    };
    UtilityService.prototype.GetPropertiesForTelemtry = function () {
        var obj = {};
        obj.TemplateName = this.MS.NavigationService.appName;
        obj.FullUrl = window.location.href;
        obj.Origin = window.location.origin;
        obj.Host = window.location.host;
        obj.HostName = window.location.hostname;
        obj.PageNumber = this.MS.NavigationService.index;
        obj.Page = JSON.stringify(this.MS.NavigationService.pages[this.MS.NavigationService.index]);
        obj.RootSource = -this.MS.HttpService.isOnPremise ? 'MSI' : 'WEB';
        return obj;
    };
    UtilityService.prototype.HasInternetAccess = function () {
        var response = true;
        if (window.navigator && window.navigator.onLine !== null && window.navigator.onLine !== undefined) {
            response = window.navigator.onLine;
        }
        return response;
    };
    UtilityService.prototype.ValidateImpersonation = function (username, password, useCurrentUser) {
        return __awaiter(this, void 0, Promise, function* () {
            var isValid = true;
            var domain = '';
            if (!useCurrentUser && username.includes('\\')) {
                var usernameSplit = username.split('\\');
                domain = usernameSplit[0];
                username = usernameSplit[1];
            }
            else if (!useCurrentUser && username.length > 0) {
                this.MS.ErrorService.message = 'Username must be in <domain>\\<username> or <machinename>\\<username> format.';
                isValid = false;
            }
            if (isValid) {
                //TODO
                //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationDomain', domain);
                //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationUsername', username);
                //this.MS.DataService.AddToDataStore('Credentials', 'ImpersonationPassword', password);
                var response = yield this.MS.HttpService.executeAsync('Microsoft-ValidateNTCredential', {});
                isValid = response.IsSuccess;
                if (isValid) {
                    var responseAdmin = yield this.MS.HttpService.executeAsync('Microsoft-ValidateAdminPrivileges', {});
                    isValid = responseAdmin.IsSuccess;
                    if (isValid) {
                        var responseSecurity = yield this.MS.HttpService.executeAsync('Microsoft-ValidateSecurityOptions', {});
                        isValid = responseSecurity.IsSuccess;
                    }
                }
            }
            return isValid;
        });
    };
    // Add items to the session storage - should use DataStore where possible
    UtilityService.prototype.SaveItem = function (key, value) {
        var val = JSON.stringify(value);
        if (window.sessionStorage.getItem(key)) {
            window.sessionStorage.removeItem(key);
        }
        window.sessionStorage.setItem(key, val);
    };
    UtilityService.prototype.ClearSessionStorage = function () {
        window.sessionStorage.clear();
    };
    UtilityService.prototype.GetItem = function (key) {
        var item = JSON.parse(window.sessionStorage.getItem(key));
        return item;
    };
    UtilityService.prototype.RemoveItem = function (key) {
        window.sessionStorage.removeItem(key);
    };
    return UtilityService;
}());
exports.UtilityService = UtilityService;
