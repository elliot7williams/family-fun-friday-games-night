using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lea.Print
{
    // Semantic snapshot layout only: never parse XAML/HTML, invoke links or load resources.
    public static class Pm
    {
        public const int MaxNodes = 8192, MaxColumns = 128;
        public static FlowDocument Cp(Ps snapshot)
        { return Cp(snapshot, false); }
        public static FlowDocument Cp(Ps snapshot, bool grayscale)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            if (snapshot.Nodes.Count > MaxNodes) throw new InvalidOperationException("Structured print node limit");
            return new Builder(snapshot, grayscale).Build();
        }
        public static FlowDocument Ci(Ps snapshot, Pg geometry, IReadOnlyDictionary<int, Pi> images)
        {
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA) throw new InvalidOperationException("STA required");
            if (snapshot == null || geometry == null || images == null) throw new ArgumentNullException("Image snapshot, geometry and map required");
            if (snapshot.Nodes.Count > MaxNodes || images.Count > 16) throw new InvalidOperationException("Image layout node/count limit");
            int bytes = 0;
            foreach (var pair in images)
            {
                if (pair.Key < 0 || pair.Key >= snapshot.Nodes.Count || snapshot.Nodes[pair.Key].Kind != "img" || pair.Value == null) throw new ArgumentException("Invalid image map entry");
                if (pair.Value.Bytes > 8 * 1024 * 1024 - bytes) throw new InvalidOperationException("Image layout byte limit");
                bytes += pair.Value.Bytes;
            }
            for (int i = 0; i < snapshot.Nodes.Count; i++)
                if (snapshot.Nodes[i].Kind == "img" && !images.ContainsKey(i)) throw new InvalidOperationException("Image layout resource missing");
            return new Builder(snapshot, geometry.Grayscale, images, geometry.LayoutWidth, geometry.LayoutHeight).Build();
        }
        private sealed class WhiteSpace { internal bool Pending, Content; }
        private sealed class Builder
        {
            private readonly Ps snapshot;
            private readonly bool grayscale;
            private readonly IReadOnlyDictionary<int, Pi> images;
            private readonly double imageWidth, imageHeight;
            private readonly List<int>[] children;
            private readonly List<int> roots = new List<int>();
            private int characters, allocatedColumns;
            internal Builder(Ps snapshot, bool grayscale, IReadOnlyDictionary<int, Pi> images = null, double imageWidth = 0, double imageHeight = 0)
            {
                this.snapshot = snapshot;
                this.grayscale = grayscale;
                this.images = images; this.imageWidth = imageWidth; this.imageHeight = imageHeight;
                children = new List<int>[snapshot.Nodes.Count];
                int raw = 0;
                for (int i = 0; i < children.Length; i++) children[i] = new List<int>();
                for (int i = 0; i < children.Length; i++)
                {
                    Pn node = snapshot.Nodes[i];
                    if (node.Parent < -1 || node.Parent >= i) throw new InvalidOperationException("Invalid snapshot order");
                    if (node.Text.Length > Pw.MaxText - raw) throw new InvalidOperationException("Structured print text limit");
                    raw += node.Text.Length;
                    string alt;
                    if (node.Kind == "img" && node.Attributes.TryGetValue("alt", out alt))
                    {
                        if (alt.Length > Pw.MaxText - raw) throw new InvalidOperationException("Structured print text limit");
                        raw += alt.Length;
                    }
                    if (node.Parent == -1) roots.Add(i); else children[node.Parent].Add(i);
                }
            }
            internal FlowDocument Build()
            {
                var document = new FlowDocument();
                Blocks(roots, document.Blocks, false);
                return document;
            }
            private string Kind(int i) { return snapshot.Nodes[i].Kind.ToLowerInvariant(); }
            private bool BlockNode(int i)
            {
                Py style = snapshot.Nodes[i].Style;
                if (style != null && style.Display == "inline") return false;
                return Block(Kind(i)) || style != null && (style.Display == "block" || style.Display == "list-item");
            }
            private static bool EmptySpace(string value)
            {
                foreach (char c in value) if (c != ' ' && c != '\t' && c != '\r' && c != '\n' && c != '\f') return false;
                return true;
            }
            private static bool Block(string kind)
            {
                switch (kind)
                {
                    case "#doc": case "html": case "body": case "div": case "section": case "article":
                    case "main": case "header": case "footer": case "nav": case "aside": case "figure":
                    case "figcaption": case "address": case "p": case "blockquote": case "pre":
                    case "h1": case "h2": case "h3": case "h4": case "h5": case "h6":
                    case "ul": case "ol": case "li": case "table": case "hr": return true;
                    default: return false;
                }
            }
            private void Blocks(List<int> nodes, BlockCollection output, bool pre)
            {
                Paragraph paragraph = null;
                var space = new WhiteSpace();
                foreach (int i in nodes)
                {
                    string kind = Kind(i);
                    if (kind == "#text" && EmptySpace(snapshot.Nodes[i].Text) && paragraph == null && !pre) continue;
                    if (kind.StartsWith("#", StringComparison.Ordinal) && kind != "#text" && kind != "#doc") continue;
                    bool preserve = Pz.Pre(snapshot.Nodes[i].Style, pre);
                    if (BlockNode(i))
                    {
                        paragraph = null; space = new WhiteSpace();
                        if (kind == "ul" || kind == "ol")
                        {
                            var list = new System.Windows.Documents.List { MarkerStyle = kind == "ol" ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc, Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(24, 0, 0, 0) };
                            Pz.Ap(list, snapshot.Nodes[i].Style, grayscale);
                            foreach (int child in children[i])
                            {
                                if (Kind(child) == "#text" && EmptySpace(snapshot.Nodes[child].Text)) continue;
                                var item = new ListItem(); list.ListItems.Add(item);
                                Pz.Ap(item, snapshot.Nodes[child].Style, grayscale);
                                Blocks(Kind(child) == "li" ? children[child] : new List<int> { child }, item.Blocks, pre);
                                if (item.Blocks.Count == 0) item.Blocks.Add(new Paragraph());
                            }
                            if (list.ListItems.Count != 0) output.Add(list);
                        }
                        else if (kind == "table")
                        {
                            foreach (int child in children[i]) if (Kind(child) == "caption") Blocks(children[child], output, pre);
                            Table table = Table(i, pre); if (table.RowGroups[0].Rows.Count != 0) output.Add(table);
                        }
                        else if (kind == "hr") output.Add(new Paragraph { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 8, 0, 8) });
                        else
                        {
                            var section = new Section { Margin = new Thickness(0) };
                            if (kind == "p" || kind == "pre") section.Margin = new Thickness(0, 0, 0, 8);
                            if (kind.Length == 2 && kind[0] == 'h' && kind[1] >= '1' && kind[1] <= '6')
                            {
                                section.FontSize = new[] { 32.0, 26, 22, 20, 18, 16 }[kind[1] - '1'];
                                section.FontWeight = FontWeights.Bold; section.Margin = new Thickness(0, 8, 0, 8);
                            }
                            if (kind == "pre") section.FontFamily = new FontFamily("Consolas");
                            if (kind == "blockquote") section.Margin = new Thickness(24, 0, 0, 8);
                            Pz.Ap(section, snapshot.Nodes[i].Style, grayscale);
                            output.Add(section); Blocks(children[i], section.Blocks, Pz.Pre(snapshot.Nodes[i].Style, pre || kind == "pre"));
                        }
                    }
                    else
                    {
                        if (paragraph == null) { paragraph = new Paragraph { Margin = new Thickness(0) }; Pz.Ap(paragraph, snapshot.Nodes[i].Style, grayscale); output.Add(paragraph); }
                        Inline(i, paragraph.Inlines, space, preserve);
                    }
                }
            }
            private void Inline(int i, InlineCollection output, WhiteSpace space, bool pre)
            {
                string kind = Kind(i);
                pre = Pz.Pre(snapshot.Nodes[i].Style, pre);
                if (kind == "#text") { Text(snapshot.Nodes[i].Text, output, space, pre, snapshot.Nodes[i].Style); return; }
                if (kind == "br") { output.Add(new LineBreak()); space.Pending = space.Content = false; return; }
                if (kind == "img")
                {
                    if (images != null)
                    {
                        Pi image = images[i];
                        double scale = Math.Min(1, Math.Min(imageWidth / image.Width, Math.Max(1, imageHeight - 24) / image.Height));
                        var visual = new System.Windows.Controls.Image { Source = Mi.Cp(image, grayscale), Width = image.Width * scale, Height = image.Height * scale, Stretch = Stretch.Fill };
                        output.Add(new InlineUIContainer(visual) { BaselineAlignment = BaselineAlignment.Center });
                        space.Pending = false; space.Content = true; return;
                    }
                    string alt; snapshot.Nodes[i].Attributes.TryGetValue("alt", out alt);
                    Text(String.IsNullOrEmpty(alt) ? "[Image]" : "[Image: " + alt + "]", output, space, pre, snapshot.Nodes[i].Style); return;
                }
                var span = new Span(); output.Add(span);
                if (kind == "b" || kind == "strong") span.FontWeight = FontWeights.Bold;
                if (kind == "i" || kind == "em") span.FontStyle = FontStyles.Italic;
                if (kind == "u" || kind == "a") span.TextDecorations = TextDecorations.Underline;
                if (kind == "s" || kind == "del") span.TextDecorations = TextDecorations.Strikethrough;
                if (kind == "code" || kind == "kbd" || kind == "samp") span.FontFamily = new FontFamily("Consolas");
                Pz.Ap(span, snapshot.Nodes[i].Style, grayscale);
                if (BlockNode(i)) { span.Inlines.Add(new LineBreak()); space.Pending = space.Content = false; }
                foreach (int child in children[i]) Inline(child, span.Inlines, space, pre);
                if (BlockNode(i)) { span.Inlines.Add(new LineBreak()); space.Pending = space.Content = false; }
            }
            private void Text(string value, InlineCollection output, WhiteSpace space, bool pre, Py style)
            {
                string text = value;
                if (!pre)
                {
                    var normalized = new StringBuilder();
                    foreach (char c in value)
                    {
                        if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f') { space.Pending = space.Content; continue; }
                        if (space.Pending) normalized.Append(' ');
                        normalized.Append(c); space.Pending = false; space.Content = true;
                    }
                    text = normalized.ToString();
                }
                if (text.Length == 0) return;
                if (text.Length > Pw.MaxText - characters) throw new InvalidOperationException("Structured print text limit");
                characters += text.Length; var run = new Run(text); Pz.Ap(run, style, grayscale); output.Add(run);
            }
            private int Span(int i, string attribute)
            {
                string raw; int span;
                if (!snapshot.Nodes[i].Attributes.TryGetValue(attribute, out raw) || !Int32.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out span) || span < 1) return 1;
                if (span > MaxColumns) throw new InvalidOperationException("Structured print span limit");
                return span;
            }
            private void Rows(int parent, List<int> rows)
            {
                foreach (int i in children[parent])
                    if (Kind(i) == "tr") rows.Add(i);
                    else if (Kind(i) == "thead" || Kind(i) == "tbody" || Kind(i) == "tfoot") Rows(i, rows);
            }
            private Table Table(int i, bool pre)
            {
                var rows = new List<int>(); Rows(i, rows);
                var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 8) };
                Pz.Ap(table, snapshot.Nodes[i].Style, grayscale);
                var group = new TableRowGroup(); table.RowGroups.Add(group);
                int columns = 0;
                for (int r = 0; r < rows.Count; r++)
                {
                    var row = new TableRow(); int count = 0;
                    Pz.Ap(row, snapshot.Nodes[rows[r]].Style, grayscale);
                    foreach (int cell in children[rows[r]])
                    {
                        string kind = Kind(cell); if (kind != "td" && kind != "th") continue;
                        int span = Span(cell, "colspan"); count += span;
                        if (count > MaxColumns) throw new InvalidOperationException("Structured print column limit");
                        var content = new TableCell { ColumnSpan = span, RowSpan = Math.Min(Span(cell, "rowspan"), rows.Count - r), BorderBrush = Brushes.Gray, BorderThickness = new Thickness(.5), Padding = new Thickness(4) };
                        if (kind == "th") content.FontWeight = FontWeights.Bold;
                        Pz.Ap(content, snapshot.Nodes[cell].Style, grayscale);
                        Blocks(children[cell], content.Blocks, pre);
                        if (content.Blocks.Count == 0) content.Blocks.Add(new Paragraph());
                        row.Cells.Add(content);
                    }
                    if (row.Cells.Count != 0) group.Rows.Add(row);
                    columns = Math.Max(columns, count);
                }
                if (columns > MaxNodes - allocatedColumns) throw new InvalidOperationException("Structured print total column limit");
                allocatedColumns += columns;
                for (int c = 0; c < columns; c++) table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
                return table;
            }
        }
    }
}
