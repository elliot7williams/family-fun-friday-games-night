using System;
using System.Collections.Generic;
using System.Text;

namespace Lea.Engine
{
    // J is Lea's bounded allow-listed page command parser.
    internal static class J
    {
        private const int MaxBlocks = 8;
        private const int MaxCode = 16 * 1024;
        private const int MaxCommands = 32;
        private const int MaxAlerts = 16;
        private const int MaxVariables = 16;
        private const int MaxClasses = 32;
        private const int MaxClassName = 64;
        private const int MaxClassText = 512;
        private const int MaxFieldValue = 1024;

        internal static int Pg(P page, N root)
        {
            var scripts = new List<N>();
            Find(root, scripts);
            var left = MaxCommands;
            var count = 0;
            var blocks = 0;
            foreach (var script in scripts)
            {
                if (blocks++ >= MaxBlocks || left == 0) break;
                if (script.Av("src").Length > 0 || !Type(script.Av("type"))) continue;
                count += Run(page, Raw(script), ref left);
            }
            return count;
        }

        internal static int Run(P page, string code)
        {
            var left = MaxCommands;
            return Run(page, code, ref left);
        }

        private static int Run(P page, string code, ref int left)
        {
            if (page == null || String.IsNullOrWhiteSpace(code) || code.Length > MaxCode || left < 1) return 0;
            var count = 0;
            foreach (var statement in Statements(code))
            {
                if (left == 0) break;
                var command = Parse(statement);
                if (command == null || !Apply(page, command)) continue;
                count++;
                left--;
            }
            return count;
        }

        private static bool Apply(P page, Cm command)
        {
            if (command.N == "v")
            {
                var name = Variable(command.V);
                if (name.Length == 0 || (!page.Vs.ContainsKey(name) && page.Vs.Count >= MaxVariables)) return false;
                page.Vs[name] = Text(Expand(page, command.W), 256);
                return true;
            }
            if (command.N == "tx")
            {
                var id = Id(Expand(page, command.V));
                var target = FindId(page.D, id);
                if (target == null || !TextTarget(target.K)) return false;
                target.C.Clear();
                var child = new N("#text");
                child.X = Text(Expand(page, command.W), 512);
                child.Up = target;
                target.C.Add(child);
                return true;
            }
            if (command.N == "ac" || command.N == "rc" || command.N == "tc")
            {
                var id = Id(Expand(page, command.V));
                var name = ClassName(Expand(page, command.W));
                var target = FindId(page.D, id);
                return target != null && name.Length > 0 && ClassTarget(target.K) && SetClass(target, name, command.N);
            }
            if (command.N == "hd" || command.N == "sh")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                if (target == null || !ClassTarget(target.K)) return false;
                if (command.N == "hd")
                {
                    target.A["hidden"] = "";
                    target.A["data-lea-hidden"] = "1";
                }
                else
                {
                    target.A.Remove("hidden");
                    target.A.Remove("data-lea-hidden");
                }
                return true;
            }
            if (command.N == "op" || command.N == "cl" || command.N == "tg")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                var details = DisclosureTarget(target);
                if (details == null) return false;
                var open = command.N == "op" || (command.N == "tg" && !details.Op);
                details.Op = open;
                if (open) details.A["open"] = "";
                else details.A.Remove("open");
                return true;
            }
            if (command.N == "ds" || command.N == "en")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                if (target == null || !DisableTarget(target.K)) return false;
                if (command.N == "ds") target.A["disabled"] = "";
                else target.A.Remove("disabled");
                return true;
            }
            if (command.N == "vl")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                return SetValue(page, target, Expand(page, command.W));
            }
            if (command.N == "ck")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                return SetChecked(page, target, command.W == null ? null : Expand(page, command.W));
            }
            if (command.N == "fo")
            {
                var target = FindId(page.D, Id(Expand(page, command.V)));
                if (page.Kq != null || !FocusTarget(target) || HiddenTarget(target)) return false;
                page.Kq = target;
                return true;
            }
            if (command.N == "t")
            {
                var title = Text(Expand(page, command.V), 256);
                if (title.Length == 0) return false;
                page.T = title;
                return true;
            }
            if (command.N == "a")
            {
                if (page.Ms.Count >= MaxAlerts) return false;
                page.Ms.Add(Text(Expand(page, command.V), 512));
                return true;
            }
            if (command.N == "go")
            {
                Uri target;
                if (page.Gp || !GoUrl(page, Expand(page, command.V), out target)) return false;
                page.Gu = target;
                page.Gp = true;
                return true;
            }

            if (command.N == "bg" || command.N == "fg")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string color;
                if (!Color(Expand(page, command.W ?? command.V), out color)) return false;
                return SetStyle(target, command.N == "bg" ? "background-color" : "color", color);
            }
            if (command.N == "fs")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                int size;
                if (!Size(Expand(page, command.W ?? command.V), out size)) return false;
                return SetStyle(target, "font-size", size.ToString() + "px");
            }
            if (command.N == "dp")
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                string display;
                if (!Display(Expand(page, command.W), out display)) return false;
                return SetStyle(target, "display", display);
            }
            if (command.N == "wd")
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                string width;
                if (!E.Wd(Expand(page, command.W), out width)) return false;
                return SetStyle(target, "width", width);
            }
            if (command.N == "ht")
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                string height;
                if (!E.Ht(Expand(page, command.W), out height)) return false;
                return SetStyle(target, "height", height);
            }
            if (command.N == "dr")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string direction;
                if (!E.Dr(Expand(page, command.W ?? command.V), out direction)) return false;
                return SetStyle(target, "direction", direction);
            }
            if (command.N == "ti")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string indent;
                if (!E.Tn(Expand(page, command.W ?? command.V), out indent)) return false;
                return SetStyle(target, "text-indent", indent);
            }
            if (command.N == "ta" || command.N == "taa")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string align;
                if (!E.Tav(Expand(page, command.W ?? command.V), command.N == "ta", out align)) return false;
                return SetStyle(target, command.N == "ta" ? "text-align" : "text-align-all", align);
            }
            if (command.N == "ts")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string shadow;
                if (!E.Tsh(Expand(page, command.W ?? command.V), out shadow)) return false;
                return SetStyle(target, "text-shadow", shadow);
            }
            if (command.N == "uo")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string offset;
                if (!E.Uo(Expand(page, command.W ?? command.V), out offset)) return false;
                return SetStyle(target, "text-underline-offset", offset);
            }
            if (command.N == "tl")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string align;
                if (!E.Tal(Expand(page, command.W ?? command.V), out align)) return false;
                return SetStyle(target, "text-align-last", align);
            }
            if (command.N == "td" || command.N == "tdl" || command.N == "tdc" || command.N == "tds" || command.N == "tdw")
            {
                var target = StyleTarget(page, command);
                if (target == null) return false;
                string decoration;
                if (!E.Tdv(command.N, Expand(page, command.W ?? command.V), out decoration)) return false;
                var property = command.N == "td" ? "text-decoration" : command.N == "tdl" ? "text-decoration-line" : command.N == "tdc" ? "text-decoration-color" :
                    command.N == "tds" ? "text-decoration-style" : "text-decoration-thickness";
                return SetStyle(target, property, decoration);
            }
            var boxProperty = LogicalBoxProperty(command.N);
            if (boxProperty != null)
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                string box;
                if (!E.Bx(Expand(page, command.W), command.N[0] == 'm', command.N.Length == 2 ? 2 : 1, out box)) return false;
                return SetStyle(target, boxProperty, box);
            }
            var borderProperty = LogicalBorderProperty(command.N);
            if (borderProperty != null)
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                var component = borderProperty.EndsWith("-width", StringComparison.Ordinal) ? 0 :
                    borderProperty.EndsWith("-style", StringComparison.Ordinal) ? 1 :
                    borderProperty.EndsWith("-color", StringComparison.Ordinal) ? 2 : -1;
                var maximum = borderProperty == "border-inline-width" || borderProperty == "border-block-width" ||
                    borderProperty == "border-inline-style" || borderProperty == "border-block-style" ||
                    borderProperty == "border-inline-color" || borderProperty == "border-block-color" ? 2 : 1;
                string border;
                if (!E.Bd(Expand(page, command.W), component, maximum, out border)) return false;
                return SetStyle(target, borderProperty, border);
            }
            var radiusProperty = RadiusProperty(command.N);
            if (radiusProperty != null)
            {
                var target = StyleTarget(page, command);
                if (target == null || command.W == null) return false;
                string radius;
                if (!E.Rd(Expand(page, command.W), radiusProperty != "border-radius", out radius)) return false;
                return SetStyle(target, radiusProperty, radius);
            }
            return false;
        }

        private static Cm Parse(string source)
        {
            var dom = Dom(source);
            if (dom != null) return dom;
            var reader = new Rd(source);
            var name = reader.Name().ToLowerInvariant();
            if (name.Length == 0) return null;
            reader.Space();
            if (reader.Take('('))
            {
                reader.Space();
                string first;
                if (!reader.Value(out first)) return null;
                reader.Space();
                string second = null;
                var count = 1;
                if (reader.Take(','))
                {
                    reader.Space();
                    if (!reader.Value(out second)) return null;
                    count = 2;
                    reader.Space();
                }
                if (!reader.Take(')') || !reader.End()) return null;
                return Command(name, first, second, count);
            }
            if (reader.Take('='))
            {
                reader.Space();
                string value;
                if (!reader.Value(out value) || !reader.End()) return null;
                return Assignment(name, value);
            }
            return null;
        }

        // Dom maps a literal, allow-listed getElementById subset onto Lea commands.
        private static Cm Dom(string source)
        {
            var reader = new Rd(source);
            if (reader.Name().ToLowerInvariant() != "document.getelementbyid" || !reader.Take('(')) return null;
            string id;
            if (!reader.Quoted(out id) || !reader.Take(')') || !reader.Take('.')) return null;
            var member = reader.Name().ToLowerInvariant();
            if (member.Length == 0) return null;

            if (member == "focus")
                return reader.Take('(') && reader.Take(')') && reader.End() ? new Cm("fo", id) : null;

            if (member == "classlist.add" || member == "classlist.remove" || member == "classlist.toggle")
            {
                string name;
                if (!reader.Take('(') || !reader.Quoted(out name) || !reader.Take(')') || !reader.End()) return null;
                return new Cm(member == "classlist.add" ? "ac" : member == "classlist.remove" ? "rc" : "tc", id, name);
            }

            if (!reader.Take('=')) return null;
            if (member == "checked" || member == "disabled" || member == "hidden" || member == "open")
            {
                bool enabled;
                if (!reader.Flag(out enabled) || !reader.End()) return null;
                if (member == "checked") return new Cm("ck", id, enabled ? "1" : "0");
                if (member == "disabled") return new Cm(enabled ? "ds" : "en", id);
                if (member == "hidden") return new Cm(enabled ? "hd" : "sh", id);
                return new Cm(enabled ? "op" : "cl", id);
            }

            string value;
            if (!reader.Value(out value) || !reader.End()) return null;
            if (member == "textcontent" || member == "innertext") return new Cm("tx", id, value);
            if (member == "value") return new Cm("vl", id, value);
            if (member == "style.background" || member == "style.backgroundcolor") return new Cm("bg", id, value);
            if (member == "style.color") return new Cm("fg", id, value);
            if (member == "style.fontsize") return new Cm("fs", id, value);
            if (member == "style.display") return new Cm("dp", id, value);
            if (member == "style.width" || member == "style.inlinesize") return new Cm("wd", id, value);
            if (member == "style.height" || member == "style.blocksize") return new Cm("ht", id, value);
            if (member == "style.direction") return new Cm("dr", id, value);
            if (member == "style.textindent") return new Cm("ti", id, value);
            if (member == "style.textalign") return new Cm("ta", id, value);
            if (member == "style.textalignall") return new Cm("taa", id, value);
            if (member == "style.textshadow") return new Cm("ts", id, value);
            if (member == "style.textdecoration") return new Cm("td", id, value);
            if (member == "style.textdecorationline") return new Cm("tdl", id, value);
            if (member == "style.textdecorationcolor") return new Cm("tdc", id, value);
            if (member == "style.textdecorationstyle") return new Cm("tds", id, value);
            if (member == "style.textdecorationthickness") return new Cm("tdw", id, value);
            if (member == "style.textunderlineoffset") return new Cm("uo", id, value);
            if (member == "style.textalignlast") return new Cm("tl", id, value);
            var logicalBox = member.StartsWith("style.", StringComparison.Ordinal) ? LogicalBoxCommand(member.Substring(6)) : null;
            if (logicalBox != null) return new Cm(logicalBox, id, value);
            var logicalBorder = member.StartsWith("style.", StringComparison.Ordinal) ? LogicalBorderCommand(member.Substring(6)) : null;
            if (logicalBorder != null) return new Cm(logicalBorder, id, value);
            var radius = member.StartsWith("style.", StringComparison.Ordinal) ? RadiusCommand(member.Substring(6)) : null;
            if (radius != null) return new Cm(radius, id, value);
            return null;
        }

        private static Cm Command(string name, string first, string second, int count)
        {
            if (count == 1 && (name == "t" || name == "title")) return new Cm("t", first);
            if (count == 1 && (name == "a" || name == "alert")) return new Cm("a", first);
            if (count == 1 && (name == "bg" || name == "background")) return new Cm("bg", first);
            if (count == 1 && (name == "fg" || name == "color")) return new Cm("fg", first);
            if (count == 1 && (name == "fs" || name == "fontsize")) return new Cm("fs", first);
            if (count == 1 && (name == "dr" || name == "direction")) return new Cm("dr", first);
            if (count == 1 && (name == "ti" || name == "textindent")) return new Cm("ti", first);
            if (count == 1 && (name == "ta" || name == "textalign")) return new Cm("ta", first);
            if (count == 1 && (name == "taa" || name == "textalignall")) return new Cm("taa", first);
            if (count == 1 && (name == "ts" || name == "textshadow")) return new Cm("ts", first);
            if (count == 1 && (name == "td" || name == "textdecoration")) return new Cm("td", first);
            if (count == 1 && (name == "tdl" || name == "textdecorationline")) return new Cm("tdl", first);
            if (count == 1 && (name == "tdc" || name == "textdecorationcolor")) return new Cm("tdc", first);
            if (count == 1 && (name == "tds" || name == "textdecorationstyle")) return new Cm("tds", first);
            if (count == 1 && (name == "tdw" || name == "textdecorationthickness")) return new Cm("tdw", first);
            if (count == 1 && (name == "uo" || name == "textunderlineoffset")) return new Cm("uo", first);
            if (count == 1 && (name == "tl" || name == "textalignlast")) return new Cm("tl", first);
            if (count == 1 && name == "go") return new Cm("go", first);
            if (count == 1 && (name == "location.assign" || name == "window.location.assign" || name == "document.location.assign")) return new Cm("go", first);
            if (count == 1 && (name == "hd" || name == "hide")) return new Cm("hd", first);
            if (count == 1 && (name == "sh" || name == "show")) return new Cm("sh", first);
            if (count == 1 && (name == "ds" || name == "disable")) return new Cm("ds", first);
            if (count == 1 && (name == "en" || name == "enable")) return new Cm("en", first);
            if (count == 1 && (name == "fo" || name == "focus")) return new Cm("fo", first);
            if (count == 1 && (name == "ck" || name == "check")) return new Cm("ck", first);
            if (count == 1 && (name == "op" || name == "open")) return new Cm("op", first);
            if (count == 1 && (name == "cl" || name == "close")) return new Cm("cl", first);
            if (count == 1 && (name == "tg" || name == "toggle")) return new Cm("tg", first);
            if (count == 2 && (name == "v" || name == "var")) return new Cm("v", first, second);
            if (count == 2 && (name == "tx" || name == "text")) return new Cm("tx", first, second);
            if (count == 2 && (name == "vl" || name == "value")) return new Cm("vl", first, second);
            if (count == 2 && (name == "ck" || name == "check")) return new Cm("ck", first, second);
            if (count == 2 && (name == "bg" || name == "background")) return new Cm("bg", first, second);
            if (count == 2 && (name == "fg" || name == "color")) return new Cm("fg", first, second);
            if (count == 2 && (name == "fs" || name == "fontsize")) return new Cm("fs", first, second);
            if (count == 2 && (name == "dp" || name == "display")) return new Cm("dp", first, second);
            if (count == 2 && (name == "wd" || name == "width" || name == "is" || name == "inlinesize")) return new Cm("wd", first, second);
            if (count == 2 && (name == "ht" || name == "height" || name == "bs" || name == "blocksize")) return new Cm("ht", first, second);
            if (count == 2 && (name == "dr" || name == "direction")) return new Cm("dr", first, second);
            if (count == 2 && (name == "ti" || name == "textindent")) return new Cm("ti", first, second);
            if (count == 2 && (name == "ta" || name == "textalign")) return new Cm("ta", first, second);
            if (count == 2 && (name == "taa" || name == "textalignall")) return new Cm("taa", first, second);
            if (count == 2 && (name == "ts" || name == "textshadow")) return new Cm("ts", first, second);
            if (count == 2 && (name == "td" || name == "textdecoration")) return new Cm("td", first, second);
            if (count == 2 && (name == "tdl" || name == "textdecorationline")) return new Cm("tdl", first, second);
            if (count == 2 && (name == "tdc" || name == "textdecorationcolor")) return new Cm("tdc", first, second);
            if (count == 2 && (name == "tds" || name == "textdecorationstyle")) return new Cm("tds", first, second);
            if (count == 2 && (name == "tdw" || name == "textdecorationthickness")) return new Cm("tdw", first, second);
            if (count == 2 && (name == "uo" || name == "textunderlineoffset")) return new Cm("uo", first, second);
            if (count == 2 && (name == "tl" || name == "textalignlast")) return new Cm("tl", first, second);
            var logicalBox = LogicalBoxCommand(name);
            if (count == 2 && logicalBox != null) return new Cm(logicalBox, first, second);
            var logicalBorder = LogicalBorderCommand(name);
            if (count == 2 && logicalBorder != null) return new Cm(logicalBorder, first, second);
            var radius = RadiusCommand(name);
            if (count == 2 && radius != null) return new Cm(radius, first, second);
            if (count == 2 && name == "ac") return new Cm("ac", first, second);
            if (count == 2 && name == "rc") return new Cm("rc", first, second);
            if (count == 2 && name == "tc") return new Cm("tc", first, second);
            return null;
        }

        private static Cm Assignment(string name, string value)
        {
            if (name == "document.title") return new Cm("t", value);
            if (name == "document.body.style.background" || name == "document.body.style.backgroundcolor") return new Cm("bg", value);
            if (name == "document.body.style.color") return new Cm("fg", value);
            if (name == "document.body.style.fontsize") return new Cm("fs", value);
            if (name == "document.body.style.direction") return new Cm("dr", value);
            if (name == "document.body.style.textindent") return new Cm("ti", value);
            if (name == "document.body.style.textalign") return new Cm("ta", value);
            if (name == "document.body.style.textalignall") return new Cm("taa", value);
            if (name == "document.body.style.textshadow") return new Cm("ts", value);
            if (name == "document.body.style.textdecoration") return new Cm("td", value);
            if (name == "document.body.style.textdecorationline") return new Cm("tdl", value);
            if (name == "document.body.style.textdecorationcolor") return new Cm("tdc", value);
            if (name == "document.body.style.textdecorationstyle") return new Cm("tds", value);
            if (name == "document.body.style.textdecorationthickness") return new Cm("tdw", value);
            if (name == "document.body.style.textunderlineoffset") return new Cm("uo", value);
            if (name == "document.body.style.textalignlast") return new Cm("tl", value);
            if (name == "location" || name == "window.location" || name == "document.location" ||
                name == "location.href" || name == "window.location.href" || name == "document.location.href") return new Cm("go", value);
            return null;
        }

        private static List<string> Statements(string code)
        {
            var values = new List<string>();
            var text = new StringBuilder();
            var quote = '\0';
            var escape = false;
            var line = false;
            var block = false;
            var round = 0;
            var square = 0;
            var brace = 0;
            for (var i = 0; i < code.Length; i++)
            {
                var ch = code[i];
                var next = i + 1 < code.Length ? code[i + 1] : '\0';
                if (line)
                {
                    if (ch == '\r' || ch == '\n') { line = false; text.Append(' '); }
                    continue;
                }
                if (block)
                {
                    if (ch == '*' && next == '/') { block = false; i++; text.Append(' '); }
                    continue;
                }
                if (quote != '\0')
                {
                    text.Append(ch);
                    if (escape) escape = false;
                    else if (ch == '\\') escape = true;
                    else if (ch == quote) quote = '\0';
                    continue;
                }
                if (ch == '/' && next == '/') { line = true; i++; continue; }
                if (ch == '/' && next == '*') { block = true; i++; continue; }
                if (ch == '\'' || ch == '"') { quote = ch; text.Append(ch); continue; }
                if (ch == '(') round++;
                else if (ch == ')' && round > 0) round--;
                else if (ch == '[') square++;
                else if (ch == ']' && square > 0) square--;
                else if (ch == '{') brace++;
                else if (ch == '}' && brace > 0) brace--;
                if (ch == ';' && round == 0 && square == 0 && brace == 0)
                {
                    Add(values, text);
                    if (values.Count >= MaxCommands) break;
                }
                else text.Append(ch);
            }
            Add(values, text);
            return values;
        }

        private static void Add(List<string> values, StringBuilder text)
        {
            var value = text.ToString().Trim();
            text.Length = 0;
            if (value.Length > 0) values.Add(value);
        }

        private static bool Type(string value)
        {
            var type = (value ?? "").Trim().ToLowerInvariant();
            return type.Length == 0 || type == "text/javascript" || type == "application/javascript" || type == "text/lea" || type == "application/lea";
        }

        private static void Find(N node, List<N> found)
        {
            if (node == null || found.Count >= MaxBlocks) return;
            if (node.K == "script") found.Add(node);
            foreach (var child in node.C) Find(child, found);
        }

        private static N Body(N node)
        {
            if (node == null) return null;
            if (node.K == "body") return node;
            foreach (var child in node.C)
            {
                var found = Body(child);
                if (found != null) return found;
            }
            return node.K == "#doc" ? node : null;
        }

        private static string Raw(N node)
        {
            var value = new StringBuilder();
            Raw(node, value);
            return value.ToString();
        }

        private static void Raw(N node, StringBuilder value)
        {
            if (node.K == "#text") { value.Append(node.X); return; }
            foreach (var child in node.C) Raw(child, value);
        }

        private static N StyleTarget(P page, Cm command)
        {
            if (command.W == null) return Body(page.D);
            var target = FindId(page.D, Id(Expand(page, command.V)));
            return target != null && ClassTarget(target.K) ? target : null;
        }

        private static string LogicalBoxCommand(string name)
        {
            if (name == "mi" || name == "margininline") return "mi";
            if (name == "mb" || name == "marginblock") return "mb";
            if (name == "pi" || name == "paddinginline") return "pi";
            if (name == "pb" || name == "paddingblock") return "pb";
            if (name == "mis" || name == "margininlinestart") return "mis";
            if (name == "mie" || name == "margininlineend") return "mie";
            if (name == "mbs" || name == "marginblockstart") return "mbs";
            if (name == "mbe" || name == "marginblockend") return "mbe";
            if (name == "pis" || name == "paddinginlinestart") return "pis";
            if (name == "pie" || name == "paddinginlineend") return "pie";
            if (name == "pbs" || name == "paddingblockstart") return "pbs";
            if (name == "pbe" || name == "paddingblockend") return "pbe";
            return null;
        }

        private static string LogicalBoxProperty(string command)
        {
            if (command == "mi") return "margin-inline";
            if (command == "mb") return "margin-block";
            if (command == "pi") return "padding-inline";
            if (command == "pb") return "padding-block";
            if (command == "mis") return "margin-inline-start";
            if (command == "mie") return "margin-inline-end";
            if (command == "mbs") return "margin-block-start";
            if (command == "mbe") return "margin-block-end";
            if (command == "pis") return "padding-inline-start";
            if (command == "pie") return "padding-inline-end";
            if (command == "pbs") return "padding-block-start";
            if (command == "pbe") return "padding-block-end";
            return null;
        }

        private static string LogicalBorderCommand(string name)
        {
            if (name == "bi" || name == "borderinline") return "bi";
            if (name == "bb" || name == "borderblock") return "bb";
            if (name == "bis" || name == "borderinlinestart") return "bis";
            if (name == "bie" || name == "borderinlineend") return "bie";
            if (name == "bbs" || name == "borderblockstart") return "bbs";
            if (name == "bbe" || name == "borderblockend") return "bbe";
            if (name == "biw" || name == "borderinlinewidth") return "biw";
            if (name == "biy" || name == "borderinlinestyle") return "biy";
            if (name == "bic" || name == "borderinlinecolor") return "bic";
            if (name == "bbw" || name == "borderblockwidth") return "bbw";
            if (name == "bby" || name == "borderblockstyle") return "bby";
            if (name == "bbc" || name == "borderblockcolor") return "bbc";
            if (name == "siw" || name == "borderinlinestartwidth") return "siw";
            if (name == "siy" || name == "borderinlinestartstyle") return "siy";
            if (name == "sic" || name == "borderinlinestartcolor") return "sic";
            if (name == "eiw" || name == "borderinlineendwidth") return "eiw";
            if (name == "eiy" || name == "borderinlineendstyle") return "eiy";
            if (name == "eic" || name == "borderinlineendcolor") return "eic";
            if (name == "sbw" || name == "borderblockstartwidth") return "sbw";
            if (name == "sby" || name == "borderblockstartstyle") return "sby";
            if (name == "sbc" || name == "borderblockstartcolor") return "sbc";
            if (name == "ebw" || name == "borderblockendwidth") return "ebw";
            if (name == "eby" || name == "borderblockendstyle") return "eby";
            if (name == "ebc" || name == "borderblockendcolor") return "ebc";
            return null;
        }

        private static string LogicalBorderProperty(string command)
        {
            if (command == "bi") return "border-inline";
            if (command == "bb") return "border-block";
            if (command == "bis") return "border-inline-start";
            if (command == "bie") return "border-inline-end";
            if (command == "bbs") return "border-block-start";
            if (command == "bbe") return "border-block-end";
            if (command == "biw") return "border-inline-width";
            if (command == "biy") return "border-inline-style";
            if (command == "bic") return "border-inline-color";
            if (command == "bbw") return "border-block-width";
            if (command == "bby") return "border-block-style";
            if (command == "bbc") return "border-block-color";
            if (command == "siw") return "border-inline-start-width";
            if (command == "siy") return "border-inline-start-style";
            if (command == "sic") return "border-inline-start-color";
            if (command == "eiw") return "border-inline-end-width";
            if (command == "eiy") return "border-inline-end-style";
            if (command == "eic") return "border-inline-end-color";
            if (command == "sbw") return "border-block-start-width";
            if (command == "sby") return "border-block-start-style";
            if (command == "sbc") return "border-block-start-color";
            if (command == "ebw") return "border-block-end-width";
            if (command == "eby") return "border-block-end-style";
            if (command == "ebc") return "border-block-end-color";
            return null;
        }

        private static string RadiusCommand(string name)
        {
            if (name == "rd" || name == "borderradius") return "rd";
            if (name == "rtl" || name == "bordertopleftradius") return "rtl";
            if (name == "rtr" || name == "bordertoprightradius") return "rtr";
            if (name == "rbr" || name == "borderbottomrightradius") return "rbr";
            if (name == "rbl" || name == "borderbottomleftradius") return "rbl";
            if (name == "rss" || name == "borderstartstartradius") return "rss";
            if (name == "rse" || name == "borderstartendradius") return "rse";
            if (name == "res" || name == "borderendstartradius") return "res";
            if (name == "ree" || name == "borderendendradius") return "ree";
            return null;
        }

        private static string RadiusProperty(string command)
        {
            if (command == "rd") return "border-radius";
            if (command == "rtl") return "border-top-left-radius";
            if (command == "rtr") return "border-top-right-radius";
            if (command == "rbr") return "border-bottom-right-radius";
            if (command == "rbl") return "border-bottom-left-radius";
            if (command == "rss") return "border-start-start-radius";
            if (command == "rse") return "border-start-end-radius";
            if (command == "res") return "border-end-start-radius";
            if (command == "ree") return "border-end-end-radius";
            return null;
        }

        private static bool SetStyle(N target, string name, string value)
        {
            var result = new StringBuilder();
            foreach (var item in target.Av("style").Split(';'))
            {
                var declaration = item.Trim();
                var split = declaration.IndexOf(':');
                if (split < 1 || declaration.Substring(0, split).Trim().Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
                result.Append(declaration).Append(';');
            }
            if (value.Length > 0) result.Append(name).Append(':').Append(value).Append(';');
            if (result.Length > 4096) return false;
            if (result.Length == 0) target.A.Remove("style");
            else target.A["style"] = result.ToString();
            return true;
        }

        private static string Expand(P page, string value)
        {
            var source = value ?? "";
            var result = new StringBuilder();
            for (var at = 0; at < source.Length && result.Length < 1024;)
            {
                if (source[at] == '$' && at + 2 < source.Length && source[at + 1] == '{')
                {
                    var end = source.IndexOf('}', at + 2);
                    if (end >= 0)
                    {
                        var name = Variable(source.Substring(at + 2, end - at - 2));
                        string replacement;
                        if (name.Length > 0 && page.Vs.TryGetValue(name, out replacement)) Append(result, replacement, 1024);
                        at = end + 1;
                        continue;
                    }
                }
                result.Append(source[at++]);
            }
            return result.ToString();
        }

        private static void Append(StringBuilder target, string value, int max)
        {
            foreach (var ch in value ?? "")
            {
                if (target.Length == max) break;
                target.Append(ch);
            }
        }

        private static string Variable(string value)
        {
            var name = (value ?? "").Trim().ToLowerInvariant();
            if (name.Length < 1 || name.Length > 16 || name[0] < 'a' || name[0] > 'z') return "";
            foreach (var ch in name)
                if ((ch < 'a' || ch > 'z') && (ch < '0' || ch > '9') && ch != '_') return "";
            return name;
        }

        private static string Id(string value)
        {
            var id = (value ?? "").Trim();
            if (id.Length < 1 || id.Length > 64) return "";
            foreach (var ch in id)
                if (!Char.IsLetterOrDigit(ch) && ch != '-' && ch != '_' && ch != ':' && ch != '.') return "";
            return id;
        }

        private static N FindId(N node, string id)
        {
            if (node == null || id.Length == 0) return null;
            if (node.Av("id").Equals(id, StringComparison.OrdinalIgnoreCase)) return node;
            foreach (var child in node.C)
            {
                var found = FindId(child, id);
                if (found != null) return found;
            }
            return null;
        }

        private static bool TextTarget(string kind)
        {
            return kind != "#doc" && kind != "#text" && kind != "html" && kind != "head" && kind != "body" &&
                kind != "title" && kind != "script" && kind != "style" && kind != "link" && kind != "meta";
        }

        private static bool ClassTarget(string kind)
        {
            return kind != "#doc" && kind != "#text" && kind != "html" && kind != "head" &&
                kind != "title" && kind != "script" && kind != "style" && kind != "link" &&
                kind != "meta" && kind != "base";
        }

        private static bool DisableTarget(string kind)
        {
            return kind == "input" || kind == "textarea" || kind == "select" || kind == "button";
        }

        private static N DisclosureTarget(N target)
        {
            if (target == null) return null;
            if (target.K == "details") return target;
            var details = target.K == "summary" ? target.Up : null;
            if (details == null || details.K != "details") return null;
            foreach (var child in details.C)
                if (child.K == "summary") return Object.ReferenceEquals(child, target) ? details : null;
            return null;
        }

        private static bool FocusTarget(N target)
        {
            if (target == null || target.A.ContainsKey("disabled")) return false;
            if (target.K == "input") return !target.Av("type").Equals("hidden", StringComparison.OrdinalIgnoreCase);
            if (target.K == "textarea" || target.K == "select" || target.K == "button" || target.K == "summary") return true;
            if ((target.K == "a" || target.K == "area") && target.A.ContainsKey("href")) return true;
            var value = target.Av("tabindex").Trim();
            int number;
            return value.Length > 0 && value.Length <= 16 && Int32.TryParse(value, out number) && number >= -32767 && number <= 32767;
        }

        private static bool HiddenTarget(N target)
        {
            for (var current = target; current != null; current = current.Up)
                if (current.A.ContainsKey("hidden")) return true;
            return false;
        }

        private static Fi Field(P page, N target)
        {
            if (target == null) return null;
            foreach (var field in page.Fs)
                if (Object.ReferenceEquals(field.El, target)) return field;
            Fi retained;
            return page.Fr.TryGetValue(target, out retained) ? retained : null;
        }

        private static bool SetValue(P page, N target, string input)
        {
            var field = Field(page, target);
            if (field == null || field.K == "file" || field.K == "submit" || field.K == "reset" || field.K == "button") return false;
            var value = FieldValue(input, field.K == "textarea");
            if (field.K == "color" && !HexColor(value)) return false;
            if (field.K == "select")
            {
                var index = -1;
                for (var i = 0; i < field.Os.Count; i++)
                    if (!field.Os[i].Ds && field.Os[i].V == value) { index = i; break; }
                if (index < 0) return false;
                field.Si = index;
            }
            field.V = value;
            ClearValidity(field);
            return true;
        }

        private static bool SetChecked(P page, N target, string input)
        {
            var field = Field(page, target);
            if (field == null || (field.K != "checkbox" && field.K != "radio")) return false;
            bool value;
            if (input == null) value = !field.C;
            else if (!Boolean(input, out value)) return false;
            if (field.K == "radio" && value)
                foreach (var item in page.Fr.Values)
                    if (item.K == "radio" && Object.ReferenceEquals(item.F, field.F) && item.N == field.N)
                    {
                        item.C = false;
                        ClearValidity(item);
                    }
            field.C = value;
            ClearValidity(field);
            return true;
        }

        private static void ClearValidity(Fi field)
        {
            field.Ok = true;
            field.Vm = "";
            if (field.El != null) field.El.A.Remove("data-lea-invalid");
        }

        private static bool Boolean(string input, out bool value)
        {
            var text = (input ?? "").Trim().ToLowerInvariant();
            if (text == "1" || text == "true" || text == "on" || text == "yes") { value = true; return true; }
            if (text == "0" || text == "false" || text == "off" || text == "no") { value = false; return true; }
            value = false;
            return false;
        }

        private static bool HexColor(string value)
        {
            if (value == null || value.Length != 7 || value[0] != '#') return false;
            for (var i = 1; i < value.Length; i++) if (!Uri.IsHexDigit(value[i])) return false;
            return true;
        }

        private static string FieldValue(string input, bool multiline)
        {
            var result = new StringBuilder();
            foreach (var ch in input ?? "")
            {
                if (ch == '\0' || Char.IsControl(ch) && ch != '\r' && ch != '\n' && ch != '\t') continue;
                if (!multiline && (ch == '\r' || ch == '\n' || ch == '\t')) result.Append(' ');
                else result.Append(ch);
                if (result.Length == MaxFieldValue) break;
            }
            return result.ToString();
        }

        private static string ClassName(string value)
        {
            var name = (value ?? "").Trim();
            if (name.Length < 1 || name.Length > MaxClassName) return "";
            foreach (var ch in name)
                if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z') &&
                    (ch < '0' || ch > '9') && ch != '-' && ch != '_') return "";
            return name;
        }

        private static bool GoUrl(P page, string value, out Uri target)
        {
            target = null;
            var source = (value ?? "").Trim();
            if (source.Length < 1 || source.Length > 2048) return false;
            var baseUrl = page.Bu ?? page.U;
            if (!Uri.TryCreate(source, UriKind.Absolute, out target) &&
                (baseUrl == null || !(baseUrl.IsFile ? FileUrl(baseUrl, source, out target) : Uri.TryCreate(baseUrl, source, out target)))) return false;
            if (target == null || !target.IsAbsoluteUri || target.AbsoluteUri.Length > 4096) return false;
            if (target.IsFile) return page.U != null && page.U.IsFile && !target.IsUnc;
            return (target.Scheme == Uri.UriSchemeHttp || target.Scheme == Uri.UriSchemeHttps) && target.Host.Length > 0;
        }

        private static bool FileUrl(Uri baseUrl, string source, out Uri target)
        {
            target = null;
            var hash = source.IndexOf('#');
            var fragment = hash < 0 ? null : source.Substring(hash + 1);
            var address = hash < 0 ? source : source.Substring(0, hash);
            var mark = address.IndexOf('?');
            var query = mark < 0 ? null : address.Substring(mark + 1);
            var path = mark < 0 ? address : address.Substring(0, mark);
            Uri resolved;
            if (path.Length == 0) resolved = baseUrl;
            else if (!Uri.TryCreate(baseUrl, path, out resolved)) return false;
            try
            {
                var builder = new UriBuilder(resolved);
                if (path.Length > 0 || query != null) builder.Query = query ?? "";
                if (path.Length > 0 || query != null || fragment != null) builder.Fragment = fragment ?? "";
                target = builder.Uri;
                return true;
            }
            catch (UriFormatException) { return false; }
        }

        private static bool SetClass(N target, string name, string mode)
        {
            var source = target.Av("class");
            if (source.Length > MaxClassText) return false;
            var values = new List<string>(source.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            if (values.Count > MaxClasses) return false;
            var found = false;
            for (var i = 0; i < values.Count; i++)
                if (values[i].Equals(name, StringComparison.OrdinalIgnoreCase)) { found = true; break; }

            if (mode == "ac" && found) return true;
            if (mode == "rc" && !found) return true;
            if (found)
            {
                for (var i = values.Count - 1; i >= 0; i--)
                    if (values[i].Equals(name, StringComparison.OrdinalIgnoreCase)) values.RemoveAt(i);
            }
            else
            {
                if (values.Count == MaxClasses) return false;
                values.Add(name);
            }

            var result = String.Join(" ", values.ToArray());
            if (result.Length > MaxClassText) return false;
            if (result.Length == 0) target.A.Remove("class");
            else target.A["class"] = result;
            return true;
        }

        private static bool Color(string value, out string color)
        {
            color = (value ?? "").Trim().ToLowerInvariant();
            if (color.Length == 4 || color.Length == 7)
            {
                if (color[0] != '#') return false;
                for (var i = 1; i < color.Length; i++) if (!Uri.IsHexDigit(color[i])) return false;
                return true;
            }
            if (color.Length < 1 || color.Length > 24) return false;
            foreach (var ch in color) if (ch < 'a' || ch > 'z') return false;
            return true;
        }

        private static bool Size(string value, out int size)
        {
            var source = (value ?? "").Trim().ToLowerInvariant();
            if (source.EndsWith("px")) source = source.Substring(0, source.Length - 2).Trim();
            return Int32.TryParse(source, out size) && size >= 8 && size <= 96;
        }

        private static bool Display(string value, out string display)
        {
            display = (value ?? "").Trim().ToLowerInvariant();
            return display.Length == 0 || display == "none" || display == "block" || display == "inline" ||
                display == "inline-block" || display == "flex" || display == "inline-flex" || display == "grid" ||
                display == "inline-grid" || display == "list-item" || display == "table" || display == "table-row" ||
                display == "table-cell" || display == "initial" || display == "inherit" || display == "unset" ||
                display == "revert" || display == "revert-layer";
        }

        private static string Text(string value, int max)
        {
            var result = new StringBuilder();
            var space = true;
            foreach (var ch in value ?? "")
            {
                if (Char.IsWhiteSpace(ch) || Char.IsControl(ch)) { space = result.Length > 0; continue; }
                if (space && result.Length > 0) result.Append(' ');
                result.Append(ch);
                space = false;
                if (result.Length == max) break;
            }
            return result.ToString().Trim();
        }

        private sealed class Cm
        {
            internal readonly string N;
            internal readonly string V;
            internal readonly string W;

            internal Cm(string name, string value) : this(name, value, null) { }
            internal Cm(string name, string value, string second) { N = name; V = value; W = second; }
        }

        private sealed class Rd
        {
            private readonly string source;
            private int at;

            internal Rd(string value) { source = value ?? ""; }

            internal string Name()
            {
                Space();
                var start = at;
                while (at < source.Length)
                {
                    var ch = source[at];
                    if (!Char.IsLetterOrDigit(ch) && ch != '_' && ch != '$' && ch != '.') break;
                    at++;
                }
                return source.Substring(start, at - start);
            }

            internal bool Value(out string value)
            {
                value = "";
                Space();
                if (at >= source.Length) return false;
                if (source[at] != '\'' && source[at] != '"')
                {
                    var start = at;
                    while (at < source.Length && Char.IsDigit(source[at])) at++;
                    if (start == at) return false;
                    value = source.Substring(start, at - start);
                    return true;
                }

                var quote = source[at++];
                var result = new StringBuilder();
                while (at < source.Length && result.Length <= 1024)
                {
                    var ch = source[at++];
                    if (ch == quote) { value = result.ToString(); return true; }
                    if (ch != '\\') { result.Append(ch); continue; }
                    if (at >= source.Length) return false;
                    ch = source[at++];
                    if (ch == 'n') result.Append('\n');
                    else if (ch == 'r') result.Append('\r');
                    else if (ch == 't') result.Append('\t');
                    else if (ch == 'u')
                    {
                        if (at + 4 > source.Length) return false;
                        var hex = source.Substring(at, 4);
                        int number;
                        if (!Int32.TryParse(hex, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out number)) return false;
                        result.Append((char)number);
                        at += 4;
                    }
                    else result.Append(ch);
                }
                return false;
            }

            internal bool Quoted(out string value)
            {
                Space();
                if (at >= source.Length || source[at] != '\'' && source[at] != '"')
                {
                    value = "";
                    return false;
                }
                return Value(out value);
            }

            internal bool Flag(out bool value)
            {
                var word = Name().ToLowerInvariant();
                if (word == "true") { value = true; return true; }
                if (word == "false") { value = false; return true; }
                value = false;
                return false;
            }

            internal void Space() { while (at < source.Length && Char.IsWhiteSpace(source[at])) at++; }

            internal bool Take(char value)
            {
                Space();
                if (at >= source.Length || source[at] != value) return false;
                at++;
                return true;
            }

            internal bool End() { Space(); return at == source.Length; }
        }
    }
}
