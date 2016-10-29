import { ViewModelBase } from '../services/viewmodelbase';

export class Gettingstarted extends ViewModelBase {
    architectureDiagram: string = '';
    downloadLink: string;
    features: string[] = [];
    isDownload: boolean = false;
    isEvaluation: boolean = false;
    pricing: string[] = [];
    requirements: string[] = [];
    subtitle: string;
    templateName: string = '';
    showPrivacy:boolean;

    constructor() {
        super();
        this.showPrivacy = true;
    }

    async OnLoaded() {
        if (this.isDownload && !this.isEvaluation) {
            let response = await this.MS.HttpService.executeAsync('Microsoft-GetMsiDownloadLink', {});
            this.downloadLink = response.Body.value;
        }
    }
}