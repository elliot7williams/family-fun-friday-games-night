using System;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lea.Print
{
    // Preview borrows its paginator; disposing it never disposes the captured plan.
    public sealed class Pv : UserControl, IDisposable
    {
        private readonly Pp pages;
        private readonly PageSurface surface = new PageSurface();
        private readonly ScrollViewer scroll;
        private readonly Button previous, next;
        private readonly TextBlock counter;
        private readonly ComboBox zoom;
        private DocumentPage current;
        private int page;
        private double scale = 1;
        private int? customZoom;
        private bool disposed, updating;
        public int Page { get { Verify(); return page; } }
        public int Count { get { Verify(); return pages.PageCount; } }
        public double Zoom { get { Verify(); return scale; } }

        public Pv(Pp pages)
        {
            if (pages == null) throw new ArgumentNullException("pages");
            int count = pages.PageCount;
            if (count < 1) throw new ArgumentException("Preview pages required", "pages");
            this.pages = pages;
            var root = new DockPanel { Background = new SolidColorBrush(Color.FromRgb(229, 231, 235)) };
            var tools = new WrapPanel { Background = Brushes.White, Margin = new Thickness(0, 0, 0, 1) };
            previous = Tool("\u2190", "Previous page", () => Pg(page - 1));
            next = Tool("\u2192", "Next page", () => Pg(page + 1));
            counter = new TextBlock { Width = 100, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Center };
            tools.Children.Add(previous); tools.Children.Add(counter); tools.Children.Add(next);
            zoom = new ComboBox { Width = 88, Height = 28, Margin = new Thickness(8, 4, 4, 4), IsEditable = false };
            AutomationProperties.SetName(zoom, "Preview zoom");
            foreach (int percent in new[] { 25, 50, 75, 100, 125, 150, 200, 300 }) zoom.Items.Add(percent);
            zoom.ItemStringFormat = "{0}%";
            zoom.SelectionChanged += (sender, args) => { if (!updating && zoom.SelectedItem != null) Zm((int)zoom.SelectedItem / 100.0); };
            tools.Children.Add(zoom);
            tools.Children.Add(Tool("\u2194", "Fit page width", Fit));
            DockPanel.SetDock(tools, Dock.Top); root.Children.Add(tools);
            scroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, CanContentScroll = false };
            surface.Margin = new Thickness(24);
            surface.HorizontalAlignment = HorizontalAlignment.Center;
            surface.VerticalAlignment = VerticalAlignment.Top;
            scroll.Content = surface; root.Children.Add(scroll); Content = root;
            Pg(0);
        }
        private static Button Tool(string glyph, string name, Action action)
        {
            var button = new Button { Content = glyph, Width = 32, Height = 28, Margin = new Thickness(4), ToolTip = name };
            AutomationProperties.SetName(button, name);
            button.Click += (sender, args) => action();
            return button;
        }
        private void Verify()
        {
            Dispatcher.VerifyAccess();
            if (disposed) throw new ObjectDisposedException("Pv");
            if (!pages.IsPageCountValid) throw new InvalidOperationException("Preview requires stable pages");
        }
        public void Pg(int index)
        {
            Verify();
            if (index < 0 || index >= pages.PageCount) throw new ArgumentOutOfRangeException("index");
            var replacement = pages.GetPage(index);
            surface.Set(replacement.Visual, pages.PageSize, scale);
            if (current != null) current.Dispose();
            current = replacement; page = index;
            previous.IsEnabled = page > 0; next.IsEnabled = page + 1 < pages.PageCount;
            counter.Text = (page + 1).ToString(CultureInfo.InvariantCulture) + " / " + pages.PageCount.ToString(CultureInfo.InvariantCulture);
            UpdateZoom(); scroll.ScrollToTop(); scroll.ScrollToLeftEnd();
        }
        public void Zm(double value)
        {
            Verify();
            if (Double.IsNaN(value) || Double.IsInfinity(value) || value < .25 || value > 3) throw new ArgumentOutOfRangeException("value");
            scale = value; surface.Scale(pages.PageSize, scale); UpdateZoom();
        }
        public void Fit()
        {
            Verify();
            if (scroll.ViewportWidth <= 48) return;
            Zm(Math.Max(.25, Math.Min(3, (scroll.ViewportWidth - 48) / pages.PageSize.Width)));
        }
        private void UpdateZoom()
        {
            updating = true;
            try
            {
                if (customZoom.HasValue) { zoom.Items.Remove(customZoom.Value); customZoom = null; }
                int percent = (int)Math.Round(scale * 100);
                if (!zoom.Items.Contains(percent)) { zoom.Items.Add(percent); customZoom = percent; }
                zoom.SelectedItem = percent;
            }
            finally { updating = false; }
        }
        public void Dispose()
        {
            Dispatcher.VerifyAccess();
            if (disposed) return;
            surface.Clear();
            if (current != null) { current.Dispose(); current = null; }
            previous.IsEnabled = next.IsEnabled = zoom.IsEnabled = IsEnabled = false;
            disposed = true;
        }
        private sealed class PageSurface : FrameworkElement
        {
            private readonly ContainerVisual slot = new ContainerVisual();
            internal PageSurface() { AddVisualChild(slot); }
            protected override int VisualChildrenCount { get { return 1; } }
            protected override Visual GetVisualChild(int index) { if (index != 0) throw new ArgumentOutOfRangeException("index"); return slot; }
            internal void Set(Visual visual, Size size, double scale) { Clear(); slot.Children.Add(visual); Scale(size, scale); }
            internal void Scale(Size size, double scale) { Width = size.Width * scale; Height = size.Height * scale; slot.Transform = new ScaleTransform(scale, scale); }
            internal void Clear() { slot.Children.Clear(); }
        }
    }
}
