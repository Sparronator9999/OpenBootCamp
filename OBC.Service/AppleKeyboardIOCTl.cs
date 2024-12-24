namespace OBC.Service
{
    internal enum AppleKeyboardIOCTL : uint
    {
        SetOSXFnBehaviour = 0xB403201Cu,
        Unknown1 = 0xB4032013u,
        PalmReject1 = 0xB4032020u,
        PalmReject2 = 0xB4032024u,
        AcpiBrightnessAvailable = 0xB4032048,
    }
}
