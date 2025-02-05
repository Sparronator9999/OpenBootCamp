namespace OBC.Service
{
    internal sealed class AppleKeyboardDriver : Driver
    {
        public AppleKeyboardDriver(string name) : base(name) { }

        internal bool IOControl(AppleKeyboardIOCTL ctlCode)
        {
            return IOControl((uint)ctlCode);
        }

        internal bool IOControl<T>(AppleKeyboardIOCTL ctlCode, ref T buffer, bool isOutBuffer = false)
            where T : unmanaged
        {
            return IOControl((uint)ctlCode, ref buffer, isOutBuffer);
        }
    }
}
