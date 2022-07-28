using System;

namespace SST_WPF_Test_1;
public class VipException : Exception
{
    public VipException(string message)
        : base(message)
    { }
}