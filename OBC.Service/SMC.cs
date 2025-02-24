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
                return BitConverter.ToInt32(ToHostOrder(val), 0);
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

            if (IOControl(MacHALDriverIoCtl.GetKeyByIndex, ToHostOrder(inBuffer), outBuffer, out _))
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

    public bool ReadRawData(string key, int len, out byte[] data)
    {
        byte[] inBuffer = GetInBuffer(key),
            outBuffer = new byte[len];

        bool success = IOControl(MacHALDriverIoCtl.ReadKey, inBuffer, outBuffer, out _);
        data = success ? outBuffer : null;
        return success;
    }

    public bool WriteRawData(string key, params byte[] data)
    {
        byte[] inBuffer = GetInBuffer(key, data);
        return IOControl(MacHALDriverIoCtl.WriteKey, inBuffer);
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
        if (ReadRawData(key, 2, out byte[] data))
        {
            value = BytesToFloat(data, 0, 2, false);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteFPE2(string key, float value)
    {
        return WriteRawData(key, FloatToBytes(value, 2, false));
    }

    public bool ReadSP78(string key, out float value)
    {
        if (ReadRawData(key, 2, out byte[] data))
        {
            value = BytesToFloat(data, 0, 8, true);
            return true;
        }
        value = 0;
        return false;
    }

    public bool WriteSP78(string key, float value)
    {
        return WriteRawData(key, FloatToBytes(value, 8, true));
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

    public static float BytesToFloat(byte[] bytes, int index, int fBits, bool signed)
    {
        if (bytes.Length - index < 2)
        {
            throw new ArgumentException("bytes.Length - index must be 2 or more.");
        }

        bytes = ToHostOrder(bytes);

        int intVal = signed
            ? BitConverter.ToInt16(bytes, index)
            : BitConverter.ToUInt16(bytes, index);

        float fracVal = (intVal & BitMask(fBits)) / (float)Math.Pow(2, fBits);
        return (intVal >> fBits) + fracVal;
    }

    public static byte[] FloatToBytes(float f, int fBits, bool signed)
    {
        int intVal = (int)f;
        int fracVal = (int)((f - intVal) * Math.Pow(2, fBits));

        byte[] value;
        if (signed)
        {
            short val = (short)((intVal << fBits) + fracVal);
            value = BitConverter.GetBytes(val);
        }
        else
        {
            ushort val = (ushort)((intVal << fBits) + fracVal);
            value = BitConverter.GetBytes(val);
        }
        return ToHostOrder(value);
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

    private static byte BitMask(int value)
    {
        // TODO: is there a better way to do this?
        return value switch
        {
            0 => 0,
            1 => 0x1,
            2 => 0x3,
            3 => 0x7,
            4 => 0xf,
            5 => 0x1f,
            6 => 0x3f,
            7 => 0x7f,
            8 => 0xff,
            _ => throw new ArgumentException("value must not be more than 8"),
        };
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] buffer, bool isOutBuffer = false)
    {
        return HAL.IOControl((uint)ctlCode, buffer, isOutBuffer);
    }

    private bool IOControl(MacHALDriverIoCtl ctlCode, byte[] inBuffer, byte[] outBuffer, out uint bytesReturned)
    {
        return HAL.IOControl((uint)ctlCode, inBuffer, outBuffer, out bytesReturned);
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
    Unknown1 = 0x20,
    Write = 0x40,
    Read = 0x80,
}
