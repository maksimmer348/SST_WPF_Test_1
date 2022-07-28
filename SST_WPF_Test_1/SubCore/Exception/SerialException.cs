using System;

namespace SST_WPF_Test_1;

public class SerialException : Exception
{
    public SerialException(string message)
        : base(message)
    { }
}