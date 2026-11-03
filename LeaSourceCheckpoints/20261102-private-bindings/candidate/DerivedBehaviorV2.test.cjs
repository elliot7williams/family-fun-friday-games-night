'use strict';
const fs = require('node:fs');
const vm = require('node:vm');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const folder = 'I:/LeaBuildStorage/private-binding-source-20261102-v1';
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const proof = fs.readFileSync(`${folder}/source.json`);
assert.equal(hash(proof),'9CD78B418ED5CE1216123B7093CD0CB3593EF0D7812414FF4D35D718CC266BA2');
const plan = JSON.parse(proof);
assert.equal(plan.negativeControls,4);
assert.equal(plan.structuralAstVerified,true);
const names = ['INDEX_SIZE_ERR','DOMSTRING_SIZE_ERR','HIERARCHY_REQUEST_ERR','WRONG_DOCUMENT_ERR','INVALID_CHARACTER_ERR','NO_DATA_ALLOWED_ERR','NO_MODIFICATION_ALLOWED_ERR','NOT_FOUND_ERR','NOT_SUPPORTED_ERR','INUSE_ATTRIBUTE_ERR','INVALID_STATE_ERR','SYNTAX_ERR','INVALID_MODIFICATION_ERR','NAMESPACE_ERR','INVALID_ACCESS_ERR','VALIDATION_ERR','TYPE_MISMATCH_ERR','SECURITY_ERR','NETWORK_ERR','ABORT_ERR','URL_MISMATCH_ERR','QUOTA_EXCEEDED_ERR','TIMEOUT_ERR','INVALID_NODE_TYPE_ERR','DATA_CLONE_ERR'];
const cases = [];
for (const file of plan.files) {
    const observations = [];
    for (const [kind,path,expected] of [['original',file.original,file.originalSha256],['candidate',file.path,file.sha256]]) {
        const bytes = fs.readFileSync(path);
        assert.equal(hash(bytes),expected);
        const stages = [], calls = [];
        let next = 0, title = 'Fixture', classes = 'a b';
        const bridge = (op,...args) => {
            calls.push([op,...args]);
            switch (op) {
                case 'ts': case 'rs': return ++next;
                case 'tc': case 'rc': case 're': case 'te': return null;
                case 'id': return args[0] === 'child' ? 2 : null;
                case 'co': return args[0] === null || args[0] === args[1];
                case 'attr': return args[1] === 'class' ? classes : null;
                case 'put': if (args[1] === 'cn') classes = args[2]; else if (args[1] === 't') title = args[2]; return true;
                case 't': return title;
                case 'nw': return 10;
                case 'to': return 1000;
                case 'og': return 'null';
                default: throw Error('Unexpected mocked bridge operation '+op);
            }
        };
        const context = vm.createContext({__lea:bridge,__jobs:(...args)=>{if(args.length) stages.push(args[0]);}});
        vm.runInContext(`(()=>{const NativeMap=WeakMap,build=Reflect.construct;globalThis.__mapAllocations=0;globalThis.WeakMap=new Proxy(NativeMap,{construct(target,args,newTarget){globalThis.__mapAllocations++;return build(target,args,newTarget);}});})();`,context,{timeout:1000});
        vm.runInContext(`globalThis.__dc=(constructor,prototype)=>{for(const [index,name] of ${JSON.stringify(names)}.entries()){Object.defineProperty(constructor,name,{value:index+1,enumerable:true});Object.defineProperty(prototype,name,{value:index+1,enumerable:true});}}`,context,{timeout:1000});
        const dispatch = new vm.Script(bytes.toString('utf8'),{filename:path}).runInContext(context,{timeout:1000});
        assert.equal(typeof dispatch,'function');
        assert.equal(dispatch.name,'dispatch');
        assert.equal(dispatch.length,3);
        assert.deepEqual(stages,['declarations','scope','events','interfaces','elements','document']);
        const eager = vm.runInContext('__mapAllocations',context);
        assert.equal(eager,3);
        context.__expectedAfterList = 3;
        const earlyChecks = vm.runInContext(String.raw`
            (()=>{
                let checks=0,traps=0;
                function reject(action){let error;try{action();}catch(caught){error=caught;}checks++;if(!(error instanceof TypeError))throw Error('Invalid early receiver accepted');}
                const fake=new Proxy({}, {get(){traps++;throw Error('Proxy trap');}});
                reject(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'length').get.call({}));
                reject(()=>Object.getOwnPropertyDescriptor(DOMException.prototype,'name').get.call(fake));
                checks++;if(traps!==0)throw Error('Exception receiver trap invoked');
                reject(()=>Object.getOwnPropertyDescriptor(DOMTokenList.prototype,'length').get.call(fake));
                checks++;if(traps!==0)throw Error('Token receiver trap invoked');
                const savedMap=WeakMap,savedGet=WeakMap.prototype.get,savedSet=WeakMap.prototype.set,savedApply=Reflect.apply;
                globalThis.__restoreMap=()=>{globalThis.WeakMap=savedMap;savedMap.prototype.get=savedGet;savedMap.prototype.set=savedSet;Reflect.apply=savedApply;};
                globalThis.WeakMap=function(){throw Error('Public constructor used after bootstrap');};
                savedMap.prototype.get=savedMap.prototype.set=Reflect.apply=function(){throw Error('Public intrinsic used after bootstrap');};
                return checks;
            })()
        `,context,{timeout:1000});
        assert.equal(context.__mapAllocations,eager);
        const count = vm.runInContext(String.raw`
            (()=>{
                let checks=0;
                function check(value){checks++;if(!value)throw Error('Check '+checks);}
                function throws(action,kind){let error;try{action();}catch(caught){error=caught;}check(error instanceof kind);}
                check(!('__lea' in globalThis)&&!('__jobs' in globalThis)&&!('__dc' in globalThis));
                for(const name of ['call','checkpoint','failed','timers','frames','cache','targets','nodeKey','eventState','dispatch','StateMap']) check(Function('return typeof '+name)()==='undefined');
                check(window===globalThis);
                check(document.isConnected&&document.contains(document)&&!document.contains(null));
                const child=document.getElementById('child');
                check(child===document.getElementById('child')&&child.isConnected&&document.contains(child));
                const descriptor=Object.getOwnPropertyDescriptor(child,'isConnected');
                check(descriptor.enumerable&&descriptor.configurable&&descriptor.set===undefined);
                throws(()=>descriptor.get.call({}),TypeError);
                let traps=0;throws(()=>descriptor.get.call(new Proxy({}, {get(){traps++;}})),TypeError);check(traps===0);
                const list=child.classList;check(list===child.classList&&list.length===2&&list.contains('a'));check(__mapAllocations===__expectedAfterList);
                list.add('c');check([...list].join('|')==='a|b|c');list.remove('a');check(list.value==='b c');
                throws(()=>list.add('a b'),DOMException);
                const exception=new DOMException('fail','SyntaxError');check(exception.code===12&&String(exception)==='SyntaxError: fail');check(__mapAllocations===3);
                for(const name of Object.keys(DOMException)){const a=Object.getOwnPropertyDescriptor(DOMException,name),b=Object.getOwnPropertyDescriptor(DOMException.prototype,name);check(a.value===b.value&&!a.writable&&!a.configurable&&a.enumerable&&!b.writable&&!b.configurable&&b.enumerable);}
                check(Object.prototype.toString.call(list)==='[object DOMTokenList]');
                const timing=Object.getOwnPropertyDescriptor(globalThis,'performance').get.call(globalThis);
                check(Object.prototype.toString.call(timing)==='[object Performance]');
                check(timing.now()===10&&timing.timeOrigin===1000);
                throws(()=>Performance(),TypeError);throws(()=>DOMTokenList(),TypeError);
                throws(()=>queueMicrotask(null),TypeError);throws(()=>setTimeout(),TypeError);throws(()=>requestAnimationFrame.call({},()=>{}),TypeError);
                globalThis.__timerResult='';globalThis.__timer=setTimeout(function(value){'use strict';if(this!==window)throw Error('timer receiver');globalThis.__timerResult=value;},0,'done');
                globalThis.__frameResult=0;globalThis.__frame=requestAnimationFrame(function(value){'use strict';if(this!==window)throw Error('frame receiver');globalThis.__frameResult=value;});
                globalThis.__loads=0;addEventListener('load',function(){globalThis.__loads++;});
                return checks;
            })()
        `,context,{timeout:1000});
        assert.equal(dispatch('tm',[context.__timer]),false);
        assert.equal(context.__timerResult,'done');
        assert.equal(dispatch('rf',[context.__frame],17),false);
        assert.equal(context.__frameResult,17);
        assert.equal(dispatch('load',[]),false);
        assert.equal(context.__loads,1);
        assert.equal(dispatch('mt',[]),false);
        assert.equal(context.__mapAllocations,3);
        vm.runInContext('__restoreMap()',context,{timeout:1000});
        const apiMetadata=vm.runInContext("\n    JSON.stringify([globalThis,document,document.getElementById('child'),document.getElementById('child').style,document.getElementById('child').classList,DOMTokenList.prototype,DOMException.prototype,Performance.prototype].map(owner=>Reflect.ownKeys(owner).map(key=>{\n        const d=Object.getOwnPropertyDescriptor(owner,key);\n        const fn=value=>typeof value==='function'?[value.name,value.length]:null;\n        return [String(key),d.enumerable,d.configurable,d.writable,fn(d.value),fn(d.get),fn(d.set)];\n    })))\n",context,{timeout:1000}); observations.push({count,calls:JSON.parse(JSON.stringify(calls)),title,classes,apiMetadata});
        cases.push({file:file.name,kind,checks:count+11+earlyChecks+3,eagerMaps:eager,finalMaps:context.__mapAllocations});
    }
    assert.deepEqual(observations[1],observations[0]);
}
const result={passed:true,sourceProofSha256:hash(proof),testSha256:hash(fs.readFileSync(__filename)),cases,checks:cases.reduce((sum,item)=>sum+item.checks,0),apiMetadataComparisons:2,scope:'Derived unchanged 312-check bootstrap mock suite with three eager maps in both variants, plus public descriptors/function names/arity equality. Node only; not Jint, sandbox, timing or release qualification.'};
if(process.argv[2])fs.writeFileSync(process.argv[2],JSON.stringify(result,null,2)+'\n',{flag:'wx'});
process.stdout.write(JSON.stringify(result)+'\n');
