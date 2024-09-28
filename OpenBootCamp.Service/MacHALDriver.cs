using System;

namespace OpenBootCamp.Service
{
    internal sealed class MacHALDriver : Driver
    {
        public MacHALDriver(string name) : base(name) { }

        internal bool IOControl(MacHALDriverIOCTL ctlCode)
        {
            return IOControl((uint)ctlCode);
        }

        internal unsafe bool IOControl(MacHALDriverIOCTL ctlCode, void* buffer, uint bufSize, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, bufSize, isOutBuffer);
        }

        internal unsafe bool IOControl(MacHALDriverIOCTL ctlCode, void* buffer, uint bufSize, out uint bytesReturned, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, bufSize, out bytesReturned, isOutBuffer);
        }

        internal unsafe bool IOControl(MacHALDriverIOCTL ctlCode, void* inBuffer, uint inBufSize, void* outBuffer, uint outBufSize)
        {
            return IOControl((uint)ctlCode, inBuffer, inBufSize, outBuffer, outBufSize);
        }

        internal unsafe bool IOControl(MacHALDriverIOCTL ctlCode, void* inBuffer, uint inBufSize, void* outBuffer, uint outBufSize, out uint bytesReturned)
        {
            return IOControl((uint)ctlCode, inBuffer, inBufSize, outBuffer, outBufSize, out bytesReturned);
        }


        internal bool IOControl(MacHALDriverIOCTL ctlCode, IntPtr buffer, uint bufSize, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, bufSize, isOutBuffer);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, IntPtr buffer, uint bufSize, out uint bytesReturned, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, bufSize, out bytesReturned, isOutBuffer);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, IntPtr inBuffer, uint inBufSize, IntPtr outBuffer, uint outBufSize)
        {
            return IOControl((uint)ctlCode, inBuffer, inBufSize, outBuffer, outBufSize);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, IntPtr inBuffer, uint inBufSize, IntPtr outBuffer, uint outBufSize, out uint bytesReturned)
        {
            return IOControl((uint)ctlCode, inBuffer, inBufSize, outBuffer, outBufSize, out bytesReturned);
        }


        internal bool IOControl(MacHALDriverIOCTL ctlCode, byte[] buffer, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, isOutBuffer);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, byte[] buffer, out uint bytesReturned, bool isOutBuffer = false)
        {
            return IOControl((uint)ctlCode, buffer, out bytesReturned, isOutBuffer);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, byte[] inBuffer, byte[] outBuffer)
        {
            return IOControl((uint)ctlCode, inBuffer, outBuffer);
        }

        internal bool IOControl(MacHALDriverIOCTL ctlCode, byte[] inBuffer, byte[] outBuffer, out uint bytesReturned)
        {
            return IOControl((uint)ctlCode, inBuffer, outBuffer, out bytesReturned);
        }


        internal bool IOControl<T>(MacHALDriverIOCTL ctlCode, ref T buffer, bool isOutBuffer = false)
            where T : unmanaged
        {
            return IOControl((uint)ctlCode, ref buffer, isOutBuffer);
        }

        internal bool IOControl<T>(MacHALDriverIOCTL ctlCode, ref T buffer, out uint bytesReturned, bool isOutBuffer = false)
            where T : unmanaged
        {
            return IOControl((uint)ctlCode, ref buffer, out bytesReturned, isOutBuffer);
        }

        internal bool IOControl<T>(MacHALDriverIOCTL ctlCode, ref T inBuffer, out T outBuffer)
            where T : unmanaged
        {
            return IOControl<T>((uint)ctlCode, ref inBuffer, out outBuffer);
        }

        internal bool IOControl<T>(MacHALDriverIOCTL ctlCode, ref T inBuffer, out T outBuffer, out uint bytesReturned)
            where T : unmanaged
        {
            return IOControl<T>((uint)ctlCode, ref inBuffer, out outBuffer, out bytesReturned);
        }

        internal bool IOControl<TIn, TOut>(MacHALDriverIOCTL ctlCode, ref TIn inBuffer, out TOut outBuffer)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            return IOControl((uint)ctlCode, ref inBuffer, out outBuffer);
        }

        internal bool IOControl<TIn, TOut>(MacHALDriverIOCTL ctlCode, ref TIn inBuffer, out TOut outBuffer, out uint bytesReturned)
            where TIn : unmanaged
            where TOut : unmanaged
        {
            return IOControl((uint)ctlCode, ref inBuffer, out outBuffer, out bytesReturned);
        }
    }
}
