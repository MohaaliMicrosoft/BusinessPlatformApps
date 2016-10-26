import { ViewModelBase } from '../services/viewmodelbase';

export class gettingstarted extends ViewModelBase {
    architectureDiagram: string = '';
    downloadLink: string;
    features: string[] = [];
    isDownload: boolean = false;
    isEvaluation: boolean = false;
    pricing: string[] = [];
    requirements: string[] = [];
    subtitle: string;
    templateName: string = 'Hello Mo';
    showPrivacy:boolean;

    constructor() {
        super();
        this.showPrivacy = true;
    }

    async OnLoaded() {
        if (this.isDownload && !this.isEvaluation) {
            let response = await this.MS.HttpService.Execute('Microsoft-GetMsiDownloadLink', {});
            this.downloadLink = response.response.value;
        }
    }
}