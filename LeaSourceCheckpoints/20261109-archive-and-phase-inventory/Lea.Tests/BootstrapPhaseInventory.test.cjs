'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const {createRequire} = require('node:module');
const load = createRequire('I:/LeaBuildStorage/window-global-tools-20261109-v1/package.json');
const acorn = load('acorn');
const {inventory} = require('../Lea.Build/BootstrapPhaseInventory.cjs');
const parse = code => acorn.parse(code,{ecmaVersion:2022,sourceType:'script'});
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const sample = `{const call=__lea, mark=__jobs;mark('scope');function outer(){return ()=>1;}mark('events');const a=function named(){return 1;};mark('interfaces');mark('elements');mark('document');(()=>{mark();})();}`;
const result = inventory(sample,parse);
assert.equal(result.directFunctionDeclarations.length,1);
assert.equal(result.phases[0].nestedFunctionNodes,1);
assert.equal(result.phases[1].nestedFunctionNodes,1);
assert.equal(result.phases[4].nestedFunctionNodes,1);
assert.deepEqual(result.phases[0].directFunctions,['outer']);
let negatives=0;
for (const bad of [sample.replace("mark('scope');",''),sample.replace("mark('events')","mark('scope')"),sample.replace('mark=__jobs','mark=__other'),sample+'{}',sample.replace("mark('scope')","mark('scope','extra')")]) {
    assert.throws(()=>inventory(bad,parse)); negatives++;
}
const base='I:/LeaBuildStorage/private-binding-full-assembly-20261103-v5-assets/fixtures/Lea.Script';
const reports=[];
for (const [name,expected] of [['Dom.js','A228494E1D20D2669DC2FED38B93B7549EF8849FA2C7997F45EE32678117239A'],['Dom.compact.js','196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3']]) {
    const file=path.join(base,name), bytes=fs.readFileSync(file);
    assert.equal(hash(bytes),expected);
    reports.push({name,sourceSHA256:expected,...inventory(bytes.toString('utf8'),parse)});
    assert.equal(hash(fs.readFileSync(file)),expected);
}
assert.deepEqual(reports[0].phases.map(p=>p.directFunctions.length),reports[1].phases.map(p=>p.directFunctions.length));
const out='I:/LeaBuildStorage/bootstrap-phase-inventory-20261109-v1';
fs.mkdirSync(out);
const report={verified:true,negativeControls:negatives,syntheticChecks:5,reports,parserVersion:load('acorn/package.json').version,
    toolSHA256:hash(fs.readFileSync(require.resolve('../Lea.Build/BootstrapPhaseInventory.cjs'))),testSHA256:hash(fs.readFileSync(__filename)),
    sourceUnchanged:true,nativeExecuted:false,scope:'Static AST inventory only. No timing attribution, startup fix, runtime or security qualification.'};
fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(report,null,2)+'\n',{flag:'wx'});
console.log(JSON.stringify({negativeControls:negatives,reportSHA256:hash(fs.readFileSync(path.join(out,'result.json'))),reports:reports.map(r=>({name:r.name,entryFunctions:r.directFunctionDeclarations.length,phases:r.phases.map(p=>({label:p.label,functions:p.directFunctions.length,nested:p.nestedFunctionNodes,bytes:p.bytes}))}))}));
