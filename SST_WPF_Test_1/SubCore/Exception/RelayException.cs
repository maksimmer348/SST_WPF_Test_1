using System;

namespace SST_WPF_Test_1;

public class RelayException : Exception
{
    public RelayException(string message)
        : base(message)
    { }
}