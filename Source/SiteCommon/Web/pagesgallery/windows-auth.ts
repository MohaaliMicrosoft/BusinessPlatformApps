import {ViewModelBase} from "../services/viewmodelbase";
import {DataStoreType} from "../services/datastore";

export class SearchTerms extends ViewModelBase {
    username: string = '';
    password: string = '';
    logInAsCurrentUser: boolean = false;

    constructor() {
        super();
        this.isValidated = false;
    }

    async OnValidate():Promise<boolean> {
        if (!super.OnValidate()) {
            return false;
        }

        return true;
    }
}