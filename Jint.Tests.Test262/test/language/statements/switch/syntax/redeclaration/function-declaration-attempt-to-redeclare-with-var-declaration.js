// This file was procedurally generated from the following sources:
// - src/declarations/redeclare-with-var-declaration.case
// - src/declarations/redeclare-allow-var/switch-attempt-to-redeclare-function-declaration.template
/*---
description: redeclaration with VariableDeclaration (FunctionDeclaration in SwitchStatement)
esid: sec-switch-statement-static-semantics-early-errors
flags: [generated]
negative:
  phase: parse
  type: SyntaxError
info: |
    SwitchStatement : switch ( Expression ) CaseBlock

    It is a Syntax Error if any element of the LexicallyDeclaredNames of
    CaseBlock also occurs in the VarDeclaredNames of CaseBlock.

---*/


throw "Test262: This statement should not be evaluated.";

switch (0) { case 1: function f() {} default: var f; }
