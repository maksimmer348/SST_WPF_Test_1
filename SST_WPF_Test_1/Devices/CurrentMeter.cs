using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class  Currentmeter : BaseDevice
{
    public Currentmeter(string name) : base(name)
    {
        IsDeviceType = "Измеритель тока";
    }
}

public enum MeterMode
{
    VoltageOut1,
    VoltageOut2
}