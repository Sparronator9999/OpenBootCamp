# OpenBootCamp

An open-source implementation of some of Boot Camp's keyboard services in C#.

## Disclaimers

- This program was developed by using clean-room reverse-engineering
  techniques, including decompiling Boot Camp services.
- This program, repository and its authors are **not** affiliated with Apple
  Inc. in any way, shape, or form.

## Features

OpenBootCamp currently implements the following features of Boot Camp Manager:

- **Fn behaviour switching:** Switch whether the default Fn behaviour should be
  the standard Fn keys, or the special function printed on each key.
- **Special Fn key handling:** This includes keys like the display brightness,
  keyboard brightness, and optical drive eject keys.

Additionally, the following bugs from the official Boot Camp Manager are fixed:

- Keyboard backlight state is not remembered across reboots
- Special Fn keys do not work before login (Apple's Boot Camp Manager doesn't
  run until a user logs in)

<sub>along with introducing a whole lot of other bugs/missing features, but still...</sub>

## Planned features

- Install instructions
- Volume control on older Windows versions (7 and earlier?)
- An overlay when adjusting keyboard backlight, screen brightness, and volume
  (when not already handled by Windows)
- Ability to install Apple drivers (`KeyAgent.sys`, `MacHALDriver.sys`, etc.)
  automatically rather than having to install Boot Camp Services to get them
- A config application. Learn to edit XML for now.
- A few power saving features:
  - Turn off keyboard backlight when closing laptop lid (something which I
    would've thought would be done in hardware/drivers, but here we are)
  - Set keyboard backlight state based on power source or battery charge (e.g.
    low battery, switching between AC and battery power)
  - Turn off keyboard backlight when keyboard is inactive
- Smooth keyboard backlight animations
- More that I haven't thought of yet

## Supported systems

Only Apple computers that can run Windows are supported (i.e. any Intel Mac).
Any non-Apple Windows systems are not supported (even if using a Magic
Mouse/Trackpad/Keyboard).

Windows 7 SP1 and later with .NET Framework 4.8 should work without issues.

Windows Vista/XP may work if you downgrade the project to .NET Framework 4.6
(for Vista SP2) or 4.0 (for XP SP3), but are currently unsupported.

## Additional requirements

The following drivers are required for OpenBootCamp to work:

- The Apple Keyboard driver (`Keymagic.sys`) for OSX Fn behaviour switching
  and enabling display brightness shortcuts on Windows 8 and later.
- `KeyAgent.sys` for enabling other special Fn keys and for brightness/volume
  keys on Windows 7 and lower.
- `MacHALDriver.sys` for keyboard backlight support.

## Download

Downloads are currently unavailable.

Please [compile the program yourself](#compile) in the meantime.

<!--Development builds are available through [GitHub Actions](https://github.com/Sparronator9999/OpenBootCamp/actions).

Alternatively, if you don't have a GitHub account, you can download the latest build from [nightly.link](https://nightly.link/Sparronator9999/OpenBootCamp/workflows/build/main?preview).

(You probably want the `Release` build, unless you're debugging issues with the program)

Alternatively, you can [compile the program yourself](#compile).-->

## Compile

### Using Visual Studio

1.  Install Visual Studio 2022 with the `.NET Desktop Development` workload
    checked.
2.  Download the code repository, or clone it with `git`.
3.  Extract the downloaded code, if needed.
4.  Open `OBC.sln` in Visual Studio.
5.  Click `Build` > `Build Solution` to build everything.
6.  Your output, assuming default build settings, is located in
    `OBC.Service\bin\Debug\net48\`.
7.  ???
8.  Profit!

### From the command line

1.  Follow steps 1-3 above to install Visual Studio and download the code.
2.  Open `Developer Command Prompt for VS 2022` and `cd` to your project
    directory.
3.  Run `msbuild /t:restore` to restore the solution, including NuGet packages.
4.  Run `msbuild OBC.sln /p:platform="Any CPU" /p:configuration="Debug"` to
    build the project, substituting `Debug` with `Release` for a Release build
    instead.
5.  Your output should be located in `OBC.Service\bin\Debug\net48\` (or similar).
6.  ???
7.  Profit!

## FAQ

### How did you make this?!

By decompiling `Bootcamp.exe` (located at `C:\Program Files\Boot Camp` on a Mac
running Windows with Boot Camp installed) with [Binary Ninja](https://binary.ninja/).

I was going to use [ghidra](https://github.com/NationalSecurityAgency/ghidra),
but found Binary Ninja produced more readable pseudo-code.

I will go more in-depth in another document soon.

### *Why* did you make this?

Because I was/am unhappy with the Apple-provided Boot Camp services.

Also, reverse-engineering is fun :D (if you know what you're doing, that is)

### How do I obtain `KeyAgent.sys` and `MacHALDriver.sys`?

**NOTE: OpenBootCamp does not currently install `KeyAgent` and `MacHALDriver` for you.**
**You must install Boot Camp Services in order to use OpenBootCamp for now.**

Download the Boot Camp support software for your Mac using Boot Camp Assistant
(or [Brigadier](https://github.com/timsutton/brigadier) if you're already on Windows),
then navigate to `Drivers\Apple\BootCamp.msi` in 7-Zip. There should be a few .sys files,
but the only ones we care about are `KeyAgent.sys` and `MacHALDriver.sys`. Copy these
to the `Drivers` folder where OpenBootCamp is located.

While you're at it, install the Apple Keyboard drivers if you haven't already
(the installer's called `AppleKeyboardInstaller64.exe`, and is in the same folder as `BootCamp.msi`).

OpenBootCamp will install the drivers if no other `KeyAgent.sys` or `MacHALDriver.sys`
is installed **(coming soon)**.

### My Boot Camp support software has `KeyManager.sys`, but not `KeyAgent.sys`?

If you couldn't find `KeyAgent.sys`, but found `KeyManager.sys`, your laptop is currently
too new for OpenBootCamp. Come back when someone adds support for these laptops.

I won't be able to do this, as I don't have access to a Mac with this configuration.

### I want \<feature X\> in OpenBootCamp!

Ask nicely in the [issues](https://github.com/Sparronator9999/Sparronator9999/issues),
and I might add your feature if I know how, have time, and I think it'll benefit the project.

Alternatively, create a [pull request](https://github.com/Sparronator9999/Sparronator9999/pulls)
and add the feature yourself.

### .NET/Core?

No.

### Linux?

🤦

Linux *already has support* for Apple hardware natively (at least on the laptops I tested it on),
so a Linux port would be pointless.

## License and Copyright

Copyright © 2024 Sparronator9999.

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
PARTICULAR PURPOSE. See the [GNU General Public License](LICENSE.md) for more
details.
