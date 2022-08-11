using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class  VoltageCurrentMeter : BaseDevice
{
    [JsonIgnore]
    public double VoltageOut1 { get; set; }
    [JsonIgnore]
    public double VoltageOut2 { get; set; }
    [JsonIgnore]
    MeterMode Mode { get; set; }

    public VoltageCurrentMeter(string name) : base(name)
    {
        IsDeviceType = "Вольтметр";
    }
}

internal enum MeterMode
{
    VoltageOut1,
    VoltageOut2
}