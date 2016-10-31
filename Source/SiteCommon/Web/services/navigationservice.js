"use strict";
var NavigationService = (function () {
    function NavigationService(MainService) {
        this.currentViewModel = null;
        this.index = -1;
        this.isOnline = true;
        this.pages = [];
        this.appName = '';
        this.isCurrentlyNavigating = false;
        this.MS = MainService;
    }
    NavigationService.prototype.init = function (pagesJson) {
        this.pages = pagesJson;
        if (this.pages && this.pages.length && this.pages.length > 0) {
            this.index = 0;
            for (var i = 1; i < this.pages.length; i++) {
                this.MS.Router.addRoute({
                    route: this.pages[i].RoutePageName.toLowerCase(),
                    name: this.pages[i].PageName,
                    moduleId: '.' + this.pages[i].Path.replace(/\\/g, "/"),
                    title: this.pages[i].DisplayName,
                    nav: true
                });
            }
            this.MS.Router.addRoute({
                route: '',
                name: this.pages[0].PageName,
                moduleId: '.' + this.pages[0].Path.replace(/\\/g, "/"),
                title: this.pages[0].DisplayName,
                nav: true
            });
            this.pages[0].isActive = true;
            this.pages[0].RoutePageName = '';
            this.MS.Router.refreshNavigation();
        }
        this.UpdateIndex();
        this.MS.DataStore.CurrentRoutePage = this.pages[this.index].RoutePageName.toLowerCase();
        this.MS.LoggerService.TrackPageView(this.GetCurrentRoutePath(), window.location.href);
    };
    NavigationService.prototype.GetCurrentRoutePath = function () {
        var history = this.MS.Router.history;
        var route = history.location.hash;
        var routePage = this.MS.NavigationService.appName + route.replace('#', '');
        if (routePage.endsWith('/')) {
            routePage += '//';
            routePage.replace('///', '');
        }
        return routePage;
    };
    NavigationService.prototype.GetRoute = function () {
        var history = this.MS.Router.history;
        var route = history.location.hash;
        return route.replace('#', '').replace('/', '');
    };
    NavigationService.prototype.UpdateIndex = function () {
        var routePageName = this.GetRoute();
        for (var i = 0; i < this.pages.length; i++) {
            if (this.pages[i].RoutePageName.toLowerCase() === routePageName.toLowerCase()) {
                this.index = i;
            }
        }
        for (var i = 0; i < this.pages.length; i++) {
            this.pages[i].isActive = i === this.index;
            this.pages[i].isComplete = i < this.index;
        }
        return this.index;
    };
    NavigationService.prototype.NavigateNext = function () {
        this.UpdateIndex();
        if (this.index >= this.pages.length - 1) {
            return;
        }
        this.index = this.index + 1;
        this.NavigateToIndex();
    };
    NavigationService.prototype.NavigateBack = function () {
        this.UpdateIndex();
        if (this.index == 0) {
            return;
        }
        this.index = this.index - 1;
        this.NavigateToIndex();
    };
    NavigationService.prototype.JumpTo = function (index) {
        this.index = index;
        this.NavigateToIndex();
    };
    NavigationService.prototype.NavigateToIndex = function () {
        // do not update index here
        // Initialise the page
        this.MS.DataStore.CurrentRoutePage = this.pages[this.index].RoutePageName.toLowerCase();
        // The index is set to the next step
        this.MS.Router.navigate('#/' + this.pages[this.index].RoutePageName.toLowerCase());
        this.MS.Router.refreshNavigation();
        this.UpdateIndex();
        this.MS.LoggerService.TrackPageView(this.appName + '/' + this.pages[this.index].RoutePageName.toLowerCase(), window.location.href);
    };
    NavigationService.prototype.getCurrentSelectedPage = function () {
        return this.pages[this.index];
    };
    NavigationService.prototype.getIndex = function () {
        return this.index;
    };
    return NavigationService;
}());
exports.NavigationService = NavigationService;
