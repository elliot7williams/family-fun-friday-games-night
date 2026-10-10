'use strict';
{
    const call = __lea, checkpoint = __jobs;
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
    const owns = Object.prototype.hasOwnProperty;
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
    function eventOptions(options) {
        return options !== null && (typeof options === 'object' || typeof options === 'function') ? options : null;
    }
    function remove(state, listener) {
        if (listener.removed) return;
        listener.removed = true;
        state.listeners.splice(state.listeners.indexOf(listener), 1);
        listenerCount--;
    }
    function eventState(receiver) {
        const state = apply(getState, targets, [receiver === null || receiver === undefined ? realm : receiver]);
        if (!state) throw new listError('Illegal EventTarget receiver');
        return state;
    }
    const eventMethods = {
        addEventListener(type, callback, options = undefined) {
            const state = eventState(this);
            if (arguments.length < 2) throw new listError('An event type and listener are required');
            type = str(type);
            if (callback !== null && callback !== undefined && typeof callback !== 'function' && typeof callback !== 'object')
                throw new listError('Invalid listener');
            const fields = eventOptions(options);
            const cap = fields ? !!fields.capture : !!options;
            const once = !!(fields && fields.once), passive = !!(fields && fields.passive);
            if (fields && fields.signal !== undefined) throw new listError('AbortSignal is not supported');
            if (!eventType(type)) throw new listError('Unsupported Lea event');
            if (callback === null || callback === undefined) return;
            syncHandler(state.node, state, type);
            if (state.listeners.some(item => item.type === type && item.callback === callback && item.capture === cap)) return;
            if (listenerCount === 128) throw new RangeError('Lea listener limit');
            state.listeners.push({ type, callback, capture: cap, once, passive, removed: false });
            listenerCount++;
        },
        removeEventListener(type, callback, options = undefined) {
            const state = eventState(this);
            if (arguments.length < 2) throw new listError('An event type and listener are required');
            type = str(type);
            if (callback !== null && callback !== undefined && typeof callback !== 'function' && typeof callback !== 'object')
                throw new listError('Invalid listener');
            const fields = eventOptions(options);
            const cap = fields ? !!fields.capture : !!options;
            for (const item of state.listeners.slice())
                if (item.type === type && item.callback === callback && item.capture === cap) remove(state, item);
        }
    };
    const eventProperties = {
        __proto__: null,
        addEventListener: { __proto__: null, value: eventMethods.addEventListener },
        removeEventListener: { __proto__: null, value: eventMethods.removeEventListener }
    };
    const defineEvents = Object.defineProperties;
    for (let i = 0; i < handlerTypes.length; i++) {
        const type = handlerTypes[i];
        eventProperties['on' + type] = {
            __proto__: null,
            get() { const state = eventState(this); return syncHandler(state.node, state, type); },
            set(value) {
                const state = eventState(this);
                syncHandler(state.node, state, type);
                state.properties[type] = typeof value === 'function' ? value : null;
                syncHandler(state.node, state, type);
            }
        };
    }
    function eventTarget(node, key) {
        const state = { node, key, classTokens: null, listeners: [], properties: makeObject(null), inline: makeObject(null), handlers: makeObject(null) };
        apply(setState, targets, [node, state]);
        defineEvents(node, eventProperties);
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
        if (apply(owns, state.properties, [type])) return state.properties[type];
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
    const sliceString = String.prototype.slice, codeAt = String.prototype.charCodeAt, indexString = String.prototype.indexOf;
    const arrayKeys = Array.prototype.keys, arrayValues = Array.prototype.values;
    const arrayEntries = Array.prototype.entries, arrayForEach = Array.prototype.forEach, arrayIndexOf = Array.prototype.indexOf, arrayMap = Array.prototype.map;
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
    const exceptionKeys = ['INDEX_SIZE_ERR', 'DOMSTRING_SIZE_ERR', 'HIERARCHY_REQUEST_ERR', 'WRONG_DOCUMENT_ERR',
        'INVALID_CHARACTER_ERR', 'NO_DATA_ALLOWED_ERR', 'NO_MODIFICATION_ALLOWED_ERR', 'NOT_FOUND_ERR',
        'NOT_SUPPORTED_ERR', 'INUSE_ATTRIBUTE_ERR', 'INVALID_STATE_ERR', 'SYNTAX_ERR', 'INVALID_MODIFICATION_ERR',
        'NAMESPACE_ERR', 'INVALID_ACCESS_ERR', 'VALIDATION_ERR', 'TYPE_MISMATCH_ERR', 'SECURITY_ERR', 'NETWORK_ERR',
        'ABORT_ERR', 'URL_MISMATCH_ERR', 'QUOTA_EXCEEDED_ERR', 'TIMEOUT_ERR', 'INVALID_NODE_TYPE_ERR', 'DATA_CLONE_ERR'];
    const exceptionConstants = makeObject(null);
    for (let i = 0; i < exceptionKeys.length; i++) exceptionConstants[exceptionKeys[i]] = { value: i + 1, enumerable: true };
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
    function select(receiver, supplied, ancestors) {
        const state = apply(getState, targets, [receiver]);
        if (!state || typeof state.key !== 'number') throw new listError('Illegal Element receiver');
        if (supplied.length === 0) throw new listError('A selector is required');
        const selector = str(supplied[0]);
        if (selector.length > 1024) throw new DOMException('Lea selector limit', 'SyntaxError');
        const result = call(ancestors ? 'cl' : 'mt', state.key, selector);
        if (ancestors ? result === -1 : result === null) throw new DOMException('Invalid or unsupported Lea selector', 'SyntaxError');
        return ancestors ? wrap(result) : result;
    }
    function relative(receiver, operation) {
        if (receiver === document && operation !== 'ps' && operation !== 'ns') return call(operation, null);
        const state = apply(getState, targets, [receiver]);
        if (!state || typeof state.key !== 'number') throw new listError('Illegal tree navigation receiver');
        return call(operation, state.key);
    }
    const treeProperties = {
        __proto__: null,
        parentElement: { __proto__: null, get() { return wrap(relative(this, 'pe')); }, enumerable: true, configurable: true },
        firstElementChild: { __proto__: null, get() { return wrap(relative(this, 'fc')); }, enumerable: true, configurable: true },
        lastElementChild: { __proto__: null, get() { return wrap(relative(this, 'lc')); }, enumerable: true, configurable: true },
        previousElementSibling: { __proto__: null, get() { return wrap(relative(this, 'ps')); }, enumerable: true, configurable: true },
        nextElementSibling: { __proto__: null, get() { return wrap(relative(this, 'ns')); }, enumerable: true, configurable: true },
        childElementCount: { __proto__: null, get() { return relative(this, 'cc'); }, enumerable: true, configurable: true }
    };
    function elementState(receiver, supplied, required = 1) {
        const state = apply(getState, targets, [receiver]);
        if (!state || typeof state.key !== 'number') throw new listError('Illegal Element receiver');
        if (supplied && supplied.length < required) throw new listError('An attribute argument is required');
        return state;
    }
    const attributeSeparators = '\t\n\f\r \0/=>';
    function query(receiver, supplied, all) {
        const state = elementState(receiver, supplied);
        const selector = str(supplied[0]);
        if (selector.length > 1024) throw new DOMException('Lea selector limit', 'SyntaxError');
        const result = call(all ? 'qa' : 'q', state.key, selector);
        if (all ? result === null : result === -1) throw new DOMException('Invalid or unsupported Lea selector', 'SyntaxError');
        return all ? apply(arrayMap, result, [wrap]) : wrap(result);
    }
    const elementMethods = {
        __proto__: null,
        querySelector(selector) { return query(this, arguments, false); },
        querySelectorAll(selector) { return query(this, arguments, true); },
        getAttribute(name) {
            const state = elementState(this, arguments);
            return call('attr', state.key, str(name));
        },
        hasAttribute(name) {
            const state = elementState(this, arguments);
            return call('attr', state.key, str(name)) !== null;
        },
        setAttribute(name, value) {
            const state = elementState(this, arguments, 2);
            name = str(name);
            value = str(value);
            let invalid = name.length === 0;
            for (let i = 0; i < attributeSeparators.length && !invalid; i++)
                invalid = apply(indexString, name, [apply(sliceString, attributeSeparators, [i, i + 1])]) !== -1;
            if (invalid) throw new DOMException('Invalid attribute name', 'InvalidCharacterError');
            if (name.length > 64 || value.length > 1024 || !call('sa', state.key, name, value)) throw new listError('Lea attribute mutation limit or invalid target');
        },
        removeAttribute(name) {
            const state = elementState(this, arguments);
            name = str(name);
            if (name.length > 1024 || !call('ra', state.key, name)) throw new listError('Lea attribute mutation limit or invalid target');
        }
    };
    const elementProperties = {
        __proto__: null,
        querySelector: { __proto__: null, value: elementMethods.querySelector },
        querySelectorAll: { __proto__: null, value: elementMethods.querySelectorAll },
        getAttribute: { __proto__: null, value: elementMethods.getAttribute },
        hasAttribute: { __proto__: null, value: elementMethods.hasAttribute },
        setAttribute: { __proto__: null, value: elementMethods.setAttribute },
        removeAttribute: { __proto__: null, value: elementMethods.removeAttribute },
        classList: { __proto__: null,
            get() { const state = elementState(this); if (!state.classTokens) state.classTokens = makeList(state.key); return state.classTokens; },
            set(value) { const state = elementState(this); if (!state.classTokens) state.classTokens = makeList(state.key); state.classTokens.value = value; }
        }
    };
    function wrap(key) {
        if (key === null) return null;
        const cached = apply(getTimer, cache, [key]);
        if (cached) return cached;
        const node = {};
        const style = {};
        for (let i = 0; i < styleNames.length; i++) {
            const name = styleNames[i];
            define(style, name, {
                set(value) { put(key, styles[name], value); }, enumerable: true
            });
        }
        defineEvents(node, treeProperties);
        defineEvents(node, {
            tagName: { get() { return call('tag', key); } },
            id: { get() { return call('attr', key, 'id') || ''; }, set(value) { put(key, 'id', value); } },
            className: { get() { return call('attr', key, 'class') || ''; }, set(value) { put(key, 'cn', value); } },
            textContent: { get() { return call('text', key); }, set(value) { put(key, 'tx', value === null ? '' : value); } },
            value: { get() { return call('value', key); }, set(value) { put(key, 'vl', value); } },
            checked: { get() { return call('checked', key); }, set(value) { put(key, 'ck', value ? '1' : '0'); } },
            style: { value: style },
            querySelector: elementProperties.querySelector,
            querySelectorAll: elementProperties.querySelectorAll,
            getAttribute: elementProperties.getAttribute,
            hasAttribute: elementProperties.hasAttribute,
            setAttribute: elementProperties.setAttribute,
            removeAttribute: elementProperties.removeAttribute,
            matches: { value(selector) { return select(this, arguments, false); } },
            closest: { value(selector) { return select(this, arguments, true); } },
            classList: elementProperties.classList
        });
        eventTarget(node, key);
        apply(putTimer, cache, [key, node]);
        return node;
    }
    const document = {};
    Object.defineProperties(document, {
        firstElementChild: treeProperties.firstElementChild,
        lastElementChild: treeProperties.lastElementChild,
        childElementCount: treeProperties.childElementCount,
        readyState: { get() { return ready; } },
        title: { get() { return call('t'); }, set(value) { put(null, 't', value); } },
        body: { get() { return wrap(call('q', 'body')); } },
        documentElement: { get() { return wrap(call('q', 'html')); } },
        getElementById: { value(id) { return wrap(call('id', stringify(id))); } },
        querySelector: { value(selector) { return wrap(call('q', stringify(selector))); } },
        querySelectorAll: { value(selector) { return apply(arrayMap, call('qa', stringify(selector)), [wrap]); } }
    });
    Object.defineProperty(globalThis, 'document', { value: document });
    Object.defineProperty(globalThis, 'window', { value: globalThis });
    define(globalThis, 'origin', {
        get() {
            if (this !== realm && this !== null && this !== undefined) throw new TypeError('Invalid Lea window');
            return call('og');
        },
        set(value) {
            if (this !== realm && this !== null && this !== undefined) throw new TypeError('Invalid Lea window');
            define(realm, 'origin', { value, writable: true, configurable: true, enumerable: true });
        },
        configurable: true, enumerable: true
    });
    Object.defineProperty(globalThis, 'alert', { value(value) { put(null, 'a', value); } });
    eventTarget(document);
    eventTarget(globalThis);
    delete globalThis.__lea;
    delete globalThis.__jobs;
    // The dispatcher is returned to the interpreter host, never installed on the global object.
    (function dispatch(type, keys) {
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
            const state = eventState(node);
            syncHandler(node, state, type);
            for (const item of state.listeners.slice()) {
                if (item.removed || item.type !== type || item.capture !== cap) continue;
                if (item.once) remove(state, item);
                passive = item.passive;
                if (item.handler) {
                    const callback = handler(node, state, type);
                    if (callback && apply(callback, node, [event]) === false && cancelable) prevented = true;
                } else if (typeof item.callback === 'function') apply(item.callback, node, [event]);
                else apply(item.callback.handleEvent, item.callback, [event]);
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
    });
}
