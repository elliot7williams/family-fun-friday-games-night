using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

internal static class PrintSuiteTiming
{
    private sealed class TimedOutput : TextWriter
    {
        private readonly TextWriter output;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        internal TimedOutput(TextWriter output) { this.output = output; }
        public override Encoding Encoding { get { return output.Encoding; } }
        public override void Write(char value) { output.Write(value); }
        public override void Write(string value) { output.Write(value); }
        public override void WriteLine(string value)
        {
            output.WriteLine(value);
            output.WriteLine("TRACE " + clock.ElapsedMilliseconds + "ms " + value);
            output.Flush();
        }
    }
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2) return 2;
        TextWriter original = Console.Out;
        Console.SetOut(new TimedOutput(original));
        try
        {
            Console.WriteLine("TIMING original suite entry begins");
            var suite = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            return (int)suite.EntryPoint.Invoke(null, new object[] { new[] { Path.GetFullPath(args[1]) } });
        }
        catch (TargetInvocationException error) { Console.Error.WriteLine(error.InnerException ?? error); return 1; }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { Console.SetOut(original); }
    }
}
