using System;
using System.Windows;
using System.Windows.Documents;

namespace Lea.Print
{
    // One-based inclusive selection; the caller still owns the original plan.
    public sealed class Pr : DocumentPaginator, IDocumentPaginatorSource
    {
        private readonly Pp pages;
        private readonly int first, count;

        public Pr(Pp pages, int first, int last)
        {
            if (pages == null) throw new ArgumentNullException("pages");
            int total = pages.PageCount;
            if (first < 1 || last < first || last > total)
                throw new ArgumentOutOfRangeException("first", "Range must be within the captured document");
            this.pages = pages;
            this.first = first - 1;
            count = last - first + 1;
        }
        public override bool IsPageCountValid { get { return pages.IsPageCountValid; } }
        public override int PageCount { get { Verify(); return count; } }
        public override IDocumentPaginatorSource Source { get { Verify(); return this; } }
        DocumentPaginator IDocumentPaginatorSource.DocumentPaginator { get { Verify(); return this; } }
        public override Size PageSize
        {
            get { return pages.PageSize; }
            set { pages.PageSize = value; }
        }
        public override DocumentPage GetPage(int pageNumber)
        {
            Verify();
            if (pageNumber < 0) throw new ArgumentOutOfRangeException("pageNumber");
            if (pageNumber >= count) return DocumentPage.Missing;
            return pages.GetPage(first + pageNumber);
        }
        private void Verify()
        {
            // Reading the source also enforces its dispatcher and plan lifetime.
            if (pages.PageCount < first + count) throw new InvalidOperationException("Captured range changed");
        }
    }
}
