using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace NukeIT
{
    internal static class Program
    {
        private static Mutex? _singleInstanceMutex;

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            bool triggerMode = args.Any(a => a.Equals("--trigger", System.StringComparison.OrdinalIgnoreCase));

            if (!triggerMode)
            {
                _singleInstanceMutex = new Mutex(true, "NukeIT_SingleInstance", out bool createdNew);
                if (!createdNew)
                {
                    return;
                }
            }

            Application.Run(new PersistentApplicationContext());
        }
    }

    internal sealed class PersistentApplicationContext : ApplicationContext
    {
        private FloatingAnchor? _anchor;
        private readonly List<Form> _safeForms = new();

        public PersistentApplicationContext()
        {
            SafeScreenForm.EscapeRequested += OnEscapeRequested;
            ShowAnchor();
        }

        private void ShowAnchor()
        {
            if (_anchor == null || _anchor.IsDisposed)
            {
                _anchor = new FloatingAnchor();
                _anchor.TriggerRequested += (_, _) => TriggerNuke();
                _anchor.FormClosed += (_, _) => ExitThread();
            }

            _anchor.Show();
            _anchor.BringToFront();
            _anchor.TopMost = true;
        }

        private void TriggerNuke()
        {
            if (_anchor != null && !_anchor.IsDisposed)
                _anchor.Hide();

            _safeForms.Clear();

            var screens = Screen.AllScreens;
            var primary = screens.FirstOrDefault(s => s.Primary) ?? screens[0];
            _safeForms.Add(new SafeScreenForm(primary));

            foreach (var screen in screens.Where(s => !s.Primary))
            {
                _safeForms.Add(new SafeScreenForm(screen));
            }

            foreach (var form in _safeForms)
            {
                form.Show();
            }

            WindowNuker.Nuke();
        }

        private void OnEscapeRequested()
        {
            foreach (var form in _safeForms.ToList())
            {
                if (!form.IsDisposed)
                    form.Close();
            }

            _safeForms.Clear();
            ShowAnchor();
        }
    }
}