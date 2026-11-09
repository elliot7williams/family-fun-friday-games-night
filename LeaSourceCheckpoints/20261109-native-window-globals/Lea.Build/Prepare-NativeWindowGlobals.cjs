'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const assert = require('node:assert/strict');
const {createRequire} = require('node:module');
const load = createRequire('I:/LeaBuildStorage/window-global-tools-20261109-v1/package.json');
const acorn = load('acorn');
const hash = value => crypto.createHash('sha256').update(value).digest('hex').toUpperCase();
const root = path.resolve(__dirname, '..');
const base = 'I:/LeaBuildStorage/private-binding-full-assembly-20261103-v5-assets';
const profile = 'I:/LeaBuildStorage/current-bootstrap-profile-source-20261109-v1';
const output = 'I:/LeaBuildStorage/native-window-globals-source-20261109-v1';
const names = ['setTimeout','setInterval','clearTimeout','clearInterval','requestAnimationFrame','cancelAnimationFrame'];
const parse = code => acorn.parse(code, {ecmaVersion: 2022, sourceType: 'script'});
function key(node) { return node.type === 'Identifier' ? node.name : node.value; }
function project(source) {
    assert.ok(!source.includes('__wg'), 'Already projected resource');
    const ast = parse(source), matches = [];
    function visit(node) {
        if (!node || typeof node !== 'object') return;
        if (node.type === 'ExpressionStatement') {
            const call = node.expression;
            if (call.type === 'CallExpression' && call.callee.type === 'MemberExpression' && !call.callee.computed &&
                call.callee.object.name === 'Object' && call.callee.property.name === 'defineProperties' &&
                call.arguments.length === 2 && call.arguments[0].type === 'Identifier' && call.arguments[0].name === 'globalThis' &&
                call.arguments[1].type === 'ObjectExpression' && call.arguments[1].properties.map(p => key(p.key)).join('|') === names.join('|')) matches.push(node);
        }
        for (const value of Object.values(node)) {
            if (Array.isArray(value)) value.forEach(visit);
            else if (value && typeof value === 'object') visit(value);
        }
    }
    visit(ast);
    assert.equal(matches.length, 1, 'One fixed window descriptor installation required');
    const statement = matches[0], functions = [];
    for (const [index, property] of statement.expression.arguments[1].properties.entries()) {
        assert.equal(property.computed, false);
        assert.equal(property.value.type, 'ObjectExpression');
        const fields = property.value.properties;
        assert.equal(fields.length, 4);
        assert.deepEqual(fields.map(p => key(p.key)).sort(), ['configurable','enumerable','value','writable']);
        for (const flag of ['writable','enumerable','configurable']) {
            const field = fields.find(p => key(p.key) === flag);
            assert.equal(field.computed, false);
            assert.equal(field.value.type, 'Literal');
            assert.equal(field.value.value, true);
        }
        const value = fields.find(p => key(p.key) === 'value').value;
        assert.equal(value.type, 'FunctionExpression');
        assert.equal(value.id.name, names[index]);
        functions.push(source.slice(value.start, value.end));
    }
    const original = source.slice(statement.start, statement.end);
    const replacement = `__wg(${functions.join(',')});delete globalThis.__wg;`;
    const code = source.slice(0, statement.start) + replacement + source.slice(statement.end);
    assert.equal(code.slice(0, statement.start) + original + code.slice(statement.start + replacement.length), source);
    parse(code);
    return {code, original, replacement, functions, start: statement.start, end: statement.end};
}
assert.ok(!fs.existsSync(output), 'Preserve candidate source');
const proofBytes = fs.readFileSync(`${base}/assembly.json`);
assert.equal(hash(proofBytes), 'F647679D939FD32355585B61561761BAFAB7B6239D83601A0FFE23B0BCF21DEA');
const baseline = JSON.parse(proofBytes);
const profileProof = fs.readFileSync(`${profile}/source.json`);
assert.equal(hash(profileProof), 'D74BB7C6812012197DFC11A803C13E493796D40685E953E1B6194ED9624F2968');
const host = JSON.parse(profileProof).files.find(f => f.name === '8.cs');
assert.equal(hash(fs.readFileSync(host.path)), host.sha256);
const originalHost = fs.readFileSync(host.path, 'utf8');
const anchor = 'new Dc().Bind(vm);';
assert.equal(originalHost.split(anchor).length, 2);
assert.ok(!originalHost.includes('new Gd().Bind(vm);'));
const newline = originalHost.includes('\r\n') ? '\r\n' : '\n';
const hostReplacement = `new Gd().Bind(vm);${newline}            ${anchor}`;
const candidateHost = originalHost.replace(anchor, hostReplacement);
assert.equal(candidateHost.replace(hostReplacement, anchor), originalHost);
const planned = [];
let negativeControls = 0;
for (const name of ['Dom.js','Dom.compact.js']) {
    const member = baseline.files.find(f => f.path === `fixtures/Lea.Script/${name}`);
    const originalPath = path.join(base, member.path);
    const bytes = fs.readFileSync(originalPath);
    assert.equal(hash(bytes), member.sha256);
    const source = bytes.toString('utf8'), result = project(source);
    const invalid = [source.slice(0,result.start) + source.slice(result.end), source + '\n' + result.original, result.code,
        source.slice(0,result.start) + result.original.replace(/writable\s*:\s*true/, 'writable:false') + source.slice(result.end)];
    for (const bad of invalid) { assert.throws(() => project(bad)); negativeControls++; }
    assert.equal(result.functions.length, 6);
    planned.push({name, code:result.code, original:originalPath, originalSha256:member.sha256, savedBytes:bytes.length-Buffer.byteLength(result.code)});
}
fs.mkdirSync(output);
const files = [];
for (const item of planned) {
    const to = path.join(output, item.name);
    fs.writeFileSync(to, item.code, {flag:'wx'});
    files.push({name:item.name,path:to,sha256:hash(fs.readFileSync(to)),original:item.original,originalSha256:item.originalSha256,savedBytes:item.savedBytes});
}
fs.writeFileSync(`${output}/8.cs`, candidateHost, {flag:'wx'});
fs.copyFileSync(path.join(__dirname,'WindowGlobals.cs'), `${output}/WindowGlobals.cs`, fs.constants.COPYFILE_EXCL);
const result = {verified:true,inverseVerified:true,negativeControls,files,
    host:{path:`${output}/8.cs`,sha256:hash(fs.readFileSync(`${output}/8.cs`)),originalSha256:host.sha256},
    installer:{path:`${output}/WindowGlobals.cs`,sha256:hash(fs.readFileSync(`${output}/WindowGlobals.cs`))},
    sourceOnly:true,nativeExecuted:false,policiesChanged:false,functionBodiesPreserved:true,globalOrder:names,
    scriptSha256:hash(fs.readFileSync(__filename)),parserVersion:load('acorn/package.json').version,
    lockSha256:hash(fs.readFileSync('I:/LeaBuildStorage/window-global-tools-20261109-v1/package-lock.json'))};
fs.writeFileSync(`${output}/source.json`,JSON.stringify(result,null,2)+'\n',{flag:'wx'});
process.stdout.write(JSON.stringify({sourceSha256:hash(fs.readFileSync(`${output}/source.json`)),negativeControls,savedBytes:files.map(f=>({name:f.name,bytes:f.savedBytes}))})+'\n');
