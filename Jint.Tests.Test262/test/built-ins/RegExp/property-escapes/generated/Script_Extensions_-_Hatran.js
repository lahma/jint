// Copyright 2018 Mathias Bynens. All rights reserved.
// This code is governed by the BSD license found in the LICENSE file.

/*---
author: Mathias Bynens
description: >
  Unicode property escapes for `Script_Extensions=Hatran`
info: |
  Generated by https://github.com/mathiasbynens/unicode-property-escapes-tests
  Unicode v11.0.0
esid: sec-static-semantics-unicodematchproperty-p
features: [regexp-unicode-property-escapes]
includes: [regExpUtils.js]
---*/

const matchSymbols = buildString({
  loneCodePoints: [],
  ranges: [
    [0x0108E0, 0x0108F2],
    [0x0108F4, 0x0108F5],
    [0x0108FB, 0x0108FF]
  ]
});
testPropertyEscapes(
  /^\p{Script_Extensions=Hatran}+$/u,
  matchSymbols,
  "\\p{Script_Extensions=Hatran}"
);
testPropertyEscapes(
  /^\p{Script_Extensions=Hatr}+$/u,
  matchSymbols,
  "\\p{Script_Extensions=Hatr}"
);
testPropertyEscapes(
  /^\p{scx=Hatran}+$/u,
  matchSymbols,
  "\\p{scx=Hatran}"
);
testPropertyEscapes(
  /^\p{scx=Hatr}+$/u,
  matchSymbols,
  "\\p{scx=Hatr}"
);

const nonMatchSymbols = buildString({
  loneCodePoints: [
    0x0108F3
  ],
  ranges: [
    [0x00DC00, 0x00DFFF],
    [0x000000, 0x00DBFF],
    [0x00E000, 0x0108DF],
    [0x0108F6, 0x0108FA],
    [0x010900, 0x10FFFF]
  ]
});
testPropertyEscapes(
  /^\P{Script_Extensions=Hatran}+$/u,
  nonMatchSymbols,
  "\\P{Script_Extensions=Hatran}"
);
testPropertyEscapes(
  /^\P{Script_Extensions=Hatr}+$/u,
  nonMatchSymbols,
  "\\P{Script_Extensions=Hatr}"
);
testPropertyEscapes(
  /^\P{scx=Hatran}+$/u,
  nonMatchSymbols,
  "\\P{scx=Hatran}"
);
testPropertyEscapes(
  /^\P{scx=Hatr}+$/u,
  nonMatchSymbols,
  "\\P{scx=Hatr}"
);
