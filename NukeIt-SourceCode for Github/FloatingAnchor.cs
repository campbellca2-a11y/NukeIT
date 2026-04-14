using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NukeIT
{
    public class FloatingAnchor : Form
    {
        public event EventHandler? TriggerRequested;

        private bool _dragging;
        private Point _dragStart;
        private bool _moved;

        public FloatingAnchor()
        {
            FormBorderStyle = FormBorderStyle.None;
            Width = 44;
            Height = 44;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            Opacity = 0.7;

            PositionTopRight();
            Cursor = Cursors.Hand;

            Paint += (_, e) =>
            {
                using var brush = new SolidBrush(Color.White);
                using var font = new Font("Segoe UI", 14, FontStyle.Bold);
                var size = e.Graphics.MeasureString("N", font);
                var x = (Width - size.Width) / 2f;
                var y = (Height - size.Height) / 2f - 1f;
                e.Graphics.DrawString("N", font, brush, new PointF(x, y));
            };

            MouseDown += Anchor_MouseDown;
            MouseMove += Anchor_MouseMove;
            MouseUp += Anchor_MouseUp;
            MouseClick += Anchor_MouseClick;
        }

        private void PositionTopRight()
        {
            var screen = Screen.PrimaryScreen?.WorkingArea ?? Screen.AllScreens[0].WorkingArea;

            int margin = 24;

            Location = new Point(
                screen.Right - Width - margin,
                screen.Top + margin
            );
        }

        private void Anchor_MouseDown(object? sender, MouseEventArgs e)
        {
            _dragging = true;
            _moved = false;
            _dragStart = e.Location;
        }

        private void Anchor_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            int dx = e.X - _dragStart.X;
            int dy = e.Y - _dragStart.Y;

            if (Math.Abs(dx) > 2 || Math.Abs(dy) > 2)
                _moved = true;

            var newPos = Location;
            newPos.Offset(dx, dy);
            Location = newPos;
        }

        private void Anchor_MouseUp(object? sender, MouseEventArgs e)
        {
            _dragging = false;

            if (!_moved && e.Button == MouseButtons.Left)
            {
                TriggerRequested?.Invoke(this, EventArgs.Empty);
                Hide();
            }
        }

        private void Anchor_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
                    {
                        if (form is FloatingAnchor)
                        {
                            form.Close();
                        }
                    }
                }
                else
                {
                    Close();
                }
            }
        }
    }
}