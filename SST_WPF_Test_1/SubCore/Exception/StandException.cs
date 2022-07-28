using System;

namespace SST_WPF_Test_1;

public class StandException : Exception
{
    public StandException(string message)
        : base(message)
    { }
}