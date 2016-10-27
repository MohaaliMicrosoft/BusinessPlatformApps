import MainService from './mainservice';

export class ViewModelBase {
    isActivated: boolean = false;
    isValidated: boolean = true;

    showValidation: boolean = false;
    showValidationDetails: boolean = false;
    validationText: string;

    MS: MainService;

    textNext: string = 'Next';

    showBack: boolean = true;
    showNext: boolean = true;

    
    onNext: any[] = [];
    navigationMessage: string = '';
    useDefaultValidateButton: boolean = false;

    viewmodel: ViewModelBase;

    parametersLoaded:boolean = false;

    constructor() {
        this.MS = (<any>window).MainService;
        this.viewmodel = this;
    }

    loadParameters() {
          // Load the parameters from the additionalParamters section
        if (!this.parametersLoaded) {
            var parameters = this.MS.NavigationService.getCurrentSelectedPage().Parameters;
            for (let propertyName in parameters) {
                this[propertyName] = parameters[propertyName];
            }
        }

        this.parametersLoaded = true;
    }

    async NavigateNext() {
        if (this.MS.NavigationService.isCurrentlyNavigating) {
            return;
        }

        this.MS.NavigationService.isCurrentlyNavigating = true;
        let isNavigationSuccessful: boolean = true;

        try {
            isNavigationSuccessful = await this.NavigatingNext();

        } catch (e) {
            isNavigationSuccessful = false;
        }
        this.navigationMessage = '';

        if (isNavigationSuccessful) {
            let currentRoute = this.MS.NavigationService
                .getCurrentSelectedPage()
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

        this.MS.NavigationService.isCurrentlyNavigating = false;
    }

    NavigateBack() {
        if (this.MS.NavigationService.isCurrentlyNavigating) {
            return;
        }

        this.MS.NavigationService.isCurrentlyNavigating = true;
        let currentRoute = this.MS.NavigationService
            .getCurrentSelectedPage()
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

        // Persistence is lost here for maintaining pages the user has visited
        this.MS.NavigationService.NavigateBack();
        this.MS.DeploymentService.hasError = false;
        this.MS.ErrorService.Clear();

        this.MS.NavigationService.isCurrentlyNavigating = false;
    }

    async activate(params, navigationInstruction) {
        this.isActivated = false;
        this.loadParameters();
        this.MS.UtilityService.SaveItem('Current Page', window.location.href);
        var nav = navigationInstruction.route.replace('/', '');
        this.MS.UtilityService.SaveItem('Current Route', nav);
        let viewmodelPreviousSave = window.sessionStorage.getItem(nav);

        // Restore view model state
        if (viewmodelPreviousSave) {
            let jsonParsed = JSON.parse(viewmodelPreviousSave);
            for (let propertyName in jsonParsed) {
                this[propertyName] = jsonParsed[propertyName];
            }

            this.viewmodel = this;
            this.viewmodel.MS = (<any>window).MainService;
        }

        this.MS.NavigationService.currentViewModel = this;
        this.isActivated = true;
    }

   
    ///////////////////////////////////////////////////////////////////////
    /////////////////// Methods to override ///////////////////////////////
    ///////////////////////////////////////////////////////////////////////

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

    // Called when object has initiated navigating next
    async NavigatingNext(): Promise<boolean> {

        // Dont bother doing anything here

        //for (let actionsToExecute in this.onNext) {

        //    var response = await this.MS.HttpService.executeAsync(actionsToExecute, {});
        //    if (!response.isSuccess) {
        //        return false;
        //    } else {
        //        // Store directly into Datastore here 
        //    }
        //}

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
}