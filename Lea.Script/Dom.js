(function (call, checkpoint) {
    'use strict';
    let failed = false, failure;
    function microtaskErrors() {
        if (!failed) return;
        const error = failure;
        failed = false;
        failure = undefined;
        throw error;
    }
    Object.defineProperty(globalThis, 'queueMicrotask', { value: function queueMicrotask(callback) {
        if (typeof callback !== 'function') throw new TypeError('A microtask callback is required');
        call('qm');
        (async function () {
            await 0;
            try { callback(); }
            catch (error) { if (!failed) { failed = true; failure = error; } }
        })();
    } });
    const realm = globalThis, apply = Reflect.apply, evaluate = eval, stringify = String;
    const timers = new Map(), getTimer = Map.prototype.get, putTimer = Map.prototype.set, deleteTimer = Map.prototype.delete;
    function schedule(handler, timeout, repeat, supplied) {
        if (supplied.length === 0) throw new TypeError('A timer handler is required');
        if (typeof handler === 'symbol') throw new TypeError('A timer handler cannot be a Symbol');
        const callback = typeof handler === 'function' ? handler : stringify(handler);
        if (typeof callback === 'string' && callback.length > 16384) throw new RangeError('Lea timer code limit');
        if (supplied.length > 66) throw new RangeError('Lea timer argument limit');
        const delay = timeout | 0;
        const args = [];
        for (let i = 2; i < supplied.length; i++) args[i - 2] = supplied[i];
        const id = call('ts', delay < 0 ? 0 : delay, repeat ? 1 : 0);
        apply(putTimer, timers, [id, { callback, args, repeat }]);
        return id;
    }
    function clear(id) {
        id = id | 0;
        if (id <= 0) return;
        call('tc', id);
        apply(deleteTimer, timers, [id]);
    }
    Object.defineProperties(globalThis, {
        setTimeout: { value: function setTimeout(handler, timeout = 0) { return schedule(handler, timeout, false, arguments); }, writable: true, configurable: true, enumerable: true },
        setInterval: { value: function setInterval(handler, timeout = 0) { return schedule(handler, timeout, true, arguments); }, writable: true, configurable: true, enumerable: true },
        clearTimeout: { value: function clearTimeout(id = 0) { clear(id); }, writable: true, configurable: true, enumerable: true },
        clearInterval: { value: function clearInterval(id = 0) { clear(id); }, writable: true, configurable: true, enumerable: true }
    });
    const cache = new Map();
    const targets = new WeakMap();
    const handlerTypes = ['click', 'input', 'change', 'submit', 'load'];
    function eventType(type) {
        return type === 'click' || type === 'input' || type === 'change' || type === 'submit' ||
            type === 'DOMContentLoaded' || type === 'load';
    }
    let listenerCount = 0;
    let ready = 'loading';
    const styles = { color: 'fg', backgroundColor: 'bg', width: 'wd', height: 'ht', display: 'dp', fontSize: 'fs' };
    const styleNames = Object.keys(styles);
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
                if (!eventType(type)) throw new TypeError('Unsupported Lea event');
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
        for (let i = 0; i < handlerTypes.length; i++) {
            const type = handlerTypes[i];
            Object.defineProperty(node, 'on' + type, {
                get() { return syncHandler(node, state, type); },
                set(value) {
                    syncHandler(node, state, type);
                    state.properties[type] = typeof value === 'function' ? value : null;
                    syncHandler(node, state, type);
                }
            });
        }
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
    function str(value) {
        if (typeof value === 'symbol') throw new TypeError('A DOM string cannot be a Symbol');
        return stringify(value);
    }
    function put(key, name, value) {
        if (!call('put', key, name, str(value))) throw new TypeError('Unsupported Lea mutation');
    }
    function wrap(key) {
        if (key === null) return null;
        if (cache.has(key)) return cache.get(key);
        const node = {};
        const style = {};
        for (let i = 0; i < styleNames.length; i++) {
            const name = styleNames[i];
            Object.defineProperty(style, name, {
                set(value) { put(key, styles[name], value); }, enumerable: true
            });
        }
        Object.defineProperties(node, {
            tagName: { get() { return call('tag', key); } },
            id: { get() { return call('attr', key, 'id') || ''; }, set(value) { put(key, 'id', value); } },
            className: { get() { return call('attr', key, 'class') || ''; }, set(value) { put(key, 'cn', value); } },
            textContent: { get() { return call('text', key); }, set(value) { put(key, 'tx', value === null ? '' : value); } },
            value: { get() { return call('value', key); }, set(value) { put(key, 'vl', value); } },
            checked: { get() { return call('checked', key); }, set(value) { put(key, 'ck', value ? '1' : '0'); } },
            style: { value: style },
            getAttribute: { value(name) { return call('attr', key, str(name)); } },
            hasAttribute: { value(name) { return call('attr', key, str(name)) !== null; } },
            classList: { value: Object.freeze({
                add(name) { put(key, 'ac', name); },
                remove(name) { put(key, 'rc', name); },
                toggle(name) {
                    const before = (call('attr', key, 'class') || '').split(/[ \t\n\f\r]+/).includes(String(name));
                    put(key, 'tc', name);
                    return !before;
                },
                contains(name) { return (call('attr', key, 'class') || '').split(/[ \t\n\f\r]+/).includes(String(name)); }
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
    delete globalThis.__jobs;
    // The dispatcher is returned to the interpreter host, never installed on the global object.
    return function dispatch(type, keys) {
        if (type === 'mt') { microtaskErrors(); return false; }
        if (type === 'tm') {
            if (keys.length !== 1) throw new TypeError('Invalid Lea timer');
            const timer = apply(getTimer, timers, [keys[0]]);
            if (!timer) return false;
            if (!timer.repeat) apply(deleteTimer, timers, [keys[0]]);
            try {
                if (typeof timer.callback === 'function') apply(timer.callback, realm, timer.args);
                else evaluate(timer.callback);
            } catch (error) { if (!failed) { failed = true; failure = error; } }
            // A Promise continuation is a microtask, not a nested timer task.
            call('te');
            checkpoint();
            microtaskErrors();
            return false;
        }
        if (!eventType(type) || keys.length > 128) throw new TypeError('Invalid Lea event');
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
        } finally { current = null; phase = 0; passive = false; }
        const canceled = prevented;
        // Drain Jint's queue while this invocation's constraints are still active.
        checkpoint();
        microtaskErrors();
        return canceled;
    };
})(__lea, __jobs);
