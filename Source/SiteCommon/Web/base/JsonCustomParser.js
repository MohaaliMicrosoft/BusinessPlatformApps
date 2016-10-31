"use strict";
var JsonCustomParser = (function () {
    function JsonCustomParser() {
    }
    JsonCustomParser.isVariable = function (value) {
        value = value.toString();
        if (value.startsWith('$(') && value.endsWith(')')) {
            return true;
        }
        else {
            return false;
        }
    };
    JsonCustomParser.extractVariable = function (value) {
        var intermediate = value.replace('$(', '');
        var result = intermediate.slice(0, intermediate.length - 1);
        var resultSplit = result.split(',');
        return resultSplit[0].trim();
    };
    JsonCustomParser.isPermenantEntryIntoDataStore = function (value) {
        var intermediate = value.replace('$(', '');
        var result = intermediate.slice(0, intermediate.length - 1);
        var resultSplit = result.split(',');
        for (var index = 0; index < resultSplit.length; index++) {
            if (index < 1) {
                continue;
            }
            var param = resultSplit[index].trim().toLowerCase();
            var paramSplit = param.split('=');
            if (paramSplit[0] === 'issaved' && paramSplit[1] === 'true') {
                return true;
            }
        }
        return false;
    };
    return JsonCustomParser;
}());
exports.JsonCustomParser = JsonCustomParser;
