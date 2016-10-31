"use strict";
var Dictionary = (function () {
    function Dictionary() {
        this.internalKeys = new Array();
        this.internalValues = new Array();
    }
    Dictionary.prototype.length = function () {
        return this.internalKeys.length;
    };
    Dictionary.prototype.keys = function () {
        return this.internalKeys;
    };
    Dictionary.prototype.values = function () {
        return this.internalValues;
    };
    Dictionary.prototype.get = function (key) {
        var index = this.internalKeys.indexOf(key);
        return this.internalValues[index];
    };
    Dictionary.prototype.getItem = function (index) {
        var key = this.internalKeys[index];
        var value = this.internalValues[index];
        return [key, value];
    };
    Dictionary.prototype.add = function (key, value) {
        if (this.internalKeys.indexOf(key) > -1) {
            throw new Error("Key is already inside dictionary");
        }
        this.internalKeys.push(key);
        this.internalValues.push(value);
    };
    Dictionary.prototype.modify = function (key, value) {
        var index = this.internalKeys.indexOf(key);
        if (index === -1) {
            throw new Error("Key is not found inside dictionary");
        }
        this.internalValues[index] = value;
    };
    Dictionary.prototype.remove = function (key) {
        var index = this.internalKeys.indexOf(key, 0);
        if (index > -1) {
            this.internalKeys.splice(index, 1);
            this.internalValues.splice(index, 1);
        }
    };
    Dictionary.prototype.containsKey = function (key) {
        if (this.internalKeys.indexOf(key) === -1) {
            return false;
        }
        return true;
    };
    Dictionary.prototype.toJSON = function () {
        var toConvert = {};
        for (var i = 0; i < this.length(); i++) {
            toConvert[this.internalKeys[i]] = this.internalValues[i];
        }
        return toConvert;
    };
    return Dictionary;
}());
exports.Dictionary = Dictionary;
