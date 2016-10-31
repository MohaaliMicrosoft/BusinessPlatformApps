import { ViewModelBase } from '../services/viewmodelbase';
import {DataStoreType} from "../services/datastore";
import {SqlServerValidationUtility} from "../base/sql-server-validation-utility";

export class SqlServer extends ViewModelBase {
    subtitle: string = '';
    title: string = '';

    auth: string = 'Windows';
    azureSqlSuffix: string = '.database.windows.net';
    checkSqlVersion: boolean = false;
    database: string = null;
    databases: string[] = [];
    hideSqlAuth: boolean = false;
    isAzureSql: boolean = false;
    isWindowsAuth: boolean = true;
    logInAsCurrentUser: boolean = false;
    newSqlDatabase: string = null;
    password: string = '';
    passwordConfirmation: string = '';
    showAllWriteableDatabases: boolean = true;
    showAzureSql: boolean = true;
    showLogInAsCurrentUser: boolean = false;
    showCredsWhenWindowsAuth: boolean = false;
    showDatabases: boolean = false;
    showNewSqlOption: boolean = false;
    sqlInstance: string = 'ExistingSql';
    sqlServer: string = '';
    username: string = '';
    validateWindowsCredentials: boolean = false;
    validationTextBox: string = '';

    useImpersonation:boolean = false;

    constructor() {
        super();
        this.isValidated = false;
    }

    Invalidate() {
        super.Invalidate();
        this.database = null;
        this.databases = [];
        this.onAuthChange();
        this.showDatabases = false;
    }

    onAuthChange() {
        this.isWindowsAuth = this.auth.toLowerCase() === 'windows';
    }

    onDatabaseChange() {
    }

    async OnValidate() {
        super.OnValidate();

        this.sqlServer = this.sqlServer.toLowerCase();
        if (this.sqlInstance === 'ExistingSql') {
                let databasesResponse = await this.GetDatabases();
                if (databasesResponse.IsSuccess) {
                    this.databases = databasesResponse.Body.value;
                    this.isValidated = true;
                    this.showDatabases = true;
                    this.showValidation = true;
                } else {
                    this.isValidated = false;
                    this.showDatabases = false;
                    this.showValidation = false;
                }
        } else if (this.sqlInstance === 'NewSql') {
            let newSqlError: string = SqlServerValidationUtility.validateAzureSQLCreate(this.sqlServer, this.username, this.password, this.passwordConfirmation);
            if (newSqlError) {
                this.MS.ErrorService.message = newSqlError;
            } else {
                let databasesResponse = await this.ValidateAzureServerIsAvailable();
                if ((databasesResponse.IsSuccess)) {
                    this.isValidated = true;
                    this.showValidation = true;
                } else {
                    this.isValidated = false;
                    this.showValidation = false;
                }
            }
        }
    }

    async NavigatingNext(): Promise<boolean> {
        let body = this.GetBody(true);
        let response = null;

        if (this.sqlInstance === 'ExistingSql') {
            response = await this.MS.HttpService.executeAsync('Microsoft-GetSqlConnectionString', body);
        } else if (this.sqlInstance === 'NewSql') {
            response = await this.CreateDatabaseServer();
        }

        if (response.isSuccess) {
            this.MS.DataStore.addToDataStore('SqlConnectionString', response.response.value, DataStoreType.Private);
            this.MS.DataStore.addToDataStore('Server', this.getSqlServer(), DataStoreType.Public);
            this.MS.DataStore.addToDataStore('Database', this.database, DataStoreType.Public);
            this.MS.DataStore.addToDataStore('Username', this.username, DataStoreType.Public);

            let isProperVersion: boolean = true;

            if (this.checkSqlVersion) {
                let responseVersion = await this.MS.HttpService.executeAsync('Microsoft-CheckSQLVersion', {});
                isProperVersion = responseVersion.IsSuccess;
            }

            return isProperVersion;
        }

        return false;
    }

    private async CreateDatabaseServer() {
        this.navigationMessage = 'Creating a new SQL database, this may take 2-3 minutes';
        let body = this.GetBody(true);
        body['SqlCredentials']['Database'] = this.newSqlDatabase;
        return await this.MS.HttpService.executeAsync('Microsoft-CreateAzureSql', body);
    }

    private async ValidateAzureServerIsAvailable() {
        let body = this.GetBody(false);
        return await this.MS.HttpService.executeAsync('Microsoft-ValidateAzureSqlExists', body);
    }

    private async GetDatabases() {
        let body = this.GetBody(true);
        
        if (this.showAllWriteableDatabases) {
            return await this.MS.HttpService.executeAsync('Microsoft-ValidateAndGetWritableDatabases', body);
        }

        return await this.MS.HttpService.executeAsync('Microsoft-ValidateAndGetAllDatabases', body);
    }

    private GetBody(withDatabase: boolean) {
        super.OnValidate();
        let body = {};
        body['SqlCredentials'] = {};
        body['SqlCredentials']['Server'] = this.getSqlServer();
        body['SqlCredentials']['User'] = this.username;
        body['SqlCredentials']['Password'] = this.password;
        body['SqlCredentials']['AuthType'] = this.isWindowsAuth ? 'windows' : 'sql';

        if (this.isAzureSql) {
            body['SqlCredentials']['AuthType'] = 'sql';
        }

        if (withDatabase) {
            body['SqlCredentials']['Database'] = this.database;
        }

        return body;
    }

    private getSqlServer(): string {
        let sqlServer: string = this.sqlServer;
        if (this.isAzureSql && !sqlServer.includes(this.azureSqlSuffix)) {
            sqlServer += this.azureSqlSuffix;
        }
        return sqlServer;
    }
}