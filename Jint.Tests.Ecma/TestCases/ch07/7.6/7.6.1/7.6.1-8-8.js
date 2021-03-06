/// Copyright (c) 2012 Ecma International.  All rights reserved. 
/**
 * @path ch07/7.6/7.6.1/7.6.1-8-8.js
 * @description Allow reserved words as property names by set function within an object, accessed via indexing: this, with, default
 */


function testcase() {
        var test0 = 0, test1 = 1, test2 = 2;
        var tokenCodes  = {
            set this(value){
                test0 = value;
            },
            get this(){
                return test0;
            },
            set with(value){
                test1 = value;
            },
            get with(){
                return test1;
            },
            set default(value){
                test2 = value;
            },
            get default(){
                return test2;
            }
        }; 
        var arr = [
            'this', 
            'with', 
            'default'
        ];
        for (var i = 0; i < arr.length; i++) {
            if (tokenCodes[arr[i]] !== i) {
                return false;
            };
        }
        return true;
    }
runTestCase(testcase);
