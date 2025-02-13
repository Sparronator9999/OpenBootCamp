using System;

namespace OBC.Service.Hardware;

internal static class FloatExtensions
{
    public static float FromFPE2(byte[] bytes, int index = 0)
    {
        return BytesToFloat(bytes, index, 2, false);
    }

    public static float FromSP78(byte[] bytes, int index = 0)
    {
        return BytesToFloat(bytes, index, 8, true);
    }

    public static byte[] ToFPE2(this float f)
    {
        return FloatToBytes(f, 2, false);
    }

    public static byte[] ToSP78(this float f)
    {
        return FloatToBytes(f, 8, true);
    }

    private static byte[] FloatToBytes(float f, int fBits, bool signed)
    {
        int intVal = (int)f;
        int fracVal = (int)((f - intVal) * Math.Pow(2, fBits));

        if (signed)
        {
            short val = (short)((intVal << fBits) + fracVal);
            return BitConverter.GetBytes(val);
        }
        else
        {
            ushort val = (ushort)((intVal << fBits) + fracVal);
            return BitConverter.GetBytes(val);
        }
    }

    private static float BytesToFloat(byte[] bytes, int index, int fBits, bool signed)
    {
        if (bytes.Length - index < 2)
        {
            throw new ArgumentException("bytes.Length - index must be 2 or more.");
        }

        int intVal = signed
            ? BitConverter.ToInt16(bytes, index)
            : BitConverter.ToUInt16(bytes, index);

        float fracVal = (intVal & BitMask(fBits)) / (float)Math.Pow(2, fBits);
        return (intVal >> fBits) + fracVal;
    }

    private static byte BitMask(int value)
    {
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
}
