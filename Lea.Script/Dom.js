(function (call) {
    'use strict';
    const cache = new Map();
    const targets = new WeakMap();
    const types = new Set(['click', 'input', 'change', 'submit', 'DOMContentLoaded', 'load']);
    let listenerCount = 0;
    let ready = 'loading';
    const styles = { color: 'fg', backgroundColor: 'bg', width: 'wd', height: 'ht', display: 'dp', fontSize: 'fs' };
    function capture(options) { return typeof options === 'boolean' ? options : !!(options && options.capture); }
    function remove(state, listener) {
        if (listener.removed) return;
        listener.removed = true;
        state.listeners.splice(state.listeners.indexOf(listener), 1);
        listenerCount--;
    }
    function eventTarget(node) {
        const state = { listeners: [], properties: Object.create(null), inline: Object.create(null), handlers: Object.create(null) };
        targets.set(node, state);
        Object.defineProperties(node, {
            addEventListener: { value(type, callback, options) {
                type = String(type);
                if (!types.has(type)) throw new TypeError('Unsupported Lea event');
                if (callback === null || callback === undefined) return;
                if (typeof callback !== 'function' && typeof callback !== 'object') throw new TypeError('Invalid listener');
                if (options && options.signal) throw new TypeError('AbortSignal is not supported');
                syncHandler(node, state, type);
                const cap = capture(options);
                if (state.listeners.some(item => item.type === type && item.callback === callback && item.capture === cap)) return;
                if (listenerCount === 128) throw new RangeError('Lea listener limit');
                state.listeners.push({ type, callback, capture: cap, once: !!(options && options.once), passive: !!(options && options.passive), removed: false });
                listenerCount++;
            } },
            removeEventListener: { value(type, callback, options) {
                const cap = capture(options);
                for (const item of state.listeners.slice())
                    if (item.type === String(type) && item.callback === callback && item.capture === cap) remove(state, item);
            } }
        });
        for (const type of ['click', 'input', 'change', 'submit', 'load']) Object.defineProperty(node, 'on' + type, {
            get() { return syncHandler(node, state, type); },
            set(value) {
                syncHandler(node, state, type);
                state.properties[type] = typeof value === 'function' ? value : null;
                syncHandler(node, state, type);
            }
        });
    }
    function syncHandler(node, state, type) {
        const callback = handler(node, state, type);
        const slot = state.handlers[type];
        if (callback && !slot) {
            if (listenerCount === 128) throw new RangeError('Lea listener limit');
            const item = { type, capture: false, handler: true, removed: false };
            state.handlers[type] = item;
            state.listeners.push(item);
            listenerCount++;
        } else if (!callback && slot) {
            remove(state, slot);
            delete state.handlers[type];
        }
        return callback;
    }
    function handler(node, state, type) {
        if (Object.prototype.hasOwnProperty.call(state.properties, type)) return state.properties[type];
        if (typeof node.getAttribute !== 'function') return null;
        const code = node.getAttribute('on' + type);
        if (!code) return null;
        if (code.length > 16384) throw new RangeError('Lea inline handler limit');
        if (!state.inline[type] || state.inline[type].code !== code)
            state.inline[type] = { code, callback: new Function('event', code) };
        return state.inline[type].callback;
    }
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
        eventTarget(node);
        cache.set(key, node);
        return node;
    }
    const document = {};
    Object.defineProperties(document, {
        readyState: { get() { return ready; } },
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
    eventTarget(document);
    eventTarget(globalThis);
    delete globalThis.__lea;
    // The dispatcher is returned to the interpreter host, never installed on the global object.
    return function dispatch(type, keys) {
        if (!types.has(type) || keys.length > 128) throw new TypeError('Invalid Lea event');
        if (type === 'DOMContentLoaded') ready = 'interactive';
        if (type === 'load') ready = 'complete';
        const route = keys.map(wrap);
        if (type === 'load' && keys.length === 0) route.push(globalThis);
        else { route.push(document); route.push(globalThis); }
        const target = route[0];
        const cancelable = type === 'click' || type === 'submit';
        const bubbles = type !== 'load';
        let current = null, phase = 0, prevented = false, stopped = false, immediate = false, passive = false;
        const event = {};
        Object.defineProperties(event, {
            type: { value: type }, target: { value: target },
            currentTarget: { get() { return current; } }, eventPhase: { get() { return phase; } },
            bubbles: { value: bubbles }, cancelable: { value: cancelable },
            defaultPrevented: { get() { return prevented; } }, isTrusted: { value: false },
            timeStamp: { value: Date.now() },
            preventDefault: { value() { if (cancelable && !passive) prevented = true; } },
            stopPropagation: { value() { stopped = true; } },
            stopImmediatePropagation: { value() { stopped = true; immediate = true; } },
            composedPath: { value() { return phase === 0 ? [] : route.slice(); } }
        });
        function invoke(node, cap) {
            current = node;
            const state = targets.get(node);
            syncHandler(node, state, type);
            for (const item of state.listeners.slice()) {
                if (item.removed || item.type !== type || item.capture !== cap) continue;
                if (item.once) remove(state, item);
                passive = item.passive;
                if (item.handler) {
                    const callback = handler(node, state, type);
                    if (callback && callback.call(node, event) === false && cancelable) prevented = true;
                } else if (typeof item.callback === 'function') item.callback.call(node, event);
                else item.callback.handleEvent.call(item.callback, event);
                passive = false;
                if (immediate) return;
            }
        }
        try {
            phase = 1;
            for (let i = route.length - 1; i > 0 && !stopped; i--) invoke(route[i], true);
            if (!stopped) {
                phase = 2;
                invoke(target, true);
                if (!stopped) invoke(target, false);
            }
            phase = 3;
            if (bubbles) for (let i = 1; i < route.length && !stopped; i++) invoke(route[i], false);
            return prevented;
        } finally { current = null; phase = 0; passive = false; }
    };
})(__lea);
