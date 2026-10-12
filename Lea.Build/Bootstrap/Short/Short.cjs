"use strict";
const fs = require("node:fs");
const path = require("node:path");
const crypto = require("node:crypto");

async function main() {
    const flags = new Map();
    const aliases = { "-t": "--self-test", "-i": "--input", "-o": "--out", "-d": "--deps" };
    for (let i = 2; i < process.argv.length; i++) {
        const flag = aliases[process.argv[i]] || process.argv[i];
        if (!["--self-test", "--input", "--out", "--deps"].includes(flag) || flags.has(flag)) throw Error("Unknown or duplicate option.");
        const value = flag === "--self-test" ? true : process.argv[++i];
        if (value === undefined || typeof value === "string" && value.startsWith("-")) throw Error("Missing option value.");
        flags.set(flag, value);
    }
    const deps = flags.get("--deps") || path.join(__dirname, "node_modules");
    if (!path.isAbsolute(deps)) throw Error("Use an absolute dependency directory.");
    const load = name => require(require.resolve(name, { paths: [deps] }));
    for (const [name, version] of Object.entries({ acorn: "8.15.0", "eslint-scope": "8.4.0", terser: "5.46.1" })) {
        if (load(name + "/package.json").version !== version) throw Error("Unexpected developer dependency version: " + name);
    }
    const acorn = load("acorn"), scope = load("eslint-scope"), terser = load("terser");
    const options = {
        ecma: 2020, compress: false, module: false, toplevel: false,
        keep_fnames: true, keep_classnames: true,
        mangle: { eval: false, properties: false, toplevel: false },
        format: { ascii_only: true, comments: false, semicolons: true }
    };
    const ignored = new Set(["start", "end", "loc", "range", "raw", "shorthand"]);

    function fingerprint(source) {
        const ast = acorn.parse(source, { ecmaVersion: 2022, sourceType: "script", ranges: true });
        const locations = new WeakMap();
        function locate(node, location) {
            if (!node || typeof node !== "object") return;
            if (node.type) {
                if (!locations.has(node)) locations.set(node, location);
                if (node.type === "WithStatement" || node.type === "CallExpression" && node.callee.type === "Identifier" && node.callee.name === "eval")
                    throw Error("Direct eval/with is not supported by this experimental verifier.");
            }
            for (const key of Object.keys(node).filter(key => !ignored.has(key))) {
                if (Array.isArray(node[key])) node[key].forEach((child, i) => locate(child, location + "." + key + "[" + i + "]"));
                else if (node[key] && typeof node[key] === "object") locate(node[key], location + "." + key);
            }
        }
        locate(ast, "$root");
        const manager = scope.analyze(ast, { ecmaVersion: 2022, sourceType: "script", optimistic: false, ignoreEval: false });
        const bindings = new Map(), names = new WeakMap();
        manager.scopes.forEach((current, index) => {
            for (const variable of current.variables) {
                const declaration = variable.identifiers[0];
                const id = "scope:" + index + ":" + (declaration ? locations.get(declaration) : "implicit:" + variable.name);
                bindings.set(variable, id);
                for (const identifier of variable.identifiers) names.set(identifier, id);
            }
        });
        for (const current of manager.scopes) {
            for (const reference of current.references) names.set(reference.identifier,
                reference.resolved ? bindings.get(reference.resolved) : "global:" + reference.identifier.name);
        }
        function normalize(node, parent, field) {
            if (node === null || typeof node !== "object") return typeof node === "bigint" ? { bigint: String(node) } : node;
            if (Array.isArray(node)) return node.map(child => normalize(child, parent, field));
            if (node.type === "Identifier") {
                const literal = parent && ((field === "key" && !parent.computed && ["Property", "MethodDefinition", "PropertyDefinition"].includes(parent.type)) ||
                    field === "property" && !parent.computed && parent.type === "MemberExpression" || parent.type === "MetaProperty" || field === "label");
                return { type: "Identifier", name: literal ? "property:" + node.name : names.get(node) || "unresolved:" + node.name };
            }
            const result = {};
            for (const key of Object.keys(node).filter(key => !ignored.has(key)).sort()) result[key] = normalize(node[key], node, key);
            if (["FunctionDeclaration", "FunctionExpression", "ClassDeclaration", "ClassExpression"].includes(node.type) && node.id)
                result.preservedName = node.id.name;
            if (node.type === "VariableDeclarator" && node.id.type === "Identifier" && node.init &&
                (node.init.type === "ArrowFunctionExpression" || node.init.type === "FunctionExpression" && !node.init.id)) result.inferredName = node.id.name;
            return result;
        }
        return JSON.stringify({ scopes: manager.scopes.map(item => ({ type: item.type, strict: item.isStrict })), ast: normalize(ast) });
    }
    function equivalent(original, candidate) {
        if (fingerprint(original) !== fingerprint(candidate)) throw Error("Candidate changed structure, bindings, public names or inferred function names.");
    }
    const positive = [
        ["'use strict';{const longerName=1;longerName;}", "'use strict';{const a=1;a;}"],
        ["{let outer=1;function read(input){let inner=2;return outer+input+inner;}read(3);}", "{let x=1;function read(n){let y=2;return x+n+y;}read(3);}"],
        ["{const value=1;({value});}", "{const a=1;({value:a});}"],
        ["{let a=1;function read(a){return a;}read(a);}", "{let x=1;function read(y){return y;}read(x);}"]
    ];
    const negative = [
        [positive[0][0], "'use strict';{const a=2;a;}"],
        [positive[1][0], "{let x=1;function read(n){let y=2;return y+n+y;}read(3);}"],
        [positive[1][0], "{let x=1;function read(n){let y=2;return x-n+y;}read(3);}"],
        [positive[2][0], "{const a=1;({different:a});}"],
        [positive[1][0], "{let x=1;function renamed(n){let y=2;return x+n+y;}renamed(3);}"],
        ["{const callback=()=>1;callback();}", "{const a=()=>1;a();}"],
        ["Object.keys({});", "Reflect.keys({});"]
    ];
    for (const pair of positive) equivalent(...pair);
    for (const pair of negative) {
        let rejected = false; try { equivalent(...pair); } catch { rejected = true; }
        if (!rejected) throw Error("Verifier accepted a changed negative control.");
    }
    let directRejected = false; try { fingerprint("{let hidden=1;eval('hidden');}"); } catch { directRejected = true; }
    if (!directRejected) throw Error("Direct eval control was accepted.");
    console.log("PASS short-bootstrap verifier: 4 rename controls, 7 mutation controls, direct-eval rejection.");
    if (flags.has("--self-test")) {
        if (flags.has("--input") || flags.has("--out")) throw Error("Self-test does not accept input or output.");
        return;
    }
    const input = flags.get("--input"), out = flags.get("--out");
    if (!input || !out || !path.isAbsolute(input) || !path.isAbsolute(out)) throw Error("Supply absolute --input and new --out directory paths.");
    const bytes = fs.readFileSync(input);
    if (bytes.length > 128 * 1024 || bytes.subarray(0, 3).equals(Buffer.from([239, 187, 191]))) throw Error("Input must be bounded UTF-8 without a BOM.");
    const source = new TextDecoder("utf-8", { fatal: true }).decode(bytes);
    const first = (await terser.minify(source, options)).code;
    const second = (await terser.minify(source, options)).code;
    if (first !== second) throw Error("Generation was not deterministic.");
    equivalent(source, first);
    const output = Buffer.from(first + "\r\n", "utf8");
    if (output.length >= bytes.length) throw Error("Short output did not reduce the source bytes.");
    if (fs.existsSync(out) || !fs.statSync(path.dirname(out)).isDirectory()) throw Error("Supply a fresh output directory with an existing parent.");
    fs.mkdirSync(out);
    fs.writeFileSync(path.join(out, "Dom.short.js"), output, { flag: "wx" });
    const sha = data => crypto.createHash("sha256").update(data).digest("hex").toUpperCase();
    const report = {
        kind: "experimental-private-binding-mangle", input, inputBytes: bytes.length, outputBytes: output.length,
        inputSHA256: sha(bytes), outputSHA256: sha(output), fingerprintSHA256: sha(fingerprint(source)),
        terser: "5.46.1", acorn: "8.15.0", scopeAnalyzer: "8.4.0", options,
        controls: 12, bindingAwareStructureMatched: true, deterministic: true,
        normalGeneratorChanged: false, publicPropertyMangle: false, runtimeDependencyAdded: false,
        productionDefaultChanged: false, startupFixClaim: false, speedupClaim: false
    };
    fs.writeFileSync(path.join(out, "report.json"), JSON.stringify(report, null, 2) + "\n", { flag: "wx" });
    console.log("Short bootstrap: " + bytes.length + " -> " + output.length + " bytes; " + report.outputSHA256 + "; experimental only.");
}
main().catch(error => { console.error(error.stack || error); process.exitCode = 1; });
