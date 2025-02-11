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

namespace OBC.Service
{
    // names taken from SMCKit and adapted for MacHALDriver.
    // link: https://github.com/beltex/SMCKit/blob/master/SMCKit/SMC.swift#L153
    internal enum MacHALDriverIOCTL : uint
    {
        Unknown1 = 0x9C402410u,
        Unknown2 = 0x9C402414u,
        Unknown3 = 0x9C402440u,
        Unknown4 = 0x9C402444u,
        Unknown5 = 0x9C402448u,
        Unknown6 = 0x9C40244Cu,
        Unknown7 = 0x9C402450u,
        ReadKey = 0x9C402454u,
        WriteKey = 0x9C402458u,
        GetKeyByIndex = 0x9C40245Cu,
        GetKeyInfo = 0x9C402460u,
        Unknown12 = 0x9C402464u,
        Unknown13 = 0x9C402484u,
        Unknown14 = 0x9C402488u,
        Unknown15 = 0x9C40248Cu
    }
}
