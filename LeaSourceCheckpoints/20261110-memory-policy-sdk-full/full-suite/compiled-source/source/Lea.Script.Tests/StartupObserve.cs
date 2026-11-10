using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;
using Lea.Diagnostics;
using Lea.Win;

namespace Lea.Script.Tests
{
    internal static class StartupObserve
    {
        internal static void Run(string fixture, string reportPath)
        {
            reportPath = WorkerObservation.ValidateReport(reportPath);
            var probe = new WorkerObservation(Views);
            Exception failure = null;
            bool retired;
            try
            {
                probe.Start();
                try { View.Run(fixture, reportPath + "-view"); }
                catch (Exception error) { failure = error; }
            }
            finally { retired = probe.Stop(); }
            probe.Save(reportPath, "native-view-worker-observation", failure);
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
            Program.Check(retired, "native observation sampler retires after the existing view flow");
            Program.Check(probe.HasCounters && probe.HasThreads, "native observation captures an owned worker with thread/counter data");
        }

        private static IEnumerable<V> Views()
        {
            return Application.OpenForms.Cast<Form>().SelectMany(Controls).OfType<V>();
        }

        private static IEnumerable<Control> Controls(Control parent)
        {
            yield return parent;
            foreach (Control child in parent.Controls)
                foreach (var nested in Controls(child)) yield return nested;
        }
    }
}
