using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class  CurrentMeter : BaseDevice
{
    public CurrentMeter(string name) : base(name)
    {
        IsDeviceType = "Измеритель тока";
    }
}

internal enum MeterMode
{
    VoltageOut1,
    VoltageOut2
}