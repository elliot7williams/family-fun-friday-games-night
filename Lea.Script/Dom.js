(function (call) {
    'use strict';
    const cache = new Map();
    const styles = { color: 'fg', backgroundColor: 'bg', width: 'wd', height: 'ht', display: 'dp', fontSize: 'fs' };
    function put(key, name, value) {
        if (!call('put', key, name, String(value))) throw new TypeError('Unsupported Lea mutation');
    }
    function wrap(key) {
        if (key === null) return null;
        if (cache.has(key)) return cache.get(key);
        const node = {};
        const style = {};
        for (const name of Object.keys(styles)) Object.defineProperty(style, name, {
            set(value) { put(key, styles[name], value); }, enumerable: true
        });
        Object.defineProperties(node, {
            tagName: { get() { return call('tag', key); } },
            textContent: { get() { return call('text', key); }, set(value) { put(key, 'tx', value === null ? '' : value); } },
            value: { get() { return call('value', key); }, set(value) { put(key, 'vl', value); } },
            checked: { get() { return call('checked', key); }, set(value) { put(key, 'ck', value ? '1' : '0'); } },
            style: { value: style },
            getAttribute: { value(name) { return call('attr', key, String(name)); } },
            classList: { value: Object.freeze({
                add(name) { put(key, 'ac', name); },
                remove(name) { put(key, 'rc', name); },
                toggle(name) {
                    const before = (call('attr', key, 'class') || '').split(/\s+/).includes(String(name));
                    put(key, 'tc', name);
                    return !before;
                },
                contains(name) { return (call('attr', key, 'class') || '').split(/\s+/).includes(String(name)); }
            }) }
        });
        cache.set(key, node);
        return node;
    }
    const document = {};
    Object.defineProperties(document, {
        title: { get() { return call('t'); }, set(value) { put(null, 't', value); } },
        body: { get() { return wrap(call('q', 'body')); } },
        documentElement: { get() { return wrap(call('q', 'html')); } },
        getElementById: { value(id) { return wrap(call('id', String(id))); } },
        querySelector: { value(selector) { return wrap(call('q', String(selector))); } },
        querySelectorAll: { value(selector) { return call('qa', String(selector)).map(wrap); } }
    });
    Object.defineProperty(globalThis, 'document', { value: document });
    Object.defineProperty(globalThis, 'window', { value: globalThis });
    Object.defineProperty(globalThis, 'alert', { value(value) { put(null, 'a', value); } });
    delete globalThis.__lea;
})(__lea);
