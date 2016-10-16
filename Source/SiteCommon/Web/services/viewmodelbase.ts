import MainService from './mainservice';

export class ViewModelBase {
    downloadLink: string;
    isActivated: boolean = false;
    isValidated: boolean = true;
    MS: MainService;
    showBack: boolean = true;
    showNext: boolean = true;
    showPrivacy: boolean = false;
    showValidation: boolean = false;
    showValidationDetails: boolean = false;
    textNext: string = 'Next';
    validationText: string;
    isCurrentlyNavigating: boolean = false;
    navigationMessage: string = '';
    useDefaultValidateButton: boolean = false;
    viewmodel: ViewModelBase;

    constructor() {
        this.MS = (<any>window).MainService;
        this.viewmodel = this;
    }

    async NavigateNext() {
        this.isCurrentlyNavigating = true;
        let isNavigationSuccessful: boolean = true;

        try {
            isNavigationSuccessful = await this.NavigatingNext();

        } catch (e) {
            isNavigationSuccessful = false;
        }
        this.navigationMessage = '';

        if (isNavigationSuccessful) {
            let currentRoute = this.MS.NavigationService
                .GetCurrentSelectedPage()
                .RoutePageName.toLowerCase();
            let viewmodelPreviousSave = window.sessionStorage.getItem(currentRoute);

            // Save view model state
            if (viewmodelPreviousSave) {
                window.sessionStorage.removeItem(currentRoute);
            }

            this.viewmodel = null;
            this.MS = null;
            window.sessionStorage.setItem(currentRoute, JSON.stringify(this));
            this.viewmodel = this;
            this.MS = (<any>window).MainService;
            this.MS.NavigationService.NavigateNext();
            this.NavigatedNext();
        }

        this.isCurrentlyNavigating = false;
    }

    NavigateBack() {
        let currentRoute = this.MS.NavigationService
            .GetCurrentSelectedPage()
            .RoutePageName.toLowerCase();
        let viewmodelPreviousSave = window.sessionStorage.getItem(currentRoute);

        // Save view model state
        if (viewmodelPreviousSave) {
            window.sessionStorage.removeItem(currentRoute);
        }

        this.viewmodel = null;
        this.MS = null;
        window.sessionStorage.setItem(currentRoute, JSON.stringify(this));
        this.viewmodel = this;
        this.MS = (<any>window).MainService;
        this.MS.NavigationService.NavigateBack();

        this.MS.DeploymentService.hasError = false;
        this.MS.ErrorService.Clear();
    }

    async activate(params, navigationInstruction) {
        this.isActivated = false;
        this.MS.DataService.SaveItem('Current Page', window.location.href);
        var nav = navigationInstruction.route.replace('/', '');
        this.MS.DataService.SaveItem('Current Route', nav);
        let viewmodelPreviousSave = window.sessionStorage.getItem(nav);

        // Save view model state
        if (viewmodelPreviousSave) {
            let jsonParsed = JSON.parse(viewmodelPreviousSave);
            for (let propertyName in jsonParsed) {
                this[propertyName] = jsonParsed[propertyName];
            }
            this.viewmodel = this;
            this.viewmodel.MS = (<any>window).MainService;
        }

        this.MS.NavigationService.currentViewModel = this;
        await this.Activated();
        this.isActivated = true;
    }

    // Called when object is no longer valid
    Invalidate() {
        this.isValidated = false;
        this.showValidation = false;
        this.validationText = null;
        this.MS.ErrorService.details = '';
        this.MS.ErrorService.message = '';
    }

    // Called when object is validating user input
    OnValidate() {
        this.isValidated = false;
        this.showValidation = false;
        this.MS.ErrorService.details = '';
        this.MS.ErrorService.message = '';
    }

    ///////////////////////////////////////////////////////////////////////
    /////////////////// Methods to override ///////////////////////////////
    ///////////////////////////////////////////////////////////////////////

    // Called when object has initiated navigating next
    async NavigatingNext(): Promise<boolean> {
        return true;
    }

    // Called when object has navigated next -only simple cleanup logic should go here
    NavigatedNext() {
    }

    async attached() {
        await this.OnLoaded();
    }

    // Called when the view model is attached completely
    async OnLoaded() {

    }

    // Called when object is activated by the view model base but not loaded
    async Activated() {
    }
}