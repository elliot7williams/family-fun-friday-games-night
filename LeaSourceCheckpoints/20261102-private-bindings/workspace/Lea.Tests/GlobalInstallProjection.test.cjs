'use strict';
const fs = require('node:fs');
const vm = require('node:vm');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');

const folder = 'I:/LeaBuildStorage/global-install-source-20261102-v1';
const proof = fs.readFileSync(`${folder}/source.json`);
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
assert.equal(hash(proof), '8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5');
const plan = JSON.parse(proof);
assert.equal(plan.files.length, 2);
assert.equal(plan.negativeControls, 6);
const cases = [];
const checks = String.raw`
    const makeObject = Object.create;
    let count = 0;
    function check(value) { count++; if (!value) throw new Error('Check '+count); }
    function throws(action, kind) { let error; try { action(); } catch (caught) { error = caught; } check(error instanceof kind); }
    const names = ['queueMicrotask','setTimeout','setInterval','clearTimeout','clearInterval','requestAnimationFrame','cancelAnimationFrame'];
    const lengths = [1,1,1,0,0,1,1];
    check(Reflect.ownKeys(globalThis).filter(key => names.includes(key)).join('|') === names.join('|'));
    for (let i=0; i<names.length; i++) {
        const descriptor = Object.getOwnPropertyDescriptor(globalThis, names[i]);
        check(typeof descriptor.value === 'function');
        check(descriptor.value.name === names[i]);
        check(descriptor.value.length === lengths[i]);
        check(descriptor.writable === (i !== 0));
        check(descriptor.enumerable === (i !== 0));
        check(descriptor.configurable === (i !== 0));
    }
    throws(() => queueMicrotask(), TypeError);
    throws(() => queueMicrotask('code'), TypeError);
    throws(() => setTimeout(), TypeError);
    throws(() => setTimeout(Symbol()), TypeError);
    throws(() => setInterval('x'.repeat(16385)), RangeError);
    throws(() => setTimeout(()=>{},0,...Array(65)), RangeError);
    function callback() {}
    const timeout = setTimeout(callback, -10, 'a', 7);
    check(timers[timeout].callback === callback);
    check(timers[timeout].args.join('|') === 'a|7');
    check(timers[timeout].repeat === false);
    check(__calls.at(-1).join('|') === 'ts|0|0');
    const interval = setInterval('test', 12);
    check(timers[interval].callback === 'test');
    check(timers[interval].repeat === true);
    check(__calls.at(-1).join('|') === 'ts|12|1');
    clearTimeout(timeout);
    check(!(timeout in timers));
    check(__calls.at(-1).join('|') === 'tc|'+timeout);
    clearInterval(interval);
    check(!(interval in timers));
    const beforeClear = __calls.length;
    clearTimeout(); clearInterval(-1);
    check(__calls.length === beforeClear);
    throws(() => requestAnimationFrame.call({}, callback), TypeError);
    throws(() => requestAnimationFrame(null), TypeError);
    throws(() => cancelAnimationFrame.call({}, 1), TypeError);
    throws(() => cancelAnimationFrame(), TypeError);
    const frame = requestAnimationFrame(callback);
    check(frames[frame] === callback);
    check(__calls.at(-1).join('|') === 'rs');
    cancelAnimationFrame(frame);
    check(!(frame in frames));
    check(__calls.at(-1).join('|') === 'rc|'+frame);
    const beforeCancel = __calls.length;
    cancelAnimationFrame(0); cancelAnimationFrame(-1);
    check(__calls.length === beforeCancel);
    globalThis.__result = { checks: count, calls: __calls };
`;
for (const file of plan.files) {
    const observations = [];
    for (const [kind, path, expected] of [['original',file.original,file.originalSha256],['candidate',file.path,file.sha256]]) {
        const bytes = fs.readFileSync(path);
        assert.equal(hash(bytes), expected);
        const source = bytes.toString('utf8');
        new vm.Script(source, {filename:path});
        const anchor = file.name === 'Dom.js' ? '    const targets = new WeakMap();' : 'const targets=new WeakMap;';
        assert.equal(source.split(anchor).length, 2);
        const calls = [];
        let id = 0;
        const context = vm.createContext({__calls:calls, __lea:(...args)=>{calls.push(args);return ++id;}, __jobs:()=>{}});
        new vm.Script(source.slice(0,source.indexOf(anchor)) + checks + '\n}').runInContext(context, {timeout:1000});
        const result = JSON.parse(JSON.stringify(context.__result));
        observations.push(result);
        cases.push({file:file.name,kind,checks:result.checks});
    }
    assert.deepEqual(observations[1],observations[0]);
}
const result = {passed:true,sourceProofSha256:hash(proof),testSha256:hash(fs.readFileSync(__filename)),cases,checks:cases.reduce((sum,item)=>sum+item.checks,0),scope:'Node VM prefix behavior only; full bootstraps syntax checked. Not Lea/Jint execution, performance, worker, rendering or release qualification.'};
if (process.argv[2]) fs.writeFileSync(process.argv[2], JSON.stringify(result,null,2)+'\n', {flag:'wx'});
process.stdout.write(JSON.stringify(result)+'\n');
