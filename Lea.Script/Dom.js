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
        if (typeof value === 'symbol') throw new listError('A DOM string cannot be a Symbol');
        return stringify(value);
    }
    const listStates = new WeakMap(), getState = WeakMap.prototype.get, setState = WeakMap.prototype.set;
    const makeObject = Object.create, define = Object.defineProperty;
    const reflectGet = Reflect.get, reflectSet = Reflect.set, reflectHas = Reflect.has;
    const reflectKeys = Reflect.ownKeys, reflectDescriptor = Reflect.getOwnPropertyDescriptor;
    const reflectDefine = Reflect.defineProperty, reflectDelete = Reflect.deleteProperty;
    const sliceString = String.prototype.slice, codeAt = String.prototype.charCodeAt;
    const arrayKeys = Array.prototype.keys, arrayValues = Array.prototype.values;
    const arrayEntries = Array.prototype.entries, arrayForEach = Array.prototype.forEach, arrayIndexOf = Array.prototype.indexOf;
    const ListProxy = Proxy, listError = TypeError, NativeError = Error, construct = Reflect.construct;
    // Jint 4.16.4's native formatter supplies the prototype as the accessor receiver.
    define(NativeError.prototype, 'toString', { value: function toString() {
        if (this === null || typeof this !== 'object' && typeof this !== 'function') throw new listError('Illegal Error receiver');
        let name = this.name;
        name = name === undefined ? 'Error' : str(name);
        let message = this.message;
        message = message === undefined ? '' : str(message);
        return name === '' ? message : message === '' ? name : name + ': ' + message;
    }, writable: true, configurable: true });
    const exceptions = new WeakMap();
    const exceptionNames = ['IndexSizeError', '', 'HierarchyRequestError', 'WrongDocumentError', 'InvalidCharacterError', '', 'NoModificationAllowedError',
        'NotFoundError', 'NotSupportedError', 'InUseAttributeError', 'InvalidStateError', 'SyntaxError', 'InvalidModificationError', 'NamespaceError',
        'InvalidAccessError', '', 'TypeMismatchError', 'SecurityError', 'NetworkError', 'AbortError', 'URLMismatchError', 'QuotaExceededError',
        'TimeoutError', 'InvalidNodeTypeError', 'DataCloneError'];
    function exceptionState(value) {
        const state = apply(getState, exceptions, [value]);
        if (!state) throw new listError('Illegal DOMException receiver');
        return state;
    }
    function DOMException(message = '', name = 'Error') {
        if (!new.target) throw new listError('DOMException requires new');
        message = str(message);
        name = str(name);
        const value = construct(NativeError, [], new.target);
        apply(setState, exceptions, [value, { message, name }]);
        return value;
    }
    DOMException.prototype = {
        __proto__: NativeError.prototype,
        get name() { return exceptionState(this).name; },
        get message() { return exceptionState(this).message; },
        get code() {
            const name = exceptionState(this).name;
            for (let i = 0; i < exceptionNames.length; i++) if (exceptionNames[i] && exceptionNames[i] === name) return i + 1;
            return 0;
        }
    };
    define(DOMException, 'prototype', { writable: false });
    define(DOMException.prototype, 'constructor', { value: DOMException, writable: true, configurable: true });
    const exceptionConstants = {
        INDEX_SIZE_ERR: { value: 1, enumerable: true }, DOMSTRING_SIZE_ERR: { value: 2, enumerable: true },
        HIERARCHY_REQUEST_ERR: { value: 3, enumerable: true }, WRONG_DOCUMENT_ERR: { value: 4, enumerable: true },
        INVALID_CHARACTER_ERR: { value: 5, enumerable: true }, NO_DATA_ALLOWED_ERR: { value: 6, enumerable: true },
        NO_MODIFICATION_ALLOWED_ERR: { value: 7, enumerable: true }, NOT_FOUND_ERR: { value: 8, enumerable: true },
        NOT_SUPPORTED_ERR: { value: 9, enumerable: true }, INUSE_ATTRIBUTE_ERR: { value: 10, enumerable: true },
        INVALID_STATE_ERR: { value: 11, enumerable: true }, SYNTAX_ERR: { value: 12, enumerable: true },
        INVALID_MODIFICATION_ERR: { value: 13, enumerable: true }, NAMESPACE_ERR: { value: 14, enumerable: true },
        INVALID_ACCESS_ERR: { value: 15, enumerable: true }, VALIDATION_ERR: { value: 16, enumerable: true },
        TYPE_MISMATCH_ERR: { value: 17, enumerable: true }, SECURITY_ERR: { value: 18, enumerable: true },
        NETWORK_ERR: { value: 19, enumerable: true }, ABORT_ERR: { value: 20, enumerable: true },
        URL_MISMATCH_ERR: { value: 21, enumerable: true }, QUOTA_EXCEEDED_ERR: { value: 22, enumerable: true },
        TIMEOUT_ERR: { value: 23, enumerable: true }, INVALID_NODE_TYPE_ERR: { value: 24, enumerable: true },
        DATA_CLONE_ERR: { value: 25, enumerable: true }
    };
    Object.defineProperties(DOMException, exceptionConstants);
    Object.defineProperties(DOMException.prototype, exceptionConstants);
    define(DOMException.prototype, Symbol.toStringTag, { value: 'DOMException', configurable: true });
    define(globalThis, 'DOMException', { value: DOMException, writable: true, configurable: true, enumerable: true });
    function DOMTokenList() { throw new listError('Illegal constructor'); }
    function listState(receiver, supplied, required = 0) {
        const state = apply(getState, listStates, [receiver]);
        if (state === undefined) throw new listError('Illegal DOMTokenList receiver');
        if (supplied && supplied.length < required) throw new listError('A DOMTokenList argument is required');
        return state;
    }
    function space(code) { return code === 32 || code === 9 || code === 10 || code === 12 || code === 13; }
    function position(values, word) {
        return apply(arrayIndexOf, values, [word]);
    }
    function words(source) {
        const values = [];
        let start = -1;
        for (let i = 0; i <= source.length; i++) {
            if (i === source.length || space(apply(codeAt, source, [i]))) {
                if (start >= 0) {
                    const word = apply(sliceString, source, [start, i]);
                    if (position(values, word) < 0) values[values.length] = word;
                    start = -1;
                }
            } else if (start < 0) start = i;
        }
        return values;
    }
    function listData(state) {
        const raw = call('attr', state, 'class');
        return { raw, values: words(raw || '') };
    }
    function validWord(word) {
        if (word.length === 0) throw new DOMException('A class token is empty', 'SyntaxError');
        for (let i = 0; i < word.length; i++)
            if (space(apply(codeAt, word, [i]))) throw new DOMException('A class token contains ASCII whitespace', 'InvalidCharacterError');
    }
    function listArguments(supplied) {
        const values = [];
        for (let i = 0; i < supplied.length; i++) values[i] = str(supplied[i]);
        for (let i = 0; i < values.length; i++) validWord(values[i]);
        return values;
    }
    function writeList(state, data) {
        if (data.raw === null && data.values.length === 0) return;
        let text = '';
        for (let i = 0; i < data.values.length; i++) text += (i ? ' ' : '') + data.values[i];
        put(state, 'cn', text);
    }
    DOMTokenList.prototype = {
        get length() { return listData(listState(this)).values.length; },
        get value() { return call('attr', listState(this), 'class') || ''; },
        set value(value) { put(listState(this), 'cn', value); },
        item(index) { const state = listState(this, arguments, 1); index = index >>> 0; const values = listData(state).values; return index < values.length ? values[index] : null; },
        contains(token) { const state = listState(this, arguments, 1); token = str(token); return position(listData(state).values, token) >= 0; },
        add() {
            const state = listState(this), supplied = listArguments(arguments), data = listData(state);
            for (let i = 0; i < supplied.length; i++) if (position(data.values, supplied[i]) < 0) data.values[data.values.length] = supplied[i];
            writeList(state, data);
        },
        remove() {
            const state = listState(this), supplied = listArguments(arguments), data = listData(state), remaining = [];
            for (let i = 0; i < data.values.length; i++)
                if (position(supplied, data.values[i]) < 0) remaining[remaining.length] = data.values[i];
            data.values = remaining;
            writeList(state, data);
        },
        toggle(token, force = undefined) {
            const state = listState(this, arguments, 1);
            token = str(token);
            validWord(token);
            const data = listData(state), found = position(data.values, token);
            const add = force === undefined ? found < 0 : !!force;
            if ((found >= 0) === add) return add;
            if (add) data.values[data.values.length] = token;
            else {
                for (let i = found; i < data.values.length - 1; i++) data.values[i] = data.values[i + 1];
                data.values.length--;
            }
            writeList(state, data);
            return add;
        },
        replace(token, newToken) {
            const state = listState(this, arguments, 2);
            token = str(token);
            newToken = str(newToken);
            if (token.length === 0 || newToken.length === 0) throw new DOMException('A class token is empty', 'SyntaxError');
            validWord(token);
            validWord(newToken);
            const data = listData(state), old = position(data.values, token), next = position(data.values, newToken);
            if (old < 0) return false;
            const remaining = [];
            for (let i = 0; i < data.values.length; i++) {
                if (i === old) { if (next < 0 || old <= next) remaining[remaining.length] = newToken; }
                else if (i !== next || next < old) remaining[remaining.length] = data.values[i];
            }
            data.values = remaining;
            writeList(state, data);
            return true;
        },
        supports(token) { listState(this, arguments, 1); str(token); throw new listError('Class has no supported-token vocabulary'); },
        toString() { return call('attr', listState(this), 'class') || ''; },
        keys: arrayKeys, values: arrayValues, entries: arrayEntries, forEach: arrayForEach
    };
    const listPrototype = DOMTokenList.prototype;
    define(listPrototype, 'constructor', { value: DOMTokenList, writable: true, configurable: true });
    define(DOMTokenList, 'prototype', { writable: false });
    define(listPrototype, Symbol.iterator, { value: arrayValues, writable: true, configurable: true });
    define(listPrototype, Symbol.toStringTag, { value: 'DOMTokenList', configurable: true });
    define(globalThis, 'DOMTokenList', { value: DOMTokenList, writable: true, configurable: true, enumerable: true });
    function listIndex(name) {
        if (typeof name !== 'string' || name.length === 0) return -1;
        const number = name >>> 0;
        return number !== 4294967295 && stringify(number) === name ? number : -1;
    }
    function makeList(key) {
        const target = makeObject(listPrototype);
        const proxy = new ListProxy(target, {
            get(target, name, receiver) {
                const index = listIndex(name);
                if (index >= 0) {
                    const values = listData(key).values;
                    if (index < values.length) return values[index];
                }
                return reflectGet(target, name, receiver);
            },
            has(target, name) { const index = listIndex(name); return index >= 0 && index < listData(key).values.length || reflectHas(target, name); },
            getOwnPropertyDescriptor(target, name) {
                const index = listIndex(name);
                if (index >= 0) {
                    const values = listData(key).values;
                    if (index < values.length) return { value: values[index], writable: false, enumerable: true, configurable: true };
                }
                return reflectDescriptor(target, name);
            },
            ownKeys(target) {
                const keys = [], size = listData(key).values.length, extra = reflectKeys(target);
                for (let i = 0; i < size; i++) keys[keys.length] = stringify(i);
                for (let i = 0; i < extra.length; i++) keys[keys.length] = extra[i];
                return keys;
            },
            set(target, name, value, receiver) {
                const index = listIndex(name);
                if (index >= 0 && index < listData(key).values.length) return false;
                return reflectSet(target, name, value, receiver);
            },
            defineProperty(target, name, descriptor) { return listIndex(name) < 0 && reflectDefine(target, name, descriptor); },
            deleteProperty(target, name) { const index = listIndex(name); return !(index >= 0 && index < listData(key).values.length) && reflectDelete(target, name); },
            preventExtensions() { return false; }
        });
        apply(setState, listStates, [proxy, key]);
        return proxy;
    }
    function put(key, name, value) {
        if (!call('put', key, name, str(value))) throw new listError('Unsupported Lea mutation');
    }
    function wrap(key) {
        if (key === null) return null;
        if (cache.has(key)) return cache.get(key);
        const node = {};
        let classTokens;
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
            classList: {
                get() { if (!classTokens) classTokens = makeList(key); return classTokens; },
                set(value) { if (!classTokens) classTokens = makeList(key); classTokens.value = value; }
            }
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
