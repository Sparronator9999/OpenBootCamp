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

using OBC.Common.Configs;
using OBC.Service.Logs;

namespace OBC.Service.Modules;

internal sealed class BattManager : IObcModule
{
    private readonly BattManConf Config;
    private readonly Logger Log;
    private readonly SMC SMC;

    public BattManager(BattManConf cfg, Logger logger, SMC smc)
    {
        Config = cfg;
        Log = logger;
        SMC = smc;
    }

    public void Start()
    {
        // There is no way that changing the battery charge
        // limit is as simple as writing one value to SMC.
        // (other projects have to do workarounds to fix the charging LED,
        // and they apparently don't work while the laptop is asleep/powered off.
        // Maybe that applies to the Apple Silicon laptops?)
        // I'm still putting this in a separate module since I might implement
        // extra features like auto-calibration, as well as workarounds/fixes if needed.
        if (Config.Enabled && SMC.IsOpen)
        {
            Log.Info($"Setting battery charge limit to {Config.ChargeLimit}%...", nameof(BattManager));
            SMC.WriteUInt8("BCLM", Config.ChargeLimit);
        }
    }

    public void Stop()
    {
    }

    public void Wake()
    {
        // see Start() method
        if (Config.Enabled && SMC.IsOpen)
        {
            Log.Info($"Setting battery charge limit to {Config.ChargeLimit}%...", nameof(BattManager));
            SMC.WriteUInt8("BCLM", Config.ChargeLimit);
        }
    }

    public void Sleep()
    {
    }
}
