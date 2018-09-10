// Copyright (c) 2012 Ecma International.  All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/*---
esid: sec-array.prototype.map
es5id: 15.4.4.19-4-10
description: >
    Array.prototype.map - the exception is not thrown if exception was
    thrown by step 2
---*/

var obj = {
  0: 11,
  1: 12
};

Object.defineProperty(obj, "length", {
  get: function() {
    throw new Test262Error();
  },
  configurable: true
});

assert.throws(Test262Error, function() {
  Array.prototype.map.call(obj, undefined);
});
