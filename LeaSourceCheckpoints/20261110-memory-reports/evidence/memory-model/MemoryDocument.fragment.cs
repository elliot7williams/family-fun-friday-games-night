        // Host-generated, non-file HTML. External stylesheets are not fetched here.
        public P Ht(string html)
        {
            if (html == null) throw new ArgumentNullException("html");
            var url = new Uri("about:lea-memory/" + Guid.NewGuid().ToString("N"));
            CheckNavigation(url);
            var page = MemoryPage(html, url);
            Push(page);
            return page;
        }

        private P MemoryPage(string html, Uri url)
        {
            if (html.Length > MaxHtml || Encoding.UTF8.GetByteCount(html) > MaxHtml)
                throw new Er("large", url, 0, false, "Lea in-memory HTML exceeds the document limit.", null);
            BeginInput();
            try
            {
                var reply = new Cr(url, html, 200, "text/html", "", "utf-8",
                    Encoding.UTF8.GetByteCount(html), true, DateTime.MinValue, "", "");
                var page = Page(new Ld(reply, false));
                page.Mm = true;
                // Do not inherit a file/web base or load external stylesheet resources.
                page.Bu = url;
                page.D.Bu = url;
                Complete(page, Css(page.D, url, new Dictionary<N, Sc>()));
                page.Je = false;
                return page;
            }
            finally { EndInput(); }
        }
