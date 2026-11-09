'use strict';
const assert = require('node:assert/strict');

// Static structure only: node counts are not elapsed-time measurements.
function inventory(source, parse) {
    const ast = parse(source);
    const blocks = ast.body.filter(node => node.type === 'BlockStatement');
    assert.equal(blocks.length, 1, 'One trusted outer block required');
    const block = blocks[0];
    const bridge = block.body[0];
    assert.equal(bridge.type, 'VariableDeclaration');
    const checkpoint = bridge.declarations.find(d => d.init && d.init.type === 'Identifier' && d.init.name === '__jobs');
    assert.ok(checkpoint && checkpoint.id.type === 'Identifier', 'Captured checkpoint required');
    const name = checkpoint.id.name;
    const labels = ['scope', 'events', 'interfaces', 'elements', 'document'];
    const markers = [];
    for (const [index, statement] of block.body.entries()) {
        if (statement.type !== 'ExpressionStatement') continue;
        const call = statement.expression;
        if (call.type !== 'CallExpression' || call.callee.type !== 'Identifier' || call.callee.name !== name) continue;
        assert.equal(call.arguments.length, 1, 'One stage label required');
        assert.equal(call.arguments[0].type, 'Literal');
        markers.push({label: call.arguments[0].value, index, start: statement.start, end: statement.end});
    }
    assert.deepEqual(markers.map(m => m.label), labels, 'Exact stage sequence required');
    function count(node, type) {
        let total = node.type === type ? 1 : 0;
        for (const value of Object.values(node)) {
            if (Array.isArray(value)) {
                for (const child of value) if (child && typeof child.type === 'string') total += count(child, type);
            } else if (value && typeof value.type === 'string') total += count(value, type);
        }
        return total;
    }
    const declarations = block.body.filter(n => n.type === 'FunctionDeclaration').map(n => ({name:n.id.name, start:n.start, end:n.end, bytes:Buffer.byteLength(source.slice(n.start,n.end))}));
    const phases = markers.map((marker, i) => {
        const next = markers[i + 1];
        const statements = block.body.slice(marker.index + 1, next ? next.index : block.body.length);
        return {label:marker.label, start:marker.end, end:next ? next.start : block.end - 1,
            bytes:Buffer.byteLength(source.slice(marker.end,next ? next.start : block.end - 1)),
            directStatements:statements.length,
            directFunctions:statements.filter(n=>n.type==='FunctionDeclaration').map(n=>n.id.name),
            nestedFunctionNodes:statements.reduce((sum,n)=>sum+count(n,'FunctionExpression')+count(n,'ArrowFunctionExpression'),0),
            note:'Lexical function declarations may be instantiated at block entry, not in this textual span. Nested bodies are not necessarily executed here.'};
    });
    return {checkpointName:name, blockEntry:block.start, firstMarker:markers[0].start,
        directFunctionDeclarations:declarations, phases, sourceBytes:Buffer.byteLength(source)};
}
module.exports = {inventory};
