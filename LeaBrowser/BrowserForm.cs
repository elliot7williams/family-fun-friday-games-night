using Lea.Core;
using System.Drawing;
using System.Windows.Forms;

namespace LeaBrowser;

public sealed class BrowserForm : Form
{
    private readonly LeaEngine engine = new();
    private readonly TextBox addressBox = new();
    private readonly Button backButton = new();
    private readonly Button forwardButton = new();
    private readonly Button reloadButton = new();
    private readonly Button goButton = new();
    private readonly TextBox pageView = new();
    private readonly ListBox linkList = new();
    private readonly StatusStrip statusStrip = new();
    private readonly ToolStripStatusLabel statusLabel = new();

    public BrowserForm()
    {
        Text = "Lea Test Browser";
        MinimumSize = new Size(860, 560);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        UpdateButtons();
    }

    private void BuildLayout()
    {
        var topBar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            ColumnCount = 5,
            Padding = new Padding(8, 8, 8, 4)
        };

        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));

        ConfigureButton(backButton, "<", BackAsync);
        ConfigureButton(forwardButton, ">", ForwardAsync);
        ConfigureButton(reloadButton, "R", ReloadAsync);
        ConfigureButton(goButton, "Go", OpenAsync);

        addressBox.Dock = DockStyle.Fill;
        addressBox.Font = new Font("Segoe UI", 10);
        addressBox.PlaceholderText = "example.com";
        addressBox.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await OpenAsync();
            }
        };

        topBar.Controls.Add(backButton, 0, 0);
        topBar.Controls.Add(forwardButton, 1, 0);
        topBar.Controls.Add(reloadButton, 2, 0);
        topBar.Controls.Add(addressBox, 3, 0);
        topBar.Controls.Add(goButton, 4, 0);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            SplitterDistance = 580
        };

        pageView.Dock = DockStyle.Fill;
        pageView.Multiline = true;
        pageView.ScrollBars = ScrollBars.Vertical;
        pageView.ReadOnly = true;
        pageView.WordWrap = true;
        pageView.Font = new Font("Segoe UI", 10);

        linkList.Dock = DockStyle.Fill;
        linkList.Font = new Font("Segoe UI", 9);
        linkList.DoubleClick += async (_, _) => await FollowSelectedAsync();
        linkList.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await FollowSelectedAsync();
            }
        };

        split.Panel1.Controls.Add(pageView);
        split.Panel2.Controls.Add(linkList);

        statusStrip.Items.Add(statusLabel);
        statusLabel.Text = "Ready";

        Controls.Add(split);
        Controls.Add(topBar);
        Controls.Add(statusStrip);
    }

    private static void ConfigureButton(Button button, string text, Func<Task> action)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(0, 0, 6, 0);
        button.Click += async (_, _) => await action();
    }

    private async Task OpenAsync()
    {
        if (string.IsNullOrWhiteSpace(addressBox.Text))
        {
            return;
        }

        await LoadPageAsync(() => engine.OpenAsync(addressBox.Text));
    }

    private async Task ReloadAsync()
    {
        await LoadPageAsync(engine.ReloadAsync);
    }

    private Task BackAsync()
    {
        Render(engine.Back());
        return Task.CompletedTask;
    }

    private Task ForwardAsync()
    {
        Render(engine.Forward());
        return Task.CompletedTask;
    }

    private async Task FollowSelectedAsync()
    {
        if (linkList.SelectedIndex < 0)
        {
            return;
        }

        await LoadPageAsync(() => engine.FollowAsync(linkList.SelectedIndex + 1));
    }

    private async Task LoadPageAsync(Func<Task<Page?>> load)
    {
        SetBusy(true);

        try
        {
            Render(await load());
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Error: " + ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Render(Page? page)
    {
        if (page is null)
        {
            UpdateButtons();
            return;
        }

        Text = page.Title + " - Lea Test Browser";
        addressBox.Text = page.Url.ToString();
        pageView.Text = page.Title + Environment.NewLine
            + page.Url + Environment.NewLine
            + new string('-', 72) + Environment.NewLine
            + page.Text;

        linkList.Items.Clear();
        for (var i = 0; i < Math.Min(page.Links.Count, 100); i++)
        {
            linkList.Items.Add($"{i + 1}. {page.Links[i].Text}");
        }

        statusLabel.Text = $"{page.Links.Count} links found";
        UpdateButtons();
    }

    private void SetBusy(bool busy)
    {
        UseWaitCursor = busy;
        goButton.Enabled = !busy;
        reloadButton.Enabled = !busy && engine.Current is not null;
        statusLabel.Text = busy ? "Loading..." : statusLabel.Text;
    }

    private void UpdateButtons()
    {
        backButton.Enabled = engine.CanGoBack;
        forwardButton.Enabled = engine.CanGoForward;
        reloadButton.Enabled = engine.Current is not null;
    }
}
