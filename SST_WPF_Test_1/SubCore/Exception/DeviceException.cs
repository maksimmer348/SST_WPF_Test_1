using System;
namespace SST_WPF_Test_1;

//TODO обработка исключений сделать
public class DeviceException : Exception
{
    public DeviceException(string message)
        : base(message)
    { }
}