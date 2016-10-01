using System;

namespace CodeRedundancyCheck.Model
{
    [Flags]
    public enum CodeLineMeaning
    {
        EnterNamespace = 0x1,
        ExitNamespace = 0x2,
        EnterMethod = 0x4,
        ExitMethod = 0x8,
        EnterScore = 0x10,
        ExitScore = 0x20,
    }
}