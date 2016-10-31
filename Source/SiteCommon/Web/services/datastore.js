"use strict";
var Dictionary_1 = require("../base/Dictionary");
var DataStore = (function () {
    function DataStore(MainService) {
        this.CurrentRoutePage = '';
        this.DeploymentIndex = '';
        this.MS = MainService;
        this.PrivateDataStore = new Dictionary_1.Dictionary();
        this.PublicDataStore = new Dictionary_1.Dictionary();
        this.loadDataStores();
    }
    DataStore.prototype.toJSON = function () {
        var toConvert = {};
        toConvert.PublicDataStore = this.PublicDataStore;
        toConvert.PrivateDataStore = this.PrivateDataStore;
        toConvert.CurrentRoutePage = this.CurrentRoutePage;
        toConvert.DeploymentIndex = this.DeploymentIndex;
        return toConvert;
    };
    DataStore.prototype.loadDataStoreFromJson = function (value) {
        if (!value) {
            return;
        }
        var privateStore = value.PrivateDataStore;
        var publicStore = value.PublicDataStore;
        if (privateStore) {
            this.PrivateDataStore = new Dictionary_1.Dictionary();
            for (var route in privateStore) {
                var valueToAdd = new Dictionary_1.Dictionary();
                for (var key in privateStore[route]) {
                    valueToAdd.add(key, privateStore[route][key]);
                }
                this.PrivateDataStore.add(route, valueToAdd);
            }
        }
        if (publicStore) {
            this.PublicDataStore = new Dictionary_1.Dictionary();
            for (var route in publicStore) {
                var valueToAdd = new Dictionary_1.Dictionary();
                for (var key in publicStore[route]) {
                    valueToAdd.add(key, publicStore[route][key]);
                }
                this.PublicDataStore.add(route, valueToAdd);
            }
        }
    };
    DataStore.prototype.currentRoute = function () {
        return this.CurrentRoutePage + "-" + this.DeploymentIndex;
    };
    /// This method will be used on startup from the main service
    DataStore.prototype.loadDataStores = function () {
        var datastore = this.MS.UtilityService.GetItem(this.MS.NavigationService.appName + " datastore");
        if (!datastore) {
            this.PublicDataStore = new Dictionary_1.Dictionary();
            this.PrivateDataStore = new Dictionary_1.Dictionary();
        }
        else {
            this.loadDataStoreFromJson(datastore);
        }
    };
    DataStore.prototype.cacheDataStores = function () {
        this.MS.UtilityService.SaveItem(this.MS.NavigationService.appName + " datastore", this);
    };
    DataStore.prototype.routeExists = function (route, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        var found = false;
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
    };
    DataStore.prototype.keyExists = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getValueAndRoutesFromDataStore(dataStoreType, key).length > 0;
    };
    DataStore.prototype.routeAndKeyExists = function (route, key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        var found = false;
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
    };
    DataStore.prototype.addToDataStoreWithCustomRoute = function (route, key, value, dataStoreType) {
        this.updateValue(dataStoreType, this.currentRoute(), key, value);
        this.cacheDataStores();
    };
    DataStore.prototype.addToDataStore = function (key, value, dataStoreType) {
        this.updateValue(dataStoreType, this.currentRoute(), key, value);
        this.cacheDataStores();
    };
    DataStore.prototype.addObjectToDataStore = function (value, dataStoreType) {
        for (var propertyName in value) {
            this.updateValue(dataStoreType, this.currentRoute(), propertyName, value[propertyName]);
        }
        this.cacheDataStores();
    };
    DataStore.prototype.addObjectToDataStoreWithCustomRoute = function (route, value, dataStoreType) {
        for (var propertyName in value) {
            this.updateValue(dataStoreType, route, propertyName, value[propertyName]);
        }
        this.cacheDataStores();
    };
    DataStore.prototype.getJson = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getFirstValueFromDataStore(key, dataStoreType);
    };
    DataStore.prototype.getJsonWithRoute = function (route, key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getValueWithRouteAndKey(dataStoreType, route, key);
    };
    DataStore.prototype.getValue = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getFirstValueFromDataStore(key, dataStoreType).toString();
    };
    DataStore.prototype.getValueWithRoute = function (route, key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getValueWithRouteAndKey(dataStoreType, route, key).toString();
    };
    DataStore.prototype.getAllJson = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getAllValueFromDataStore(key, dataStoreType);
    };
    DataStore.prototype.getAllValues = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getAllValueFromDataStore(key, dataStoreType).map(function (p) { return p.toString(); });
    };
    DataStore.prototype.getAllDataStoreItems = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        return this.getValueAndRoutesFromDataStore(dataStoreType, key);
    };
    DataStore.prototype.getFirstValueFromDataStore = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        var values;
        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            values = DataStore.getValueAndRoutesFromDataStore(this.PrivateDataStore, key, DataStoreType.Private);
            if (values.length > 0) {
                return values[0].value;
            }
        }
        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            values = DataStore.getValueAndRoutesFromDataStore(this.PublicDataStore, key, DataStoreType.Private);
            if (values.length > 0) {
                return values[0].value;
            }
        }
        return null;
    };
    DataStore.prototype.getAllValueFromDataStore = function (key, dataStoreType) {
        if (dataStoreType === void 0) { dataStoreType = DataStoreType.Any; }
        var items = this.getValueAndRoutesFromDataStore(dataStoreType, key);
        return items.map(function (value, index, array) { return value.value; });
    };
    DataStore.prototype.getValueAndRoutesFromDataStore = function (dataStoreType, key) {
        var valuesToReturn = new Array();
        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            valuesToReturn = valuesToReturn.concat(DataStore.getValueAndRoutesFromDataStore(this.PrivateDataStore, key, DataStoreType.Private));
        }
        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            valuesToReturn = valuesToReturn.concat(DataStore.getValueAndRoutesFromDataStore(this.PublicDataStore, key, DataStoreType.Public));
        }
        return valuesToReturn;
    };
    DataStore.prototype.getValueWithRouteAndKey = function (dataStoreType, route, key) {
        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            if (this.PrivateDataStore.containsKey(route) && this.PrivateDataStore.get(route).containsKey(key)) {
                return this.PrivateDataStore.get(route).get(key);
            }
        }
        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            if (this.PublicDataStore.containsKey(route) && this.PublicDataStore.get(route).containsKey(key)) {
                return this.PublicDataStore.get(route).get(key);
            }
        }
        return null;
    };
    DataStore.prototype.updateValue = function (dataStoreType, route, key, value) {
        var foundInPrivate = false;
        var foundInPublic = false;
        if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
            foundInPrivate = DataStore.updateItemIntoDataStore(this.PrivateDataStore, route, key, value);
        }
        if (dataStoreType === DataStoreType.Public || dataStoreType === DataStoreType.Any) {
            foundInPublic = DataStore.updateItemIntoDataStore(this.PublicDataStore, route, key, value);
        }
        if (!foundInPublic && !foundInPrivate) {
            if (dataStoreType === DataStoreType.Private || dataStoreType === DataStoreType.Any) {
                DataStore.addModifyItemToDataStore(this.PrivateDataStore, route, key, value);
            }
            if (dataStoreType === DataStoreType.Public) {
                DataStore.addModifyItemToDataStore(this.PublicDataStore, route, key, value);
            }
        }
    };
    DataStore.getValueAndRoutesFromDataStore = function (store, key, dataStoreType) {
        var itemsMatching = new Array();
        for (var i = 0; i < store.length(); i++) {
            var item = store.getItem(i);
            if (item["1"].containsKey(key)) {
                var itemToAdd = new DataStoreItem();
                itemToAdd.route = item["0"];
                itemToAdd.key = key;
                itemToAdd.value = item["1"].get(key);
                itemsMatching.push(itemToAdd);
            }
        }
        return itemsMatching;
    };
    DataStore.updateItemIntoDataStore = function (store, route, key, value) {
        var found = false;
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
    };
    DataStore.addModifyItemToDataStore = function (store, route, key, value) {
        if (!store.containsKey(route)) {
            store.add(route, new Dictionary_1.Dictionary());
        }
        if (!store.get(route).containsKey(key)) {
            store.get(route).add(key, value);
        }
        store.get(route).modify(key, value);
    };
    return DataStore;
}());
exports.DataStore = DataStore;
var DataStoreItem = (function () {
    function DataStoreItem() {
    }
    return DataStoreItem;
}());
exports.DataStoreItem = DataStoreItem;
(function (DataStoreType) {
    DataStoreType[DataStoreType["Public"] = 0] = "Public";
    DataStoreType[DataStoreType["Private"] = 1] = "Private";
    DataStoreType[DataStoreType["Any"] = 2] = "Any";
})(exports.DataStoreType || (exports.DataStoreType = {}));
var DataStoreType = exports.DataStoreType;
