import {Dictionary} from '../base/Dictionary'

export class DataStore {
    PublicDataStore: Dictionary<Dictionary<any>>;
    PrivateDataStore: Dictionary<Dictionary<any>>;

    CurrentRoutePage: string;
    DeploymentIndex: string;

    constructor() {
        this.PrivateDataStore = new Dictionary<Dictionary<any>>();
        this.PublicDataStore = new Dictionary<Dictionary<any>>();
    }

    public currentRoute(): string {
        return this.CurrentRoutePage + "-" + this.DeploymentIndex;
    }

    public routeExists(route: string, dataStoreType: DataStoreType = DataStoreType.Any): boolean {
        var found: boolean = false;

        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            if (this.PrivateDataStore.containsKey(route)) {
                found = true;
            }
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            if (this.PublicDataStore.containsKey(route)) {
                found = true;
            }
        }
        return found;
    }

    public keyExists(key: string, dataStoreType: DataStoreType = DataStoreType.Any): boolean {
        return this.getValueAndRoutesFromDataStore(dataStoreType, key).length > 0;
    }

    public routeAndKeyExists(route: string, key:string, dataStoreType: DataStoreType = DataStoreType.Any): boolean {
        var found: boolean = false;

        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            if (this.PrivateDataStore.containsKey(route)) {
                if (this.PrivateDataStore.get(route).containsKey(key)) {
                    found = true;
                }
            }
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            if (this.PublicDataStore.containsKey(route)) {
                if (this.PublicDataStore.get(route).containsKey(key)) {
                    found = true;
                }
            }
        }
        return found;
    }

    public addToDataStoreWithCustomRoute(route:string, key: string, value: any, dataStoreType: DataStoreType) {
        this.updateValue(dataStoreType, this.currentRoute(), key, value);
    }

    public addToDataStore(key:string, value: any, dataStoreType: DataStoreType) {
       this.updateValue(dataStoreType, this.currentRoute(), key, value);
    }

    public addObjectDataStore(value: any, dataStoreType: DataStoreType)
    {
        let jsonParsed = JSON.parse(value);
        for (let propertyName in jsonParsed) {
            this.updateValue(dataStoreType, this.currentRoute(), propertyName, jsonParsed[propertyName]);
        }
    }

    public addObjectDataStoreWithCustomRoute(route:string, value: any, dataStoreType: DataStoreType) {
        let jsonParsed = JSON.parse(value);
        for (let propertyName in jsonParsed) {
            this.updateValue(dataStoreType, route, propertyName, jsonParsed[propertyName]);
        }
    }

    public getJson(key: string, dataStoreType: DataStoreType = DataStoreType.Any): any {
        return this.getFirstValueFromDataStore(key, dataStoreType);
    }

    public getJsonWithRoute(route:string, key: string, dataStoreType: DataStoreType = DataStoreType.Any): any {
        return this.getValueWithRouteAndKey(dataStoreType, route, key);
    }

    public getValue(key: string, dataStoreType: DataStoreType = DataStoreType.Any): string {
        return this.getFirstValueFromDataStore(key, dataStoreType).toString();
    }

    public getValueWithRoute(route: string, key: string, dataStoreType: DataStoreType = DataStoreType.Any): string {
        return this.getValueWithRouteAndKey(dataStoreType, route, key).toString();
    }

    public getAllJson(key: string, dataStoreType: DataStoreType = DataStoreType.Any):any[] {
        return this.getAllValueFromDataStore(key, dataStoreType);
    }

    public getAllValues(key: string, dataStoreType: DataStoreType = DataStoreType.Any):string[] {
        return this.getAllValueFromDataStore(key, dataStoreType).map(p => p.toString());
    }

    public getAllDataStoreItems(key: string, dataStoreType: DataStoreType = DataStoreType.Any):DataStoreItem[] {
        return this.getValueAndRoutesFromDataStore(dataStoreType, key);
    }

    private getFirstValueFromDataStore(key: string, dataStoreType: DataStoreType = DataStoreType.Any):any {
        var values: DataStoreItem[];

        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            values = DataStore.getValueAndRoutesFromDataStore(this.PrivateDataStore, key, DataStoreType.Private);
            if (values.length >0 ) {
                return values[0].value;
            }
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            values = DataStore.getValueAndRoutesFromDataStore(this.PublicDataStore, key, DataStoreType.Private);
            if (values.length > 0 ) {
                return values[0].value;
            }
        }

        return null;
    }

    private getAllValueFromDataStore(key: string, dataStoreType: DataStoreType = DataStoreType.Any):any[] {
        var items: DataStoreItem[]  = this.getValueAndRoutesFromDataStore(dataStoreType, key);

        return items.map((value, index, array) => { return value.value });
    }

    private getValueAndRoutesFromDataStore(dataStoreType: DataStoreType, key: string): DataStoreItem[] {
        var valuesToReturn: DataStoreItem[] = new Array<DataStoreItem>();

        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {

            valuesToReturn = valuesToReturn.concat(DataStore.getValueAndRoutesFromDataStore(this.PrivateDataStore, key, DataStoreType.Private));
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            valuesToReturn = valuesToReturn.concat(DataStore.getValueAndRoutesFromDataStore(this.PublicDataStore, key, DataStoreType.Public));
        }

        return valuesToReturn;
    }

    private getValueWithRouteAndKey(dataStoreType: DataStoreType, route: string, key: string) {
        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            if (this.PrivateDataStore.containsKey(route) && this.PrivateDataStore.get(route).ContainsKey(key)) {
                return this.PrivateDataStore.get(route).get(key);
            }
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            if (this.PublicDataStore.containsKey(route) && this.PublicDataStore.get(route).ContainsKey(key)) {
                return this.PublicDataStore.get(route).get(key);
            }
        }

        return null;
    }

    private updateValue(dataStoreType: DataStoreType, route: string, key: string, value: any) {
        var foundInPrivate: boolean = false;
        var foundInPublic: boolean = false;

        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            foundInPrivate = DataStore.updateItemIntoDataStore(this.PrivateDataStore, route, key, value);
        }

        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            foundInPublic = DataStore.updateItemIntoDataStore(this.PrivateDataStore, route, key, value);
        }

        if (!foundInPublic && !foundInPrivate) {
            if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
                DataStore.addModifyItemToDataStore(this.PrivateDataStore, route, key, value);
            }

            if (dataStoreType === DataStoreType.Public) {
                DataStore.addModifyItemToDataStore(this.PublicDataStore, route, key, value);
            }
        }
    }

    private static getValueAndRoutesFromDataStore(store: Dictionary<Dictionary<any>>,
        key: string,
        dataStoreType: DataStoreType): DataStoreItem[] {
        var itemsMatching = new Array<DataStoreItem>();

        for (var i = 0; i < store.length(); i++) {
            var item: [string, Dictionary<any>] = store.getItem(i);

            if (item["1"].containsKey(key)) {
                var itemToAdd: DataStoreItem = new DataStoreItem();
                itemToAdd.route = item["0"];
                itemToAdd.key = key;
                itemToAdd.value = item["1"].get(key);
                itemsMatching.push(itemToAdd);
            }
        }

        return itemsMatching;
    }

    private static updateItemIntoDataStore(store: Dictionary<Dictionary<any>>,
        route: string,
        key: string,
        value: any): boolean {
        var found: boolean = false;

        if (store.containsKey(route) && store.get(route).containsKey(key)) {
            found = true;
            if (value === null) {
                store.get(route).remove(key);
            }
            else {
                store.get(route).modify(key, value);
            }
        }

        return found;
    }

    private static addModifyItemToDataStore(store: Dictionary<Dictionary<any>>,
        route: string,
        key: string,
        value: any): void {

        if (store.containsKey(route)) {
            store.add(route, new Dictionary<any>());
        }

        if (!store.get(route).ContainsKey(key)) {
            store.get(route).add(key, value);
        }

        store.get(route).modify(key);
    }
}

export class DataStoreItem {
    route: string;
    key: string;
    value: any;
}

export enum DataStoreType {
    Public,
    Private,
    Any
}