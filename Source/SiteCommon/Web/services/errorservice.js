"use strict";
var ErrorService = (function () {
    function ErrorService(MainService) {
        this.details = '';
        this.logLocation = '';
        this.message = '';
        this.showContactUs = false;
        this.MS = MainService;
    }
    ErrorService.prototype.Clear = function () {
        this.details = '';
        this.logLocation = '';
        this.message = '';
        this.showContactUs = false;
    };
    return ErrorService;
}());
exports.ErrorService = ErrorService;
