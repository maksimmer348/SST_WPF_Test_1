using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class ThermoVoltmeter : BaseDevice
{
    [JsonIgnore]
    public double Temperature { get; set; }
    [JsonIgnore]
    public double VoltageOut1 { get; set; }
    [JsonIgnore]
    public double VoltageOut2 { get; set; }
    /// <summary>
    /// Режим измерения канал - 1 или 2
    /// </summary>
    [JsonIgnore]
    MeterMode Mode { get; set; }

    public ThermoVoltmeter(string name ) : base(name)
    {
        IsDeviceType = "ТермоВольтметр";
    }
}