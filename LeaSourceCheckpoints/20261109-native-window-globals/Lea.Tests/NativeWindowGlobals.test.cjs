'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const Module = require('node:module');
const assert = require('node:assert/strict');
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const base = fs.readFileSync(path.join(__dirname,'LazyMapProjection.test.cjs'));
assert.equal(hash(base),'04656413A21987D4C544E8A894078067B49FB965E43ACFE9BEEEADEE22D50709');
const folder = 'I:/LeaBuildStorage/native-window-globals-source-20261109-v1';
const proof = fs.readFileSync(`${folder}/source.json`);
assert.equal(hash(proof),'6E5B5FD89DCE987831D6BD0D190A27AC1EF76CE625537333F46BFC63579378E4');
const mock = String.raw`(()=>{
    let used=false;
    const names=['setTimeout','setInterval','clearTimeout','clearInterval','requestAnimationFrame','cancelAnimationFrame'];
    globalThis.__wg=function(...functions){
        if(used||functions.length!==6||functions.some(f=>typeof f!=='function'))throw Error('Invalid native installer mock');
        used=true;
        for(let i=0;i<names.length;i++)Object.defineProperty(globalThis,names[i],{value:functions[i],writable:true,enumerable:true,configurable:true});
    };
})();`;
const metadata = String.raw`JSON.stringify([globalThis,document,document.getElementById('child'),document.getElementById('child').style,document.getElementById('child').classList,DOMTokenList.prototype,DOMException.prototype,Performance.prototype].map(owner=>Reflect.ownKeys(owner).map(key=>{
    const d=Object.getOwnPropertyDescriptor(owner,key);
    const fn=value=>typeof value==='function'?[value.name,value.length]:null;
    return [String(key),d.enumerable,d.configurable,d.writable,fn(d.value),fn(d.get),fn(d.set)];
})))`;
const anchor="        const dispatch = new vm.Script(bytes.toString('utf8'),{filename:path}).runInContext(context,{timeout:1000});";
const changes=[
    ["const folder = 'I:/LeaBuildStorage/lazy-map-source-20261102-v1';",`const folder = '${folder}';`],
    ['4E8524E487B5F6E59408097EE7C5A7950C7BCADFF272B45BD7847B64334B5803',hash(proof)],
    ['assert.equal(plan.negativeControls,16);','assert.equal(plan.negativeControls,8);'],
    ["assert.equal(eager,kind === 'candidate' ? 1 : 3);",'assert.equal(eager,3);'],
    ["context.__expectedAfterList = kind === 'candidate' ? 2 : 3;",'context.__expectedAfterList = 3;'],
    [anchor,`        if(kind==='candidate')vm.runInContext(${JSON.stringify(mock)},context,{timeout:1000});\n${anchor}`],
    ['observations.push({count,calls:JSON.parse(JSON.stringify(calls)),title,classes});',`assert.equal(vm.runInContext("'__wg' in globalThis",context),false); const apiMetadata=vm.runInContext(${JSON.stringify(metadata)},context,{timeout:1000}); observations.push({count,calls:JSON.parse(JSON.stringify(calls)),title,classes,apiMetadata});`],
    ["scope:'Node full-bootstrap mock equivalence and counted private map allocations with poisoned public intrinsics. Not Lea/Jint, native sandbox, timing or release qualification.'", "apiMetadataComparisons:2,installerMockOnly:true,scope:'Existing312-check full-bootstrap behavior suite plus public metadata/hidden-helper equivalence for native window installer projection. Node mock only, not C# installer, Jint, sandbox or performance qualification.'"]
];
let code=base.toString('utf8');
for(const [before,after] of changes){assert.equal(code.split(before).length,2);code=code.replace(before,after);}
let inverse=code;
for(const [before,after] of [...changes].reverse()){assert.equal(inverse.split(after).length,2);inverse=inverse.replace(after,before);}
assert.equal(inverse,base.toString('utf8'));
const file=`${folder}/DerivedWindowGlobals.test.cjs`;
fs.writeFileSync(file,code,{flag:'wx'});
const derived=new Module(file,module);
derived.filename=file;derived.paths=Module._nodeModulePaths(path.dirname(file));
derived._compile(code,file);
