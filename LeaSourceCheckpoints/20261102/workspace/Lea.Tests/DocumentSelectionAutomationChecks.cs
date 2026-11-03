using System;
using System.IO;
using System.Reflection;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Automation.Text;
using Lea.Win;

internal static class DocumentSelectionAutomationChecks
{
    internal static int Run(V view, string folder)
    {
        int count = 0;
        Action<bool, string> check = (value, name) => { if (!value) throw new Exception(name); count++; };
        var provider = (ITextProvider)typeof(V).GetProperty("UiRoot", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view, null);
        check(provider.SupportedTextSelection == SupportedTextSelection.Single, "UIA single selection");
        string text = view.Dtx();
        var document = provider.DocumentRange;
        check(document.GetText(-1) == text && !text.Contains("SELECTION_SECRET"), "UIA rendered model without secrets");
        var bravo = document.FindText("bravo", false, false);
        check(bravo != null, "UIA find rendered text");
        bravo.Select();
        check(view.Sds() == "bravo", "UIA selection applied to view");
        var selected = provider.GetSelection();
        check(selected.Length == 1 && selected[0].GetText(-1) == "bravo", "UIA selected range readback");
        view.Sel(0, 0);
        selected = provider.GetSelection();
        check(selected.Length == 1 && selected[0].GetText(-1) == "", "UIA collapsed caret range");
        foreach (var token in new[] { "\ud83d\ude00", "e\u0301" })
        {
            var range = document.FindText(token, false, false);
            check(range != null, "UIA Unicode fixture");
            range.ExpandToEnclosingUnit(TextUnit.Character);
            check(range.GetText(-1) == token, "UIA character preserves text element");
            var next = range.Clone();
            check(next.Move(TextUnit.Character, 1) == 1, "UIA character moves forward");
            check(next.Move(TextUnit.Character, -1) == -1 && next.GetText(-1) == token, "UIA character movement roundtrip");
            var end = range.Clone();
            end.MoveEndpointByRange(TextPatternRangeEndpoint.Start, end, TextPatternRangeEndpoint.End);
            check(end.MoveEndpointByUnit(TextPatternRangeEndpoint.Start, TextUnit.Character, -1) == -1 && end.GetText(-1) == token, "UIA endpoint movement preserves Unicode element");
        }
        check((bool)document.GetAttributeValue(TextPatternIdentifiers.IsReadOnlyAttribute.Id), "UIA document remains noneditable");
        check(provider.GetVisibleRanges().Length > 0, "UIA visible ranges");
        var visible = provider.GetVisibleRanges();
        check(visible.Length == 1 && visible[0].GetText(-1).Contains("Alpha bravo") && visible[0].GetText(-1).Contains("SECOND_LINE"), "UIA contiguous visible lines form one range");
        var snapshot = typeof(V).GetMethod("DocumentSelectionSnapshot", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, null);
        var parts = (System.Collections.IList)snapshot.GetType().GetField("Ps").GetValue(snapshot);
        check(parts.Count >= 3 && (string)parts[1].GetType().GetField("X").GetValue(parts[1]) == "SECOND_LINE", "visible clipping fixture middle part");
        var visibleMethod = typeof(V).GetMethod("DocumentVisibleRanges", BindingFlags.Instance | BindingFlags.NonPublic);
        // Change only a fresh synthetic snapshot, never live layout or selected text.
        var outside = new System.Drawing.Rectangle(0, view.Height + 1000, 100, 20);
        parts[1].GetType().GetField("R").SetValue(parts[1], outside);
        var split = (ITextRangeProvider[])visibleMethod.Invoke(view, new[] { snapshot });
        check(split.Length == 2 && !split[0].GetText(-1).Contains("SECOND_LINE") && !split[1].GetText(-1).Contains("SECOND_LINE"), "UIA invisible middle part separates visible spans");
        foreach (var part in parts) part.GetType().GetField("R").SetValue(part, outside);
        var empty = (ITextRangeProvider[])visibleMethod.Invoke(view, new[] { snapshot });
        check(empty.Length == 1 && empty[0].GetText(-1) == "", "UIA fully clipped snapshot returns degenerate range");
        check(view.Dtx() == text, "synthetic clipping leaves live document unchanged");
        check(bravo.GetBoundingRectangles().Length >= 4, "UIA measured selection geometry");
        var rectangles = bravo.GetBoundingRectangles();
        var hit = provider.RangeFromPoint(new System.Windows.Point(rectangles[0] + 1, rectangles[1] + rectangles[3] / 2));
        hit.ExpandToEnclosingUnit(TextUnit.Character);
        check(hit.GetText(-1) == "b", "UIA measured point resolves first selected character");
        var edge = provider.RangeFromPoint(new System.Windows.Point(-100000, -100000));
        check(edge.GetText(-1) == "", "UIA offscreen point produces collapsed range");
        bool invalidPoint = false;
        try { provider.RangeFromPoint(new System.Windows.Point(Double.NaN, 0)); } catch (ArgumentException) { invalidPoint = true; }
        check(invalidPoint, "UIA nonfinite point rejected");
        bool denied = false;
        try { bravo.RemoveFromSelection(); } catch (InvalidOperationException) { denied = true; }
        check(denied, "UIA disjoint removal rejected");
        var key = typeof(V).GetMethod("DocumentSelectionKey", BindingFlags.Instance | BindingFlags.NonPublic);
        Func<System.Windows.Forms.Keys, bool> press = value => {
            var args = new System.Windows.Forms.KeyEventArgs(value);
            bool handled = (bool)key.Invoke(view, new object[] { args });
            check(!handled || args.Handled && args.SuppressKeyPress, "selection keyboard consumes handled key");
            return handled;
        };
        foreach (var token in new[] { "\ud83d\ude00", "e\u0301" })
        {
            int start = text.IndexOf(token, StringComparison.Ordinal);
            view.Sel(start, start);
            check(press(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right) && view.Sds() == token, "keyboard extends whole Unicode element");
            check(press(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Left) && view.Sds() == "", "keyboard retracts whole Unicode element");
        }
        int bravoStart = text.IndexOf("bravo", StringComparison.Ordinal);
        view.Sel(bravoStart, bravoStart + 5);
        check(press(System.Windows.Forms.Keys.Left) && view.Sds() == "", "keyboard left collapses range");
        press(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.Right);
        check(view.Sds() == "b", "keyboard collapse chooses left edge");
        view.Sel(bravoStart, bravoStart);
        press(System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.End);
        check(view.Sds() == text.Substring(bravoStart), "keyboard document end extension");
        press(System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Home);
        press(System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.End);
        check(view.Sds() == text.Split('\n')[0], "keyboard rendered line end extension");
        view.Go(Path.Combine(folder, "print-friendly.html"));
        denied = false;
        try { bravo.Select(); } catch (ElementNotAvailableException) { denied = true; }
        check(denied, "UIA stale range selection rejected");
        denied = false;
        try { document.GetText(-1); } catch (ElementNotAvailableException) { denied = true; }
        check(denied, "UIA stale range text rejected");
        view.Go(Path.Combine(folder, "document-selection.html")); view.Refresh(); System.Windows.Forms.Application.DoEvents();
#if DOCUMENT_BROKER
        count += DocumentBrokerChecks.Run(view);
#endif
        return count;
    }
}
