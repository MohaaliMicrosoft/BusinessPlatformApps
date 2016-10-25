import { Aurelia } from 'aurelia-framework';
import { inject } from 'aurelia-framework';
import { Router } from 'aurelia-router';
import { HttpClient } from 'aurelia-http-client';

import { DeploymentService } from './deploymentservice';
import { ErrorService } from './errorservice';
import { LoggerService } from './loggerservice';
import { NavigationService } from './navigationservice';
import { HttpService } from './httpservice';
import { DataService } from './DataService';
import { UtilityService } from './utilityservice';
import { ViewModelBase } from './viewmodelbase';




@inject(Router, HttpClient)
export default class MainService {
    Router: Router;
    MS: MainService;
    ErrorService: ErrorService;
    LoggerService: LoggerService;
    HttpService: HttpService;
    NavigationService: NavigationService;
    DataService: DataService;
    DeploymentService: DeploymentService;
    UtilityService: UtilityService;
    appName: string;
    templateData: any;

    constructor(router, httpClient) {
        this.Router = router;
        (<any>window).MainService = this;
    

        this.UtilityService = new UtilityService(this);
        this.appName = this.UtilityService.GetQueryParameter('name');

        this.ErrorService = new ErrorService(this);
        this.HttpService = new HttpService(this, httpClient);
        this.NavigationService = new NavigationService(this);
        this.NavigationService.appName = this.appName;
        this.DataService = new DataService(this);

        if (this.DataService.GetItem('App Name') !== this.appName) {
            this.DataService.ClearSessionStorage();
        }

        this.DataService.SaveItem('App Name', this.appName);

        if (!this.DataService.GetItem('UserGeneratedId')) {
            this.DataService.SaveItem('UserGeneratedId', this.UtilityService.GetUniqueId(15));
        }

        this.LoggerService = new LoggerService(this);
        this.DeploymentService = new DeploymentService(this);
    }

    async init() {
        if (this.appName && this.appName !== '') {
            this.templateData = await this.HttpService.GetApp(this.appName);
            if (this.templateData && this.templateData['Pages']) {
                this.NavigationService.init(this.templateData['Pages']);
            }
            if (this.templateData && this.templateData['Actions']) {
                this.DeploymentService.actions = this.templateData['Actions'];
            }
        }
    }
}