import { ViewModelBase } from '../services/viewmodelbase';
import {ActionResponse} from "../services/actionresponse";

export class SearchTerms extends ViewModelBase {
    searchQuery: string = '';

    constructor() {
        super();
        this.isValidated = false;
    }

    async OnValidate() {
        super.OnValidate();

        var body: any = {};
        body.Hello = "Test";
        body.hello2 = {};
        body.hello2.hello = "Test2";

        var response: ActionResponse = await this.MS.HttpService.executeAsync("Microsoft-TestServiceAction", body);
        console.log(response);
    }
}