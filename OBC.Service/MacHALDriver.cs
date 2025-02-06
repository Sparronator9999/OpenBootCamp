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

using OBC.Common;

namespace OBC.Service
{
    internal sealed class MacHALDriver : Driver
    {
        public MacHALDriver(string name) : base(name) { }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, byte[] buffer, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, isOutBuffer);
        }
    }
}
