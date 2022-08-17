using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class Heat : BaseDevice
{
    
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