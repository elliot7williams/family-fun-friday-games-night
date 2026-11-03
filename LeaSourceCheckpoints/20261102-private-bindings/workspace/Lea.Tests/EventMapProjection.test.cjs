'use strict';
const fs = require('node:fs');
const vm = require('node:vm');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const directory = 'I:/LeaBuildStorage/event-map-source-20261102-v1';
const hash = value => crypto.createHash('sha256').update(value).digest('hex').toUpperCase();
const proof = fs.readFileSync(`${directory}/source.json`);
assert.equal(hash(proof), 'FE6451DEA2C77AF80A4D43020D5F6CEE8B7EFE47C24C32AB61ABF070F4CFC0F3');
const plan = JSON.parse(proof);
assert.equal(plan.negativeControls, 12);
assert.equal(plan.inverseVerified, true);
const cases = [];
for (const file of plan.files) {
    const observations = [];
    for (const [kind, path, expected] of [['original', file.original, file.originalSha256], ['candidate', file.path, file.sha256]]) {
        const source = fs.readFileSync(path);
        assert.equal(hash(source), expected);
        const calls = [], stages = [];
        let classes = 'a b';
        const bridge = (op, ...args) => {
            calls.push([op, ...args]);
            switch (op) {
                case 'id': return args[0] === 'child' ? 2 : args[0] === 'parent' ? 1 : null;
                case 'attr': return args[1] === 'class' ? classes : null;
                case 'put': if (args[1] === 'cn') classes = args[2]; return true;
                case 'co': return args[0] === null || args[0] === args[1];
                case 'pe': return args[0] === 2 ? 1 : null;
                case 'mt': return args[1] === '.a';
                case 'cl': return args[1] === '.a' ? 2 : null;
                default: throw Error('Unexpected bridge operation: ' + op);
            }
        };
        const context = vm.createContext({__lea: bridge, __jobs: value => { if (value) stages.push(value); }, __dc: () => {}});
        vm.runInContext(`(()=>{const Map=WeakMap,build=Reflect.construct;globalThis.__allocations=0;globalThis.WeakMap=new Proxy(Map,{construct(target,args,newTarget){__allocations++;return build(target,args,newTarget);}});})()`, context);
        const dispatch = new vm.Script(source.toString('utf8'), {filename: path}).runInContext(context, {timeout: 1000});
        assert.equal(typeof dispatch, 'function');
        assert.deepEqual(stages, ['declarations', 'scope', 'events', 'interfaces', 'elements', 'document']);
        const eager = context.__allocations;
        assert.equal(eager, kind === 'candidate' ? 0 : 1);
        context.__eager = eager;
        vm.runInContext(`
            globalThis.__checks=0;globalThis.__log=[];
            globalThis.__check=value=>{__checks++;if(!value)throw Error('Check '+__checks);};
            globalThis.__reject=action=>{let error;try{action();}catch(e){error=e;}__check(error instanceof TypeError);};
            (()=>{
                let traps=0;
                const fake=new Proxy({}, {get(){traps++;throw Error('receiver trap');},getPrototypeOf(){traps++;throw Error('prototype trap');}});
                const add=document.addEventListener,remove=document.removeEventListener;
                const click=Object.getOwnPropertyDescriptor(document,'onclick');
                for(const bad of [{},fake,1,'x',true,Symbol('x')]){
                    __reject(()=>add.call(bad,'click',()=>{}));
                    __reject(()=>remove.call(bad,'click',()=>{}));
                    __reject(()=>click.get.call(bad));
                    __reject(()=>click.set.call(bad,()=>{}));
                }
                __check(traps===0);__check(__allocations===__eager);
                __reject(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'length').get.call(fake));
                __reject(()=>Object.getOwnPropertyDescriptor(DOMException.prototype,'name').get.call(fake));
                __check(traps===0);__check(__allocations===__eager);
                const SavedMap=WeakMap;
                globalThis.WeakMap=function(){throw Error('Public constructor');};
                SavedMap.prototype.get=SavedMap.prototype.set=Reflect.apply=function(){throw Error('Public intrinsic');};
                const temporary=()=>{throw Error('Removed callback ran');};
                add.call(null,'load',temporary);remove.call(undefined,'load',temporary);
                document.addEventListener('DOMContentLoaded',function(event){__check(this===document&&event.target===document);__log.push('ready');},{once:true});
                Object.getOwnPropertyDescriptor(globalThis,'onload').set.call(null,function(event){__check(this===window&&event.target===window);__log.push('load');});
                __check(__allocations===__eager);
            })();
        `, context, {timeout: 1000});
        assert.equal(dispatch('DOMContentLoaded', []), false);
        assert.equal(dispatch('DOMContentLoaded', []), false);
        assert.equal(dispatch('load', []), false);
        assert.equal(context.__allocations, eager);
        vm.runInContext(`
            (()=>{
                const child=document.getElementById('child'),parent=document.getElementById('parent');
                __check(child===document.getElementById('child'));__check(__allocations===1);
                __check(child.parentElement===parent&&child.matches('.a')&&child.closest('.a')===child);
                __check(child.isConnected&&document.contains(child));
                const getter=Object.getOwnPropertyDescriptor(child,'isConnected').get;
                let traps=0;const fake=new Proxy({}, {get(){traps++;throw Error('trap');}});
                __reject(()=>getter.call(fake));__reject(()=>child.matches.call(fake,'.a'));
                __reject(()=>Object.getOwnPropertyDescriptor(child,'parentElement').get.call(fake));
                __reject(()=>child.getAttribute.call(fake,'class'));__check(traps===0);
                __check(child.classList.length===2);child.classList.add('c');__check(child.classList.value==='a b c');__check(__allocations===2);
                __check(new DOMException('message','SyntaxError').name==='SyntaxError');__check(__allocations===3);
                for(const name of ['targetState','windowTarget','documentTarget','targets','StateMap','eventState'])__check(Function('return typeof '+name)()==='undefined');
                const log=value=>function(event){__check(this===event.currentTarget&&event.target===child);__log.push(value);};
                addEventListener.call(null,'click',log('window-capture'),true);
                document.addEventListener('click',log('document-capture'),true);
                parent.addEventListener('click',log('parent-capture'),true);
                child.addEventListener('click',log('target-capture'),true);
                child.addEventListener('click',log('once'),{once:true});
                child.addEventListener('click',event=>{event.preventDefault();__log.push('passive');},{passive:true});
                child.onclick=function(event){__check(this===child&&event.eventPhase===2);__log.push('handler');return false;};
                parent.addEventListener('click',log('parent-bubble'));
                document.addEventListener('click',log('document-bubble'));
                addEventListener.call(null,'click',log('window-bubble'));
                const removed=()=>{throw Error('removed');};child.addEventListener('click',removed);child.removeEventListener('click',removed);
            })();
        `, context, {timeout: 1000});
        assert.equal(dispatch('click', [2, 1]), true);
        assert.equal(dispatch('click', [2, 1]), true);
        const log = JSON.parse(vm.runInContext('JSON.stringify(__log)', context));
        const route = ['window-capture', 'document-capture', 'parent-capture', 'target-capture', 'passive', 'handler', 'parent-bubble', 'document-bubble', 'window-bubble'];
        assert.deepEqual(log, ['ready', 'load', ...route.slice(0, 4), 'once', ...route.slice(4), ...route]);
        assert.equal(context.__allocations, 3);
        observations.push({calls, classes, log});
        cases.push({file: file.name, kind, checks: context.__checks + 10, eagerMaps: eager, finalMaps: context.__allocations});
    }
    assert.deepEqual(observations[1], observations[0]);
}
const result = {passed: true, sourceProofSha256: hash(proof), testSha256: hash(fs.readFileSync(__filename)), checks: cases.reduce((n, row) => n + row.checks, 0), cases, scope: 'Node mock equivalence only: allocation counts, early invalid receivers/proxies, captured intrinsics, direct document/window events, element brands and event routing. Not Lea/Jint, timing, sandbox or release qualification.'};
if (process.argv[2]) fs.writeFileSync(process.argv[2], JSON.stringify(result, null, 2) + '\n', {flag: 'wx'});
process.stdout.write(JSON.stringify(result) + '\n');
