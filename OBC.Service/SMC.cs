using OBC.Common;
using System;
using System.Text;

namespace OBC.Service;

/// <summary>
/// Contains methods to interface with an Apple
/// computer's System Management Controller (SMC).
/// </summary>
internal sealed class SMC : IDisposable
{
    private readonly Driver HAL;

    public bool IsOpen => HAL.IsOpen;

    public int ErrorCode => HAL.ErrorCode;

    public SMC(string name)
    {
        HAL = new Driver(name);
    }

    public bool Open() => HAL.Open();

    public void Close() => HAL.Close();

    public void Dispose() => HAL?.Dispose();

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
        if (!ReadUInt32("#KEY", out uint keyCount))
        {
            return null;
        }

        SMCKeyInfo[] keys = new SMCKeyInfo[keyCount];
        for (int i = 0; i < keyCount; i++)
        {
            byte[] inBuffer = BitConverter.GetBytes(i),
                outBuffer = new byte[5];

            // why is this the only ioctl that expects a little-endian value
            if (IOControl(MacHALDriverIoCtl.GetKeyByIndex, inBuffer, outBuffer))
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

        return IOControl(MacHALDriverIoCtl.GetKeyInfo, inBuffer, outBuffer)
            ? new SMCKeyInfo(key, outBuffer[0], Encoding.UTF8.GetString(outBuffer, 4, 4), (SMCKeyAttributes)BitConverter.ToInt32(outBuffer, 8))
            : null;
    }

    public bool ReadRawData(string key, int len, out byte[] data)
    {
        byte[] inBuffer = GetInBuffer(key);
        data = new byte[len];

        if (IOControl(MacHALDriverIoCtl.ReadKey, inBuffer, data))
        {
            return true;
        }
        data = null;
        return false;
    }

    public bool WriteRawData(string key, params byte[] data)
    {
        return IOControl(MacHALDriverIoCtl.WriteKey, GetInBuffer(key, data));
    }

    public bool ReadInt8(string key, out sbyte value)
    {
        if (ReadRawData(key, sizeof(sbyte), out byte[] data))
        {
            value = unchecked((sbyte)data[0]);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteInt8(string key, sbyte value)
    {
        return WriteRawData(key, unchecked((byte)value));
    }

    public bool ReadUInt8(string key, out byte value)
    {
        if (ReadRawData(key, sizeof(byte), out byte[] data))
        {
            value = data[0];
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteUInt8(string key, byte value)
    {
        return WriteRawData(key, value);
    }

    public bool ReadInt16(string key, out short value)
    {
        if (ReadRawData(key, sizeof(short), out byte[] data))
        {
            value = BitConverter.ToInt16(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteInt16(string key, short value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadUInt16(string key, out ushort value)
    {
        if (ReadRawData(key, sizeof(ushort), out byte[] data))
        {
            value = BitConverter.ToUInt16(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteUInt16(string key, ushort value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadInt32(string key, out int value)
    {
        if (ReadRawData(key, sizeof(int), out byte[] data))
        {
            value = BitConverter.ToInt32(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteInt32(string key, int value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadUInt32(string key, out uint value)
    {
        if (ReadRawData(key, sizeof(uint), out byte[] data))
        {
            value = BitConverter.ToUInt32(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteUInt32(string key, uint value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadInt64(string key, out long value)
    {
        if (ReadRawData(key, sizeof(long), out byte[] data))
        {
            value = BitConverter.ToInt64(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteInt64(string key, long value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadUInt64(string key, out ulong value)
    {
        if (ReadRawData(key, sizeof(ulong), out byte[] data))
        {
            value = BitConverter.ToUInt64(ToHostOrder(data), 0);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteUInt64(string key, ulong value)
    {
        return WriteRawData(key, ToHostOrder(BitConverter.GetBytes(value)));
    }

    public bool ReadFPE2(string key, out float value)
    {
        if (ReadUInt16(key, out ushort val))
        {
            // value / Math.Pow(2, fBits)
            value = val / 4f;
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteFPE2(string key, float value)
    {
        // value * Math.Pow(2, fBits)
        return WriteUInt16(key, (ushort)(value * 4));
    }

    public bool ReadSP78(string key, out float value)
    {
        if (ReadInt16(key, out short val))
        {
            // value / Math.Pow(2, fBits)
            value = val / 256f;
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteSP78(string key, float value)
    {
        // value * Math.Pow(2, fBits)
        return WriteInt16(key, (short)(value * 256));
    }

    private static byte[] GetInBuffer(string code, byte[] data = null)
    {
        if (code.Length != 4)
        {
            throw new ArgumentException("Code length must be equal to 4.", nameof(code));
        }

        byte[] buffer = new byte[5 + (data?.Length ?? 0)];
        byte[] codeStr = Encoding.UTF8.GetBytes(code);

        Array.Copy(codeStr, buffer, codeStr.Length);
        if (data is not null)
        {
            Array.Copy(data, 0, buffer, 5, data.Length);
        }

        return buffer;
    }

    public static byte[] ToHostOrder(byte[] bytes)
    {
        return ToHostOrder(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// Converts an SMC integer value to host order
    /// (or a host value to SMC order).
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte[] ToHostOrder(byte[] bytes, int idx, int len)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes, idx, len);
        }
        return bytes;
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] buffer, bool isOutBuffer = false)
    {
        return HAL.IOControl((uint)ctlCode, buffer, isOutBuffer);
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] inBuffer, byte[] outBuffer)
    {
        return HAL.IOControl((uint)ctlCode, inBuffer, outBuffer);
    }
}

internal sealed class SMCKeyInfo
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
    Write = 0x40,
    Read = 0x80,
}
