"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator.throw(value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var JsonCustomParser_1 = require("../base/JsonCustomParser");
var datastore_1 = require("./datastore");
var ViewModelBase = (function () {
    function ViewModelBase() {
        this.isActivated = false;
        this.isValidated = true;
        this.showValidation = false;
        this.showValidationDetails = false;
        this.textNext = 'Next';
        this.showBack = true;
        this.showNext = true;
        this.onNext = [];
        this.onValidate = [];
        this.navigationMessage = '';
        this.useDefaultValidateButton = false;
        this.parametersLoaded = false;
        this.MS = window.MainService;
        this.viewmodel = this;
    }
    ViewModelBase.prototype.loadParameters = function () {
        // Load the parameters from the additionalParamters section
        if (!this.parametersLoaded) {
            var parameters = this.MS.NavigationService.getCurrentSelectedPage().Parameters;
            for (var propertyName in parameters) {
                var val = parameters[propertyName];
                if (JsonCustomParser_1.JsonCustomParser.isVariable(val)) {
                    var codeToRun = JsonCustomParser_1.JsonCustomParser.extractVariable(val);
                    val = eval(codeToRun);
                    if (JsonCustomParser_1.JsonCustomParser.isPermenantEntryIntoDataStore(parameters[propertyName])) {
                        this.MS.DataStore.addToDataStore(propertyName, val, datastore_1.DataStoreType.Private);
                    }
                }
                this[propertyName] = val;
            }
        }
        this.parametersLoaded = true;
    };
    ViewModelBase.prototype.NavigateNext = function () {
        return __awaiter(this, void 0, void 0, function* () {
            if (this.MS.NavigationService.isCurrentlyNavigating) {
                return;
            }
            try {
                this.MS.NavigationService.isCurrentlyNavigating = true;
                var isNavigationSuccessful = true;
                isNavigationSuccessful = yield this.NavigatingNext();
                this.navigationMessage = '';
                if (isNavigationSuccessful) {
                    var currentRoute = this.MS.NavigationService
                        .getCurrentSelectedPage()
                        .RoutePageName.toLowerCase();
                    var viewmodelPreviousSave = window.sessionStorage.getItem(currentRoute);
                    // Save view model state
                    if (viewmodelPreviousSave) {
                        window.sessionStorage.removeItem(currentRoute);
                    }
                    this.viewmodel = null;
                    this.MS = null;
                    window.sessionStorage.setItem(currentRoute, JSON.stringify(this));
                    this.viewmodel = this;
                    this.MS = window.MainService;
                    this.MS.NavigationService.NavigateNext();
                    this.NavigatedNext();
                }
            }
            catch (e) {
            }
            finally {
                this.MS.NavigationService.isCurrentlyNavigating = false;
            }
        });
    };
    ViewModelBase.prototype.NavigateBack = function () {
        if (this.MS.NavigationService.isCurrentlyNavigating) {
            return;
        }
        this.MS.NavigationService.isCurrentlyNavigating = true;
        var currentRoute = this.MS.NavigationService
            .getCurrentSelectedPage()
            .RoutePageName.toLowerCase();
        var viewmodelPreviousSave = window.sessionStorage.getItem(currentRoute);
        // Save view model state
        if (viewmodelPreviousSave) {
            window.sessionStorage.removeItem(currentRoute);
        }
        this.viewmodel = null;
        this.MS = null;
        window.sessionStorage.setItem(currentRoute, JSON.stringify(this));
        this.viewmodel = this;
        this.MS = window.MainService;
        // Persistence is lost here for maintaining pages the user has visited
        this.MS.NavigationService.NavigateBack();
        this.MS.DeploymentService.hasError = false;
        this.MS.ErrorService.Clear();
        this.MS.NavigationService.isCurrentlyNavigating = false;
    };
    ViewModelBase.prototype.activate = function (params, navigationInstruction) {
        return __awaiter(this, void 0, void 0, function* () {
            this.isActivated = false;
            this.loadParameters();
            this.MS.UtilityService.SaveItem('Current Page', window.location.href);
            var nav = navigationInstruction.route.replace('/', '');
            this.MS.UtilityService.SaveItem('Current Route', nav);
            var viewmodelPreviousSave = window.sessionStorage.getItem(nav);
            // Restore view model state
            if (viewmodelPreviousSave) {
                var jsonParsed = JSON.parse(viewmodelPreviousSave);
                for (var propertyName in jsonParsed) {
                    this[propertyName] = jsonParsed[propertyName];
                }
                this.viewmodel = this;
                this.viewmodel.MS = window.MainService;
            }
            this.MS.NavigationService.currentViewModel = this;
            this.isActivated = true;
        });
    };
    ///////////////////////////////////////////////////////////////////////
    /////////////////// Methods to override ///////////////////////////////
    ///////////////////////////////////////////////////////////////////////
    // Called when object is no longer valid
    ViewModelBase.prototype.Invalidate = function () {
        this.isValidated = false;
        this.showValidation = false;
        this.validationText = null;
        this.MS.ErrorService.details = '';
        this.MS.ErrorService.message = '';
    };
    // Called when object is validating user input
    ViewModelBase.prototype.OnValidate = function () {
        return __awaiter(this, void 0, Promise, function* () {
            this.isValidated = false;
            this.showValidation = false;
            this.MS.ErrorService.Clear();
            this.isValidated = yield this.executeActions(this.onValidate);
            return;
        });
    };
    // Called when object has initiated navigating next
    ViewModelBase.prototype.NavigatingNext = function () {
        return __awaiter(this, void 0, Promise, function* () {
            return yield this.executeActions(this.onNext);
        });
    };
    // Called when object has navigated next -only simple cleanup logic should go here
    ViewModelBase.prototype.NavigatedNext = function () {
    };
    ViewModelBase.prototype.attached = function () {
        return __awaiter(this, void 0, void 0, function* () {
            yield this.OnLoaded();
        });
    };
    // Called when the view model is attached completely
    ViewModelBase.prototype.OnLoaded = function () {
        return __awaiter(this, void 0, void 0, function* () {
        });
    };
    ViewModelBase.prototype.executeActions = function (actions) {
        return __awaiter(this, void 0, Promise, function* () {
            for (var index in actions) {
                var actionToExecute = actions[index];
                var name_1 = actionToExecute.name;
                if (name_1) {
                    var body = {};
                    for (var prop in actionToExecute) {
                        var val = actionToExecute[prop];
                        if (JsonCustomParser_1.JsonCustomParser.isVariable(val)) {
                            var codeToRun = JsonCustomParser_1.JsonCustomParser.extractVariable(val);
                            val = eval(codeToRun);
                            if (JsonCustomParser_1.JsonCustomParser.isPermenantEntryIntoDataStore(actionToExecute[prop])) {
                                this.MS.DataStore.addToDataStore(prop, val, datastore_1.DataStoreType.Private);
                            }
                        }
                        body[prop] = val;
                    }
                    var response = yield this.MS.HttpService.executeAsync(name_1, body);
                    if (!response.IsSuccess) {
                        return false;
                    }
                    this.MS.DataStore.addObjectToDataStore(response, datastore_1.DataStoreType.Private);
                }
            }
            return true;
        });
    };
    return ViewModelBase;
}());
exports.ViewModelBase = ViewModelBase;
