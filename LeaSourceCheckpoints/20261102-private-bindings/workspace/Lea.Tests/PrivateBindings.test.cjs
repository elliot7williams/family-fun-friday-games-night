'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const Module = require('node:module');
const assert = require('node:assert/strict');
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const original = fs.readFileSync(path.join(__dirname, 'LazyMapProjection.test.cjs'));
assert.equal(hash(original), '04656413A21987D4C544E8A894078067B49FB965E43ACFE9BEEEADEE22D50709');
const metadata = String.raw`
    JSON.stringify([globalThis,document,document.getElementById('child'),document.getElementById('child').style,document.getElementById('child').classList,DOMTokenList.prototype,DOMException.prototype,Performance.prototype].map(owner=>Reflect.ownKeys(owner).map(key=>{
        const d=Object.getOwnPropertyDescriptor(owner,key);
        const fn=value=>typeof value==='function'?[value.name,value.length]:null;
        return [String(key),d.enumerable,d.configurable,d.writable,fn(d.value),fn(d.get),fn(d.set)];
    })))
`;
const pairs = [
    ["const folder = 'I:/LeaBuildStorage/lazy-map-source-20261102-v1';", "const folder = 'I:/LeaBuildStorage/private-binding-source-20261102-v1';"],
    ['4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803', '9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2'],
    ['assert.equal(plan.negativeControls,16);', 'assert.equal(plan.negativeControls,4);'],
    ['assert.equal(plan.inverseVerified,true);', 'assert.equal(plan.structuralAstVerified,true);'],
    ["assert.equal(eager,kind === 'candidate' ? 1 : 3);", 'assert.equal(eager,3);'],
    ["context.__expectedAfterList = kind === 'candidate' ? 2 : 3;", 'context.__expectedAfterList = 3;'],
    ['observations.push({count,calls:JSON.parse(JSON.stringify(calls)),title,classes});', `const apiMetadata=vm.runInContext(${JSON.stringify(metadata)},context,{timeout:1000}); observations.push({count,calls:JSON.parse(JSON.stringify(calls)),title,classes,apiMetadata});`],
    ["scope:'Node full-bootstrap mock equivalence and counted private map allocations with poisoned public intrinsics. Not Lea/Jint, native sandbox, timing or release qualification.'", "apiMetadataComparisons:2,scope:'Derived unchanged 312-check bootstrap mock suite with three eager maps in both variants, plus public descriptors/function names/arity equality. Node only; not Jint, sandbox, timing or release qualification.'"]
];
let code = original.toString('utf8');
for (const [before, after] of pairs) { assert.equal(code.split(before).length, 2); code = code.replace(before, after); }
let inverse = code;
for (const [before, after] of [...pairs].reverse()) { assert.equal(inverse.split(after).length, 2); inverse = inverse.replace(after, before); }
assert.equal(inverse, original.toString('utf8'));
const file = 'I:/LeaBuildStorage/private-binding-source-20261102-v1/DerivedBehaviorV2.test.cjs';
if (fs.existsSync(file)) assert.equal(fs.readFileSync(file, 'utf8'), code);
else fs.writeFileSync(file, code, {flag: 'wx'});
const derived = new Module(file, module);
derived.filename = file;
derived.paths = Module._nodeModulePaths(path.dirname(file));
derived._compile(code, file);
