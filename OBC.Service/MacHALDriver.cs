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
