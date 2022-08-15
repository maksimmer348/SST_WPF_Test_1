using Newtonsoft.Json;

namespace SST_WPF_Test_1;

public class BigLoad : BaseDevice
{
    /// <summary>
    /// Статус выхода устройства
    /// </summary>
    [JsonIgnore]
    public bool Output { get; set; }
    public BigLoad(string name) : base(name)
    {
        IsDeviceType = "Большая нагрузка/Генератор";
    }
}