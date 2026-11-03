'use strict';
var connectedGroup = document.getElementById('group');
var connectedChild = document.getElementById('child');
if (document.isConnected !== true || connectedGroup.isConnected !== true || connectedChild.isConnected !== true) throw Error('attached connectivity');
var connectedGetter = Object.getOwnPropertyDescriptor(connectedGroup, 'isConnected');
if (typeof connectedGetter.get !== 'function' || connectedGetter.set !== undefined || !connectedGetter.enumerable || !connectedGetter.configurable) throw Error('connectivity descriptor');
if (connectedGetter.get.call(document) !== true) throw Error('document receiver');
var connectedTrapCalls = 0;
var connectedProxy = new Proxy({}, { get() { connectedTrapCalls++; throw Error('proxy trap'); } });
for (var invalidConnected of [null, undefined, {}, connectedProxy]) {
    var connectedRejected = false;
    try { connectedGetter.get.call(invalidConnected); } catch (error) { if (!(error instanceof TypeError)) throw error; connectedRejected = true; }
    if (!connectedRejected) throw Error('unbranded receiver');
}
if (connectedTrapCalls !== 0) throw Error('receiver coercion');
var connectedWriteRejected = false;
try { connectedChild.isConnected = false; } catch (error) { if (!(error instanceof TypeError)) throw error; connectedWriteRejected = true; }
if (!connectedWriteRejected || connectedChild.isConnected !== true) throw Error('readonly connectivity');
var connectedOldGet = WeakMap.prototype.get, connectedOldApply = Reflect.apply;
WeakMap.prototype.get = Reflect.apply = function () { throw Error('mutable public intrinsic'); };
try { if (connectedChild.isConnected !== true) throw Error('captured bridge'); }
finally { WeakMap.prototype.get = connectedOldGet; Reflect.apply = connectedOldApply; }
connectedGroup.remove();
if (connectedGroup.isConnected !== false || connectedChild.isConnected !== false || document.isConnected !== true) throw Error('detached connectivity');
if (!connectedGroup.contains(connectedChild)) throw Error('detached containment retained');
var connectedDetachedWriteRejected = false;
try { connectedChild.id = 'not-allowed'; } catch (error) { if (!(error instanceof TypeError)) throw error; connectedDetachedWriteRejected = true; }
if (!connectedDetachedWriteRejected) throw Error('connectivity grants no detached write capability');
