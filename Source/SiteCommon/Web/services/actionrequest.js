"use strict";
var datastore_1 = require('./datastore');
var ActionRequest = (function () {
    function ActionRequest(parameters, datastore) {
        this.DataStore = datastore;
        // Read the parameters and ensure the datastore is set up correctly - 
        //we may need to clone the datastore but for now we wont
        // Delete the RequestParameters from the old request
        this.DataStore.PrivateDataStore.remove('RequestParameters');
        //Add object to data store
        this.DataStore.addObjectToDataStoreWithCustomRoute('RequestParameters', parameters, datastore_1.DataStoreType.Private);
    }
    return ActionRequest;
}());
exports.ActionRequest = ActionRequest;
