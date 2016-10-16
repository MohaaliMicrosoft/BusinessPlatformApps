import { Aurelia } from 'aurelia-framework';
import MainService from './mainservice';

export class DataService {
    MS: MainService;
    dataStore: Object;

    constructor(MainService) {
        this.MS = MainService;
        // Get cached datastore
        this.dataStore = {};
        this.LoadDataStores();
    }

    /// This method will be used on startup from the main service
    LoadDataStores() {
        this.dataStore = this.GetItem(this.MS.NavigationService.templateName + ' DataStore');
        if (!this.dataStore) {
            this.dataStore = {};
        }
    }

    CacheDataStores() {
        this.SaveItem(this.MS.NavigationService.templateName + ' DataStore', this.dataStore);
    }

    AddToDataStore(route, key, value) {
        if (!this.dataStore[route]) {
            this.dataStore[route] = {};
        }
        this.dataStore[route][key] = value;
        this.CacheDataStores();
    }

    AddObjectToDataStore(route, value) {
        for (let property in value) {
            this.AddToDataStore(route, property, value[property]);
        }
    }

    GetDataStore() {
        let data = {};
        for (let page in this.dataStore) {
            for (let property in this.dataStore[page]) {
                if (!data[property]) {
                    data[property] = [];
                }
                if (Array.isArray(this.dataStore[page][property])) {
                    for (let item in this.dataStore[page][property]) {
                        data[property].push(this.dataStore[page][property][item])
                    }
                } else {
                    data[property].push(this.dataStore[page][property])
                }
                
            }
        }
        return data;
    }

    GetItemFromDataStore(route, key) {
        return this.dataStore[route][key];
    }

    SaveItem(key, value) {
        let val = JSON.stringify(value);
        if (window.sessionStorage.getItem(key)) {
            window.sessionStorage.removeItem(key)
        }
        window.sessionStorage.setItem(key, val);
    }

    ClearSessionStorage() {
        window.sessionStorage.clear();
    }

    GetItem(key) {
        let item = JSON.parse(window.sessionStorage.getItem(key));
        return item;
    }

    RemoveItem(key) {
        window.sessionStorage.removeItem(key);
    }
}