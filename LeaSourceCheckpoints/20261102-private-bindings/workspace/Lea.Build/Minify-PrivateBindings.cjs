'use strict';
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const {createRequire} = require('node:module');
const assert = require('node:assert/strict');
const tools = 'I:/LeaBuildStorage/javascript-tools-20261102-v1';
const load = createRequire(`${tools}/package.json`);
const {minify} = load('terser');
const acorn = load('acorn');
const hash = value => crypto.createHash('sha256').update(value).digest('hex').toUpperCase();
const base = 'I:/LeaBuildStorage/global-install-source-20261102-v1/source.json';
const output = 'I:/LeaBuildStorage/private-binding-source-20261102-v1';
const options = {ecma: 2022, compress: false, mangle: {toplevel: true, eval: false, keep_fnames: true, keep_classnames: true}, keep_fnames: true, keep_classnames: true, format: {comments: false, ascii_only: true, semicolons: true}};

// A structural guard, not proof of lexical-binding equivalence. Public keys,
// function names, literals and operators remain significant; local names do not.
function shape(value, parent, key) {
    if (Array.isArray(value)) return value.filter(n => !(key === 'body' && n && n.type === 'EmptyStatement')).map(n => shape(n, parent, key));
    if (!value || typeof value !== 'object') return value;
    if (value.type === 'Identifier') {
        const publicName = parent && ((key === 'property' && parent.type === 'MemberExpression' && !parent.computed) || (key === 'id' && /^(Function|Class)/.test(parent.type)));
        return {type: 'Identifier', name: publicName ? value.name : '$binding'};
    }
    const result = {};
    for (const field of Object.keys(value).sort()) {
        if (['start', 'end', 'loc', 'range', 'raw', 'shorthand'].includes(field)) continue;
        if (field === 'key' && value.type === 'Property' && !value.computed) result.key = {type: 'PublicKey', name: value.key.type === 'Identifier' ? value.key.name : String(value.key.value)};
        else result[field] = shape(value[field], value, field);
    }
    return result;
}
const parse = code => acorn.parse(code, {ecmaVersion: 2022, sourceType: 'script'});
const signature = ast => JSON.stringify(shape(ast));
function mutate(ast, predicate, change) {
    const copy = structuredClone(ast);
    let found = false;
    function visit(node) {
        if (!node || typeof node !== 'object' || found) return;
        if (predicate(node)) { change(node); found = true; return; }
        for (const value of Object.values(node)) if (Array.isArray(value)) value.forEach(visit); else visit(value);
    }
    visit(copy); assert.equal(found, true); return copy;
}
async function main() {
    assert.equal(fs.existsSync(output), false, 'Preserve existing minification candidate');
    assert.equal(load('terser/package.json').version, '5.44.1');
    const lock = JSON.parse(fs.readFileSync(`${tools}/package-lock.json`));
    assert.equal(lock.packages['node_modules/terser'].integrity, 'sha512-t/R3R/n0MSwnnazuPpPNVO60LX0SKL45pyl9YlvxIdkH0Of7D5qM2EVe+yASRIlY5pZ73nclYJfNANGWPwFDZw==');
    const bytes = fs.readFileSync(base);
    assert.equal(hash(bytes), '8752B10BDD88F4AAFFFB016AFDC4EB0D49B4A56D054AA4738D461BAD415A46B5');
    const plan = JSON.parse(bytes);
    const input = plan.files.find(f => f.name === 'Dom.compact.js');
    const original = fs.readFileSync(input.path);
    assert.equal(hash(original), input.sha256);
    const first = await minify(original.toString('utf8'), options);
    const second = await minify(original.toString('utf8'), options);
    assert.equal(first.code, second.code, 'Deterministic minifier output required');
    const ast = parse(original.toString('utf8'));
    const expected = signature(ast);
    assert.equal(signature(parse(first.code)), expected, 'Structural AST guard differs');
    const controls = [
        mutate(ast, n => n.type === 'Literal' && n.value === 'co', n => { n.value = 'wrong-bridge'; }),
        mutate(ast, n => n.type === 'Property' && !n.computed && n.key.name === 'isConnected', n => { n.key.name = 'wrongProperty'; }),
        mutate(ast, n => /^(Function)/.test(n.type) && n.id && n.id.name === 'dispatch', n => { n.id.name = 'wrongDispatch'; }),
        mutate(ast, n => n.type === 'Literal' && n.value === 128, n => { n.value = 129; })
    ];
    for (const bad of controls) assert.notEqual(signature(bad), expected);
    assert.ok(Buffer.byteLength(first.code) < original.length, 'No source size improvement');
    fs.mkdirSync(output);
    const files = [];
    for (const file of plan.files) {
        const source = fs.readFileSync(file.path);
        assert.equal(hash(source), file.sha256);
        const target = path.join(output, file.name);
        const result = file.name === 'Dom.compact.js' ? Buffer.from(first.code) : source;
        fs.writeFileSync(target, result, {flag: 'wx'});
        assert.equal(hash(fs.readFileSync(target)), hash(result));
        files.push({name: file.name, path: target, sha256: hash(result), original: file.path, originalSha256: file.sha256, sdkOriginalSha256: file.sdkOriginalSha256});
    }
    const proof = {verified: true, candidateOnly: true, diagnosticOnly: false, runtimeTested: false, startupFixed: false, hostPolicyChanged: false, structuralAstVerified: true, lexicalEquivalenceProven: false, negativeControls: controls.length, deterministic: true, files, originalBytes: original.length, minifiedBytes: Buffer.byteLength(first.code), savedBytes: original.length - Buffer.byteLength(first.code), sourceProofSha256: hash(bytes), options, toolVersion: '5.44.1', lockSha256: hash(fs.readFileSync(`${tools}/package-lock.json`)), toolSha256: hash(fs.readFileSync(load.resolve('terser'))), scriptSha256: hash(fs.readFileSync(__filename)), scope: 'Private binding names only; no semantic compression or property mangling, function/class names retained. Structural guard is not complete binding-equivalence proof. Native and behavioral qualification pending.'};
    fs.writeFileSync(`${output}/source.json`, JSON.stringify(proof, null, 2) + '\n', {flag: 'wx'});
    process.stdout.write(JSON.stringify({sourceSha256: hash(fs.readFileSync(`${output}/source.json`)), originalBytes: proof.originalBytes, minifiedBytes: proof.minifiedBytes, savedBytes: proof.savedBytes, negativeControls: proof.negativeControls}) + '\n');
}
main().catch(error => { process.stderr.write(error.stack + '\n'); process.exitCode = 1; });
