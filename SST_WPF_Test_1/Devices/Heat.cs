using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class Heat : BaseDevice
{
    [JsonIgnore]
    public double Temperature { get; set; }
    
    /// <summary>
    /// Статус выхода устройства
    /// </summary>
    [JsonIgnore]
    public bool Output { get; set; }

    public Heat(string name ) : base(name)
    {
        IsDeviceType = "Нагрев";
    }
}