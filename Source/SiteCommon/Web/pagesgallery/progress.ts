﻿import { ViewModelBase } from '../services/viewmodelbase';

export class ProgressViewModel extends ViewModelBase {
    finishedActionName: string = '';
    isDataPullDone: boolean = false;
    isPbixReady: boolean = false;
    pbixDownloadLink: string = '';
    recordCounts: any[] = [];
    showCounts: boolean = false;
    sliceStatus: any[] = [];
    sqlServerIndex: number = 0;
    successMessage: string = 'All done! You can now download your Power BI report and start exploring your data.';
    targetSchema: string = '';

    constructor() {
        super();
        this.showNext = false;
    }

    async OnLoaded() {
        if (!this.MS.DeploymentService.isFinished) {
            await this.MS.DeploymentService.ExecuteActions();

            let body: any = {};
            body.FileName = 'SalesManagementReport.pbix';
            this.showCounts = true;
            let response = await this.MS.HttpService.executeAsync('Microsoft-WranglePBI', body);
            if (response.IsSuccess) {
                this.pbixDownloadLink = response.Body.value;
                this.isPbixReady = true;
            }

            this.QueryRecordCounts();
        }
    }

    async QueryRecordCounts() {
        if (this.showCounts && !this.isDataPullDone && !this.MS.DeploymentService.hasError) {
            let body: any = {};
            body.FinishedActionName = this.finishedActionName;
            body.IsWaiting = false;
            body.SqlServerIndex = this.sqlServerIndex;
            body.TargetSchema = this.targetSchema;

            let response = await this.MS.HttpService.executeAsync('Microsoft-GetDataPullStatus', body);
            if (response.IsSuccess) {
                this.isDataPullDone = response.Body.isFinished;
                this.recordCounts = response.Body.status;
                this.sliceStatus = response.Body.slices;
                if (this.finishedActionName !== '') {
                    this.QueryRecordCounts();
                } else {
                    this.isDataPullDone = true;
                }
            } else {
                this.MS.DeploymentService.hasError = true;
            }
        }
    }
}