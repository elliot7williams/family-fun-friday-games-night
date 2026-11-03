using System;
using System.Reflection;
using System.Threading;

internal static class DecoderOwnerProof
{
    private static int checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new InvalidOperationException(name);
        checks++; Console.WriteLine("PASS " + name);
    }

    private static int Main(string[] args)
    {
        try
        {
            Check((int)typeof(DecoderRetirementProof).GetMethod("Main", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { args }) == 0, "prior retirement and queue controls retained");
            var decoder = Assembly.LoadFrom(args[0]).GetType("Lea.Win.Di", true);
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var snapshot = decoder.GetMethod("OwnerSnapshot", flags);
            var mark = decoder.GetMethod("MarkOwner", flags);
            var active = decoder.GetField("activeOwner", flags);
            var ownerType = decoder.GetNestedType("DecoderOwner", BindingFlags.NonPublic);
            Check((string)snapshot.Invoke(null, null) == "; owner=none", "idle queue has no owner");
            using (var cancel = new CancellationTokenSource())
            {
                try
                {
                    var owner = Activator.CreateInstance(ownerType, BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new object[] { "--image-fake", cancel.Token }, null);
                    active.SetValue(null, owner);
                    string text = (string)snapshot.Invoke(null, null);
                    Check(text.Contains(":fake:entry:age=") && text.EndsWith(":cancel=0"), "snapshot identifies fake helper and live owner");
                    mark.Invoke(null, new object[] { "hello" });
                    Check(((string)snapshot.Invoke(null, null)).Contains(":fake:hello:"), "native stage updates owner snapshot");
                    cancel.Cancel();
                    mark.Invoke(null, new object[] { "stop" });
                    text = (string)snapshot.Invoke(null, null);
                    Check(text.Contains(":fake:stop:") && text.EndsWith(":cancel=1"), "canceled retirement stage remains attributable");
                    var other = Activator.CreateInstance(ownerType, BindingFlags.Instance | BindingFlags.NonPublic,
                        null, new object[] { new string('x', 10000), CancellationToken.None }, null);
                    active.SetValue(null, other);
                    text = (string)snapshot.Invoke(null, null);
                    Check(text.Contains(":other:entry:") && text.Length < 128, "arbitrary arguments are not exposed or unbounded");
                }
                finally { active.SetValue(null, null); }
            }
            Check((string)snapshot.Invoke(null, null) == "; owner=none", "owner control restores idle state");
            Console.WriteLine("Decoder owner: checks=" + checks + "; failures=0; no native worker launched");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
