// This file is part of OpenBootCamp.
// Copyright © Sparronator9999 2024-2025.
//
// OpenBootCamp is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option)
// any later version.
//
// OpenBootCamp is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
//
// You should have received a copy of the GNU General Public License along with
// OpenBootCamp. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.Win32;
using OBC.Common;
using OBC.IPC;
using OBC.Overlays.Win32;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OBC.Overlays
{
    public partial class MainForm : Form
    {
        private ObcEventType OverlayMode = ObcEventType.None;
        private int Value;
        private bool ShowTextVal;

        private readonly SolidBrush fgBrush = new(Color.FromArgb(176, 16, 16, 16));
        private readonly Pen fgPen = new(Color.FromArgb(176, 16, 16, 16), 6);
        private readonly StringFormat ValFmt = new()
        {
            Alignment = StringAlignment.Center,
        };

        private readonly NamedPipeClient<ObcEvent> IPCClient = new("ObcEvents");

        private readonly Timer FadeTimer = new();

        // whether the overlay has a blurred background
        private bool BlurEnabled;

        protected override CreateParams CreateParams
        {
            get
            {
                // prevent overlay from being focused
                CreateParams cParams = base.CreateParams;
                cParams.ExStyle |= 0x08000000;  // WS_EX_NOACTIVATE
                return cParams;
            }
        }

        public MainForm()
        {
            ClientSize = new Size(192, 192);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            Font = new Font("Segoe UI", 10, GraphicsUnit.Point);
            FormBorderStyle = FormBorderStyle.None;
            Icon = Utils.GetEntryAssemblyIcon();
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;

            FadeTimer.Tick += FadeTimerTick;

            IPCClient.Error += IPCError;
            IPCClient.ServerMessage += IPCMessage;
            IPCClient.Start();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x1A)  // WM_SETTINGCHANGE
            {
                bool blurEnable = GetBlurEnabled();
                if (blurEnable != BlurEnabled)
                {
                    BlurEnabled = blurEnable;
                    BackColor = blurEnable
                        ? Color.FromArgb(112, 112, 112)
                        : Color.FromArgb(176, 176, 176);
                    User32.SetBlur(Handle, blurEnable);
                }
            }
            base.WndProc(ref m);
        }

        protected override void SetVisibleCore(bool value)
        {
            // hide overlay on first launch
            if (!IsHandleCreated)
            {
                value = false;
                CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            BlurEnabled = GetBlurEnabled();
            BackColor = BlurEnabled
                ? Color.FromArgb(112, 112, 112)
                : Color.FromArgb(176, 176, 176);
            User32.SetBlur(Handle, BlurEnabled);
            base.OnHandleCreated(e);
        }

        private void FadeTimerTick(object sender, EventArgs e)
        {
            if (FadeTimer.Interval == 2000)
            {
                FadeTimer.Stop();
                FadeTimer.Interval = 50;
                FadeTimer.Start();
            }
            else if (Opacity > 0)
            {
                Opacity -= 0.1;
            }
            else
            {
                FadeTimer.Stop();
                Hide();
            }
        }

        private void IPCMessage(object sender, PipeMessageEventArgs<ObcEvent, ObcEvent> e)
        {
            Invoke(ShowOverlay, e.Message.Event, e.Message.Value);
        }

        private void ShowOverlay(ObcEventType type, int value)
        {
            FadeTimer.Stop();

            OverlayMode = type;
            Value = value;
            ShowTextVal = type == ObcEventType.Volume;

            Opacity = 1;
            FadeTimer.Interval = 2000;
            FadeTimer.Start();

            Invalidate();
            Show();
        }

        private void IPCError(object sender, PipeErrorEventArgs<ObcEvent, ObcEvent> e)
        {
            Utils.ShowError(
                $"ERROR in IPC client connection:\n" +
                $"{e.Exception.GetType()}: {e.Exception.Message}");
            Invoke(Close);
        }

        #region Overlay rendering
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = CurrentAutoScaleDimensions.Width / 96f;

            switch (OverlayMode)
            {
                case ObcEventType.Volume:
                    if (Value > 0)
                    {
                        DrawVolUnmuted(g);
                    }
                    else
                    {
                        DrawVolMuted(g);
                    }
                    break;
                case ObcEventType.DispBright:
                    DrawBrightness(g);
                    break;
                case ObcEventType.KeyLightBright:
                    if (Value > 0)
                    {
                        DrawKeyLightOn(g);
                    }
                    else
                    {
                        DrawKeyLightOff(g);
                    }
                    break;
                case ObcEventType.Eject:
                    DrawEject(g);
                    break;
            }

            // render the "progress bar"
            if (Value > -1)
            {
                int value = (int)(Value * scale * 128 / 100);

                int x = (int)(32 * scale);
                int y = (int)(160 * scale);
                int h = (int)(4 * scale);

                g.FillRectangle(fgBrush, x - 1, y - 1, (int)(130 * scale), h + 2);
                g.FillRectangle(Brushes.White, x, y, value, h);

                if (ShowTextVal)
                {
                    g.DrawString($"{Value}%", Font, fgBrush, 96 * scale, 166 * scale, ValFmt);
                }
            }
        }

        private void DrawEject(Graphics g)
        {
            float scale = CurrentAutoScaleDimensions.Width / 96f;
            g.FillPath(fgBrush, Triangle(64, 64, 64, 40, scale));
            g.FillRectangle(fgBrush, 64, 112, 64, 8, scale);
        }

        private void DrawKeyLightOff(Graphics g)
        {
            float scale = CurrentAutoScaleDimensions.Width / 96f;
            g.FillRectangle(fgBrush, 72, 102, 48, 8, scale);
        }

        private void DrawKeyLightOn(Graphics g)
        {
            DrawKeyLightOff(g);

            float scale = CurrentAutoScaleDimensions.Width / 96f;
            g.DrawLine(fgPen, 40, 94, 61, 99, scale);
            g.DrawLine(fgPen, 58, 64, 71, 79, scale);
            g.DrawLine(fgPen, 96, 52, 96, 70, scale);
            g.DrawLine(fgPen, 134, 64, 121, 79, scale);
            g.DrawLine(fgPen, 152, 94, 131, 99, scale);
        }

        private void DrawVolMuted(Graphics g)
        {
            float scale = CurrentAutoScaleDimensions.Width / 96f;
            g.FillPath(fgBrush, Speaker(58, 58, 32, 48, scale));
        }

        private void DrawVolUnmuted(Graphics g)
        {
            DrawVolMuted(g);

            float scale = CurrentAutoScaleDimensions.Width / 96f;
            g.DrawArc(fgPen, 80, 70, 24, 24, 300, 120, scale);
            g.DrawArc(fgPen, 68, 58, 48, 48, 300, 120, scale);
            g.DrawArc(fgPen, 56, 46, 72, 72, 300, 120, scale);
        }

        private void DrawBrightness(Graphics g)
        {
            float scale = CurrentAutoScaleDimensions.Width / 96f;
            int sunX = 48, sunY = 34;

            g.DrawEllipse(fgPen, sunX + 38, sunY + 38, 20, 20, scale);

            g.DrawLine(fgPen, sunX + 48, sunY,      sunX + 48, sunY + 28, scale);  // top
            g.DrawLine(fgPen, sunX + 82, sunY + 14, sunX + 62, sunY + 34, scale);  // top-right
            g.DrawLine(fgPen, sunX + 68, sunY + 48, sunX + 96, sunY + 48, scale);  // right
            g.DrawLine(fgPen, sunX + 82, sunY + 82, sunX + 62, sunY + 62, scale);  // bottom-right
            g.DrawLine(fgPen, sunX + 48, sunY + 68, sunX + 48, sunY + 96, scale);  // bottom
            g.DrawLine(fgPen, sunX + 14, sunY + 82, sunX + 34, sunY + 62, scale);  // bottom-left
            g.DrawLine(fgPen, sunX,      sunY + 48, sunX + 28, sunY + 48, scale);  // left
            g.DrawLine(fgPen, sunX + 14, sunY + 14, sunX + 34, sunY + 34, scale);  // top-left
        }

        private static GraphicsPath Speaker(int x, int y, int w, int h, float scale)
        {
            GraphicsPath gp = new();
            gp.StartFigure();

            int xs = (int)(x * scale),
                ys = (int)(y * scale),
                ws = (int)(w * scale),
                hs = (int)(h * scale),
                w2 = ws / 2,
                h3 = hs / 3;

            Point[] pts =
            [
                new(xs, ys + h3),
                new(xs + w2, ys + h3),
                new(xs + ws, ys),
                new(xs + ws, ys + hs),
                new(xs + w2, ys + h3 + h3),
                new(xs, ys + h3 + h3),
            ];
            gp.AddLines(pts);

            gp.CloseFigure();
            return gp;
        }

        private static GraphicsPath Triangle(int x, int y, int w, int h, float scale)
        {
            GraphicsPath gp = new();
            gp.StartFigure();

            int xs = (int)(x * scale),
                ys = (int)(y * scale),
                ws = (int)(w * scale),
                hs = (int)(h * scale),
                w2 = ws / 2;

            Point[] pts =
            [
                new(xs, ys + hs),
                new(xs + w2, ys),
                new(xs + ws, ys + hs),
            ];
            gp.AddLines(pts);

            gp.CloseFigure();
            return gp;
        }
        #endregion  // Overlay rendering

        private static bool GetBlurEnabled()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
            {
                object value = key?.GetValue("EnableTransparency");
                return value is int i && i != 0;
            }
        }
    }
}
