        // Print-only cascade query. No navigation, script, live viewport or paint-tree mutation.
        public System.Collections.ObjectModel.ReadOnlyDictionary<N, S> Pst(P page, int width, int height)
        {
            if (page == null) throw new ArgumentNullException("page");
            if (!OwnsPage(page)) throw new ArgumentException("The page does not belong to this Lea engine.", "page");
            if (width < 1 || width > MaxViewport || height < 1 || height > MaxViewport) throw new ArgumentOutOfRangeException("width");
            const int maxNodes = 8192, maxDepth = 256;
            var clock = System.Diagnostics.Stopwatch.StartNew();
            var nodes = new List<Tuple<N, int, int>>();
            var pending = new Stack<Tuple<N, int, int>>();
            var seen = new HashSet<N>();
            pending.Push(Tuple.Create(page.D, -1, 0));
            int scheduled = 1;
            while (pending.Count != 0)
            {
                var item = pending.Pop(); N node = item.Item1;
                if (node == null || !seen.Add(node) || item.Item3 > maxDepth ||
                    !Object.ReferenceEquals(node.Up, item.Item2 < 0 ? null : nodes[item.Item2].Item1))
                    throw new InvalidOperationException("Malformed print style tree");
                int parent = nodes.Count; nodes.Add(item);
                if (node.C.Count > maxNodes - scheduled) throw new InvalidOperationException("Print style node limit");
                scheduled += node.C.Count;
                for (int i = node.C.Count - 1; i >= 0; i--) pending.Push(Tuple.Create(node.C[i], parent, item.Item3 + 1));
                if (clock.ElapsedMilliseconds > 1000) throw new InvalidOperationException("Print style time limit");
            }
            var all = page.Ra as List<R>;
            if (all == null) throw new InvalidOperationException("Loaded page rules required");
            var active = new List<R>();
            foreach (var rule in all)
                if (rule.On(width, height, 96, page.Th, page.Rm, page.Pc, 'n', page.Pdt, 'n', 'n', 'n', 'n', true)) active.Add(rule);
            var context = new E();
            context.vw = width; context.vh = height; context.dp = 96;
            var styles = new Dictionary<N, S>();
            try
            {
                foreach (var item in nodes)
                {
                    N node = item.Item1;
                    S inherited = item.Item2 < 0 ? null : styles[nodes[item.Item2].Item1];
                    S style = node.K == "#doc" ? new S() : node.K == "#text" ? inherited.Inherit() : context.Style(node, inherited, active);
                    if (DisclosureHidden(node)) style.D = "none";
                    styles.Add(node, style);
                    if (clock.ElapsedMilliseconds > 1000) throw new InvalidOperationException("Print style time limit");
                }
            }
            finally { context.web.Dispose(); }
            return new System.Collections.ObjectModel.ReadOnlyDictionary<N, S>(styles);
        }

