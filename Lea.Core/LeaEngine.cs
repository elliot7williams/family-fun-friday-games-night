namespace Lea.Core;

public sealed class LeaEngine
{
    private readonly HttpClient http = new()
    {
        Timeout = TimeSpan.FromSeconds(12)
    };

    private readonly List<Page> history = [];
    private int cursor = -1;

    public Page? Current => cursor >= 0 && cursor < history.Count ? history[cursor] : null;
    public bool CanGoBack => cursor > 0;
    public bool CanGoForward => cursor < history.Count - 1;

    public async Task<Page> OpenAsync(string input)
    {
        var url = ToUri(input, Current?.Url);
        using var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var page = LeaParser.Parse(url, html);

        if (cursor < history.Count - 1)
        {
            history.RemoveRange(cursor + 1, history.Count - cursor - 1);
        }

        history.Add(page);
        cursor = history.Count - 1;
        return page;
    }

    public async Task<Page?> ReloadAsync()
    {
        var current = Current;
        return current is null ? null : await OpenWithoutHistoryAsync(current.Url);
    }

    public Page? Back()
    {
        if (CanGoBack)
        {
            cursor--;
        }

        return Current;
    }

    public Page? Forward()
    {
        if (CanGoForward)
        {
            cursor++;
        }

        return Current;
    }

    public async Task<Page?> FollowAsync(int linkNumber)
    {
        var current = Current;
        if (current is null || linkNumber < 1 || linkNumber > current.Links.Count)
        {
            return null;
        }

        return await OpenAsync(current.Links[linkNumber - 1].Url.ToString());
    }

    private async Task<Page> OpenWithoutHistoryAsync(Uri url)
    {
        using var response = await http.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var page = LeaParser.Parse(url, html);
        history[cursor] = page;
        return page;
    }

    private static Uri ToUri(string input, Uri? baseUrl)
    {
        input = input.Trim();

        if (Uri.TryCreate(input, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        if (baseUrl is not null && Uri.TryCreate(baseUrl, input, out var relative))
        {
            return relative;
        }

        return new Uri("https://" + input);
    }
}
