using OBC.Common;
using System;
using System.Text;

namespace OBC.Service.Hardware;

/// <summary>
/// Contains methods to interface with an Apple
/// computer's System Management Controller (SMC).
/// </summary>
internal class SMC : Driver
{
    public SMC(string name) : base(name) { }

    public int GetKeyCount()
    {
        byte[] inBuffer = GetInBuffer("#KEY");
        byte[] outBuffer = new byte[32];
        if (IOControl(MacHALDriverIoCtl.ReadKey, inBuffer, outBuffer, out _))
        {
            if (BitConverter.IsLittleEndian)
            {
                // reverse or smth
                byte[] val = new byte[4];
                Array.Copy(outBuffer, val, val.Length);
                Array.Reverse(val);
                return BitConverter.ToInt32(val, 0);
            }
            return BitConverter.ToInt32(outBuffer, 0);
        }
        return 0;
    }

    /// <summary>
    /// Gets all supported keys (SMC functions) for the current computer.
    /// </summary>
    /// <param name="keys">
    /// If successful, will contain an array of SMC keys for the computer.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if successful, otherwise <see langword="false"/>.
    /// </returns>
    public SMCKeyInfo[] GetSupportedKeys()
    {
        SMCKeyInfo[] keys = new SMCKeyInfo[GetKeyCount()];

        int keyCount = GetKeyCount();

        for (int i = 0; i < keyCount; i++)
        {
            byte[] inBuffer = BitConverter.GetBytes(i),
                outBuffer = new byte[5];

            // apparently the index number should be little-endian here,
            // but the value returned by the IOControl call in GetKeyCount()
            // returns a big-endian value???
            // thanks apple for being so easy to work with...
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(inBuffer);
            }

            if (IOControl(MacHALDriverIoCtl.GetKeyByIndex, inBuffer, outBuffer, out _))
            {
                string key = Encoding.UTF8.GetString(outBuffer, 0, 4);
                keys[i] = GetKeyInfo(key);
            }
            else
            {
                return null;
            }
        }
        return keys;
    }

    public SMCKeyInfo GetKeyInfo(string key)
    {
        byte[] inBuffer = GetInBuffer(key),
            outBuffer = new byte[12];

        return IOControl(MacHALDriverIoCtl.GetKeyInfo, inBuffer, outBuffer, out _)
            ? new SMCKeyInfo(key, outBuffer[0], Encoding.UTF8.GetString(outBuffer, 4, 4), (SMCKeyAttributes)BitConverter.ToInt32(outBuffer, 8))
            : null;
    }

    public byte[] ReadData(string key, int len)
    {
        byte[] inBuffer = GetInBuffer(key),
            outBuffer = new byte[len];

        return IOControl(MacHALDriverIoCtl.ReadKey, inBuffer, outBuffer, out _)
            ? outBuffer
            : null;
    }

    public bool WriteData(string key, params byte[] data)
    {
        byte[] inBuffer = GetInBuffer(key, data);
        return IOControl(MacHALDriverIoCtl.WriteKey, inBuffer);
    }

    private static byte[] GetInBuffer(string code, byte[] data = null)
    {
        if (code.Length != 4)
        {
            throw new ArgumentException("Code length must be equal to 4.", nameof(code));
        }

        byte[] buffer = new byte[5 + data?.Length ?? 5];
        byte[] codeStr = Encoding.UTF8.GetBytes(code);

        Array.Copy(codeStr, buffer, codeStr.Length);
        if (data is not null)
        {
            Array.Copy(data, 0, buffer, 5, data.Length);
        }

        return buffer;
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] buffer, bool isOutBuffer = false)
    {
        return IOControl((uint)ctlCode, buffer, isOutBuffer);
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] inBuffer, byte[] outBuffer, out uint bytesReturned)
    {
        return IOControl((uint)ctlCode, inBuffer, outBuffer, out bytesReturned);
    }
}

internal class SMCKeyInfo
{
    public string Key;
    /// <summary>
    /// The length of the data returned by the SMC for the key.
    /// </summary>
    public byte Length;
    /// <summary>
    /// The data type returned/expected by the SMC.
    /// </summary>
    public string TypeString;
    /// <summary>
    /// Attributes associated with the SMC data.
    /// </summary>
    public SMCKeyAttributes Attributes;

    public SMCKeyInfo(string key, byte len, string type, SMCKeyAttributes attr)
    {
        Key = key;
        Length = len;
        TypeString = type;
        Attributes = attr;
    }
}

[Flags]
// values taken from: https://github.com/acidanthera/VirtualSMC/blob/master/VirtualSMCSDK/AppleSmc.h
internal enum SMCKeyAttributes
{
    None = 0,
    PrivateWrite = 1,
    PrivateRead = 2,
    Atomic = 4,
    Const = 8,
    Function = 0x10,
    Unknown1 = 0x20,
    Write = 0x40,
    Read = 0x80,
}
