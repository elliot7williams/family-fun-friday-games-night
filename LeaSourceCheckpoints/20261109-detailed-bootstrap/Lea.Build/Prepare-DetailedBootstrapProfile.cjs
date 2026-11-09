'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const {createRequire} = require('node:module');
const load = createRequire('I:/LeaBuildStorage/window-global-tools-20261109-v1/package.json');
const acorn = load('acorn');
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex').toUpperCase();
const base = 'I:/LeaBuildStorage/current-bootstrap-profile-source-20261109-v1';
const output = 'I:/LeaBuildStorage/detailed-bootstrap-profile-source-20261109-v1';
const resource = 'I:/LeaBuildStorage/private-binding-full-assembly-20261103-v5-assets/fixtures/Lea.Script/Dom.compact.js';
const labels = ['script-entry','block-entry','scope-ready','timers-begin','timers-end','events-ready','interfaces-ready','bootstrap-done'];
function project(source) {
    assert.ok(!source.includes('__phase'), 'Already projected or colliding helper');
    const ast = acorn.parse(source,{ecmaVersion:2022,sourceType:'script'});
    const blocks = ast.body.filter(n=>n.type==='BlockStatement');
    assert.equal(blocks.length,1);
    const block = blocks[0];
    assert.equal(ast.body[0].directive,'use strict');
    const capture = block.body[0].declarations.find(d=>d.init && d.init.name==='__jobs');
    assert.ok(capture && capture.id.type==='Identifier');
    function stage(label) {
        const found=block.body.filter(n=>n.type==='ExpressionStatement' && n.expression.type==='CallExpression' && n.expression.callee.name===capture.id.name && n.expression.arguments.length===1 && n.expression.arguments[0].value===label);
        assert.equal(found.length,1); return found[0];
    }
    const windows=block.body.filter(n=>n.type==='ExpressionStatement' && n.expression.type==='CallExpression' && n.expression.callee.type==='MemberExpression' && n.expression.callee.object.name==='Object' && n.expression.callee.property.name==='defineProperties' && n.expression.arguments[0].name==='globalThis' && n.expression.arguments[1].type==='ObjectExpression' && n.expression.arguments[1].properties.map(p=>p.key.name||p.key.value).join('|')==='setTimeout|setInterval|clearTimeout|clearInterval|requestAnimationFrame|cancelAnimationFrame');
    assert.equal(windows.length,1);
    const cleanup=block.body.filter(n=>n.type==='ExpressionStatement' && n.expression.type==='UnaryExpression' && n.expression.operator==='delete' && n.expression.argument.type==='MemberExpression' && n.expression.argument.object.name==='globalThis' && n.expression.argument.property.name==='__jobs');
    assert.equal(cleanup.length,1);
    const offsets=[ast.body[0].end,block.start+1,stage('scope').end,windows[0].start,windows[0].end,stage('events').start,stage('interfaces').start,cleanup[0].start];
    assert.ok(offsets.every((p,i)=>i===0||p>offsets[i-1]),'Strict marker ordering');
    const inserts=offsets.map((offset,i)=>({offset,text:`\n__phase(${JSON.stringify(labels[i])});${i===labels.length-1?'delete globalThis.__phase;':''}\n`}));
    let code=source;
    for (const item of [...inserts].reverse()) code=code.slice(0,item.offset)+item.text+code.slice(item.offset);
    let inverse=code;
    for (const item of inserts) { assert.equal(inverse.split(item.text).length,2); inverse=inverse.replace(item.text,''); }
    assert.equal(inverse,source);
    acorn.parse(code,{ecmaVersion:2022,sourceType:'script'});
    return {code,inserts};
}
assert.ok(!fs.existsSync(output),'Preserve existing source');
assert.equal(hash(fs.readFileSync(`${base}/source.json`)),'D74BB7C6812012197DFC11A803C13E493796D40685E953E1B6194ED9624F2968');
const hostPath=`${base}/8.cs`, host=fs.readFileSync(hostPath,'utf8');
assert.equal(hash(Buffer.from(host)),'622FB7A291C118D5D676C385C71BF0839E2DE7EF110FD35DC2AF38735E3D26ED');
const source=fs.readFileSync(resource,'utf8');
assert.equal(hash(Buffer.from(source)),'196BCE962F2C2F375C77644D0AE19485820F599170D4D80013829A6ECD2886E3');
const projected=project(source);
let negativeControls=0;
for (const bad of [projected.code,source.replace(/(['"])scope\1/,'"missing"'),source+source,source.replace(/setTimeout\s*:/,'missingTimeout:')]) {
    assert.notEqual(bad,source,'Malformed control must actually change source');
    assert.throws(()=>project(bad));negativeControls++;
}
const anchor='            new Dc().Bind(vm);';
assert.equal(host.split(anchor).length,2);
const newline=host.includes('\r\n')?'\r\n':'\n';
const binding=`            var detailLabels = new[] { ${labels.map(s=>JSON.stringify(s)).join(', ')} };
            var detailNext = 0;
            vm.SetValue("__phase", (Jint.Native.JsValue)new ClrFunction(vm, "__phase", (_, args) =>
            {
                if (detailNext >= detailLabels.Length || args.Length != 1 || !args[0].IsString() || args[0].AsString() != detailLabels[detailNext])
                    throw new InvalidDataException("Invalid detailed bootstrap marker.");
                profileMark("detail-" + detailLabels[detailNext++]);
                return Jint.Native.JsValue.Undefined;
            }));
${anchor}`.replace(/\r?\n/g,newline);
let updated=host.replace(anchor,binding);
const evaluate='try { dispatch = vm.Evaluate(syntax); }';
assert.equal(updated.split(evaluate).length,2);
const checked=evaluate.replace(' }',' if (detailNext != detailLabels.Length) throw new InvalidDataException("Incomplete detailed bootstrap markers."); }');
updated=updated.replace(evaluate,checked);
assert.equal(updated.replace(checked,evaluate).replace(binding,anchor),host);
fs.mkdirSync(output);
const files=[];
for (const [name,code] of [['8.cs',updated],['Dom.compact.js',projected.code]]) {
    const target=path.join(output,name);fs.writeFileSync(target,code,{flag:'wx'});
    files.push({name,path:target,sha256:hash(fs.readFileSync(target))});
}
const result={verified:true,inverseVerified:true,criticalOperationsVerified:true,negativeControls,budgetsChanged:false,files,labels,
    originalResourceSHA256:hash(Buffer.from(source)),originalHostSHA256:hash(Buffer.from(host)),
    scriptSHA256:hash(fs.readFileSync(__filename)),parserVersion:load('acorn/package.json').version,
    lockSHA256:hash(fs.readFileSync('I:/LeaBuildStorage/window-global-tools-20261109-v1/package-lock.json')),
    nativeExecuted:false,diagnosticOnly:true};
fs.writeFileSync(`${output}/source.json`,JSON.stringify(result,null,2)+'\n',{flag:'wx'});
console.log(JSON.stringify({sourceSHA256:hash(fs.readFileSync(`${output}/source.json`)),negativeControls,labels}));
