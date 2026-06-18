using System.Text;
using Lea.Core;

namespace Lea;

internal static class LeaConsole
{
    private static readonly LeaEngine Engine = new();

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Title = "Lea";

        if (args.Length > 0)
        {
            await RunScriptAsync(args[0]);
            return;
        }

        PrintWelcome();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("lea> ");
            Console.ResetColor();

            var line = Console.ReadLine();
            if (line is null)
            {
                return;
            }

            var keepGoing = await RunCommand(line);
            if (!keepGoing)
            {
                return;
            }
        }
    }

    private static async Task RunScriptAsync(string filePath)
    {
        foreach (var line in await File.ReadAllLinesAsync(filePath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("lea> " + trimmed);
            Console.ResetColor();

            var keepGoing = await RunCommand(trimmed);
            if (!keepGoing)
            {
                return;
            }
        }
    }

    private static async Task<bool> RunCommand(string line)
    {
        var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return true;
        }

        var command = parts[0].ToLowerInvariant();
        var arg = parts.Length > 1 ? parts[1] : "";

        try
        {
            switch (command)
            {
                case "u":
                case "url":
                    Require(arg, "Try: u example.com");
                    Render(await Engine.OpenAsync(arg));
                    break;
                case "o":
                case "open":
                    Require(arg, "Try: o https://example.com");
                    Render(await Engine.OpenAsync(arg));
                    break;
                case "g":
                case "go":
                    Require(arg, "Try: g 1");
                    RenderMaybe(await Engine.FollowAsync(int.Parse(arg)));
                    break;
                case "b":
                case "back":
                    RenderMaybe(Engine.Back());
                    break;
                case "f":
                case "forward":
                    RenderMaybe(Engine.Forward());
                    break;
                case "r":
                case "reload":
                    RenderMaybe(await Engine.ReloadAsync());
                    break;
                case "l":
                case "links":
                    PrintLinks(Engine.Current);
                    break;
                case "h":
                case "help":
                    PrintHelp();
                    break;
                case "q":
                case "quit":
                    return false;
                default:
                    Console.WriteLine("Unknown command. Type h for help.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Lea could not do that: " + ex.Message);
            Console.ResetColor();
        }

        return true;
    }

    private static void Render(Page page)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine(page.Title);
        Console.ResetColor();
        Console.WriteLine(page.Url);
        var lineWidth = Console.IsOutputRedirected ? 72 : Math.Clamp(Console.WindowWidth - 1, 20, 72);
        Console.WriteLine(new string('-', lineWidth));
        Console.WriteLine(page.Text);
        Console.WriteLine();
        PrintLinks(page);
    }

    private static void RenderMaybe(Page? page)
    {
        if (page is null)
        {
            Console.WriteLine("Nothing to show.");
            return;
        }

        Render(page);
    }

    private static void PrintLinks(Page? page)
    {
        if (page is null)
        {
            Console.WriteLine("No page loaded.");
            return;
        }

        if (page.Links.Count == 0)
        {
            Console.WriteLine("No links found.");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Links");
        Console.ResetColor();

        for (var i = 0; i < Math.Min(page.Links.Count, 20); i++)
        {
            Console.WriteLine($"{i + 1}. {page.Links[i].Text} -> {page.Links[i].Url}");
        }
    }

    private static void PrintWelcome()
    {
        Console.WriteLine("Lea - tiny web reader engine");
        PrintHelp();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("u site   open URL     g n  follow link");
        Console.WriteLine("b        back         f    forward");
        Console.WriteLine("r        reload       l    links");
        Console.WriteLine("h        help         q    quit");
    }

    private static void Require(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message);
        }
    }
}
