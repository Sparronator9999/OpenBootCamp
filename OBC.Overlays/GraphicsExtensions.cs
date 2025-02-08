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

using System.Drawing;

namespace OBC.Overlays
{
    internal static class GraphicsExtensions
    {
        /// <inheritdoc cref="Graphics.FillRectangle(Brush, int, int, int, int)"/>
        public static void FillRectangle(this Graphics g, Brush brush, int x, int y, int width, int height, float scale)
        {
            g.FillRectangle(brush, (int)(x * scale), (int)(y * scale), (int)(width * scale), (int)(height * scale));
        }

        /// <inheritdoc cref="Graphics.DrawLine(Pen, int, int, int, int)"/>
        public static void DrawLine(this Graphics g, Pen pen, int x1, int y1, int x2, int y2, float scale)
        {
            g.DrawLine(pen, (int)(x1 * scale), (int)(y1 * scale), (int)(x2 * scale), (int)(y2 * scale));
        }

        /// <inheritdoc cref="Graphics.DrawArc(Pen, int, int, int, int, int, int)"/>
        public static void DrawArc(this Graphics g, Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle, float scale)
        {
            g.DrawArc(pen, (int)(x * scale), (int)(y * scale), (int)(width * scale), (int)(height * scale), startAngle, sweepAngle);
        }

        /// <inheritdoc cref="Graphics.DrawEllipse(Pen, int, int, int, int)"/>
        public static void DrawEllipse(this Graphics g, Pen pen, int x, int y, int width, int height, float scale)
        {
            g.DrawEllipse(pen, (int)(x * scale), (int)(y * scale), (int)(width * scale), (int)(height * scale));
        }
    }
}
