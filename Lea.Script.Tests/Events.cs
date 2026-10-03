using System;
using Lea.Engine;
using Lea.Script;

namespace Lea.Script.Tests
{
    internal static class Events
    {
        internal static void Run(string fixture)
        {
            var engine = new E { Jb = false };
            var page = engine.Go(fixture);
            using (var script = new Js(engine, page)) Suite(engine, page, script.Ex, script.Ev, "in-process");
            page = engine.Go(fixture);
            using (var script = new Jp(engine, page) { Tm = 5000 }) Suite(engine, page, script.Ex, script.Ev, "isolated");
            var target = engine.Id(page, "alias");
            Check(engine.Jn(page, target) && !engine.Jn(page, null), "connected node predicate");
            var other = new E();
            var foreign = other.Go(fixture);
            Check(!engine.Jn(page, other.Id(foreign, "target")), "foreign node predicate");
            Check(engine.Id(page, "alias").Av("class") == "literal", "Lea script type survives optional classic ownership");
            var alias = engine.Id(page, "alias");
            alias.A["onclick"] = "t('classic');";
            Check(engine.Ck(page, alias) == 0 && page.T != "classic", "classic onclick disabled for optional runtime");
            alias.A["data-lea"] = "t('explicit');";
            Check(engine.Ck(page, alias) == 1 && page.T == "explicit", "explicit data-lea remains available");
            alias.A.Remove("data-lea");
            engine.Jb = true;
            Check(engine.Ck(page, alias) == 1 && page.T == "classic", "classic command fallback remains opt-in");
        }

        private static void Suite(E engine, P page, Func<string, Jr> ex, Func<N, string, Jr> ev, string mode)
        {
            var target = engine.Id(page, "target");
            void Code(string code) { var result = ex(code); Check(result.Ok, mode + " event setup: " + result.S + "; " + result.M); }
            Jr Fire(string type = "click", N node = null) { var result = ev(node ?? target, type); Check(result.Ok, mode + " event dispatch: " + result.S + "; " + result.M); return result; }
            Code(@"var target = document.getElementById('target'), outer = document.getElementById('outer');
var trace = [], saved, listeners = [];
function add(node, name, capture) {
  var callback = function(e) {
    if (this !== node || e.target !== target || e.currentTarget !== node || e.type !== 'click' || !e.bubbles || !e.cancelable || e.isTrusted) throw new Error('event identity');
    trace.push(name + e.eventPhase); saved = e;
  };
  node.addEventListener('click', callback, capture); listeners.push([node, callback, capture]);
}
add(window,'w',true); add(document,'d',true); add(outer,'o',true); add(target,'t',true);
add(target,'t',false); add(outer,'o',false); add(document,'d',false); add(window,'w',false);");
            Check(!Fire().D, mode + " uncanceled click");
            Code("document.title = trace.join(','); if(saved.currentTarget !== null || saved.eventPhase !== 0 || saved.composedPath().length !== 0) throw new Error('post-dispatch'); for(var item of listeners) item[0].removeEventListener('click',item[1],item[2]);");
            Check(page.T == "w1,d1,o1,t2,t2,o3,d3,w3", mode + " capture target bubble order and cleared event");

            Code("var passive = function(e){e.preventDefault();}; target.addEventListener('click',passive,{passive:true});");
            Check(!Fire().D, mode + " passive listener cannot cancel");
            Code("target.removeEventListener('click',passive); target.onclick = function(e){e.preventDefault();};");
            Check(Fire().D, mode + " preventDefault returned to parent");
            Code("target.onclick = null; var stopped=[]; var a=function(e){stopped.push('a');e.stopPropagation();}; var b=function(e){stopped.push('b');}; var c=function(){stopped.push('c');}; target.addEventListener('click',a); target.addEventListener('click',b); outer.addEventListener('click',c);");
            Fire();
            Code("document.title=stopped.join(''); target.removeEventListener('click',a);target.removeEventListener('click',b);outer.removeEventListener('click',c);");
            Check(page.T == "ab", mode + " stopPropagation keeps same invocation listeners");
            Code("stopped=[]; a=function(e){stopped.push('a');e.stopImmediatePropagation();};target.addEventListener('click',a);target.addEventListener('click',b);");
            Fire();
            Code("document.title=stopped.join('');target.removeEventListener('click',a);target.removeEventListener('click',b);");
            Check(page.T == "a", mode + " stopImmediatePropagation suppresses remaining listeners");
            Code("stopped=[];a=function(e){stopped.push('a');e.stopPropagation();};target.addEventListener('click',a,true);target.addEventListener('click',b);");
            Fire();
            Code("document.title=stopped.join('');target.removeEventListener('click',a,true);target.removeEventListener('click',b);");
            Check(page.T == "a", mode + " capture stop blocks later bubble invocation");

            Code("var once=0; var listener={handleEvent:function(e){if(this!==listener || e.currentTarget!==target)throw new Error('object');once++;}};target.addEventListener('click',listener,{once:true});target.addEventListener('click',listener,{once:true});");
            Fire(); Fire();
            Code("document.title=String(once);");
            Check(page.T == "1", mode + " duplicate once listener and object this binding");
            Code("var order=[];b=function(){order.push('b');};c=function(){order.push('c');};a=function(){order.push('a');target.removeEventListener('click',b);target.addEventListener('click',c);};target.addEventListener('click',a);target.addEventListener('click',b);");
            Fire();
            Code("document.title=order.join('');");
            Check(page.T == "a", mode + " removal affects snapshot and addition waits");
            Fire();
            Code("document.title=order.join('');target.removeEventListener('click',a);target.removeEventListener('click',c);");
            Check(page.T == "aac", mode + " added listener runs on following dispatch");

            var inline = engine.Id(page, "inline");
            Code("document.title='';var inline=document.getElementById('inline');inline.addEventListener('click',function(){document.title+='B';});");
            Check(Fire("click", inline).D && page.T == "AB", mode + " authored inline handler order and false cancellation");
            Code("document.title='';inline.onclick=function(){document.title+='C';};");
            Check(!Fire("click", inline).D && page.T == "CB", mode + " DOM0 replacement retains listener slot");
            Code("document.title='';inline.onclick=null;inline.onclick=function(){document.title+='D';};");
            Check(!Fire("click", inline).D && page.T == "BD", mode + " DOM0 deactivate reactivation moves slot");

            Code("var ready=[];if(document.readyState!=='loading')throw new Error('initial state'); document.addEventListener('DOMContentLoaded',function(e){if(e.target!==document||e.cancelable||!e.bubbles)throw new Error('ready target');ready.push(document.readyState);}); window.onload=function(e){if(e.target!==window||e.bubbles||e.cancelable)throw new Error('load');ready.push(document.readyState);};");
            Check(ev(null, "DOMContentLoaded").Ok && ev(null, "load").Ok, mode + " lifecycle events");
            Code("document.title=ready.join(',');if(typeof __lea!=='undefined'||typeof dispatch!=='undefined'||typeof document.dispatchEvent!=='undefined')throw new Error('hidden dispatcher');");
            Check(page.T == "interactive,complete", mode + " lifecycle readyState and hidden host dispatcher");
            Code("target.oninput=function(e){e.preventDefault();document.title='input';};target.onsubmit=function(e){e.preventDefault();};");
            Check(!Fire("input").D && page.T == "input" && Fire("submit").D, mode + " input noncancelable and submit cancelable");
            var other = new E();
            var foreign = other.Go(page.U.LocalPath);
            Check(ev(other.Id(foreign, "target"), "click").S == "node", mode + " foreign event target rejected");
            Code("var old=document.getElementById('target');document.getElementById('outer').textContent='removed';");
            Check(ev(target, "click").S == "node", mode + " detached event target rejected");
            try { ev(null, "invented"); throw new Exception("Unknown event accepted"); }
            catch (ArgumentException) { Check(true, mode + " event whitelist"); }
            Check(ex("throw new Error('reset');").S == "js", mode + " event realm reset setup");
            Code("for(var i=0;i<128;i++) window.addEventListener('click',function(){});");
            Check(ex("window.onclick=function(){};").S == "js", mode + " registered and DOM0 listener ceiling");
            Code("if(typeof once!=='undefined')throw new Error('old realm');document.title='fresh';");
            Check(page.T == "fresh", mode + " listener budget failure drops realm");
        }

        private static void Check(bool condition, string name) { Program.Check(condition, name); }
    }
}
