import { ViewModelBase } from '../services/viewmodelbase';
import {ActionResponse} from "../services/actionresponse";
import {DataStoreType} from "../services/datastore";

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

        this.MS.DataStore.addToDataStore("testVal", "val", DataStoreType.Public);
        this.MS.DataStore.addToDataStore("testVal2", "val2", DataStoreType.Public);
        var response: ActionResponse = await this.MS.HttpService.executeAsync("Microsoft-TestServiceAction", body);
        console.log(response);
    }
}