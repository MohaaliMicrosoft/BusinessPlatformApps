import { DataStore } from './datastore';

export class ActionRequest {
   
    DataStore:DataStore;
        

    constructor(parameters:any, datastore:DataStore) {
        this.DataStore = datastore;
    }
}
