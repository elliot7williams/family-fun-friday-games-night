function New-FocusClickDiagnostics {
    param([string]$Original)
    $anchor = '                    Click(view, view.Kb);'
    $replacement = @'
                    var focusClickBounds = view.Kb;
                    var focusClickPoint = new Point(focusClickBounds.Left + focusClickBounds.Width / 2, focusClickBounds.Top + focusClickBounds.Height / 2);
                    Console.Error.WriteLine("FOCUSCLICK bounds=" + focusClickBounds + "; scroll=" + view.Sy + "; client=" + view.ClientRectangle);
                    foreach (var focusProbeName in new[] { "BoxAt", "HitAt", "FocusAt" })
                    {
                        var focusProbeMethod = typeof(V).GetMethod(focusProbeName, BindingFlags.Instance | BindingFlags.NonPublic);
                        if (focusProbeMethod == null) throw new InvalidOperationException("Focus diagnostic method missing: " + focusProbeName);
                        var focusProbe = focusProbeMethod.Invoke(view, focusProbeName == "FocusAt" ? new object[] { focusClickPoint, null } : new object[] { focusClickPoint });
                        var focusProbeElement = focusProbe == null ? null : (N)focusProbe.GetType().GetField("El", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(focusProbe);
                        Console.Error.WriteLine("FOCUSCLICK " + focusProbeName + "=" + (focusProbe == null ? "null" : focusProbeElement == null ? "no-element" : focusProbeElement.Av("id")));
                    }
                    Click(view, focusClickBounds);
                    Console.Error.WriteLine("FOCUSCLICK after=" + (view.Ke == null ? "null" : view.Ke.Av("id")));
'@
    if ([regex]::Matches($Original, [regex]::Escape($anchor)).Count -ne 1) { throw 'Focus click anchor differs' }
    $updated = $Original.Replace($anchor, $replacement)
    if ($updated.Replace($replacement, $anchor) -cne $Original) { throw 'Focus diagnostic inverse differs' }
    return $updated
}
