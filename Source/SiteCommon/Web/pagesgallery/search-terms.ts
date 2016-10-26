import { ViewModelBase } from '../services/viewmodelbase';

export class SearchTerms extends ViewModelBase {
    searchQuery: string = '';

    constructor() {
        super();
        this.isValidated = false;
    }

    async OnValidate() {
        super.OnValidate();

        // Execute my test action here
        //if (this.searchQuery.length > 0) {
        //    this.isValidated = true;
        //    this.showValidation = true;
        //    this.MS.DataService.AddToDataStore('Customize', 'SearchQuery', this.searchQuery);
        //}
    }
}