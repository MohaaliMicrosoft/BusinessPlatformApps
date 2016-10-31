"use strict";
var ActionResponse = (function () {
    function ActionResponse() {
    }
    return ActionResponse;
}());
exports.ActionResponse = ActionResponse;
(function (ActionStatus) {
    ActionStatus[ActionStatus["Failure"] = 0] = "Failure";
    ActionStatus[ActionStatus["FailureExpected"] = 1] = "FailureExpected";
    ActionStatus[ActionStatus["BatchNoState"] = 2] = "BatchNoState";
    ActionStatus[ActionStatus["BatchWithState"] = 3] = "BatchWithState";
    ActionStatus[ActionStatus["UserInteractionRequired"] = 4] = "UserInteractionRequired";
    ActionStatus[ActionStatus["Success"] = 5] = "Success";
})(exports.ActionStatus || (exports.ActionStatus = {}));
var ActionStatus = exports.ActionStatus;
var ActionResponseExceptionDetail = (function () {
    function ActionResponseExceptionDetail() {
    }
    return ActionResponseExceptionDetail;
}());
exports.ActionResponseExceptionDetail = ActionResponseExceptionDetail;
