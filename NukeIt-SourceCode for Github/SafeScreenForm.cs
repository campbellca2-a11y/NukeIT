using System;
using System.Drawing;
using System.Windows.Forms;

namespace NukeIT
{
    public class SafeScreenForm : Form
    {
        public static event Action? EscapeRequested;

        private Label _timeLabel = null!;
        private Label _dateLabel = null!;
        private Label _statusLabel = null!;
        private Label _kpi1Value = null!;
        private Label _kpi2Value = null!;
        private Label _kpi3Value = null!;
        private Label _kpi4Value = null!;
        private Label _activityLabel = null!;
        private System.Windows.Forms.Timer _uiTimer = null!;
        private readonly Random _random = new();

        public SafeScreenForm(Screen targetScreen)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = targetScreen.Bounds;

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            KeyPreview = true;
            BackColor = Color.FromArgb(242, 245, 249);
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.FromArgb(242, 245, 249),
                Padding = new Padding(28, 22, 28, 22)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var header = BuildHeader();
            var kpiRow = BuildKpiRow();
            var lowerGrid = BuildLowerGrid();

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(kpiRow, 0, 1);
            root.Controls.Add(lowerGrid, 0, 2);

            Controls.Add(root);

            _uiTimer = new System.Windows.Forms.Timer { Interval = 15000 };
            _uiTimer.Tick += (_, _) => RefreshDashboard();
            _uiTimer.Start();

            KeyDown += SafeScreenForm_KeyDown;
            MouseDown += (_, _) => Activate();
            Shown += (_, _) => Activate();

            RefreshDashboard();
        }

        private Control BuildHeader()
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            var left = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            var title = new Label
            {
                Text = "Dashboard",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 32, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusLabel = new Label
            {
                Text = "System active",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(92, 102, 115),
                TextAlign = ContentAlignment.MiddleLeft
            };

            left.Controls.Add(_statusLabel);
            left.Controls.Add(title);

            var right = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            _timeLabel = new Label
            {
                Text = "12:00 PM",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 32, 40),
                TextAlign = ContentAlignment.MiddleRight
            };

            _dateLabel = new Label
            {
                Text = "Monday, January 1",
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(92, 102, 115),
                TextAlign = ContentAlignment.MiddleRight
            };

            right.Controls.Add(_dateLabel);
            right.Controls.Add(_timeLabel);

            header.Controls.Add(left, 0, 0);
            header.Controls.Add(right, 1, 0);

            return header;
        }

        private Control BuildKpiRow()
        {
            var row = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 8)
            };

            for (int i = 0; i < 4; i++)
                row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            _kpi1Value = new Label();
            _kpi2Value = new Label();
            _kpi3Value = new Label();
            _kpi4Value = new Label();

            row.Controls.Add(BuildKpiCard("Queue", _kpi1Value), 0, 0);
            row.Controls.Add(BuildKpiCard("Active Sessions", _kpi2Value), 1, 0);
            row.Controls.Add(BuildKpiCard("System Load", _kpi3Value), 2, 0);
            row.Controls.Add(BuildKpiCard("Tasks Today", _kpi4Value), 3, 0);

            return row;
        }

        private Control BuildKpiCard(string title, Label valueLabel)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(16, 14, 16, 14)
            };
            card.Paint += Card_Paint;

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(98, 109, 122),
                TextAlign = ContentAlignment.MiddleLeft
            };

            valueLabel.Text = "--";
            valueLabel.Dock = DockStyle.Fill;
            valueLabel.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            valueLabel.ForeColor = Color.FromArgb(25, 32, 40);
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;

            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);

            return card;
        }

        private Control BuildLowerGrid()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 8, 0, 0)
            };

            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            grid.Controls.Add(BuildActivityPanel(), 0, 0);
            grid.Controls.Add(BuildStatusPanel(), 1, 0);
            grid.Controls.Add(BuildSchedulePanel(), 0, 1);
            grid.Controls.Add(BuildNotesPanel(), 1, 1);

            return grid;
        }

        private Control BuildActivityPanel()
        {
            var card = CreateCardPanel();

            var title = CreateSectionTitle("Operations Activity");
            _activityLabel = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(58, 68, 79),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(2, 8, 2, 2)
            };

            card.Controls.Add(_activityLabel);
            card.Controls.Add(title);
            return card;
        }

        private Control BuildStatusPanel()
        {
            var card = CreateCardPanel();

            var title = CreateSectionTitle("Status Summary");
            var body = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(58, 68, 79),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(2, 8, 2, 2),
                Text =
                    "• Core services: nominal\r\n" +
                    "• Sync state: active\r\n" +
                    "• Review queue: monitored\r\n" +
                    "• Alerts: none requiring action\r\n\r\n" +
                    "Environment stable."
            };

            card.Controls.Add(body);
            card.Controls.Add(title);
            return card;
        }

        private Control BuildSchedulePanel()
        {
            var card = CreateCardPanel();

            var title = CreateSectionTitle("Today");
            var body = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(58, 68, 79),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(2, 8, 2, 2),
                Text =
                    "09:30  Team sync\r\n" +
                    "11:00  Review block\r\n" +
                    "13:30  Ops check-in\r\n" +
                    "15:00  Project follow-up\r\n" +
                    "16:30  Summary pass"
            };

            card.Controls.Add(body);
            card.Controls.Add(title);
            return card;
        }

        private Control BuildNotesPanel()
        {
            var card = CreateCardPanel();

            var title = CreateSectionTitle("Notes");
            var body = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(58, 68, 79),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(2, 8, 2, 2),
                Text =
                    "• Priority items reviewed\r\n" +
                    "• Pending items queued\r\n" +
                    "• Daily metrics updated\r\n\r\n" +
                    "TEST MODE — Press ESC to exit"
            };

            card.Controls.Add(body);
            card.Controls.Add(title);
            return card;
        }

        private Panel CreateCardPanel()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(8),
                Padding = new Padding(16, 14, 16, 14)
            };
            card.Paint += Card_Paint;
            return card;
        }

        private Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 32, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void RefreshDashboard()
        {
            var now = DateTime.Now;

            _timeLabel.Text = now.ToString("h:mm tt");
            _dateLabel.Text = now.ToString("dddd, MMMM d");
            _statusLabel.Text = GetStatusLine(now);

            if (_kpi1Value.Text == "--")
            {
                _kpi1Value.Text = _random.Next(4, 19).ToString();
                _kpi2Value.Text = _random.Next(2, 8).ToString();
                _kpi3Value.Text = _random.Next(18, 63) + "%";
                _kpi4Value.Text = _random.Next(12, 31).ToString();
            }

            _activityLabel.Text =
                "• Review queue updated " + now.ToString("h:mm tt") + "\r\n" +
                "• Reporting window synced successfully\r\n" +
                "• Daily status package refreshed\r\n" +
                "• Internal checks completed\r\n\r\n" +
                "Next review cycle in 12 minutes.";
        }

        private string GetStatusLine(DateTime now)
        {
            if (now.Hour < 12) return "Morning review window active";
            if (now.Hour < 17) return "Midday operations stable";
            return "Late-day summary cycle active";
        }

        private void Card_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel)
                return;

            var rect = panel.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            using var borderPen = new Pen(Color.FromArgb(221, 227, 234));
            e.Graphics.DrawRectangle(borderPen, rect);
        }

        private void SafeScreenForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                EscapeRequested?.Invoke();
            }
        }

        protected override bool ShowWithoutActivation => false;
    }
}