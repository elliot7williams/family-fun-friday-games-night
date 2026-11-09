'use strict';
const fs=require('node:fs');
const crypto=require('node:crypto');
const assert=require('node:assert/strict');
const Module=require('node:module');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex').toUpperCase();
const base=fs.readFileSync(require.resolve('./LazyMapProjection.test.cjs'));
assert.equal(hash(base),'04656413A21987D4C544E8A894078067B49FB965E43ACFE9BEEEADEE22D50709');
const folder='I:/LeaBuildStorage/detailed-bootstrap-profile-source-20261109-v1';
const proof=fs.readFileSync(`${folder}/source.json`);
assert.equal(hash(proof),'BFBEEF565BE2EEEE0683C8A729E754DEDB17B49C458BA3C7782CED5C33C6926F');
const plan=JSON.parse(proof), member=plan.files.find(f=>f.name==='Dom.compact.js');
const files=[{...member,original:'I:/LeaBuildStorage/private-binding-full-assembly-20261103-v5-assets/fixtures/Lea.Script/Dom.compact.js',originalSha256:plan.originalResourceSHA256}];
const anchor="        const dispatch = new vm.Script(bytes.toString('utf8'),{filename:path}).runInContext(context,{timeout:1000});";
const changes=[
    ["const folder = 'I:/LeaBuildStorage/lazy-map-source-20261102-v1';",`const folder = '${folder}';`],
    ['4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803',hash(proof)],
    ['assert.equal(plan.negativeControls,16);','assert.equal(plan.negativeControls,4);'],
    ['for (const file of plan.files) {',`for (const file of ${JSON.stringify(files)}) {`],
    ["assert.equal(eager,kind === 'candidate' ? 1 : 3);",'assert.equal(eager,3);'],
    ["context.__expectedAfterList = kind === 'candidate' ? 2 : 3;",'context.__expectedAfterList = 3;'],
    [anchor,`        const detail=[];if(kind==='candidate')context.__phase=label=>detail.push(label);\n${anchor}\n        assert.deepEqual(detail,kind==='candidate'?${JSON.stringify(plan.labels)}:[]);\n        assert.equal(vm.runInContext("'__phase' in globalThis",context),false);`],
    ["scope:'Node full-bootstrap mock equivalence and counted private map allocations with poisoned public intrinsics. Not Lea/Jint, native sandbox, timing or release qualification.'","detailSequenceVerified:true,hiddenHelperRemoved:true,scope:'Compact resource original/candidate mock behavior equivalence only. Not native/Jint, sandbox, timing or release qualification.'"]
];
let code=base.toString('utf8');
for(const [before,after] of changes){assert.equal(code.split(before).length,2);code=code.replace(before,after);}
let inverse=code;
for(const [before,after] of [...changes].reverse()){assert.equal(inverse.split(after).length,2);inverse=inverse.replace(after,before);}
assert.equal(inverse,base.toString('utf8'));
const file=`${folder}/DerivedDetailedBootstrap.test.cjs`;
fs.writeFileSync(file,code,{flag:'wx'});
const derived=new Module(file,module);derived.filename=file;derived.paths=module.paths;derived._compile(code,file);
