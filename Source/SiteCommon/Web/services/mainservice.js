"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator.throw(value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments)).next());
    });
};
var aurelia_framework_1 = require('aurelia-framework');
var aurelia_router_1 = require('aurelia-router');
var aurelia_http_client_1 = require('aurelia-http-client');
var deploymentservice_1 = require('./deploymentservice');
var errorservice_1 = require('./errorservice');
var loggerservice_1 = require('./loggerservice');
var navigationservice_1 = require('./navigationservice');
var httpservice_1 = require('./httpservice');
var DataStore_1 = require('./DataStore');
var utilityservice_1 = require('./utilityservice');
var MainService = (function () {
    function MainService(router, httpClient) {
        this.Router = router;
        window.MainService = this;
        this.UtilityService = new utilityservice_1.UtilityService(this);
        this.appName = this.UtilityService.GetQueryParameter('name');
        this.ErrorService = new errorservice_1.ErrorService(this);
        this.HttpService = new httpservice_1.HttpService(this, httpClient);
        this.NavigationService = new navigationservice_1.NavigationService(this);
        this.NavigationService.appName = this.appName;
        this.DataStore = new DataStore_1.DataStore(this);
        if (this.UtilityService.GetItem('App Name') !== this.appName) {
            this.UtilityService.ClearSessionStorage();
        }
        this.UtilityService.SaveItem('App Name', this.appName);
        if (!this.UtilityService.GetItem('UserGeneratedId')) {
            this.UtilityService.SaveItem('UserGeneratedId', this.UtilityService.GetUniqueId(15));
        }
        this.LoggerService = new loggerservice_1.LoggerService(this);
        this.DeploymentService = new deploymentservice_1.DeploymentService(this);
    }
    // Uninstall or any other types go here
    MainService.prototype.init = function () {
        return __awaiter(this, void 0, void 0, function* () {
            if (this.appName && this.appName !== '') {
                this.templateData = yield this.HttpService.getApp(this.appName);
                if (this.templateData && this.templateData['Pages']) {
                    this.NavigationService.init(this.templateData['Pages']);
                }
                if (this.templateData && this.templateData['Actions']) {
                    this.DeploymentService.actions = this.templateData['Actions'];
                }
            }
        });
    };
    MainService = __decorate([
        aurelia_framework_1.inject(aurelia_router_1.Router, aurelia_http_client_1.HttpClient)
    ], MainService);
    return MainService;
}());
Object.defineProperty(exports, "__esModule", { value: true });
exports.default = MainService;
