namespace OpenBootCamp.Service
{
    internal enum AppleKeyboardIOCTL : uint
    {
        SetOSXFnBehaviour = 0xB403201Cu,
        Unknown1 = 0xB4032013u,
        PalmRejection1 = 0xB4032020u,
        PalmRejection2 = 0xB4032024u,
        AcpiBrightnessAvailable = 0xB4032048,
    }
}
