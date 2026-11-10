using System;
using System.Windows;
using System.Windows.Documents;

namespace Lea.Print
{
    // Adapter only: the caller retains the plan and its owning dispatcher.
    public sealed class Pp : DocumentPaginator, IDocumentPaginatorSource
    {
        private readonly Pw plan;
        private readonly Pf frame;
        private readonly Size size;
        internal Pg Geometry { get { plan.Verify(); return plan.Geometry; } }
        internal bool HasImages { get { plan.Verify(); return plan.UnresolvedImages != 0; } }

        public Pp(Pw plan, string title, string address)
        {
            if (plan == null) throw new ArgumentNullException("plan");
            plan.Verify();
            this.plan = plan;
            frame = new Pf(plan, title, address);
            size = new Size(plan.Geometry.PaperWidth * .96, plan.Geometry.PaperHeight * .96);
        }
        public override bool IsPageCountValid { get { plan.Verify(); return true; } }
        public override int PageCount { get { plan.Verify(); return plan.PageText.Count; } }
        public override IDocumentPaginatorSource Source { get { plan.Verify(); return this; } }
        DocumentPaginator IDocumentPaginatorSource.DocumentPaginator { get { plan.Verify(); return this; } }
        public override Size PageSize
        {
            get { plan.Verify(); return size; }
            set
            {
                plan.Verify();
                if (value != size) throw new ArgumentException("Capture a new plan for different paper geometry", "value");
            }
        }
        public override DocumentPage GetPage(int pageNumber)
        {
            plan.Verify();
            if (pageNumber < 0) throw new ArgumentOutOfRangeException("pageNumber");
            if (pageNumber >= plan.PageText.Count) return DocumentPage.Missing;
            // Fresh wrappers may be disposed independently; the plan owns body pages.
            var bounds = new Rect(size);
            return new DocumentPage(frame.Dr(pageNumber), size, bounds, bounds);
        }
    }
}
