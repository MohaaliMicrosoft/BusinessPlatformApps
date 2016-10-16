import { Aurelia } from 'aurelia-framework';
import { inject } from 'aurelia-framework';
import MainService from './mainservice';

export class NavigationService {
    currentViewModel: any = null;
    index: number = -1;
    isOnline: boolean = true;
    MS: MainService;
    pages: any[] = [];
    templateName: string = '';

    constructor(MainService) {
        this.MS = MainService;
    }

    init(pagesJson) {
        this.pages = pagesJson;

        if (this.pages && this.pages.length && this.pages.length > 0) {
            this.index = 0;

            for (let i = 1; i < this.pages.length; i++) {
                this.MS.Router.addRoute({
                    route: this.pages[i].RoutePageName.toLowerCase(),
                    name: this.pages[i].PageName,
                    moduleId: './' + this.pages[i].Path,
                    title: this.pages[i].DisplayName,
                    nav: true
                });
            }

            this.MS.Router.addRoute({
                route: '',
                name: this.pages[0].PageName,
                moduleId: './' + this.pages[0].Path,
                title: this.pages[0].DisplayName,
                nav: true
            });

            this.pages[0].isActive = true;
            this.pages[0].RoutePageName = '';
            this.MS.Router.refreshNavigation();
        }

        this.UpdateIndex();
        this.MS.LoggerService.TrackPageView(this.GetCurrentRoutePath(), window.location.href);
    }

    GetCurrentRoutePath(): string {
        let history: any = this.MS.Router.history;
        let route: string = history.location.hash;
        let routePage = this.MS.NavigationService.templateName + route.replace('#', '');
        if (routePage.endsWith('/')) {
            routePage += '//';
            routePage.replace('///', '');
        }

        return routePage;
    }

    GetRoute(): string {
        let history: any = this.MS.Router.history;
        let route: string = history.location.hash;
        return route.replace('#', '').replace('/','');
    }

    UpdateIndex() {
        let routePageName = this.GetRoute();
        for (let i = 0; i < this.pages.length; i++) {
            if (this.pages[i].RoutePageName.toLowerCase() === routePageName.toLowerCase()) {
                this.index = i;
            }
        }

        for (let i = 0; i < this.pages.length; i++) {
            this.pages[i].isActive = i === this.index;
            this.pages[i].isComplete = i < this.index;
        }
        return this.index;
    }

    NavigateNext() {
        this.UpdateIndex();
        if (this.index >= this.pages.length - 1) {
            return;
        }
        this.index = this.index + 1;
        this.NavigateToIndex();
    }

    NavigateBack() {
        this.UpdateIndex();
        if (this.index == 0) {
            return;
        }
        this.index = this.index - 1;
        this.NavigateToIndex();
    }

    JumpTo(index) {
        this.index = index;
        this.NavigateToIndex();
    }

    NavigateToIndex() {
        // The index is set to the next step // do not update index here
        this.MS.Router.navigate('#/' + this.pages[this.index].RoutePageName.toLowerCase());
        this.MS.Router.refreshNavigation();
        this.UpdateIndex();
        this.MS.LoggerService.TrackPageView(this.templateName + '/' + this.pages[this.index].RoutePageName.toLowerCase(),
            window.location.href);
    }

    GetCurrentSelectedPage() {
        this.UpdateIndex();
        return this.pages[this.index];
    }
}