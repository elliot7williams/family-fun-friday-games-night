using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Lea.Print
{
    public static class Ht
    {
        public const int MaxOutput = 16 * 1024 * 1024;
        public static string Cp(Ps snapshot, Po options, string selection)
        { return Cp(snapshot, options, selection, null); }
        public static string Cp(Ps snapshot, Po options, string selection, IReadOnlyDictionary<int, Pi> images)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            if (options == null) throw new ArgumentNullException("options");
            if (selection != null && selection.Length > Ps.MaxCharacters) throw new ArgumentException("Selection limit");
            if (options.SelectionOnly && String.IsNullOrWhiteSpace(selection)) throw new InvalidOperationException("Select page text first");
            if (images != null)
            {
                if (images.Count > 16 || options.SelectionOnly && images.Count != 0) throw new ArgumentException("Print image count/selection limit");
                int bytes = 0;
                foreach (var item in images)
                {
                    if (item.Key < 0 || item.Key >= snapshot.Nodes.Count || snapshot.Nodes[item.Key].Kind != "img" || item.Value == null)
                        throw new ArgumentException("Image must belong to a snapshot image node");
                    if (item.Value.Bytes > 8 * 1024 * 1024 - bytes) throw new InvalidOperationException("Print image aggregate limit");
                    bytes += item.Value.Bytes;
                }
                if (!options.SelectionOnly)
                    for (int index = 0; index < snapshot.Nodes.Count; index++)
                        if (snapshot.Nodes[index].Kind == "img" && !images.ContainsKey(index)) throw new InvalidOperationException("Print image is not ready");
            }
            var output = new LimitedWriter();
            using (var writer = XmlWriter.Create(output, new XmlWriterSettings { OmitXmlDeclaration = true, CheckCharacters = true }))
            {
                writer.WriteDocType("html", null, null, null);
                writer.WriteStartElement("html"); writer.WriteStartElement("head");
                writer.WriteStartElement("meta"); writer.WriteAttributeString("charset", "utf-8"); writer.WriteEndElement();
                writer.WriteStartElement("meta"); writer.WriteAttributeString("http-equiv", "Content-Security-Policy");
                writer.WriteAttributeString("content", "default-src 'none'; img-src data:; style-src 'unsafe-inline'; base-uri 'none'; form-action 'none'"); writer.WriteEndElement();
                writer.WriteElementString("title", snapshot.Title);
                writer.WriteStartElement("style");
                writer.WriteString("@page{size:" + (options.Landscape ? "landscape" : "portrait") + ";margin:" + options.MarginInches.ToString(CultureInfo.InvariantCulture) +
                    "in}body{background:white;color:#111;font:12pt Georgia,serif;line-height:1.5;margin:0 auto;max-width:780px;padding:28px}" +
                    "article{zoom:" + (options.ScalePercent / 100.0).ToString(CultureInfo.InvariantCulture) + ";" + (options.Grayscale ? "filter:grayscale(100%);" : "") + "}" +
                    "img{max-width:100%;height:auto}table{border-collapse:collapse}td,th{border:1px solid #999;padding:4px 7px}pre{white-space:pre-wrap}a{overflow-wrap:anywhere}" +
                    "header,footer{font:9pt Arial,sans-serif;border-top:1px solid #999;padding:8px 0}header h1{font:700 22pt Arial,sans-serif}" +
                    ".page-number{display:none}@media print{.page-number{display:inline}.page-number:after{content:' - Page ' counter(page)}body{padding:0;max-width:none}footer{position:fixed;bottom:0}h1,h2,h3{page-break-after:avoid}}");
                writer.WriteEndElement(); writer.WriteEndElement(); writer.WriteStartElement("body");
                if (options.Header)
                {
                    writer.WriteStartElement("header"); writer.WriteElementString("h1", snapshot.Title);
                    writer.WriteElementString("p", Address(snapshot.Source, true)); writer.WriteEndElement();
                }
                writer.WriteStartElement("article");
                if (options.SelectionOnly) writer.WriteElementString("pre", selection);
                else Content(writer, snapshot, images);
                writer.WriteFullEndElement();
                if (options.Footer)
                {
                    writer.WriteStartElement("footer"); writer.WriteString("Firespider Internet");
                    writer.WriteStartElement("span"); writer.WriteAttributeString("class", "page-number"); writer.WriteFullEndElement(); writer.WriteEndElement();
                }
                writer.WriteFullEndElement(); writer.WriteFullEndElement();
            }
            return output.ToString();
        }
        private static void Content(XmlWriter writer, Ps snapshot, IReadOnlyDictionary<int, Pi> images)
        {
            var parents = new Stack<int>();
            for (int index = 0; index < snapshot.Nodes.Count; index++)
            {
                Pn node = snapshot.Nodes[index];
                while (parents.Count != 0 && parents.Peek() != node.Parent) { writer.WriteFullEndElement(); parents.Pop(); }
                if (node.Parent != -1 && (parents.Count == 0 || parents.Peek() != node.Parent)) throw new InvalidOperationException("Invalid snapshot ancestry");
                if (node.Kind == "#text") { writer.WriteString(node.Text); continue; }
                Pi image;
                if (node.Kind == "img" && images != null && images.TryGetValue(index, out image))
                {
                    writer.WriteStartElement("img"); writer.WriteAttributeString("src", image.Data);
                    writer.WriteAttributeString("width", image.Width.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("height", image.Height.ToString(CultureInfo.InvariantCulture));
                    string alt; if (node.Attributes.TryGetValue("alt", out alt)) writer.WriteAttributeString("alt", alt);
                    writer.WriteEndElement(); continue;
                }
                string tag = Tag(node.Kind);
                writer.WriteStartElement(tag);
                if (tag == "a")
                {
                    string address = Address(node.Link, false);
                    if (address.Length != 0) { writer.WriteAttributeString("href", address); writer.WriteAttributeString("rel", "noreferrer noopener"); }
                }
                foreach (string key in new[] { "colspan", "rowspan" })
                {
                    string value; int number;
                    if ((tag == "td" || tag == "th") && node.Attributes.TryGetValue(key, out value) && Int32.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out number) && number > 0 && number <= 1000)
                        writer.WriteAttributeString(key, number.ToString(CultureInfo.InvariantCulture));
                }
                if (node.Kind == "img") { string alt; if (node.Attributes.TryGetValue("alt", out alt)) writer.WriteString(alt); }
                if (tag == "br" || tag == "hr") writer.WriteEndElement();
                else parents.Push(index);
            }
            while (parents.Count != 0) { writer.WriteFullEndElement(); parents.Pop(); }
        }
        private static string Tag(string kind)
        {
            switch (kind)
            {
                case "p": case "div": case "span": case "section": case "article":
                case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                case "a": case "b": case "strong": case "i": case "em": case "u":
                case "s": case "sub": case "sup": case "small": case "pre": case "code":
                case "blockquote": case "ul": case "ol": case "li": case "dl": case "dt": case "dd":
                case "table": case "thead": case "tbody": case "tfoot": case "tr": case "td": case "th":
                case "caption": case "br": case "hr": return kind;
                default: return "span";
            }
        }
        private static string Address(string value, bool metadata)
        {
            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri) || (uri.Scheme != "http" && uri.Scheme != "https") || uri.UserInfo.Length != 0) return "";
            return metadata ? new UriBuilder(uri) { Query = "", Fragment = "" }.Uri.AbsoluteUri : uri.AbsoluteUri;
        }
        private sealed class LimitedWriter : TextWriter
        {
            private readonly StringBuilder text = new StringBuilder();
            public override Encoding Encoding { get { return Encoding.UTF8; } }
            public override void Write(char value) { Reserve(1); text.Append(value); }
            public override void Write(char[] buffer, int index, int count) { Reserve(count); text.Append(buffer, index, count); }
            public override void Write(string value) { if (value != null) { Reserve(value.Length); text.Append(value); } }
            private void Reserve(int count) { if (count > MaxOutput - text.Length) throw new InvalidOperationException("Print HTML output limit"); }
            public override string ToString() { return text.ToString(); }
        }
    }
}
