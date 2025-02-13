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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OBC.Overlays;

public partial class MainForm : Form
{
    private ObcEventType OverlayMode = ObcEventType.None;
    private int Value = -1;
    private bool ShowTextVal;
    private bool BlurEnabled;
    private readonly float DpiScale;

    private SolidBrush fgBrush;
    private Pen fgPen;
    private readonly StringFormat ValFmt = new()
    {
        Alignment = StringAlignment.Center,
    };

    private readonly NamedPipeClient<ObcEvent> IPCClient = new("ObcEvents");

    private readonly Timer FadeTimer = new();

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
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 10, GraphicsUnit.Point);
        FormBorderStyle = FormBorderStyle.None;
        Icon = Utils.GetEntryAssemblyIcon();
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        TopMost = true;

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        DpiScale = AutoScaleDimensions.Width / 96F;

        FadeTimer.Tick += FadeTimerTick;

        IPCClient.Error += IPCError;
        IPCClient.ServerMessage += IPCMessage;
        IPCClient.Start();
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case 0x1A:  // WM_SETTINGCHANGE
                bool blurEnable = GetBlurEnabled();
                if (blurEnable != BlurEnabled)
                {
                    BlurEnabled = blurEnable;
                    SetTheme();
                }
                break;
            case 0x218: // WM_POWERBROADCAST
                if (m.WParam.ToInt32() == 0x8013)   // PBT_POWERSETTINGCHANGE
                {
                    IntPtr pbsPtr = m.LParam;
                    PowerBroadcastSetting pbsStruct = Marshal.PtrToStructure<PowerBroadcastSetting>(pbsPtr);

                    if (pbsStruct.PowerSetting != User32.LidSwitchGuid)
                    {
                        break;
                    }

                    IPCClient?.PushMessage(new ObcEvent(
                        ObcEventType.LidSwitchChange, pbsStruct.Data));
                }
                break;
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
        SetTheme();
        User32.RegisterLidEvents(Handle);
        base.OnHandleCreated(e);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        User32.UnregisterLidEvents();
        base.OnFormClosing(e);
    }

    private void SetTheme()
    {
        BackColor = BlurEnabled
            ? Color.FromArgb(112, 112, 112)
            : Color.FromArgb(176, 176, 176);
        Color fgColor = BlurEnabled
            ? Color.FromArgb(176, 0, 0, 0)
            : Color.FromArgb(32, 32, 32);

        fgPen?.Dispose();
        fgBrush?.Dispose();
        fgPen = new Pen(fgColor, 6);
        fgBrush = new SolidBrush(fgColor);

        User32.SetBlur(Handle, BlurEnabled);
    }

    private void FadeTimerTick(object sender, EventArgs e)
    {
        if (FadeTimer.Interval > 50)
        {
            FadeTimer.Stop();
            FadeTimer.Interval = 50;
            FadeTimer.Start();
        }
        else
        {
            Opacity -= 0.1;
            if (Opacity <= 0)
            {
                FadeTimer.Stop();
                Hide();
            }
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
        g.SmoothingMode = SmoothingMode.None;
        if (Value > -1)
        {
            int value = (int)(Value * DpiScale * 128 / 100);

            int x = (int)(32 * DpiScale);
            int y = (int)(160 * DpiScale);
            int h = (int)(4 * DpiScale);

            g.FillRectangle(fgBrush, x - 1, y - 1, (int)(130 * DpiScale), h + 2);
            g.FillRectangle(Brushes.White, x, y, value, h);

            if (ShowTextVal)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawString($"{Value}%", Font, fgBrush, 96 * DpiScale, 166 * DpiScale, ValFmt);
            }
        }
    }

    private void DrawEject(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillPolygon(fgBrush, Triangle(64, 64, 64, 40));
        g.SmoothingMode = SmoothingMode.None;
        g.FillRectangle(fgBrush, 64, 112, 64, 8, DpiScale);
    }

    private void DrawKeyLightOff(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.None;
        g.FillRectangle(fgBrush, 72, 102, 48, 8, DpiScale);
    }

    private void DrawKeyLightOn(Graphics g)
    {
        DrawKeyLightOff(g);
        g.DrawLine(fgPen, 96, 51, 96, 71, DpiScale);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.DrawLine(fgPen, 41, 94, 62, 99, DpiScale);
        g.DrawLine(fgPen, 59, 64, 72, 79, DpiScale);
        g.DrawLine(fgPen, 133, 64, 120, 79, DpiScale);
        g.DrawLine(fgPen, 151, 94, 130, 99, DpiScale);
    }

    private void DrawVolMuted(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillPolygon(fgBrush, Speaker(58, 58, 32, 48));
    }

    private void DrawVolUnmuted(Graphics g)
    {
        DrawVolMuted(g);
        g.DrawArc(fgPen, 80, 70, 24, 24, 300, 120, DpiScale);
        g.DrawArc(fgPen, 68, 58, 48, 48, 300, 120, DpiScale);
        g.DrawArc(fgPen, 56, 46, 72, 72, 300, 120, DpiScale);
    }

    private void DrawBrightness(Graphics g)
    {
        int sunX = 48, sunY = 34;

        g.SmoothingMode = SmoothingMode.None;
        g.DrawLine(fgPen, sunX + 48, sunY, sunX + 48, sunY + 28, DpiScale);   // top
        g.DrawLine(fgPen, sunX + 68, sunY + 48, sunX + 96, sunY + 48, DpiScale);   // right
        g.DrawLine(fgPen, sunX + 48, sunY + 68, sunX + 48, sunY + 96, DpiScale);   // bottom
        g.DrawLine(fgPen, sunX, sunY + 48, sunX + 28, sunY + 48, DpiScale);   // left

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.DrawEllipse(fgPen, sunX + 38, sunY + 38, 20, 20, DpiScale);
        g.DrawLine(fgPen, sunX + 82, sunY + 14, sunX + 62, sunY + 34, DpiScale);   // top-right
        g.DrawLine(fgPen, sunX + 82, sunY + 82, sunX + 62, sunY + 62, DpiScale);   // bottom-right
        g.DrawLine(fgPen, sunX + 14, sunY + 82, sunX + 34, sunY + 62, DpiScale);   // bottom-left
        g.DrawLine(fgPen, sunX + 14, sunY + 14, sunX + 34, sunY + 34, DpiScale);   // top-left
    }

    private Point[] Speaker(int x, int y, int w, int h)
    {
        int xs = (int)(x * DpiScale),
            ys = (int)(y * DpiScale),
            ws = (int)(w * DpiScale),
            hs = (int)(h * DpiScale),
            w2 = ws / 2,
            h3 = hs / 3;

        return
        [
            new Point(xs, ys + h3),
            new Point(xs + w2, ys + h3),
            new Point(xs + ws, ys),
            new Point(xs + ws, ys + hs),
            new Point(xs + w2, ys + h3 + h3),
            new Point(xs, ys + h3 + h3),
        ];
    }

    private Point[] Triangle(int x, int y, int w, int h)
    {
        int xs = (int)(x * DpiScale),
            ys = (int)(y * DpiScale),
            ws = (int)(w * DpiScale),
            hs = (int)(h * DpiScale),
            w2 = ws / 2;

        return
        [
            new Point(xs, ys + hs),
            new Point(xs + w2, ys),
            new Point(xs + ws, ys + hs),
        ];
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
